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
}
