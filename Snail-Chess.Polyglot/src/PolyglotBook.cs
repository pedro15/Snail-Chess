using static SnailChess.Polyglot.PolyglotData;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

using SnailChess.Core;
using SnailChess.Core.MoveGen;

namespace SnailChess.Polyglot
{
    public enum SelectionMode
    {
        Random,
        Weighted
    }

    public sealed class PolyglotBook
	{
        private static System.Random random = new System.Random();
        private PolyglotEntry[] bookEntries = new PolyglotEntry[0];
        public int Count => bookEntries.Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OpenBook(in byte[] _bookData , int _maximunEntriesLimit = -1)
        {
            if (_maximunEntriesLimit <= 0) _maximunEntriesLimit = int.MaxValue;

            if ((_maximunEntriesLimit % 2) != 0)
                _maximunEntriesLimit--;

            bookEntries = ReadBookEntries(in _bookData , _maximunEntriesLimit).ToArray();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Move GetBookMove(in ChessBoard _board , SelectionMode _selectionMode = SelectionMode.Weighted, float _weightSmoothStrenght = 0.6f)
        {
            ulong polyKey = ComputePolyglotHash(in _board);
			if (bookEntries.Length > 0)
			{
                PolyglotEntry[] GetValidEntries()
                {
                    List<PolyglotEntry> total_entries = new List<PolyglotEntry>();
                    PolyglotEntry current_entry;
                    
                    for (int i = 0; i < bookEntries.Length; i++)
                    {
                        current_entry = bookEntries[i];
                        if (current_entry.key == polyKey)
                        {
                            total_entries.Add(bookEntries[i]);
                        }
                    }

                    return total_entries.ToArray();
                }

                PolyglotEntry[] valid_entries = GetValidEntries();
				int index = -1;

                #if DEBUG 
                System.Console.WriteLine("[PolyglotBook] total entries found: " + valid_entries.Length);
                #endif

                if (_selectionMode == SelectionMode.Weighted)
                {
                    index = GetWeightedEntryIndex(in valid_entries, _weightSmoothStrenght);
                }else 
                {
                    index = random.Next(0,valid_entries.Length);
                }

                if (index >= 0 && index < valid_entries.Length)
                {
                    Move[] legal_moves = Move.GetLegalMoves(_board.ExportPosition());
                    return Move.FromLAN(valid_entries[index].move.ToLAN(), in legal_moves);
                }
			}

            return Move.NO_MOVE;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetWeightedEntryIndex(in PolyglotEntry[] _entries, float _smoothStrenght = 0.6f)
        {
            void SmoothWeights (ref int[] _weights) 
            {
		    	float sum = 0;
		    	
                for (int i = 0; i < _weights.Length; i++) 
		    		sum += _weights[i];
		    	float avg = sum / _weights.Length;
		    	
                for (int i = 0; i < _weights.Length; i++)
                {
		    		float offsetFromAvg = avg - _weights[i];
		    		_weights[i] += (int)(offsetFromAvg * _smoothStrenght);
		    	}
		    }

            int total = 0;
            int[] weights = _entries.Select((x) => (int)x.weight).ToArray();
            SmoothWeights(ref weights);
            
            for (int x = 0; x < _entries.Length; x++)
            {
                PolyglotEntry entry = _entries[x];
                #if DEBUG
                System.Console.WriteLine($"[PolyglotBook] move {entry.move.ToLAN()} weight {weights[x]}");
                #endif
                total += weights[x];
            }

            double rnd = random.NextDouble() * total;

            for (int x = 0; x < _entries.Length; x++)
            {
                if (rnd < weights[x])
                {
                    return x;
                }else 
                {
                    rnd -= weights[x];
                }
            }

            return _entries.Length - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static List<PolyglotEntry> ReadBookEntries(in byte[] _bookData , int _countLimit = -1)
        {
            int entry_size = Marshal.SizeOf<PolyglotEntry>();
            List<PolyglotEntry> total_entries  = new List<PolyglotEntry>();
            if (_countLimit <= 0) _countLimit = int.MaxValue - 1;
            MemoryStream stream = new MemoryStream(_bookData);
            long entries_count = System.Math.Min(stream.Length / entry_size, _countLimit);

            if (entries_count > 0)
            {
                for (int entry_index = 0; entry_index < entries_count; entry_index++)
                {
                    byte[] buffer = new byte[entry_size];
                    stream.Seek(entry_index * entry_size , SeekOrigin.Begin);
                    stream.Read(buffer, 0 , entry_size);

                    // Swap big endian to little endian square representation
                    // hack forked from 'Cosette' chess engine
                    System.Array.Reverse(buffer, 0, 8);
                    System.Array.Reverse(buffer, 8, 2);
                    System.Array.Reverse(buffer, 10, 2);
                    System.Array.Reverse(buffer, 12, 4);

                    GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    PolyglotEntry entry = Marshal.PtrToStructure<PolyglotEntry>(handle.AddrOfPinnedObject());

                    if (entry.move)
                        total_entries.Add(entry);
                    
                    handle.Free();
                }
            }
            stream.Close();
            stream.Dispose();
            return total_entries;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong ComputePolyglotHash(in ChessBoard _board)
        {
            ulong hash = 0UL;
            ulong bitboard;

            byte sq, piece_kind = 0;
            for (byte pc = Piece.Pawn; pc <= Piece.King; ++pc)
            {
                for (byte color = Piece.Black; color <= Piece.White; ++color)
                {
                    switch(pc)
                    {
                        case Piece.Pawn:
                            if (color == Piece.White)
                                piece_kind = (byte)PolyglotPiece.white_pawn;
                            else 
                                piece_kind = (byte)PolyglotPiece.black_pawn;
                        break;

                        case Piece.Knight:
                            if (color == Piece.White)
                                piece_kind = (byte)PolyglotPiece.white_knight;
                            else 
                                piece_kind = (byte)PolyglotPiece.black_knight;
                        break;

                        case Piece.Bishop:
                            if (color == Piece.White)
                                piece_kind = (byte)PolyglotPiece.white_bishop;
                            else 
                                piece_kind = (byte)PolyglotPiece.black_bishop;
                        break;

                        case Piece.Rook:
                            if (color == Piece.White)
                                piece_kind = (byte)PolyglotPiece.white_rook;
                            else 
                                piece_kind = (byte)PolyglotPiece.black_rook;
                        break;

                        case Piece.Queen:
                            if (color == Piece.White)
                                piece_kind = (byte)PolyglotPiece.white_queen;
                            else 
                                piece_kind = (byte)PolyglotPiece.black_queen;
                        break;

                        case Piece.King:
                            if (color == Piece.White)
                                piece_kind = (byte)PolyglotPiece.white_king;
                            else 
                                piece_kind = (byte)PolyglotPiece.black_king;
                        break;
                    }

                    bitboard = _board.bitboards[color] & _board.bitboards[pc];
                    while(bitboard != 0)
                    {
                        sq = BitUtils.BitScan(bitboard);
                        hash ^= keys[64 * piece_kind + sq];
                        bitboard = BitUtils.PopLsb(bitboard);
                    }
                }
            }

            // Castling
            if (_board.castleRights != CastleRights.None)
            {
                if ((_board.castleRights & CastleRights.KingSide_White) != CastleRights.None)
                {
                    hash ^= keys[768 + (byte)PolyglotCastle.white_short];
                }

                if ((_board.castleRights & CastleRights.QueenSide_White) != CastleRights.None)
                {
                    hash ^= keys[768 + (byte)PolyglotCastle.white_long];
                }

                if ((_board.castleRights & CastleRights.KingSide_Black) != CastleRights.None)
                {
                    hash ^= keys[768 + (byte)PolyglotCastle.black_short];
                }

                if ((_board.castleRights & CastleRights.QueenSide_Black) != CastleRights.None)
                {
                    hash ^= keys[768 + (byte)PolyglotCastle.black_long];
                }
            }
            
            // En-passant
            if (_board.ep_square != 0)
            {
                if (_board.sideToMove == Piece.White)
                {
                    if ((MoveGenerator.PawnAttacksMoves(_board.ep_square, Piece.FlipColor(_board.sideToMove)) & 
                        _board.bitboards[Piece.Pawn] & _board.bitboards[Piece.White]) != 0)
                        {
                            hash ^= keys[772 + BoardUtils.SquareToFile(_board.ep_square)];
                        }
                }else 
                {
                     if ((MoveGenerator.PawnAttacksMoves(_board.ep_square, Piece.FlipColor(_board.sideToMove)) & 
                        _board.bitboards[Piece.Pawn] & _board.bitboards[Piece.Black] ) != 0)
                        {
                            hash ^= keys[772 + BoardUtils.SquareToFile(_board.ep_square)];
                        }
                }
            }
            
            // side to move
            if (_board.sideToMove == Piece.White)
            {
                hash ^= keys[780];
            }
            
            return hash;
        }
	}
}