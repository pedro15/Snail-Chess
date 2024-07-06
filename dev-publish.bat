SET DIRECTORY="%~dp0Publish"
IF EXIST %DIRECTORY% @RD /S /Q %DIRECTORY%
dotnet build -c "Release"
ROBOCOPY "%~dp0Snail-Chess.Cmd/bin/Release/net7.0" %DIRECTORY% /mir
PAUSE