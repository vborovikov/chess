namespace Chess;

using System;
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
    None,
    WhitePawn,   // 1
    BlackPawn,   // 2
    WhiteKnight, // 3
    BlackKnight, // 4
    WhiteBishop, // 5
    BlackBishop, // 6
    WhiteRook,   // 7
    BlackRook,   // 8
    WhiteQueen,  // 9
    BlackQueen,  // 10
    WhiteKing,   // 11
    BlackKing,   // 12
}

public interface IPiece
{
    Square Square { get; }
    PieceDesign Design { get; }
    PieceColor Color => Piece.GetColor(this.Design);
    PieceType Type => Piece.GetType(this.Design);
    char Symbol => Piece.GetSymbol(this.Design);
    char Char => Piece.GetChar(this.Type);
}

public sealed class Piece : IPiece
{
    private readonly IGame game;

    private Piece(IGame game, PieceDesign design)
    {
        this.game = game;
        this.Design = design;
    }

    public Square Square => this.game.Find(this);

    public PieceDesign Design { get; }

    public override string ToString() => this.Design.ToString();

    public static Piece Create(IGame game, PieceDesign design)
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
    public static PieceColor GetColor(PieceDesign design) => (PieceColor)((int)(design - 1) & 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PieceType GetType(PieceDesign design) => (PieceType)((int)(design -1) / 2);

    public static char GetSymbol(PieceDesign design) => design switch
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

    public static char GetChar(PieceType type) => type switch
    {
        PieceType.Pawn => 'P',
        PieceType.Knight => 'N',
        PieceType.Bishop => 'B',
        PieceType.Rook => 'R',
        PieceType.Queen => 'Q',
        PieceType.King => 'K',
        _ => '?',
    };
}
