C# Chess engine with hand crafted evaluation.

Move Ordering:
- MMV/LVA
- SEE
- Killer moves
- History heuristic
- Promotions

Search: 
- Iterative deeping
- LMR
- Quiescence search
- Transposition tables
- Check extension
- Reverse futility pruning
- Internal Iterative Reductions
- Null move pruning
- Razoring
- SEE Pruning
- Late Move Pruning
- PVS

Evaluation:
- Piece-square-tables
- Material balance
- Doubled pawns
- Isolated Pawns
- Passed pawns
- Protected Passed pawns
- Tapered evaluation
- Basic Mobility (Bishop, Rook, Queen)
- Material adjust for rooks based on number of pawns
- Basic King Safety using king shield zone
- Tempo

Estimated elo:
1.0.0: ~2350
