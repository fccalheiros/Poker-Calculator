// Pure static-file host for the equity calculator's frontend (wwwroot). No API logic
// lives here - it just serves index.html/styles.css/app.js, which talk to
// PokerCalculator.Api directly from the browser.
var app = WebApplication.CreateBuilder(args).Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();
