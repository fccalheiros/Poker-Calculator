#!/usr/bin/env bash
# Smoketest against the public Poker Calculator API: N Hold'em + N Omaha requests
# with randomized hero/board/villains. Emails a summary via Resend only when at
# least one request fails (non-200). Deployed to /opt/poker-smoketest/smoketest.sh
# on the VM and run daily via cron at 3:12am (see deploy guide for the cron entry).
#
# Usage: smoketest.sh [pace_seconds] [count_per_game] [mode]
#   pace_seconds   seconds between request launches (default 0.5)
#   count_per_game requests per game, i.e. Hold'em and Omaha each (default 25)
#   mode           "sequential" (default): wait for each response before the
#                  next launch, so the real gap is response_time + pace_seconds.
#                  "burst": launch in background without waiting for the
#                  response, so requests actually overlap - use to generate
#                  real concurrent load (e.g. to observe CPU peaks).
set -uo pipefail

BASE_URL="https://felipecalheiros.com.br/poker/api/equity"
LOG_FILE="/var/log/poker-smoketest.log"
ENV_FILE="/etc/poker/notify.env"
RESEND_TO="fccalheiros@yahoo.com"
RESEND_FROM="Poker Test <onboarding@resend.dev>"
PACE_SECONDS="${1:-0.5}"
COUNT_PER_GAME="${2:-25}"
MODE="${3:-sequential}"
MAX_LOG_LINES=5000
TRIMMED_LOG_LINES=2000

RANKS=(2 3 4 5 6 7 8 9 T J Q K A)
SUITS=(h d c s)
BOARD_COUNTS=(0 3 4)

ERROR_FILE=$(mktemp /tmp/poker-smoketest-errors.XXXXXX)
trap 'rm -f "$ERROR_FILE" "${ERROR_FILE}.lock"' EXIT
PIDS=()
LOG_LOCK_FILE="/tmp/poker-smoketest.log.lock"

# Appends are flock-serialized since burst mode writes from many concurrent
# background subshells. The lock lives in /tmp, not next to LOG_FILE - opc can
# append to the existing /var/log/poker-smoketest.log but can't create new
# files in /var/log (root-owned, 755).
log() {
  { flock -x 200; echo "$(date -u +%FT%TZ) $*" >> "$LOG_FILE"; } 200>>"$LOG_LOCK_FILE"
}

record_error() {
  { flock -x 201; echo "$1" >> "$ERROR_FILE"; } 201>>"${ERROR_FILE}.lock"
}

# Keeps the log from growing forever across daily cron runs - trims to the most
# recent TRIMMED_LOG_LINES once it passes MAX_LOG_LINES.
trim_log() {
  [ -f "$LOG_FILE" ] || return
  if [ "$(wc -l < "$LOG_FILE")" -gt "$MAX_LOG_LINES" ]; then
    tail -n "$TRIMMED_LOG_LINES" "$LOG_FILE" > "${LOG_FILE}.tmp" && mv "${LOG_FILE}.tmp" "$LOG_FILE"
  fi
}

