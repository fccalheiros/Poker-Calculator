// Card/range pickers replace free-text entry for Hero, Board and villain ranges. The
// underlying request payload sent to the API is unchanged (hero/board strings,
// villainRanges as arrays of range tokens) - the picker UI just builds those values by
// clicking instead of typing, so every token it produces is well-formed by construction.

const MAX_VILLAINS = { Holdem: 8, Omaha: 4 };
const HERO_CARD_COUNT = { Holdem: 2, Omaha: 4 };

const RANKS = ["A", "K", "Q", "J", "T", "9", "8", "7", "6", "5", "4", "3", "2"];
const RANK_TO_INDEX = { A: 0, K: 1, Q: 2, J: 3, T: 4, "9": 5, "8": 6, "7": 7, "6": 8, "5": 9, "4": 10, "3": 11, "2": 12 };
const SUITS = [
  { letter: "s", symbol: "♠", cls: "black" },
  { letter: "h", symbol: "♥", cls: "red" },
  { letter: "d", symbol: "♦", cls: "red" },
  { letter: "c", symbol: "♣", cls: "black" },
];

// Every Hold'em starting-hand category ranked strongest-to-weakest, ported verbatim from
// the WinForms app's own StrengthOrder table (Distribution.cs) so the percentage slider
// here matches the desktop app's behavior exactly.
const STRENGTH_ORDER = [
  "AA", "KK", "QQ", "JJ", "TT", "AKs", "99", "AQs", "AKo", "AJs", "KQs", "88", "ATs", "AQo", "KJs", "KTs", "QJs", "AJo", "KQo", "QTs",
  "A9s", "77", "ATo", "JTs", "KJo", "A8s", "K9s", "QJo", "A7s", "KTo", "Q9s", "A5s", "66", "A6s", "QTo", "J9s", "A9o", "T9s",
  "A4s", "K8s", "JTo", "K7s", "A8o", "A3s", "Q8s", "K9o", "A2s", "K6s", "J8s", "T8s", "A7o", "55", "Q9o", "98s", "K5s", "Q7s",
  "J9o", "A5o", "T9o", "A6o", "K4s", "K8o", "Q6s", "J7s", "T7s", "A4o", "97s", "K3s", "87s", "Q5s", "K7o", "44", "Q8o", "A3o", "K2s",
  "J8o", "Q4s", "T8o", "J6s", "K6o", "A2o", "T6s", "98o", "76s", "86s", "96s", "Q3s", "J5s", "K5o", "Q7o", "Q2s", "J4s", "33", "65s",
  "J7o", "T7o", "K4o", "75s", "T5s", "Q6o", "J3s", "95s", "87o", "85s", "97o", "T4s", "K3o", "J2s", "54s", "Q5o", "64s", "T3s", "22",
  "K2o", "74s", "76o", "T2s", "Q4o", "J6o", "84s", "94s", "86o", "T6o", "96o", "53s", "93s", "Q3o", "J5o", "63s", "43s", "92s", "73s",
  "65o", "Q2o", "J4o", "83s", "75o", "52s", "85o", "82s", "T5o", "95o", "J3o", "62s", "54o", "42s", "T4o", "J2o", "72s", "64o", "T3o",
  "32s", "74o", "84o", "T2o", "94o", "53o", "93o", "63o", "43o", "92o", "73o", "83o", "52o", "82o", "42o", "62o", "72o", "32o",
];
const STRENGTH_RANK = new Map(STRENGTH_ORDER.map((token, i) => [token, i]));
const TOTAL_COMBOS = (52 * 51) / 2; // 1326

function comboCount(token) {
  if (token.length === 2) return 6; // pair
  return token[2] === "s" ? 4 : 12; // suited / offsuit
}

// Returns { token, category } for grid cell (row, col), row/col in 0..12 (0 = A, 12 = 2).
// Diagonal = pair, above diagonal = suited, below = offsuit.
function holdemCellInfo(row, col) {
  if (row === col) return { token: RANKS[row] + RANKS[row], category: "pair" };
  if (col > row) return { token: RANKS[row] + RANKS[col] + "s", category: "suited" };
  return { token: RANKS[col] + RANKS[row] + "o", category: "offsuit" };
}

function sortedHoldemTokens(tokenSet) {
  return Array.from(tokenSet).sort((a, b) => STRENGTH_RANK.get(a) - STRENGTH_RANK.get(b));
}

