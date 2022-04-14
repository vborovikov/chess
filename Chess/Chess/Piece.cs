namespace Chess;

using System.Runtime.CompilerServices;

public enum PieceColor
{
    White, // 0
    Black, // 1
}

public enum PieceType
{
    Pawn,   // 0
    Knight, // 1
    Bishop, // 2
    Rook,   // 3
    Queen,  // 4
    King,   // 5
}

public enum PieceDesign
{
    WhitePawn,   // 0
    BlackPawn,   // 1
    WhiteKnight, // 2
    BlackKnight, // 3
    WhiteBishop, // 4
    BlackBishop, // 5
    WhiteRook,   // 6
    BlackRook,   // 7
    WhiteQueen,  // 8
    BlackQueen,  // 9
    WhiteKing,   // 10
    BlackKing,   // 11
}

public interface IPiece
{
    Square Square { get; }
    PieceDesign Design { get; }
    PieceColor Color => Piece.GetColor(this.Design);
    PieceType Type => Piece.GetType(this.Design);
    char Char => Piece.GetChar(this.Design);
}

public sealed class Piece : IPiece
{
    private readonly Game game;

    private Piece(Game game, PieceDesign design)
    {
        this.game = game;
        this.Design = design;
    }

    public Square Square => this.game.Find(this);

    public PieceDesign Design { get; }

    public static Piece Create(Game game, PieceDesign design)
    {
        return new Piece(game, design);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SquareFile GetFile(Square square) => (SquareFile)((int)square % 8);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static SquareRank GetRank(Square square) => (SquareRank)((int)square / 8);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Square GetSquare(SquareFile file, SquareRank rank) => (Square)((((int)rank) * 8) + (int)file);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PieceColor GetColor(PieceDesign design) => (PieceColor)((int)design & 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PieceType GetType(PieceDesign design) => (PieceType)((int)design / 2);

    public static char GetChar(PieceDesign design) =>
        design switch
        {
            PieceDesign.WhitePawn => '\u2659',
            PieceDesign.BlackPawn => '\u265F',
            PieceDesign.WhiteKnight => '\u2658',
            PieceDesign.BlackKnight => '\u265E',
            PieceDesign.WhiteBishop => '\u2657',
            PieceDesign.BlackBishop => '\u265D',
            PieceDesign.WhiteRook => '\u2656',
            PieceDesign.BlackRook => '\u265C',
            PieceDesign.WhiteQueen => '\u2655',
            PieceDesign.BlackQueen => '\u265B',
            PieceDesign.WhiteKing => '\u2654',
            PieceDesign.BlackKing => '\u265A',
            _ => '?',
        };
}
