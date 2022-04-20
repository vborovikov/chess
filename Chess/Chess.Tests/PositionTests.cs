namespace Chess.Tests;

using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class PositionTests
{
    [TestMethod]
    public void GetLegalMoves_InitialPosition_Perft1_4()
    {
        var game = new Game();
        game.Reset(Game.FenInitialPosition);

        Assert.AreEqual((20UL, 0UL, 0UL), CountMoves(game.Position, game.Color, 1));
        Assert.AreEqual((400UL, 0UL, 0UL), CountMoves(game.Position, game.Color, 2));
        Assert.AreEqual((8902UL, 34UL, 0UL), CountMoves(game.Position, game.Color, 3));
        Assert.AreEqual((197281UL, 1576UL, 0UL), CountMoves(game.Position, game.Color, 4));
    }

    [TestMethod]
    public void GetLegalMoves_InitialPosition_Perft5()
    {
        var game = new Game();
        game.Reset(Game.FenInitialPosition);

        Assert.AreEqual((4865609UL, 82719UL, 258UL), CountMoves(game.Position, game.Color, 5));
        //Assert.AreEqual(119060324UL, CountMoves(game.Position, game.Color, 6));
        //Assert.AreEqual(3195901860UL, CountMoves(game.Position, game.Color, 7));
        //Assert.AreEqual(84998978956UL, CountMoves(game.Position, game.Color, 8));
    }

    private (ulong moves, ulong captures, ulong enPassants) CountMoves(Position position, PieceColor color, int depth) =>
        CountMoves(position, default, color, depth);


    private (ulong moves, ulong captures, ulong enPassants) CountMoves(Position position, Move afterMove, PieceColor color, int depth)
    {
        if (depth == 0)
        {
            //if (afterMove.Flags.HasFlag(MoveFlags.Capture))
            //{
            //    Debug.WriteLine(position.ToString());
            //    Debug.WriteLine(afterMove.ToString());
            //}

            return (
                moves: 1UL,
                captures: afterMove.Flags.HasFlag(MoveFlags.Capture) ? 1UL : 0UL,
                enPassants: afterMove.Flags.HasFlag(MoveFlags.EnPassant) ? 1UL : 0UL);
        }

        var totalCounts = (moves: 0UL, captures: 0UL, enPassants: 0UL);

        foreach (var piece in position.Pieces)
        {
            if (piece.Color != color)
                continue;

            foreach (var move in position.GetLegalMoves(piece))
            {
                var madeMove = position.Change(move);

                var posCounts = CountMoves(position, madeMove, color == PieceColor.White ? PieceColor.Black : PieceColor.White, depth - 1);
                totalCounts.moves += posCounts.moves;
                totalCounts.captures += posCounts.captures;
                totalCounts.enPassants += posCounts.enPassants;

                position.ChangeBack(madeMove);
            }
        }

        return totalCounts;
    }
}
