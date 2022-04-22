namespace Chess.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class FenTests
{
    [TestMethod]
    public void FromFen_StartingPosition_SamePosition()
    {
        var game = Game.FromFen(Game.FenInitialPosition);

        Assert.AreEqual(Game.FenInitialPosition, game.ToString());
    }
}
