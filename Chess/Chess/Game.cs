namespace Chess;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Game
{
    public static SquareFile GetFile(Square square) => (SquareFile)(7 - ((int)square / 8));

    public static SquareRank GetRank(Square square) => (SquareRank)((int)square % 8);

    public static Square GetSquare(SquareFile file, SquareRank rank) => (Square)(((7 - (int)file) * 8) + (int)rank);
}
