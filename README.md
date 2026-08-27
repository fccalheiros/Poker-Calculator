# Poker Calculator

A Texas Hold'em / Omaha equity calculator. A bitmask-based hand evaluator drives a
Monte Carlo simulation engine, exposed through a web API and a browser frontend, plus
a WinForms tool for manual testing.

## Projects

- **PokerCalculator.Core** - hand evaluator (`PEval.GeneralProcessCardSet`), Monte Carlo
  simulation, range parsing. No dependencies outside the BCL.
- **PokerCalculator.Api** - ASP.NET Core minimal API. Runs simulations on a worker pool,
  caches results, and exposes them over HTTP.
- **PokerCalculator.Web** - static frontend (`wwwroot/`) for building a hand, board and
  villain ranges by clicking cards, plus a small reverse proxy that forwards `/api/*` to
  the API so the two can be deployed as a single origin (no CORS needed). UI text is in
  Portuguese.
- **PokerCalculator** (root `.csproj`) - a WinForms tool (`FMTest.cs`) for exercising the
  evaluator manually. Windows-only (`net8.0-windows`).

## Running locally

```powershell
dotnet run --project PokerCalculator.Api    # http://localhost:5113
dotnet run --project PokerCalculator.Web    # http://localhost:5180
```

Open `http://localhost:5180`. The frontend's "URL base" field defaults to the Web
project's own origin (relative paths through its proxy); point it elsewhere if calling
the API directly during development.

## API

All endpoints take a hero hand, an optional board, and one or more villain ranges, and
return win/loss/tie percentages plus equity over a number of Monte Carlo simulations
(cached when the range/board combination repeats):

- `POST /api/equity` - `game` field (`Holdem` or `Omaha`) selects the variant
- `POST /api/equity/holdem` / `POST /api/equity/omaha` - same payload, game fixed by route

Simulation sizing, worker count, and per-request limits (max villains, max range tokens,
max request body size) are configured under `Simulation` in `appsettings.json`.

## Deployment

The API and Web are deployed as two `systemd` services behind a single Cloudflare
Tunnel (the Web app is the only one with a public route; it proxies to the API, which
stays bound to `localhost`). Deploying a new build is a local `deploy.ps1`/`deploy.bat`
that aren't tracked in this repo (they hold the target VM's address and SSH key path).
