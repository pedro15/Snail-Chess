@echo off 
SET PGN_FILE="%~dp0_tournament.pgn" 
set CUTECHESS="C:\Program Files (x86)\Cute Chess\cutechess-cli.exe"
CALL %CUTECHESS% -engine cmd="Publish_Dev\Snail-Chess.exe" proto=uci tc=inf nodes=2000 -engine cmd="engines/beginner.exe" proto=uci tc=inf nodes=1000 -maxmoves 200 -openings file="G:\Software\AI\Chess\books\epd\8moves_v3.epd" format=epd plies=8 order=random -games 2 -rounds 500 -repeat 2 -concurrency 7 -ratinginterval 10 -pgnout %PGN_FILE%
PAUSE