// --- Application state ---

function createVillain() {
  return {
    holdemTokens: new Set(),
    omahaTokens: [], // { ranks: ['A','A','K','K'], qualifier: null | 'r' | 's' | 'ds' }
  };
}

let heroCards = []; // card codes like "Ah", "Ks", in selection order
let boardCards = [];
let villains = [createVillain()];
let activeVillainIndex = 0;
let omahaSlots = []; // ranks currently being built for the active villain's Omaha token
let omahaQualifier = null;
let cardPickerTarget = "hero"; // "hero" | "board" - which set the single shared grid currently edits

// --- DOM references ---

const form = document.getElementById("equity-form");
const pickerToggleEl = document.getElementById("picker-toggle");
const cardGridEl = document.getElementById("card-grid");
const heroPreviewEl = document.getElementById("hero-preview");
const heroError = document.getElementById("hero-error");
const boardPreviewEl = document.getElementById("board-preview");
const boardError = document.getElementById("board-error");
const villainTabsEl = document.getElementById("villain-tabs");
const rangeEditorEl = document.getElementById("range-editor");
const villainError = document.getElementById("villain-error");
const addVillainBtn = document.getElementById("add-villain");
const submitBtn = document.getElementById("submit-btn");
const resultSection = document.getElementById("result");
const apiErrorBox = document.getElementById("api-error");
const apiBaseInput = document.getElementById("api-base");

function currentGame() {
  return document.querySelector('input[name="game"]:checked').value;
}

// --- Hero / Board card pickers ---

function cardChip(code) {
  const rank = code[0];
  const suit = SUITS.find((s) => s.letter === code[1]);
  const chip = document.createElement("span");
  chip.className = "card-chip " + suit.cls;
  chip.textContent = rank + suit.symbol;
  return chip;
}

function renderCardPreview(container, cards) {
  container.innerHTML = "";
  if (cards.length === 0) {
    const empty = document.createElement("span");
    empty.className = "hint";
    empty.textContent = "Nenhuma carta selecionada";
    container.appendChild(empty);
    return;
  }
  cards.forEach((code) => container.appendChild(cardChip(code)));
}

function renderCardGrid(container, selected, otherSelected, maxCount, onToggle) {
  container.innerHTML = "";
  const grid = document.createElement("div");
  grid.className = "card-grid";
  for (let s = 0; s < 4; s++) {
    for (let r = 0; r < 13; r++) {
      const code = RANKS[r] + SUITS[s].letter;
      const isSelected = selected.includes(code);
      const isBlocked = !isSelected && otherSelected.includes(code);
      const isFull = !isSelected && selected.length >= maxCount;
      const cell = document.createElement("button");
      cell.type = "button";
      cell.className = "card-cell " + SUITS[s].cls + (isSelected ? " selected" : "") + (isBlocked || isFull ? " blocked" : "");
      cell.disabled = isBlocked || isFull;
      cell.textContent = RANKS[r] + SUITS[s].symbol;
      if (isBlocked) cell.title = "Carta já usada no outro conjunto (hero/board)";
      cell.addEventListener("click", () => onToggle(code));
      grid.appendChild(cell);
    }
  }
  container.appendChild(grid);
}

function toggleHeroCard(code) {
  const i = heroCards.indexOf(code);
  if (i >= 0) {
    heroCards.splice(i, 1);
  } else {
    if (boardCards.includes(code)) return; // guarded by disabled state; kept as a safety net
    if (heroCards.length >= HERO_CARD_COUNT[currentGame()]) return;
    heroCards.push(code);
  }
  renderCardPicker();
  renderHeroPreview();
  renderBoardPreview();
}

function toggleBoardCard(code) {
  const i = boardCards.indexOf(code);
  if (i >= 0) {
    boardCards.splice(i, 1);
  } else {
    if (heroCards.includes(code)) return; // guarded by disabled state; kept as a safety net
    if (boardCards.length >= 5) return;
    boardCards.push(code);
  }
  renderCardPicker();
  renderHeroPreview();
  renderBoardPreview();
}

function renderHeroPreview() {
  renderCardPreview(heroPreviewEl, heroCards);
}

function renderBoardPreview() {
  renderCardPreview(boardPreviewEl, boardCards);
}

