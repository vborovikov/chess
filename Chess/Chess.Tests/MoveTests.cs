namespace Chess.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class MoveTests
{
    [TestMethod]
    public void CanMove_RookC3D5_False()
    {
        Assert.IsFalse(Movement.CanMove(PieceDesign.WhiteRook, Square.c3, Square.d5));
        Assert.IsFalse(Movement.CanMove(PieceDesign.BlackRook, Square.c3, Square.d5));
    }

    [TestMethod]
    public void CanMove_KnightA2C1_True()
    {
        Assert.IsTrue(Movement.CanMove(PieceDesign.WhiteKnight, Square.a2, Square.c1));
        Assert.IsTrue(Movement.CanMove(PieceDesign.BlackKnight, Square.a2, Square.c1));
    }

    [TestMethod]
    public void CanMove_BishopC8E1_False()
    {
        Assert.IsFalse(Movement.CanMove(PieceDesign.WhiteBishop, Square.c8, Square.e1));
        Assert.IsFalse(Movement.CanMove(PieceDesign.BlackBishop, Square.c8, Square.e1));
    }

    [TestMethod]
    public void Move_BlackPawnA7H1_Invalid()
    {
        var move = new Move(PieceDesign.BlackPawn, Square.a7, Square.h1);
        Assert.IsFalse(move.IsValid);
    }

    [TestMethod]
    public void CanMove_RookH3D3_Valid()
    {
        Assert.IsTrue(Movement.CanMove(PieceDesign.WhiteRook, Square.h3, Square.d3));
        Assert.IsTrue(Movement.CanMove(PieceDesign.BlackRook, Square.h3, Square.d3));
    }

    [TestMethod]
    public void Move_BlackPawnA7H1_PathEmpty()
    {
        var move = new Move(PieceDesign.BlackPawn, Square.a7, Square.h1);
        Assert.IsFalse(move.GetPath().MoveNext());
    }
}
