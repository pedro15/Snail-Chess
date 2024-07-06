using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace SnailChess.AI.HashTables
{
    internal sealed class HashTablePawns 
    {
        internal const short HASH_SIZE_MB = 4;
        private readonly int TABLE_SIZE;
        private readonly HashEntryPawns[] entires;
        public HashTablePawns()
        {
            TABLE_SIZE = HASH_SIZE_MB * 1024 * 1024 / Marshal.SizeOf<HashEntryPawns>();
            entires = new HashEntryPawns[TABLE_SIZE];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public HashEntryPawns GetEntry(uint _key)
        {
            HashEntryPawns pawns_entry = entires[_key % TABLE_SIZE];
            return pawns_entry.key == _key ? pawns_entry : HashEntryPawns.EMPTY;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RecordEntry(HashEntryPawns _entry)
        {
            _entry.valid = true;
            entires[_entry.key % TABLE_SIZE] = _entry;
        }
    }
}