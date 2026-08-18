// Card parsing helpers mirror PEval.CardRank/CardSuit on the server (PokerCalculator.Core),
// kept intentionally simple: this is a best-effort client-side check so obvious mistakes
// (wrong card count, bad rank/suit, duplicate card) never leave the browser. The server
// remains the source of truth - its FormatException messages are shown verbatim on 4xx.

const MAX_VILLAINS = { Holdem: 8, Omaha: 4 };
const HERO_CARD_COUNT = { Holdem: 2, Omaha: 4 };
const HERO_PLACEHOLDER = { Holdem: "AhKs", Omaha: "AhKsQdJc" };

function cardRank(ch) {
  const c = ch.toUpperCase();
  if (c === "A") return 12;
  if (c === "K") return 11;
  if (c === "Q") return 10;
  if (c === "J") return 9;
  if (c === "T") return 8;
  if (c >= "2" && c <= "9") return c.charCodeAt(0) - "2".charCodeAt(0);
  return -1;
}

function cardSuit(ch) {
  const c = ch.toUpperCase();
  if (c === "C") return 0;
  if (c === "H") return 1;
  if (c === "S") return 2;
  if (c === "D") return 3;
  return -1;
}

// Parses a card string into a Set of "rank-suit" keys, or an error message.
function parseCardString(raw) {
  const s = (raw || "").replace(/\s+/g, "");
  if (s.length === 0) return { cards: new Set() };
  if (s.length % 2 !== 0) return { error: "Número ímpar de caracteres - cada carta precisa de rank + naipe." };

  const cards = new Set();
  for (let i = 0; i < s.length; i += 2) {
    const rank = cardRank(s[i]);
    const suit = cardSuit(s[i + 1]);
    const token = s.substring(i, i + 2);
    if (rank < 0 || suit < 0) return { error: `Carta inválida: '${token}'.` };
    const key = rank + "-" + suit;
    if (cards.has(key)) return { error: `Carta repetida: '${token}'.` };
    cards.add(key);
  }
  return { cards };
}

function validateHero(raw, game) {
  const { cards, error } = parseCardString(raw);
  if (error) return error;
  const expected = HERO_CARD_COUNT[game];
  if (!cards || cards.size !== expected) return `Hero precisa ter exatamente ${expected} cartas distintas para ${game}.`;
  return null;
}

function validateBoard(raw, heroCards) {
  const { cards, error } = parseCardString(raw);
  if (error) return { error };
  if (cards.size === 0) return { cards };
  if (![3, 4, 5].includes(cards.size)) return { error: "Board precisa ter 0, 3, 4 ou 5 cartas distintas." };
  for (const key of cards) {
    if (heroCards.has(key)) return { error: "Board não pode repetir uma carta do hero." };
  }
  return { cards };
}

// Light shape check per range token - the real combinatorics (suited/offsuit expansion,
// rank-pattern suit assignment) live server-side in RangeParser; this just rejects
// obviously malformed tokens before they're sent.
function validateVillainToken(token, game) {
  const t = token.trim();
  if (t.length === 0) return null;

  if (game === "Omaha") {
    if (t.length === 8) return parseCardString(t).error || null;
    if (t.length >= 4 && t.length <= 6) {
      const ranks = t.substring(0, 4);
      const suffix = t.substring(4).toLowerCase();
      for (const ch of ranks) if (cardRank(ch) < 0) return `Token Omaha inválido: '${t}'.`;
      if (suffix !== "" && suffix !== "r" && suffix !== "s" && suffix !== "ds")
        return `Sufixo inválido em '${t}' (use 'r', 's' ou 'ds').`;
      return null;
    }
    return `Token Omaha deve ter 4 ranks (ex.: AAKK, AAKKds) ou um combo exato de 4 cartas (ex.: AhKdQsJc): '${t}'.`;
  }

  if (t.length === 4) return parseCardString(t).error || null;
  if (t.length === 2 || t.length === 3) {
    if (cardRank(t[0]) < 0 || cardRank(t[1]) < 0) return `Token inválido: '${t}'.`;
    if (t.length === 3 && !"so".includes(t[2].toLowerCase())) return `Sufixo inválido em '${t}' (use 's' ou 'o').`;
    return null;
  }
  return `Token inválido: '${t}'.`;
}

// --- UI wiring ---

const form = document.getElementById("equity-form");
const heroInput = document.getElementById("hero");
const heroError = document.getElementById("hero-error");
const boardInput = document.getElementById("board");
const boardError = document.getElementById("board-error");
const villainList = document.getElementById("villain-list");
const addVillainBtn = document.getElementById("add-villain");
const submitBtn = document.getElementById("submit-btn");
const resultSection = document.getElementById("result");
const apiErrorBox = document.getElementById("api-error");
const apiBaseInput = document.getElementById("api-base");

function currentGame() {
  return document.querySelector('input[name="game"]:checked').value;
}