// Renders the single shared card grid for whichever target (hero/board) is currently
// selected via the picker toggle; the other set's cards still show as blocked.
function renderCardPicker() {
  if (cardPickerTarget === "hero") {
    renderCardGrid(cardGridEl, heroCards, boardCards, HERO_CARD_COUNT[currentGame()], toggleHeroCard);
  } else {
    renderCardGrid(cardGridEl, boardCards, heroCards, 5, toggleBoardCard);
  }
}

function renderPickerToggle() {
  pickerToggleEl.querySelectorAll(".picker-toggle-btn").forEach((btn) => {
    btn.classList.toggle("active", btn.dataset.target === cardPickerTarget);
  });
}

function setPickerTarget(target) {
  cardPickerTarget = target;
  renderPickerToggle();
  renderCardPicker();
}

pickerToggleEl.querySelectorAll(".picker-toggle-btn").forEach((btn) => {
  btn.addEventListener("click", () => setPickerTarget(btn.dataset.target));
});

// --- Villain range editor: Hold'em 13x13 grid ---

function renderHoldemRangeEditor(villain, container) {
  const box = document.createElement("div");
  box.className = "range-editor-box";

  const header = document.createElement("div");
  header.className = "section-header";
  const title = document.createElement("h2");
  title.textContent = "Range do Vilão " + (activeVillainIndex + 1);
  const badge = document.createElement("span");
  badge.className = "badge";
  const combos = Array.from(villain.holdemTokens).reduce((sum, t) => sum + comboCount(t), 0);
  badge.textContent = combos + " combos";
  header.appendChild(title);
  header.appendChild(badge);
  box.appendChild(header);

  const quick = document.createElement("div");
  quick.className = "quick-selects";
  quick.appendChild(quickSelectButton("Tudo", () => selectAllCells(() => true)));
  quick.appendChild(quickSelectButton("Qualquer Suited", () => selectAllCells((row, col) => col > row)));
  quick.appendChild(quickSelectButton("Broadway", () => selectAllCells((row, col) => row < 5 && col < 5)));
  quick.appendChild(quickSelectButton("Pares", () => selectAllCells((row, col) => row === col)));
  const clearBtn = quickSelectButton("Limpar", () => {
    villain.holdemTokens.clear();
    renderRangeEditor();
  });
  clearBtn.classList.add("ghost");
  quick.appendChild(clearBtn);
  box.appendChild(quick);

  function selectAllCells(predicate) {
    for (let row = 0; row < 13; row++) {
      for (let col = 0; col < 13; col++) {
        if (!predicate(row, col)) continue;
        villain.holdemTokens.add(holdemCellInfo(row, col).token);
      }
    }
    renderRangeEditor();
  }

  const grid = document.createElement("div");
  grid.className = "range-grid";
  for (let row = 0; row < 13; row++) {
    for (let col = 0; col < 13; col++) {
      const { token, category } = holdemCellInfo(row, col);
      const cell = document.createElement("button");
      cell.type = "button";
      cell.className = "cell " + category + (villain.holdemTokens.has(token) ? " selected" : "");
      cell.textContent = token;
      cell.addEventListener("click", () => {
        if (villain.holdemTokens.has(token)) villain.holdemTokens.delete(token);
        else villain.holdemTokens.add(token);
        renderRangeEditor();
      });
      grid.appendChild(cell);
    }
  }
  box.appendChild(grid);

  const legend = document.createElement("div");
  legend.className = "legend";
  legend.innerHTML =
    '<span class="legend-item"><i class="swatch pair"></i>Par</span>' +
    '<span class="legend-item"><i class="swatch suited"></i>Suited</span>' +
    '<span class="legend-item"><i class="swatch offsuit"></i>Offsuit</span>' +
    '<span class="legend-item"><i class="swatch selected"></i>Selecionado</span>';
  box.appendChild(legend);

  const percentRow = document.createElement("div");
  percentRow.className = "percent-row";
  const percentLabel = document.createElement("label");
  percentLabel.textContent = "Top % da faixa (por força de mão) — substitui a seleção atual";
  const percentControl = document.createElement("div");
  percentControl.className = "percent-control";
  const slider = document.createElement("input");
  slider.type = "range";
  slider.min = "0";
  slider.max = "169";
  slider.value = String(currentSliderValue(villain.holdemTokens));
  const readout = document.createElement("span");
  readout.className = "percent-readout";
  readout.textContent = formatPercent(percentForSliderValue(Number(slider.value)));
  slider.addEventListener("input", () => {
    const n = Number(slider.value);
    villain.holdemTokens.clear();
    for (let i = 0; i < n; i++) villain.holdemTokens.add(STRENGTH_ORDER[i]);
    readout.textContent = formatPercent(percentForSliderValue(n));
    renderRangeEditor();
  });
  percentControl.appendChild(slider);
  percentControl.appendChild(readout);
  percentRow.appendChild(percentLabel);
  percentRow.appendChild(percentControl);
  box.appendChild(percentRow);

  const textPreview = document.createElement("p");
  textPreview.className = "range-text";
  textPreview.textContent = villain.holdemTokens.size > 0 ? sortedHoldemTokens(villain.holdemTokens).join(", ") : "(nenhum token selecionado)";
  box.appendChild(textPreview);

  container.appendChild(box);
}

