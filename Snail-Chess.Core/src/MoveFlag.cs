namespace SnailChess.Core
{
    public enum MoveFlag : byte
    {
        Quiet = 0,
        PawnDoublePush = 1,
        CastleKing = 2,
        CastleQueen = 3,
        Capture = 4,
        EnPassant = 5,
        Promotion_Knight = 8,
        Promotion_Bishop = 9 ,
        Promotion_Rook = 10,
        Promotion_Queen = 11,
        Capture_Promotion_Knight = 12,
        Capture_Promotion_Bishop = 13,
        Capture_Promotion_Rook = 14,
        Capture_Promotion_Queen = 15,
    }   
}