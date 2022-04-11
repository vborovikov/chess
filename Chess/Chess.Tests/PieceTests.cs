namespace Chess.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class PieceTests
{
    [TestMethod]
    public void GetPieceType_FromDesigns_All()
    {
        Assert.AreEqual(PieceType.Pawn, Piece.GetType(PieceDesign.WhitePawn));
        Assert.AreEqual(PieceType.Pawn, Piece.GetType(PieceDesign.BlackPawn));
        Assert.AreEqual(PieceType.Knight, Piece.GetType(PieceDesign.WhiteKnight));
        Assert.AreEqual(PieceType.Knight, Piece.GetType(PieceDesign.BlackKnight));
        Assert.AreEqual(PieceType.Bishop, Piece.GetType(PieceDesign.WhiteBishop));
        Assert.AreEqual(PieceType.Bishop, Piece.GetType(PieceDesign.BlackBishop));
        Assert.AreEqual(PieceType.Rook, Piece.GetType(PieceDesign.WhiteRook));
        Assert.AreEqual(PieceType.Rook, Piece.GetType(PieceDesign.BlackRook));
        Assert.AreEqual(PieceType.Queen, Piece.GetType(PieceDesign.WhiteQueen));
        Assert.AreEqual(PieceType.Queen, Piece.GetType(PieceDesign.BlackQueen));
        Assert.AreEqual(PieceType.King, Piece.GetType(PieceDesign.WhiteKing));
        Assert.AreEqual(PieceType.King, Piece.GetType(PieceDesign.BlackKing));
    }

    [TestMethod]
    public void GetPieceColor_FromDesigns_All()
    {
        Assert.AreEqual(PieceColor.White, Piece.GetColor(PieceDesign.WhitePawn));
        Assert.AreEqual(PieceColor.White, Piece.GetColor(PieceDesign.WhiteKnight));
        Assert.AreEqual(PieceColor.White, Piece.GetColor(PieceDesign.WhiteBishop));
        Assert.AreEqual(PieceColor.White, Piece.GetColor(PieceDesign.WhiteRook));
        Assert.AreEqual(PieceColor.White, Piece.GetColor(PieceDesign.WhiteQueen));
        Assert.AreEqual(PieceColor.White, Piece.GetColor(PieceDesign.WhiteKing));

        Assert.AreEqual(PieceColor.Black, Piece.GetColor(PieceDesign.BlackPawn));
        Assert.AreEqual(PieceColor.Black, Piece.GetColor(PieceDesign.BlackKnight));
        Assert.AreEqual(PieceColor.Black, Piece.GetColor(PieceDesign.BlackBishop));
        Assert.AreEqual(PieceColor.Black, Piece.GetColor(PieceDesign.BlackRook));
        Assert.AreEqual(PieceColor.Black, Piece.GetColor(PieceDesign.BlackQueen));
        Assert.AreEqual(PieceColor.Black, Piece.GetColor(PieceDesign.BlackKing));
    }
}