function percentForSliderValue(n) {
  let combos = 0;
  for (let i = 0; i < n; i++) combos += comboCount(STRENGTH_ORDER[i]);
  return (combos / TOTAL_COMBOS) * 100;
}

function formatPercent(p) {
  return p.toLocaleString("pt-BR", { minimumFractionDigits: 1, maximumFractionDigits: 1 }) + "%";
}

// Approximates which slider value produced the villain's current selection (used only to
// give the slider a sensible starting position when re-rendering after a manual grid edit).
function currentSliderValue(tokenSet) {
  let n = 0;
  for (let i = 0; i < STRENGTH_ORDER.length; i++) {
    if (tokenSet.has(STRENGTH_ORDER[i])) n = i + 1;
  }
  return tokenSet.size === 0 ? 0 : n;
}

function quickSelectButton(label, onClick) {
  const btn = document.createElement("button");
  btn.type = "button";
  btn.className = "chip-btn";
  btn.textContent = label;
  btn.addEventListener("click", onClick);
  return btn;
}

// --- Villain range editor: Omaha rank-pattern builder ---

function renderOmahaRangeEditor(villain, container) {
  const box = document.createElement("div");
  box.className = "range-editor-box";

  const header = document.createElement("div");
  header.className = "section-header";
  const title = document.createElement("h2");
  title.textContent = "Construtor de Padrão — Vilão " + (activeVillainIndex + 1);
  const badge = document.createElement("span");
  badge.className = "badge";
  badge.textContent = villain.omahaTokens.length + " padrões";
  header.appendChild(title);
  header.appendChild(badge);
  box.appendChild(header);

  const builderBox = document.createElement("div");
  builderBox.className = "builder-box";

  const slotsLabel = document.createElement("span");
  slotsLabel.className = "builder-label";
  slotsLabel.textContent = "Ranks escolhidos (clique em um rank abaixo para preencher; clique num slot para remover)";
  builderBox.appendChild(slotsLabel);

  const slotsRow = document.createElement("div");
  slotsRow.className = "slots";
  for (let i = 0; i < 4; i++) {
    const slot = document.createElement("button");
    slot.type = "button";
    const rank = omahaSlots[i];
    slot.className = "slot " + (rank ? "filled" : "empty");
    slot.textContent = rank || "";
    if (rank) {
      slot.title = "Remover";
      slot.addEventListener("click", () => {
        omahaSlots.splice(i, 1);
        renderRangeEditor();
      });
    }
    slotsRow.appendChild(slot);
  }
  builderBox.appendChild(slotsRow);

  const palette = document.createElement("div");
  palette.className = "rank-palette";
  RANKS.forEach((rank) => {
    const key = document.createElement("button");
    key.type = "button";
    key.className = "rank-key";
    key.textContent = rank;
    key.disabled = omahaSlots.length >= 4;
    key.addEventListener("click", () => {
      if (omahaSlots.length >= 4) return;
      omahaSlots.push(rank);
      renderRangeEditor();
    });
    palette.appendChild(key);
  });
  builderBox.appendChild(palette);

  const qualifierWrap = document.createElement("div");
  qualifierWrap.className = "suit-qualifier";
  const qLabel = document.createElement("span");
  qLabel.className = "builder-label";
  qLabel.textContent = "Naipe";
  qualifierWrap.appendChild(qLabel);
  const qTabs = document.createElement("div");
  qTabs.className = "qualifier-tabs";
  [
    { value: null, label: "Qualquer" },
    { value: "r", label: "Rainbow (r)" },
    { value: "s", label: "Single (s)" },
    { value: "ds", label: "Double (ds)" },
  ].forEach((opt) => {
    const tab = document.createElement("button");
    tab.type = "button";
    tab.className = "qualifier-tab" + (omahaQualifier === opt.value ? " active" : "");
    tab.textContent = opt.label;
    tab.addEventListener("click", () => {
      omahaQualifier = opt.value;
      renderRangeEditor();
    });
    qTabs.appendChild(tab);
  });
  qualifierWrap.appendChild(qTabs);
  builderBox.appendChild(qualifierWrap);

  const addRow = document.createElement("div");
  addRow.className = "add-token-row";
  const addBtn = document.createElement("button");
  addBtn.type = "button";
  addBtn.className = "chip-btn";
  addBtn.textContent = "Adicionar ao Range";
  addBtn.disabled = omahaSlots.length !== 4;
  addBtn.addEventListener("click", () => {
    if (omahaSlots.length !== 4) return;
    const ranksSorted = [...omahaSlots].sort((a, b) => RANK_TO_INDEX[a] - RANK_TO_INDEX[b]);
    const token = { ranks: ranksSorted, qualifier: omahaQualifier };
    const label = ranksSorted.join("") + (omahaQualifier || "");
    if (!villain.omahaTokens.some((t) => t.ranks.join("") + (t.qualifier || "") === label)) {
      villain.omahaTokens.push(token);
    }
    omahaSlots = [];
    omahaQualifier = null;
    renderRangeEditor();
  });
  addRow.appendChild(addBtn);
  builderBox.appendChild(addRow);

  box.appendChild(builderBox);

  const listHeader = document.createElement("div");
  listHeader.className = "section-header";
  const listTitle = document.createElement("h2");
  listTitle.textContent = "Padrões no Range";
  listHeader.appendChild(listTitle);
  box.appendChild(listHeader);

  const tokenList = document.createElement("div");
  tokenList.className = "token-list";
  if (villain.omahaTokens.length === 0) {
    const empty = document.createElement("span");
    empty.className = "hint";
    empty.textContent = "Nenhum padrão adicionado ainda";
    tokenList.appendChild(empty);
  }
  villain.omahaTokens.forEach((t, i) => {
    const chip = document.createElement("div");
    chip.className = "token-chip";
    const label = document.createElement("span");
    label.textContent = t.ranks.join("");
    chip.appendChild(label);
    const tag = document.createElement("span");
    tag.className = "qtag" + (t.qualifier ? "" : " none");
    tag.textContent = t.qualifier || "qualquer";
    chip.appendChild(tag);
    const removeBtn = document.createElement("button");
    removeBtn.type = "button";
    removeBtn.className = "remove-x";
    removeBtn.textContent = "✕";
    removeBtn.addEventListener("click", () => {
      villain.omahaTokens.splice(i, 1);
      renderRangeEditor();
    });
    chip.appendChild(removeBtn);
    tokenList.appendChild(chip);
  });
  box.appendChild(tokenList);

  const textPreview = document.createElement("p");
  textPreview.className = "range-text";
  textPreview.textContent =
    villain.omahaTokens.length > 0
      ? villain.omahaTokens.map((t) => t.ranks.join("") + (t.qualifier || "")).join(", ")
      : "(nenhum padrão adicionado)";
  box.appendChild(textPreview);

  container.appendChild(box);
}

