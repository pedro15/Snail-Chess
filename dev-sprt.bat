@echo off
SET PGN_FILE="%~dp0_sprt.pgn" 
IF EXIST %PGN_FILE% DEL %PGN_FILE%
SET CUTECHESS="C:\Program Files (x86)\Cute Chess\cutechess-cli.exe"
SET TC=inf/6+0.06
CALL %CUTECHESS% -engine cmd="Publish_Dev\Snail-Chess.exe" proto=uci -engine cmd="Publish\Snail-Chess.exe" proto=uci -each tc=%TC% -maxmoves 200 -openings file="G:\Software\AI\Chess\books\epd\8moves_v3.epd" format=epd plies=8 order=random -games 2 -rounds 5000 -repeat 2 -concurrency 7 -sprt elo0=0 elo1=5 alpha=0.05 beta=0.1 -ratinginterval 10 -pgnout %PGN_FILE%
PAUSE