function addVillainRow() {
  const game = currentGame();
  if (villainList.children.length >= MAX_VILLAINS[game]) return;

  const row = document.createElement("div");
  row.className = "villain-row";

  const input = document.createElement("input");
  input.type = "text";
  input.placeholder = game === "Omaha" ? "AAKK, AKQJ" : "AA, AKs, TT";
  input.autocomplete = "off";
  input.spellcheck = false;

  const err = document.createElement("span");
  err.className = "error";

  const wrap = document.createElement("div");
  wrap.style.flex = "1";
  wrap.appendChild(input);
  wrap.appendChild(err);

  const removeBtn = document.createElement("button");
  removeBtn.type = "button";
  removeBtn.className = "remove-villain";
  removeBtn.textContent = "✕";
  removeBtn.title = "Remover vilão";
  removeBtn.addEventListener("click", () => {
    row.remove();
    updateAddVillainState();
  });

  row.appendChild(wrap);
  row.appendChild(removeBtn);
  villainList.appendChild(row);
  updateAddVillainState();
}

function updateAddVillainState() {
  const game = currentGame();
  addVillainBtn.disabled = villainList.children.length >= MAX_VILLAINS[game];
}

function onGameChange() {
  const game = currentGame();
  heroInput.placeholder = HERO_PLACEHOLDER[game];
  villainList.querySelectorAll("input[type=text]").forEach((input) => {
    input.placeholder = game === "Omaha" ? "AAKK, AKQJds" : "AA, AKs, TT";
  });
  while (villainList.children.length > MAX_VILLAINS[game]) {
    villainList.lastElementChild.remove();
  }
  updateAddVillainState();
  clearErrors();
}

document.querySelectorAll('input[name="game"]').forEach((el) => el.addEventListener("change", onGameChange));
addVillainBtn.addEventListener("click", addVillainRow);

function clearErrors() {
  heroError.textContent = "";
  boardError.textContent = "";
  villainList.querySelectorAll(".error").forEach((el) => (el.textContent = ""));
  apiErrorBox.classList.add("hidden");
  apiErrorBox.textContent = "";
}

function showApiError(message) {
  apiErrorBox.textContent = message;
  apiErrorBox.classList.remove("hidden");
}

// Returns the built request payload, or null if a validation error was found and
// displayed inline (submission is aborted in that case).
function validateAndBuildRequest() {
  clearErrors();
  const game = currentGame();
  let hasError = false;

  const heroErr = validateHero(heroInput.value, game);
  if (heroErr) {
    heroError.textContent = heroErr;
    hasError = true;
  }

  const heroCards = parseCardString(heroInput.value).cards || new Set();
  const boardResult = validateBoard(boardInput.value, heroCards);
  if (boardResult.error) {
    boardError.textContent = boardResult.error;
    hasError = true;
  }

  const villainRanges = [];
  villainList.querySelectorAll(".villain-row").forEach((row) => {
    const input = row.querySelector("input[type=text]");
    const errEl = row.querySelector(".error");
    const tokens = input.value.split(",").map((t) => t.trim()).filter((t) => t.length > 0);

    for (const token of tokens) {
      const err = validateVillainToken(token, game);
      if (err) {
        errEl.textContent = err;
        hasError = true;
        break;
      }
    }
    villainRanges.push(tokens);
  });

  if (hasError) return null;

  return {
    hero: heroInput.value.trim(),
    board: boardInput.value.trim(),
    villainRanges,
    game,
  };
}

function renderResult(data) {
  document.getElementById("stat-win").textContent = data.winPercent.toFixed(4) + "%";
  document.getElementById("stat-loss").textContent = data.lossPercent.toFixed(4) + "%";
  document.getElementById("stat-tie").textContent = data.tiePercent.toFixed(4) + "%";
  document.getElementById("stat-equity").textContent = data.equity.toFixed(4) + "%";
  document.getElementById("stat-sims").textContent = `${data.simulationsRun.toLocaleString("pt-BR")} simulações`;
  document.getElementById("stat-cache").classList.toggle("hidden", !data.fromCache);
  resultSection.classList.remove("hidden");
}

// The API returns 400 bodies as a JSON-encoded string (Results.BadRequest(ex.Message)),
// so unwrap that; fall back to the raw text for anything else (503 has no body, network
// failures never reach here at all).
async function extractErrorMessage(response) {
  const text = await response.text();
  try {
    const parsed = JSON.parse(text);
    return typeof parsed === "string" ? parsed : text;
  } catch {
    return text || `Erro ${response.status}`;
  }
}

form.addEventListener("submit", async (event) => {
  event.preventDefault();

  const payload = validateAndBuildRequest();
  if (!payload) return;

  resultSection.classList.add("hidden");
  apiErrorBox.classList.add("hidden");
  submitBtn.disabled = true;
  submitBtn.textContent = "Calculando...";

  try {
    const response = await fetch(`${apiBaseInput.value.replace(/\/$/, "")}/api/equity`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
    });

    if (response.status === 503) {
      showApiError("Servidor ocupado no momento - tente novamente em instantes.");
    } else if (!response.ok) {
      showApiError(await extractErrorMessage(response));
    } else {
      renderResult(await response.json());
    }
  } catch {
    showApiError(`Não foi possível conectar em ${apiBaseInput.value}. A API está rodando?`);
  } finally {
    submitBtn.disabled = false;
    submitBtn.textContent = "Calcular equity";
  }
});

addVillainRow();
onGameChange();