// --- Villain tabs + editor orchestration ---

function renderVillainTabs() {
  villainTabsEl.innerHTML = "";
  villains.forEach((v, i) => {
    const btn = document.createElement("button");
    btn.type = "button";
    btn.className = "villain-tab" + (i === activeVillainIndex ? " active" : "");
    btn.textContent = "Vilão " + (i + 1);
    btn.addEventListener("click", () => {
      activeVillainIndex = i;
      renderRangeEditor();
      renderVillainTabs();
    });
    villainTabsEl.appendChild(btn);
  });
  updateAddVillainState();
}

function renderRangeEditor() {
  rangeEditorEl.innerHTML = "";
  const villain = villains[activeVillainIndex];
  if (!villain) return;

  omahaSlots = omahaSlots.length ? omahaSlots : [];
  const game = currentGame();
  if (game === "Omaha") {
    renderOmahaRangeEditor(villain, rangeEditorEl);
  } else {
    renderHoldemRangeEditor(villain, rangeEditorEl);
  }

  const footer = document.createElement("div");
  footer.className = "villain-footer";
  const removeBtn = document.createElement("button");
  removeBtn.type = "button";
  removeBtn.className = "ghost-link";
  removeBtn.textContent = "Remover Vilão " + (activeVillainIndex + 1);
  removeBtn.addEventListener("click", removeActiveVillain);
  footer.appendChild(removeBtn);
  rangeEditorEl.appendChild(footer);
}

