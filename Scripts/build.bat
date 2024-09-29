SET ARG="%1"
if %ARG% == "-d" (
    SET FOLDER="_build_DEV" 
)else (
    SET FOLDER="_build_BASE"
)
dotnet build Snail-Chess.Cmd\Snail-Chess.Cmd.csproj  -c "Release" -o %FOLDER%