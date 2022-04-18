namespace Chess.Tests;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class PositionTests
{
    [TestMethod]
    public void GetLegalMoves_CountMoves_Equal()
    {
        var game = new Game();
        game.Reset(Game.FenStartingPosition);
        Assert.AreEqual(20UL, CountMoves(game.Position, game.Color, 1));
        Assert.AreEqual(400UL, CountMoves(game.Position, game.Color, 2));
        Assert.AreEqual(8902UL, CountMoves(game.Position, game.Color, 3));
        Assert.AreEqual(197281UL, CountMoves(game.Position, game.Color, 4));
        Assert.AreEqual(4865609UL, CountMoves(game.Position, game.Color, 5));
        Assert.AreEqual(119060324UL, CountMoves(game.Position, game.Color, 6));
        Assert.AreEqual(3195901860UL, CountMoves(game.Position, game.Color, 7));
    }

    private ulong CountMoves(Position position, PieceColor color, int depth)
    {
        if (depth == 0)
        {
            return 1UL;
        }

        var count = 0UL;

        foreach (var piece in position.Pieces)
        {
            if (piece.Color != color)
                continue;

            foreach (var move in position.GetLegalMoves(piece))
            {
                var takenPiece = position.Change(move.From, move.To);
                count += CountMoves(position, color == PieceColor.White ? PieceColor.Black : PieceColor.White, depth - 1);
                position.ChangeBack(move.From, move.To, takenPiece);
            }
        }

        return count;
    }
}
