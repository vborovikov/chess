namespace Chess;

using System;

/// <summary>
/// Allows initial piece arrangment.
/// </summary>
interface IBoard
{
    void Clear();
    void Place(IPiece piece, Square square);
}

public sealed class Position : IBoard, ICloneable
{
    private readonly Game game;
    private readonly IPiece[] board;

    public Position(Game game)
        : this(game, new IPiece[Square.Last - Square.First + 1])
    {
    }

    private Position(Game game, IPiece[] board)
    {
        this.game = game;
        this.board = board;
    }

    public IPiece this[Square square]
    {
        get => this.board[(int)square];
    }

    public PieceEnumerator Pieces => new(this);

    public override string ToString()
    {
        return String.Create(Square.Last - Square.First + 1 + (7 * Environment.NewLine.Length), this.board, (span, pieces) =>
        {
            var index = 0;
            var length = span.Length;
            var newLine = Environment.NewLine;
            for (var rank = SquareRank.Eight; rank >= SquareRank.One; --rank)
            {
                for (var file = SquareFile.A; file <= SquareFile.H; ++file)
                {
                    var square = Piece.GetSquare(file, rank);
                    var piece = pieces[(int)square];
                    span[index++] = piece is not null ? piece.Symbol : '\u2610';
                    if (file == SquareFile.H && index < length)
                    {
                        for (var i = 0; i < newLine.Length; ++i)
                        {
                            span[index++] = newLine[i];
                        }
                    }
                }
            }
        });
    }

    public Square Find(IPiece piece)
    {
        if (piece is null)
            return Square.None;

        var index = Array.IndexOf(this.board, piece);
        return index > -1 ? (Square)index : Square.None;
    }

    internal IPiece Change(Square from, Square to)
    {
        var movedPiece = this.board[(int)from];
        var takenPiece = this.board[(int)to];

        this.board[(int)from] = null!;
        this.board[(int)to] = movedPiece;

        return takenPiece;
    }

    public Square CanChange(Move move)
    {
        if (move.IsValid)
        {
            var makeTo = move.To;
            foreach (var square in move.GetPath())
            {
                if (square == makeTo)
                {
                    break;
                }

                if (this.board[(int)square] is IPiece)
                {
                    makeTo = square;
                    break;
                }
            }

            var otherPiece = this.board[(int)makeTo];
            if (otherPiece is null || otherPiece.Color != Piece.GetColor(move.Design))
                return makeTo;
        }

        return Square.None;
    }

    public MoveEnumerator GetMoves(IPiece piece)
    {
        return new MoveEnumerator(this, piece);
    }

    void IBoard.Clear()
    {
        Array.Clear(this.board);
    }

    void IBoard.Place(IPiece piece, Square square)
    {
        this.board[(int)square] = piece;
    }

    internal Position Clone()
    {
        var boardCopy = new IPiece[this.board.Length];
        Array.Copy(this.board, boardCopy, this.board.Length);
        return new Position(this.game, boardCopy);
    }

    object ICloneable.Clone() => Clone();

    public struct PieceEnumerator
    {
        private readonly Position position;
        private int index;

        public PieceEnumerator(Position position)
        {
            this.position = position;
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
            for (++this.index; this.index < this.position.board.Length; ++this.index)
            {
                this.Current = this.position.board[this.index];
                if (this.Current is not null)
                    return true;
            }

            return false;
        }
    }

    public struct MoveEnumerator
    {
        private readonly Position position;
        private readonly PieceDesign design;
        private readonly Square from;
        private Square to;
        private Move move;

        public MoveEnumerator(Position position, IPiece piece)
        {
            this.position = position;
            this.design = piece.Design;
            this.from = piece.Square;
            this.to = Square.None;
            this.move = default;
        }

        public Move Current => this.move;

        public MoveEnumerator GetEnumerator() => this;

        public bool MoveNext()
        {
            for (++this.to; this.to >= Square.First && this.to <= Square.Last; ++this.to)
            {
                this.move = new Move(this.design, this.from, this.to);
                if (this.position.CanChange(this.move) == this.to)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
