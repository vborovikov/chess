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

public interface IGame
{
    bool Move(IPiece piece, Square square);
}

public class Game : IGame, IEnumerable<IPiece>
{
    public const string FenStartingPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    private readonly Position position;
    private readonly IPiece[] pieces;

    public Game()
    {
        this.position = new(this);
        this.pieces = CreatePieces(this);
    }

    public PieceColor Color { get; private set; }
    public Castling Castling { get; private set; }

    private IPiece WhiteKing => this.pieces[0];
    private IPiece BlackKing => this.pieces[1];

    public event EventHandler? BoardReset;
    public event EventHandler<GameEventArgs>? PieceTaken;
    public event EventHandler<GameEventArgs>? PieceMoved;
    //todo: public event EventHandler<MoveEventArgs>? FullMove;
    public event EventHandler? Check;
    public event EventHandler? Checkmate;

    public Square Find(IPiece piece) => this.position.Find(piece);

    public bool Move(IPiece piece, Square square)
    {
        var pieceSquare = Find(piece);
        if (pieceSquare == Square.None || square == pieceSquare ||
            square < Square.First || square > Square.Last)
        {
            return false;
        }

        if (CanMove(piece, pieceSquare, ref square))
        {
            var takenPiece = this.position.Change(pieceSquare, square);

            this.Color = this.Color == PieceColor.White ? PieceColor.Black : PieceColor.White;
            if (takenPiece is not null)
            {
                this.PieceTaken?.Invoke(this, new GameEventArgs(takenPiece, square));
            }
            this.PieceMoved?.Invoke(this, new GameEventArgs(piece, pieceSquare));

            CheckPosition();

            return true;
        }

        return false;
    }

    private void CheckPosition()
    {
        if (IsCheckFor(this.Color))
        {
            this.Check?.Invoke(this, EventArgs.Empty);
            if (IsCheckmateFor(this.Color))
            {
                WriteLine("Checkmate!");
                this.Checkmate?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private bool IsCheckmateFor(PieceColor color)
    {
        var king = color == PieceColor.White ? this.WhiteKing : this.BlackKing;
        var kingSquare = Find(king);

        foreach (var piece in this.position.Pieces)
        {
            if (piece.Color != color)
            {
                continue;
            }

            foreach (var move in this.position.GetMoves(piece))
            {
                foreach (var square in move.GetPath())
                {
                    if (square == kingSquare)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private bool IsCheckFor(PieceColor color)
    {
        var king = color == PieceColor.White ? this.WhiteKing : this.BlackKing;
        var kingSquare = Find(king);

        foreach (var piece in this.position.Pieces)
        {
            if (piece != king && piece.Color != color && Movement.CanMove(piece.Design, Find(piece), kingSquare))
            {
                WriteLine($"Check by {piece.Design}! {Find(piece)}-{kingSquare}");
                return true;
            }
        }

        return false;
    }

    private bool CanMove(IPiece piece, Square oldSquare, ref Square newSquare)
    {
        if (this.Color != piece.Color)
            return false;

        var makeTo = this.position.CanChange(new Move(piece.Design, oldSquare, newSquare));
        if (makeTo != Square.None)
        {
            newSquare = makeTo;
            return true;
        }
        
        return false;
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
        var len = record.Length;
        var i = 0;

        // position
        var file = SquareFile.A;
        var rank = SquareRank.Eight;
        for (; i < len; ++i)
        {
            var c = record[i];
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

        // active color
        if (i < len)
        {
            var c = record[++i];
            if (c == 'b')
            {
                this.Color = PieceColor.Black;
            }
            else
            {
                this.Color = PieceColor.White;
            }
            ++i;
        }

        // castling rights
        if (i < len)
        {
            var c = record[++i];
            if (c == '-')
            {
                this.Castling = Castling.None;
            }
            else
            {
                this.Castling = Castling.None;
                while (c != ' ')
                {
                    this.Castling |= c switch
                    {
                        'K' => Castling.WhiteKingSide,
                        'Q' => Castling.WhiteQueenSide,
                        'k' => Castling.BlackKingSide,
                        'q' => Castling.BlackQueenSide,
                        _ => Castling.None,
                    };

                    if (i < len)
                    {
                        c = record[i++];
                    }
                    else
                    {
                        break;
                    }
                }
            }
            ++i;
        }

        //todo: en passant
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
