namespace Chess.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class SquareTests
{
    [TestMethod]
    public void GetSquare_A1_Index0()
    {
        var square = Game.GetSquare(SquareFile.A, SquareRank.One);
        
        Assert.AreEqual(Square.a1, square);
        Assert.AreEqual(SquareFile.A, Game.GetFile(square));
        Assert.AreEqual(SquareRank.One, Game.GetRank(square));
    }

    [TestMethod]
    public void GetSquare_H8_Index63()
    {
        var square = Game.GetSquare(SquareFile.H, SquareRank.Eight);

        Assert.AreEqual(Square.h8, square);
        Assert.AreEqual(SquareFile.H, Game.GetFile(square));
        Assert.AreEqual(SquareRank.Eight, Game.GetRank(square));
    }

    [TestMethod]
    public void GetSquare_H6_Index47()
    {
        var square = Game.GetSquare(SquareFile.H, SquareRank.Six);

        Assert.AreEqual(Square.h6, square);
        Assert.AreEqual(SquareFile.H, Game.GetFile(square));
        Assert.AreEqual(SquareRank.Six, Game.GetRank(square));
    }
}