# Draws $1 distinct cards, avoiding any already present in the remaining args.
generate_distinct_cards() {
  local count=$1
  shift
  local used=("$@")
  local out=()
  while [ ${#out[@]} -lt "$count" ]; do
    local candidate="${RANKS[$((RANDOM % 13))]}${SUITS[$((RANDOM % 4))]}"
    local skip=0
    for c in "${used[@]}" "${out[@]}"; do
      [ "$c" == "$candidate" ] && skip=1 && break
    done
    [ $skip -eq 0 ] && out+=("$candidate")
  done
  echo "${out[*]}"
}

random_holdem_token() {
  local r1=${RANKS[$((RANDOM % 13))]}
  local t=$((RANDOM % 3))
  if [ $t -eq 0 ]; then
    echo "${r1}${r1}"
  else
    local r2=${RANKS[$((RANDOM % 13))]}
    while [ "$r2" == "$r1" ]; do r2=${RANKS[$((RANDOM % 13))]}; done
    if [ $t -eq 1 ]; then echo "${r1}${r2}s"; else echo "${r1}${r2}o"; fi
  fi
}

random_omaha_token() {
  local ranks=""
  for _ in 1 2 3 4; do ranks="${ranks}${RANKS[$((RANDOM % 13))]}"; done
  local quals=("" "r" "s" "ds")
  echo "${ranks}${quals[$((RANDOM % 4))]}"
}

# At least 5 tokens per villain, as requested.
random_villain_range() {
  local game=$1
  local n=$((RANDOM % 4 + 5)) # 5..8
  local tokens=""
  for _ in $(seq 1 "$n"); do
    if [ "$game" == "holdem" ]; then
      tokens="${tokens}\"$(random_holdem_token)\","
    else
      tokens="${tokens}\"$(random_omaha_token)\","
    fi
  done
  echo "[${tokens%,}]"
}

# 0-2 villains. 0 means an empty villainRanges array (the API's existing "one
# villain, unknown hand" default - there's no "facing nobody" mode to request).
build_villain_ranges() {
  local game=$1
  local n_villains=$((RANDOM % 3)) # 0,1,2
  if [ "$n_villains" -eq 0 ]; then
    echo "[]"
    return
  fi
  local parts=""
  for _ in $(seq 1 "$n_villains"); do
    parts="${parts}$(random_villain_range "$game"),"
  done
  echo "[${parts%,}]"
}

# Reseed per-invocation: background subshells forked in quick succession would
# otherwise inherit the same $RANDOM state from the parent and draw identical
# "random" hands.
do_request() {
  RANDOM=$BASHPID
  local game=$1 hero_count=$2
  local board_count=${BOARD_COUNTS[$((RANDOM % 3))]}
  local hero board
  read -ra hero <<< "$(generate_distinct_cards "$hero_count")"
  read -ra board <<< "$(generate_distinct_cards "$board_count" "${hero[@]}")"
  local hero_str board_str villains payload resp code body
  hero_str=$(IFS=; echo "${hero[*]}")
  board_str=$(IFS=; echo "${board[*]}")
  villains=$(build_villain_ranges "$game")
  payload="{\"hero\":\"${hero_str}\",\"board\":\"${board_str}\",\"villainRanges\":${villains},\"allowExactEnumeration\":true}"

  resp=$(curl -s --max-time 30 -w '\n%{http_code}' -X POST "${BASE_URL}/${game}" \
    -H "Content-Type: application/json" -d "$payload")
  code=$(echo "$resp" | tail -1)
  body=$(echo "$resp" | sed '$d')

  log "game=$game code=$code payload=$payload body=$body"

  if [ "$code" != "200" ]; then
    record_error "- [$game] HTTP $code
  payload: $payload
  response: $body"
  fi
}

run_tests() {
  local game=$1 hero_count=$2
  for _ in $(seq 1 "$COUNT_PER_GAME"); do
    if [ "$MODE" == "burst" ]; then
      do_request "$game" "$hero_count" &
      PIDS+=("$!")
    else
      do_request "$game" "$hero_count"
    fi
    sleep "$PACE_SECONDS"
  done
}

send_error_email() {
  if [ ! -f "$ENV_FILE" ]; then
    log "ERROR: $ENV_FILE not found, cannot send alert email ($ERROR_COUNT errors)"
    return
  fi
  # shellcheck disable=SC1090
  source "$ENV_FILE"

  local subject="Poker smoketest: ${ERROR_COUNT} error(s) - $(date -u +%F)"
  local body_text="Smoketest run at $(date -u +%FT%TZ)

Errors found (${ERROR_COUNT}):
${ERRORS}

Full log: ${LOG_FILE} (on the VM)"

  local send_result
  send_result=$(jq -n \
    --arg from "$RESEND_FROM" \
    --arg to "$RESEND_TO" \
    --arg subject "$subject" \
    --arg text "$body_text" \
    '{from: $from, to: [$to], subject: $subject, text: $text}' \
    | curl -s --max-time 30 -X POST https://api.resend.com/emails \
        -H "Authorization: Bearer ${RESEND_API_KEY}" \
        -H "Content-Type: application/json" \
        -d @-)

  log "Resend response: $send_result"
}

trim_log
log "=== smoketest start (mode=$MODE) ==="
run_tests holdem 2
run_tests omaha 4

if [ "$MODE" == "burst" ] && [ ${#PIDS[@]} -gt 0 ]; then
  wait "${PIDS[@]}"
fi

status=$(curl -s --max-time 30 "${BASE_URL%/equity}/status")
log "status=$status"

ERROR_COUNT=0
ERRORS=""
if [ -s "$ERROR_FILE" ]; then
  ERROR_COUNT=$(grep -c '^- \[' "$ERROR_FILE")
  ERRORS=$(cat "$ERROR_FILE")
fi

log "=== smoketest end: ${ERROR_COUNT} error(s) ==="

if [ "$ERROR_COUNT" -gt 0 ]; then
  send_error_email
fi
