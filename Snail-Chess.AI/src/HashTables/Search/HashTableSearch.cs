using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static SnailChess.AI.Search.SearchConstants;

namespace SnailChess.AI.HashTables
{
    internal sealed class HashTableSearch
    {
        internal const short HASH_SIZE_MB = 85;
        private readonly ulong TABLE_SIZE;
        private readonly HashEntrySearch[] entries;

        public HashTableSearch()
        {
            TABLE_SIZE = Convert.ToUInt64(HASH_SIZE_MB * 1024 * 1024 / Marshal.SizeOf<HashEntrySearch>());
            entries = new HashEntrySearch[TABLE_SIZE];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashEntrySearch GetEntry(ulong _key)
        {
            HashEntrySearch loaded_entry = entries[_key % TABLE_SIZE];
            return loaded_entry.key == _key ? loaded_entry : HashEntrySearch.EMPTY;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RecordEntry(HashEntrySearch _entry, byte _ply)
        {
            ulong entry_index = _entry.key % TABLE_SIZE;

            if (_entry.score < -VALUE_MATE_SCORE) _entry.score -= _ply;
            if (_entry.score > VALUE_MATE_SCORE) _entry.score += _ply;

            entries[entry_index] = _entry;
        }

        public void Clear() 
        {
            Array.Clear(entries, 0 , entries.Length);
        }
    }
}