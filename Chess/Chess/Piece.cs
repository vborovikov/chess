namespace Chess
{
    using System;

    [Flags]
    public enum PieceColor
    {
        White = 0b0000,
        Black = 0b0001,
    }

    [Flags]
    public enum PieceType
    {
        Pawn = 0b0000,
        Rook = 0b0010,
        Knight = 0b0100,
        Bishop = 0b0110,
        Queen = 0b1000,
        King = 0b1010,
    }

    public enum PieceDesign
    {
        WhitePawn = PieceColor.White | PieceType.Pawn,
        WhiteRook = PieceColor.White | PieceType.Rook,
        WhiteKnight = PieceColor.White | PieceType.Knight,
        WhiteBishop = PieceColor.White | PieceType.Bishop,
        WhiteQueen = PieceColor.White | PieceType.Queen,
        WhiteKing = PieceColor.White | PieceType.King,
        BlackPawn = PieceColor.Black | PieceType.Pawn,
        BlackRook = PieceColor.Black | PieceType.Rook,
        BlackKnight = PieceColor.Black | PieceType.Knight,
        BlackBishop = PieceColor.Black | PieceType.Bishop,
        BlackQueen = PieceColor.Black | PieceType.Queen,
        BlackKing = PieceColor.Black | PieceType.King,
    }

    public interface IPiece
    {
        Square Square { get; }
        PieceDesign Design { get; }
        PieceColor Color => Piece.GetColor(this.Design);
        PieceType Type => Piece.GetType(this.Design);
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

        public static SquareFile GetFile(Square square) => (SquareFile)((int)square % 8);

        public static SquareRank GetRank(Square square) => (SquareRank)((int)square / 8);

        public static Square GetSquare(SquareFile file, SquareRank rank) => (Square)((((int)rank) * 8) + (int)file);

        public static PieceColor GetColor(PieceDesign design) => (PieceColor)((int)design & 0b0001);

        public static PieceType GetType(PieceDesign design) => (PieceType)((int)design & 0b1110);
    }
}
