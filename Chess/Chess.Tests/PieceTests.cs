namespace Chess.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class PieceTests
{
    [TestMethod]
    public void GetPieceType_FromDesigns_All()
    {
        Assert.AreEqual(PieceType.Pawn, Game.GetPieceType(PieceDesign.WhitePawn));
        Assert.AreEqual(PieceType.Pawn, Game.GetPieceType(PieceDesign.BlackPawn));
        Assert.AreEqual(PieceType.Knight, Game.GetPieceType(PieceDesign.WhiteKnight));
        Assert.AreEqual(PieceType.Knight, Game.GetPieceType(PieceDesign.BlackKnight));
        Assert.AreEqual(PieceType.Bishop, Game.GetPieceType(PieceDesign.WhiteBishop));
        Assert.AreEqual(PieceType.Bishop, Game.GetPieceType(PieceDesign.BlackBishop));
        Assert.AreEqual(PieceType.Rook, Game.GetPieceType(PieceDesign.WhiteRook));
        Assert.AreEqual(PieceType.Rook, Game.GetPieceType(PieceDesign.BlackRook));
        Assert.AreEqual(PieceType.Queen, Game.GetPieceType(PieceDesign.WhiteQueen));
        Assert.AreEqual(PieceType.Queen, Game.GetPieceType(PieceDesign.BlackQueen));
        Assert.AreEqual(PieceType.King, Game.GetPieceType(PieceDesign.WhiteKing));
        Assert.AreEqual(PieceType.King, Game.GetPieceType(PieceDesign.BlackKing));
    }

    [TestMethod]
    public void GetPieceColor_FromDesigns_All()
    {
        Assert.AreEqual(PieceColor.White, Game.GetPieceColor(PieceDesign.WhitePawn));
        Assert.AreEqual(PieceColor.White, Game.GetPieceColor(PieceDesign.WhiteKnight));
        Assert.AreEqual(PieceColor.White, Game.GetPieceColor(PieceDesign.WhiteBishop));
        Assert.AreEqual(PieceColor.White, Game.GetPieceColor(PieceDesign.WhiteRook));
        Assert.AreEqual(PieceColor.White, Game.GetPieceColor(PieceDesign.WhiteQueen));
        Assert.AreEqual(PieceColor.White, Game.GetPieceColor(PieceDesign.WhiteKing));

        Assert.AreEqual(PieceColor.Black, Game.GetPieceColor(PieceDesign.BlackPawn));
        Assert.AreEqual(PieceColor.Black, Game.GetPieceColor(PieceDesign.BlackKnight));
        Assert.AreEqual(PieceColor.Black, Game.GetPieceColor(PieceDesign.BlackBishop));
        Assert.AreEqual(PieceColor.Black, Game.GetPieceColor(PieceDesign.BlackRook));
        Assert.AreEqual(PieceColor.Black, Game.GetPieceColor(PieceDesign.BlackQueen));
        Assert.AreEqual(PieceColor.Black, Game.GetPieceColor(PieceDesign.BlackKing));
    }
}
