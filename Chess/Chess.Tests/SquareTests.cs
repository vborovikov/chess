namespace Chess.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class SquareTests
{
    [TestMethod]
    public void GetSquare_A1_Index0()
    {
        var square = Piece.GetSquare(SquareFile.A, SquareRank.One);
        
        Assert.AreEqual(Square.a1, square);
        Assert.AreEqual(SquareFile.A, Piece.GetFile(square));
        Assert.AreEqual(SquareRank.One, Piece.GetRank(square));

        Assert.AreEqual(0, (int)square);
    }

    [TestMethod]
    public void GetSquare_H8_Index63()
    {
        var square = Piece.GetSquare(SquareFile.H, SquareRank.Eight);

        Assert.AreEqual(Square.h8, square);
        Assert.AreEqual(SquareFile.H, Piece.GetFile(square));
        Assert.AreEqual(SquareRank.Eight, Piece.GetRank(square));

        Assert.AreEqual(63, (int)square);
    }

    [TestMethod]
    public void GetSquare_H6_Index47()
    {
        var square = Piece.GetSquare(SquareFile.H, SquareRank.Six);

        Assert.AreEqual(Square.h6, square);
        Assert.AreEqual(SquareFile.H, Piece.GetFile(square));
        Assert.AreEqual(SquareRank.Six, Piece.GetRank(square));

        Assert.AreEqual(47, (int)square);
    }
}
