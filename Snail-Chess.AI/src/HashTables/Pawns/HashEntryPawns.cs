using System.Runtime.CompilerServices;
using SnailChess.AI.Evaluation;

namespace SnailChess.AI.HashTables
{
    internal struct HashEntryPawns 
    {
        public static readonly HashEntryPawns EMPTY = new HashEntryPawns() { valid = false };
        public uint key;
        public bool valid;
        public TaperedEvalScore evalScore;
        
        public HashEntryPawns(uint _key, short _score_mg, short _score_eg)
        {
            key = _key;
            valid = true;
            evalScore = new TaperedEvalScore(_score_mg, _score_eg);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator bool(HashEntryPawns _entry)
        {
            return _entry.valid;
        }
    }
}