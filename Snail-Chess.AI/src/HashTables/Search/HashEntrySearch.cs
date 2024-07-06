using SnailChess.Core;
using System.Runtime.CompilerServices;

namespace SnailChess.AI.HashTables
{
    internal struct HashEntrySearch
    {
        public static readonly HashEntrySearch EMPTY = new HashEntrySearch(){ flags = HashTableFlags.NONE };
        
        public ulong key;
        public byte depth;
        public short score;
        public short evaluation;
        public HashTableFlags flags;
        public Move bestMove;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator bool(HashEntrySearch _entry)
        {
            return _entry.flags != HashTableFlags.NONE;
        }
    }
}