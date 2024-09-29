@echo off
SET CUTECHESS="C:\Program Files (x86)\Cute Chess\cutechess-cli.exe"
SET PGN_FILE="%~dp0sprt.pgn" 
SET OPENING_FILE="%~dp0Data/8moves_v3.epd"
IF EXIST %PGN_FILE% DEL %PGN_FILE%
SET TC=inf/6+0.06
CALL %CUTECHESS% -engine cmd="_build_DEV\Snail-Chess.exe" proto=uci -engine cmd="_build_BASE\Snail-Chess.exe" proto=uci -each tc=%TC% -maxmoves 200 -openings file=%OPENING_FILE% format=epd plies=8 order=random -games 2 -rounds 5000 -repeat 2 -concurrency 7 -sprt elo0=0 elo1=5 alpha=0.05 beta=0.1 -ratinginterval 10 -pgnout %PGN_FILE%