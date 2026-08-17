@echo off
REM Ten varied end-to-end smoke tests: 5 Hold'em against POST /api/equity, 5 Omaha
REM against POST /api/equity/omaha (the dedicated convenience route - no Game field
REM needed in the body, it's implied by the URL).
REM Start the API first (dotnet run, or F5), then run this .bat. Optional first arg
REM overrides the base URL (default matches launchSettings.json's http profile).

setlocal

set "BASEURL=http://localhost:5113"
if not "%~1"=="" set "BASEURL=%~1"

set "TMPDIR=%TEMP%\poker-equity-smoketest"
if not exist "%TMPDIR%" mkdir "%TMPDIR%"

REM Hold'em
echo {"hero":"AhKs","board":"","villainRanges":null}>"%TMPDIR%\test1.json"
echo {"hero":"QhQd","board":"","villainRanges":[["AKo","JJ","TT"]]}>"%TMPDIR%\test2.json"
echo {"hero":"AsKs","board":"TsJsQh","villainRanges":[["QQ","76s"]]}>"%TMPDIR%\test3.json"
echo {"hero":"AhAd","board":"","villainRanges":[["KK","QQ"],["AKs","AKo"]]}>"%TMPDIR%\test4.json"
echo {"hero":"2c2d","board":"AsKsQsJs","villainRanges":[["AhKh"]]}>"%TMPDIR%\test5.json"

REM Omaha
echo {"hero":"AhKsQdJc","board":"","villainRanges":null}>"%TMPDIR%\test6.json"
echo {"hero":"AsAdKhKd","board":"","villainRanges":[["QQJJ","AAKK"]]}>"%TMPDIR%\test7.json"
echo {"hero":"AhKhQhJh","board":"Th9h8c","villainRanges":[["2233"]]}>"%TMPDIR%\test8.json"
echo {"hero":"AhAdKhKd","board":"","villainRanges":[["QQJJ"],["TT99"]]}>"%TMPDIR%\test9.json"
echo {"hero":"2c2d2h2s","board":"AsKsQsJs","villainRanges":[["AhKhQhJh"]]}>"%TMPDIR%\test10.json"

:RunAll
echo Testing against %BASEURL%
echo.

echo ---------- Hold'em (/api/equity) ----------
call :RunTest "1. Heads-up preflop, no villain range (random villain)" "%TMPDIR%\test1.json" "/api/equity"
call :RunTest "2. Single villain range, preflop" "%TMPDIR%\test2.json" "/api/equity"
call :RunTest "3. Single villain range, on a flop board" "%TMPDIR%\test3.json" "/api/equity"
call :RunTest "4. Two villains, preflop" "%TMPDIR%\test4.json" "/api/equity"
call :RunTest "5. Heads-up, turn board, exact-combo villain range" "%TMPDIR%\test5.json" "/api/equity"

echo ---------- Omaha (/api/equity/omaha) ----------
call :RunTest "6. Heads-up preflop, no villain range (random villain)" "%TMPDIR%\test6.json" "/api/equity/omaha"
call :RunTest "7. Single villain range (rank patterns), preflop" "%TMPDIR%\test7.json" "/api/equity/omaha"
call :RunTest "8. Single villain range, on a flop board" "%TMPDIR%\test8.json" "/api/equity/omaha"
call :RunTest "9. Two villains, preflop" "%TMPDIR%\test9.json" "/api/equity/omaha"
call :RunTest "10. Heads-up, turn board, exact-combo villain range" "%TMPDIR%\test10.json" "/api/equity/omaha"

echo.
echo All tests finished.
echo.
choice /c YN /n /m "Run all tests again? [Y/N]: "
if errorlevel 2 goto :Done
if errorlevel 1 goto :RunAll

:Done
pause
goto :eof

:RunTest
echo ============================================================
echo %~1
echo Request body:
type "%~2"
echo.
echo Response:
curl -s -w "\nHTTP status: %%{http_code}\n" -X POST "%BASEURL%%~3" -H "Content-Type: application/json" -d "@%~2"
echo.
goto :eof
