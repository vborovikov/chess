namespace Chess;

using System;
using System.Collections;
using System.Text;
using static System.Diagnostics.Debug;

public enum GameNotation
{
    ForsythEdwards, // Forsyth-Edwards Notation (FEN)
    Algebraic,      // Algebraic Notation (AN)
    Portable,       // Portable Game Notation (PGN)
}

[Flags]
public enum Castling
{
    None = 0,
    WhiteKingSide = 1 << 0,
    WhiteQueenSide = 1 << 1,
    BlackKingSide = 1 << 2,
    BlackQueenSide = 1 << 3,
}

public class GameEventArgs : EventArgs
{
    public GameEventArgs(IPiece piece, Square previousSquare)
    {
        this.Piece = piece;
        this.PreviousSquare = previousSquare;
    }

    public IPiece Piece { get; }

    public Square PreviousSquare { get; }
}

public class MoveEventArgs : EventArgs
{
    public MoveEventArgs(Move whiteMove, Move blackMove)
    {
        this.WhiteMove = whiteMove;
        this.BlackMove = blackMove;
    }

    public Move WhiteMove { get; }
    public Move BlackMove { get; }
}

public interface IGame : IEnumerable<IPiece>
{
    Square Find(IPiece piece);
    bool Move(IPiece piece, Square square);
}

public class Game : IGame
{
    public const string FenInitialPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    private readonly Position position;
    private readonly IPiece[] pieces;

    public Game()
    {
        this.position = new(this);
        this.pieces = CreatePieces(this);
    }

    public Game(ReadOnlySpan<char> record, GameNotation notation = GameNotation.ForsythEdwards)
        : this()
    {
        Reset(record, notation);
    }

    public PieceColor Color { get; private set; }
    public Castling Castling { get; private set; }

    internal IPiece WhiteKing => this.pieces[0];
    internal IPiece BlackKing => this.pieces[1];

    internal Position Position => this.position;

    public event EventHandler? BoardReset;
    public event EventHandler<GameEventArgs>? PieceTaken;
    public event EventHandler<GameEventArgs>? PieceMoved;
    public event EventHandler<MoveEventArgs>? FullMove;
    public event EventHandler? Check;
    public event EventHandler? Checkmate;
    public event EventHandler? Stalemate;

    public Square Find(IPiece piece) => this.position.Find(piece);

    public bool Move(IPiece piece, Square square)
    {
        var pieceSquare = this.position.Find(piece);
        if (pieceSquare == Square.None || square == pieceSquare ||
            square < Square.First || square > Square.Last)
        {
            return false;
        }

        var move = new Move(piece.Design, pieceSquare, square);
        if (CanMake(move))
        {
            var madeMove = this.position.Change(move);

            this.Color = this.Color == PieceColor.White ? PieceColor.Black : PieceColor.White;
            if (madeMove.DesignTaken != PieceDesign.None)
            {
                this.PieceTaken?.Invoke(this, new GameEventArgs(FindSpare(madeMove.DesignTaken), square));
            }
            this.PieceMoved?.Invoke(this, new GameEventArgs(piece, pieceSquare));

            CheckPosition();

            return true;
        }

        return false;
    }

    private void CheckPosition()
    {
        var king = this.BlackKing;
        if (this.Color == PieceColor.White)
        {
            king = this.WhiteKing;
            //todo: if history has at least two moves
            this.FullMove?.Invoke(this, new MoveEventArgs(default, default));
        }

        var check = this.position.IsInCheckFor(king);
        var hasLegalMoves = this.position.GetLegalMoves(king, canSacrifice: false).MoveNext();

        if (check)
        {
            WriteLine($"{king.Color} king is in check");
            this.Check?.Invoke(this, EventArgs.Empty);
            if (!hasLegalMoves)
            {
                WriteLine($"{king.Color} king is in checkmate");
                this.Checkmate?.Invoke(this, EventArgs.Empty);
            }
        }
        //else if (!hasLegalMoves)
        //{
        //    WriteLine("Stalemate");
        //    this.Stalemate?.Invoke(this, EventArgs.Empty);
        //}
    }

    private bool CanMake(Move move)
    {
        if (this.Color != Piece.GetColor(move.Design))
            return false;

        return this.position.CanChange(move);
    }

    public override string ToString()
    {
        var fen = new StringBuilder();

        // position
        var empty = 0;
        for (var rank = SquareRank.Eight; rank >= SquareRank.One; --rank)
        {
            for (var file = SquareFile.A; file <= SquareFile.H; ++file)
            {
                var square = Piece.GetSquare(file, rank);
                var piece = this.position[square];
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
                    if (rank > SquareRank.One)
                    {
                        fen.Append('/');
                    }
                    empty = 0;
                }
            }
        }

        // active color
        fen.Append($" {(this.Color == PieceColor.White ? "w" : "b")}");

