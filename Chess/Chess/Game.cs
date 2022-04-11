namespace Chess;

using System;
using System.Text;

public class Game
{
    public const string FenStartingPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    private readonly IPiece[] board;
    private readonly IPiece[] pieces;

    public Game()
    {
        this.board = new IPiece[Square.Last - Square.First + 1];
        this.pieces = CreatePieces(this);
    }

    public PieceEnumerator Pieces => new(this);

    public event EventHandler? Moved;

    public Square Find(IPiece piece)
    {
        if (piece is null)
            return Square.None;

        var index = Array.IndexOf(this.board, piece);
        return index > -1 ? (Square)index : Square.None;
    }

    public bool Move(IPiece piece, Square square)
    {
        var pieceSquare = Find(piece);
        if (pieceSquare == Square.None || square == pieceSquare ||
            square < Square.First || square > Square.Last ||
            this.board[(int)square] is not null)
        {
            return false;
        }

        this.board[(int)pieceSquare] = null!;
        this.board[(int)square] = piece;

        this.Moved?.Invoke(this, EventArgs.Empty);

        return true;
    }

    public string ToFen()
    {
        var fen = new StringBuilder();
        var empty = 0;
        for (var rank = SquareRank.Eight; rank >= SquareRank.One; --rank)
        {
            for (var file = SquareFile.A; file <= SquareFile.H; ++file)
            {
                var square = GetSquare(file, rank);
                var piece = this.board[(int)square];
                if (piece is null)
                {
                    empty++;
                }
                else
                {
                    if (empty > 0)
                    {
                        fen.Append(empty);
                        empty = 0;
                    }
                    fen.Append(PieceToFen(piece));
                }
                if (file == SquareFile.H)
                {
                    if (empty > 0)
                    {
                        fen.Append(empty);
                    }
                    fen.Append('/');
                    empty = 0;
                }
            }
        }
        return fen.ToString();
    }

    private static char PieceToFen(IPiece piece)
    {
        return piece.Design switch
        {
            PieceDesign.WhitePawn => 'P',
            PieceDesign.WhiteKnight => 'N',
            PieceDesign.WhiteBishop => 'B',
            PieceDesign.WhiteRook => 'R',
            PieceDesign.WhiteQueen => 'Q',
            PieceDesign.WhiteKing => 'K',
            PieceDesign.BlackPawn => 'p',
            PieceDesign.BlackKnight => 'n',
            PieceDesign.BlackBishop => 'b',
            PieceDesign.BlackRook => 'r',
            PieceDesign.BlackQueen => 'q',
            PieceDesign.BlackKing => 'k',
            _ => 'x',
        };
    }

    public static Game FromFen(ReadOnlySpan<char> fenRecord)
    {
        var game = new Game();

        var len = fenRecord.Length;
        var i = 0;
        var file = SquareFile.A;
        var rank = SquareRank.Eight;
        for (; i < len; ++i)
        {
            var c = fenRecord[i];
            if (c == ' ')
            {
                break;
            }

            if (c == '/')
            {
                file = SquareFile.A;
                rank--;
            }
            else if (c >= '1' && c <= '8')
            {
                file += (c - '0');
            }
            else
            {
                var piece = game.CreatePieceFromFen(c);
                if (piece is null)
                {
                    //todo: invalid character
                    break;
                }

                game.board[(int)GetSquare(file, rank)] = piece;
                file++;
                if (file > SquareFile.H)
                {
                    //todo: invalid fen
                    break;
                }
            }
        }

        return game;
    }

    private IPiece CreatePieceFromFen(char ch)
    {
        const PieceDesign noDesign = (PieceDesign)(-1);

        var design = ch switch
        {
            'K' => PieceDesign.WhiteKing,
            'Q' => PieceDesign.WhiteQueen,
            'R' => PieceDesign.WhiteRook,
            'B' => PieceDesign.WhiteBishop,
            'N' => PieceDesign.WhiteKnight,
            'P' => PieceDesign.WhitePawn,
            'k' => PieceDesign.BlackKing,
            'q' => PieceDesign.BlackQueen,
            'r' => PieceDesign.BlackRook,
            'b' => PieceDesign.BlackBishop,
            'n' => PieceDesign.BlackKnight,
            'p' => PieceDesign.BlackPawn,
            _ => noDesign,
        };

        if (design == noDesign)
            return null!;

        return FindSpare(design);
    }

    private IPiece FindSpare(PieceDesign design)
    {
        foreach (var piece in this.pieces)
        {
            if (piece.Design == design && piece.Square == Square.None)
            {
                return piece;
            }
        }

        return null!;
    }

    private static IPiece[] CreatePieces(Game game)
    {
        var pieces = new List<IPiece>();

        for (var i = 0; i != 8; ++i)
        {
            pieces.Add(Piece.Create(game, PieceDesign.WhitePawn));
            pieces.Add(Piece.Create(game, PieceDesign.BlackPawn));
        }

        for (var i = 0; i != 2; ++i)
        {
            pieces.Add(Piece.Create(game, PieceDesign.WhiteRook));
            pieces.Add(Piece.Create(game, PieceDesign.WhiteKnight));
            pieces.Add(Piece.Create(game, PieceDesign.WhiteBishop));
            pieces.Add(Piece.Create(game, PieceDesign.BlackRook));
            pieces.Add(Piece.Create(game, PieceDesign.BlackKnight));
            pieces.Add(Piece.Create(game, PieceDesign.BlackBishop));
        }

        pieces.Add(Piece.Create(game, PieceDesign.WhiteQueen));
        pieces.Add(Piece.Create(game, PieceDesign.WhiteKing));
        pieces.Add(Piece.Create(game, PieceDesign.BlackQueen));
        pieces.Add(Piece.Create(game, PieceDesign.BlackKing));

        return pieces.ToArray();
    }

    public static SquareFile GetFile(Square square) => (SquareFile)((int)square % 8);

    public static SquareRank GetRank(Square square) => (SquareRank)((int)square / 8);

    public static Square GetSquare(SquareFile file, SquareRank rank) => (Square)((((int)rank) * 8) + (int)file);

    public struct PieceEnumerator
    {
        private readonly Game game;
        private int index;

        public PieceEnumerator(Game game)
        {
            this.game = game;
            this.index = -1;
            this.Current = default!;
        }

        public IPiece Current { get; private set; }

        public PieceEnumerator GetEnumerator() => this;

        public void Reset()
        {
            this.index = -1;
            this.Current = default!;
        }

        public bool MoveNext()
        {
            for (++this.index; this.index < this.game.board.Length; ++this.index)
            {
                if (this.game.board[this.index] is not null)
                {
                    this.Current = this.game.board[this.index];
                    return true;
                }
            }

            return false;
        }
    }
}
