namespace SnailChess.Core
{
    [System.Flags]
    public enum CastleRights : byte
    {
        None = 0,
        KingSide_White =  1,
        QueenSide_White = 2,
        KingSide_Black =  4,
        QueenSide_Black = 8,
        WhiteRights = KingSide_White | QueenSide_White,
        BlackRights = KingSide_Black | QueenSide_Black,
        All =  WhiteRights | BlackRights
    }
}