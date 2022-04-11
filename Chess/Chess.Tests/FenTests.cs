namespace Chess.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class FenTests
{
    [TestMethod]
    public void FromFen_StartingPosition_SamePosition()
    {
        var game = Game.FromFen(Game.FenStartingPosition);

        Assert.AreEqual(Game.FenStartingPosition[..Game.FenStartingPosition.IndexOf(' ')], game.ToString());
    }
}