        // castling
        fen.Append(' ');
        if (this.Castling == Castling.None)
        {
            fen.Append('-');
        }
        else
        {
            if (this.Castling.HasFlag(Castling.WhiteKingSide))
            {
                fen.Append('K');
            }
            if (this.Castling.HasFlag(Castling.WhiteQueenSide))
            {
                fen.Append('Q');
            }
            if (this.Castling.HasFlag(Castling.BlackKingSide))
            {
                fen.Append('k');
            }
            if (this.Castling.HasFlag(Castling.BlackQueenSide))
            {
                fen.Append('q');
            }
        }

        // en passant
        fen.Append(' ');
        if (this.position.EnPassant == Square.None)
        {
            fen.Append('-');
        }
        else
        {
            fen.Append(this.position.EnPassant.ToString());
        }

        return fen.ToString();
    }

    private static char PieceToFen(IPiece piece)
    {
        return piece.Color == PieceColor.White ? Char.ToUpperInvariant(piece.Char) : Char.ToLowerInvariant(piece.Char);
    }

    public static Game FromFen(ReadOnlySpan<char> fenRecord)
    {
        var game = new Game();

        game.Reset(fenRecord, GameNotation.ForsythEdwards);

        return game;
    }

    public void Reset(ReadOnlySpan<char> record, GameNotation notation = GameNotation.ForsythEdwards)
    {
        var board = (IBoard)this.position;
        board.Clear();

        switch (notation)
        {
            case GameNotation.Algebraic:
                ResetAN(board, record);
                break;
            case GameNotation.Portable:
                ResetPGN(board, record);
                break;
            default:
                ResetFen(board, record);
                break;
        }

        this.BoardReset?.Invoke(this, EventArgs.Empty);
        CheckPosition();
    }

    private void ResetPGN(IBoard board, ReadOnlySpan<char> record)
    {
        throw new NotImplementedException();
    }

    private void ResetAN(IBoard board, ReadOnlySpan<char> record)
    {
        throw new NotImplementedException();
    }

    private void ResetFen(IBoard board, ReadOnlySpan<char> record)
    {
        var fen = record.Split();

        // position
        if (fen.MoveNext())
        {
            var pos = fen.Current;
            var file = SquareFile.A;
            var rank = SquareRank.Eight;
            for (var i = 0; i < pos.Length; ++i)
            {
                var c = pos[i];

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
                    var piece = FindSparePieceFen(c);
                    if (piece is null)
                    {
                        //todo: invalid character
                        break;
                    }

                    board.Place(piece, Piece.GetSquare(file, rank));
                    file++;
                }
            }
        }

        // active color
        if (fen.MoveNext())
        {
            if (fen.Current[0] == 'b')
            {
                this.Color = PieceColor.Black;
            }
            else
            {
                this.Color = PieceColor.White;
            }
        }

        // castling rights
        if (fen.MoveNext())
        {
            var cast = fen.Current;
            this.Castling = Castling.None;
            if (cast[0] != '-')
            {
                for (var i = 0; i < cast.Length; ++i)
                {
                    this.Castling |= cast[i] switch
                    {
                        'K' => Castling.WhiteKingSide,
                        'Q' => Castling.WhiteQueenSide,
                        'k' => Castling.BlackKingSide,
                        'q' => Castling.BlackQueenSide,
                        _ => Castling.None,
                    };
                }
            }
        }

        // en passant
        if (fen.MoveNext())
        {
            board.EnPassant = Enum.TryParse<Square>(fen.Current, out var enPassant) ? enPassant : Square.None;
        }
    }

    private IPiece FindSparePieceFen(char ch)
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

    internal IPiece FindSpare(PieceDesign design)
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

        pieces.Add(Piece.Create(game, PieceDesign.WhiteKing));
        pieces.Add(Piece.Create(game, PieceDesign.BlackKing));
        pieces.Add(Piece.Create(game, PieceDesign.WhiteQueen));
        pieces.Add(Piece.Create(game, PieceDesign.BlackQueen));

        for (var i = 0; i != 2; ++i)
        {
            pieces.Add(Piece.Create(game, PieceDesign.WhiteRook));
            pieces.Add(Piece.Create(game, PieceDesign.WhiteKnight));
            pieces.Add(Piece.Create(game, PieceDesign.WhiteBishop));
            pieces.Add(Piece.Create(game, PieceDesign.BlackRook));
            pieces.Add(Piece.Create(game, PieceDesign.BlackKnight));
            pieces.Add(Piece.Create(game, PieceDesign.BlackBishop));
        }

        for (var i = 0; i != 8; ++i)
        {
            pieces.Add(Piece.Create(game, PieceDesign.WhitePawn));
            pieces.Add(Piece.Create(game, PieceDesign.BlackPawn));
        }

        return pieces.ToArray();
    }

    public IEnumerator<IPiece> GetEnumerator()
    {
        foreach (var piece in this.position.Pieces)
        {
            yield return piece;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
