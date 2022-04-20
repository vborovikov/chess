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

    private Square enPassant;

    public Position(Game game)
        : this(game, new IPiece[Square.Last - Square.First + 1])
    {
    }

    private Position(Game game, IPiece[] board)
    {
        this.game = game;
        this.board = board;
        this.enPassant = Square.None;
    }

    public IPiece this[Square square]
    {
        get => this.board[(int)square];
    }

    public PieceEnumerator Pieces => new(this);

    public Square EnPassant => this.enPassant;

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

    internal Move Change(Move move)
    {
        var movedPiece = this.board[(int)move.From];
        var takenPiece = this.board[(int)move.To];

        this.board[(int)move.From] = null!;
        this.board[(int)move.To] = movedPiece;

        var moveFlags = MoveFlags.None;

        // update en passant
        if (movedPiece.Type == PieceType.Pawn)
        {
            // detect two-square move
            var twoSquareMove = Piece.GetFile(move.From) == Piece.GetFile(move.To) &&
                Math.Abs(Piece.GetRank(move.From) - Piece.GetRank(move.To)) == 2;
            if (twoSquareMove)
            {
                var pawnSquare = Piece.GetSquare(Piece.GetFile(move.To), 
                    Piece.GetRank(move.To) + (movedPiece.Color == PieceColor.White ? -1 : +1));
                this.enPassant = pawnSquare;
            }
            else if (move.IsCaptureByPawn)
            {
                // detect en passant capture
                if (takenPiece is null && move.To == this.enPassant)
                {
                    // find the square of the pawn to take
                    var pawnSquare = Piece.GetSquare(Piece.GetFile(this.enPassant),
                        Piece.GetRank(this.enPassant) + (movedPiece.Color == PieceColor.White ? -1 : +1));
                    takenPiece = this.board[(int)pawnSquare];
                    this.enPassant = Square.None;

                    moveFlags |= MoveFlags.EnPassant;
                }
            }
            else
            {
                this.enPassant = Square.None;
            }
        }
        else
        {
            this.enPassant = Square.None;
        }

        if (takenPiece is not null)
            moveFlags |= MoveFlags.Capture;

        return new Move(move, takenPiece?.Design ?? PieceDesign.None, moveFlags);
    }

    internal void ChangeBack(Move move)
    {
        this.board[(int)move.From] = this.board[(int)move.To];

        var takenSquare = move.To;
        if (move.Flags.HasFlag(MoveFlags.EnPassant))
        {
            takenSquare = Piece.GetSquare(Piece.GetFile(move.To),
                Piece.GetRank(move.To) + (Piece.GetColor(move.Design) == PieceColor.White ? -1 : +1));
            this.enPassant = move.To;
        }
        this.board[(int)takenSquare] = this.game.FindSpare(move.DesignTaken);
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

                if (this.board[(int)square] is not null)
                {
                    makeTo = square;
                    break;
                }
            }

            var otherPiece = this.board[(int)makeTo];
            if (Piece.GetType(move.Design) == PieceType.Pawn)
            {
                if (otherPiece is null)
                    return makeTo;
            }
            else
            {
                if (otherPiece is null || otherPiece.Color != Piece.GetColor(move.Design))
                    return makeTo;
            }
        }
        else if (move.IsCaptureByPawn)
        {
            var otherSquare = move.To;
            if (otherSquare == this.enPassant)
            {
                otherSquare = Piece.GetSquare(Piece.GetFile(move.To),
                    Piece.GetRank(move.To) + (Piece.GetColor(move.Design) == PieceColor.White ? -1 : +1));
            }
            var otherPiece = this.board[(int)otherSquare];
            if (otherPiece is not null && otherPiece.Color != Piece.GetColor(move.Design))
            {
                return move.To;
            }
        }

        return Square.None;
    }

    public MoveEnumerator GetMoves(IPiece piece)
    {
        return new MoveEnumerator(this, piece);
    }

    public LegalMoveEnumerator GetLegalMoves(IPiece piece, bool canSacrifice = true)
    {
        return new LegalMoveEnumerator(this, piece, canSacrifice);
    }

    public bool IsInCheckFor(IPiece piece)
    {
        var pieceSquare = Find(piece);
        foreach (var otherPiece in this.board)
        {
            if (otherPiece is not null && otherPiece != piece && otherPiece.Color != piece.Color)
            {
                var attack = new Move(otherPiece.Design, Find(otherPiece), pieceSquare);
                if (CanChange(attack) == pieceSquare)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool IsInCheck(PieceColor color)
    {
        return IsInCheckFor(color == PieceColor.White ? this.game.WhiteKing : this.game.BlackKing);
    }

    public bool IsLegal(Move move, IPiece piece, bool canSelfSacrifice = true)
    {
        if (CanChange(move) == move.To)
        {
            move = Change(move);

            var inCheck = (!canSelfSacrifice && IsInCheckFor(piece)) ||
                (piece.Type != PieceType.King && IsInCheck(piece.Color));

            ChangeBack(move);

            if (!inCheck)
                return true;
        }

        return false;
    }

    void IBoard.Clear()
    {
        Array.Clear(this.board);
        this.enPassant = Square.None;
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

    public struct LegalMoveEnumerator
    {
        private readonly Position position;
        private readonly IPiece piece;
        private readonly bool canSacrifice;
        private readonly Square from;
        private Square to;
        private Move move;

        public LegalMoveEnumerator(Position position, IPiece piece, bool canSacrifice)
        {
            this.position = position/*.Clone()*/;
            this.piece = piece;
            this.canSacrifice = piece.Type != PieceType.King && canSacrifice;
            this.from = piece.Square;
            this.to = Square.None;
            this.move = default;
        }

        public Move Current => this.move;

        public LegalMoveEnumerator GetEnumerator() => this;

        public bool MoveNext()
        {
            for (++this.to; this.to >= Square.First && this.to <= Square.Last; ++this.to)
            {
                this.move = new Move(this.piece.Design, this.from, this.to);
                if (this.position.IsLegal(this.move, this.piece, this.canSacrifice))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