function addVillain() {
  const game = currentGame();
  if (villains.length >= MAX_VILLAINS[game]) return;
  villains.push(createVillain());
  activeVillainIndex = villains.length - 1;
  omahaSlots = [];
  omahaQualifier = null;
  renderVillainTabs();
  renderRangeEditor();
}

function removeActiveVillain() {
  villains.splice(activeVillainIndex, 1);
  if (activeVillainIndex >= villains.length) activeVillainIndex = Math.max(0, villains.length - 1);
  omahaSlots = [];
  omahaQualifier = null;
  renderVillainTabs();
  renderRangeEditor();
}

function updateAddVillainState() {
  const game = currentGame();
  addVillainBtn.disabled = villains.length >= MAX_VILLAINS[game];
}

function onGameChange() {
  // A game switch invalidates hero card count (2 vs 4) and range-token shape (Hold'em vs
  // Omaha tokens are not interchangeable), so start the hand over rather than carry over
  // selections that would no longer mean the same thing.
  heroCards = [];
  boardCards = [];
  villains = [createVillain()];
  activeVillainIndex = 0;
  omahaSlots = [];
  omahaQualifier = null;
  cardPickerTarget = "hero";
  clearErrors();
  renderPickerToggle();
  renderCardPicker();
  renderHeroPreview();
  renderBoardPreview();
  renderVillainTabs();
  renderRangeEditor();
}

document.querySelectorAll('input[name="game"]').forEach((el) => el.addEventListener("change", onGameChange));
addVillainBtn.addEventListener("click", addVillain);

function clearErrors() {
  heroError.textContent = "";
  boardError.textContent = "";
  villainError.textContent = "";
  apiErrorBox.classList.add("hidden");
  apiErrorBox.textContent = "";
}

function showApiError(message) {
  apiErrorBox.textContent = message;
  apiErrorBox.classList.remove("hidden");
}

// --- Validation + payload ---

function validateHeroCards(cards, game) {
  const expected = HERO_CARD_COUNT[game];
  if (cards.length !== expected) return `Hero precisa ter exatamente ${expected} cartas para ${game}. Selecionadas: ${cards.length}.`;
  return null;
}

function validateBoardCards(cards) {
  if (![0, 3, 4, 5].includes(cards.length)) return `Board precisa ter 0, 3, 4 ou 5 cartas. Selecionadas: ${cards.length}.`;
  return null;
}

// Belt-and-suspenders: the picker UI already blocks picking a card the other set holds,
// but this is checked again here rather than trusting that UI state never drifts.
function validateNoOverlap(hero, board) {
  const overlap = hero.filter((c) => board.includes(c));
  if (overlap.length > 0) return `Hero e board não podem compartilhar cartas: ${overlap.join(", ")}.`;
  return null;
}

function villainTokens(villain, game) {
  if (game === "Omaha") return villain.omahaTokens.map((t) => t.ranks.join("") + (t.qualifier || ""));
  return sortedHoldemTokens(villain.holdemTokens);
}

// Returns the built request payload, or null if a validation error was found and
// displayed inline (submission is aborted in that case).
function validateAndBuildRequest() {
  clearErrors();
  const game = currentGame();
  let hasError = false;

  const heroErr = validateHeroCards(heroCards, game);
  if (heroErr) {
    heroError.textContent = heroErr;
    hasError = true;
  }

  const boardErr = validateBoardCards(boardCards);
  if (boardErr) {
    boardError.textContent = boardErr;
    hasError = true;
  }

  const overlapErr = validateNoOverlap(heroCards, boardCards);
  if (overlapErr) {
    heroError.textContent = overlapErr;
    hasError = true;
  }

  if (hasError) return null;

  const villainRanges = villains.map((v) => villainTokens(v, game));

  return {
    hero: heroCards.join(""),
    board: boardCards.join(""),
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

renderPickerToggle();
renderCardPicker();
renderHeroPreview();
renderBoardPreview();
renderVillainTabs();
renderRangeEditor();
