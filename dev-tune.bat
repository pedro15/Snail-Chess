SET ARG="%1"
SET SRC_FILE="%~dp0Snail-Chess.Tuner\data\tuner_data.epd"

if %ARG% == "-c" (
COPY /A %SRC_FILE% "%~dp0Snail-Chess.Tuner\bin\Release\net7.0\dataset.epd"
) else (
dotnet build -c "Release" && CLS && Snail-Chess.Tuner\bin\Release\net7.0\ChessClubTuner.exe
)
PAUSE