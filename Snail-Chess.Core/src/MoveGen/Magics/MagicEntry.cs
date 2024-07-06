namespace SnailChess.Core.MoveGen.Magics
{
    public readonly struct MagicEntry
    {
        public static readonly MagicEntry EMPTY = new MagicEntry(0x0,0x0,0x0);
        public readonly ulong mask;
        public readonly ulong magic;
        public readonly uint offset;
        public MagicEntry(ulong _mask, ulong _magic, uint _offset)
        {
            mask = _mask;
            magic = _magic;
            offset = _offset;
        }
    }
}