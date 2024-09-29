SET ARG="%1"
SET SRC_FILE="%~dp0..\Data\tuner_data.epd"
if %ARG% == "-c" (
ECHO "Copying tuner data..."
COPY /A %SRC_FILE% "%~dp0..\Snail-Chess.Tuner\bin\Release\net7.0\dataset.epd"
) else (
ECHO "Tuner is about to start..."
dotnet build -c "Release" && CLS && Snail-Chess.Tuner\bin\Release\net7.0\ChessClubTuner.exe
)