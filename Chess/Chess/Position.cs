﻿namespace Chess;

using System;

/// <summary>
/// Allows initial piece arrangment.
/// </summary>
interface IBoard
{
    Square EnPassant { get; set; }
    Castling Castling { get; set; }

    void Clear();
    void Place(IPiece piece, Square square);
}

//todo: eliminate arrays, use bitboards
// https://www.chessprogramming.org/General_Setwise_Operations
public sealed class Position : IBoard, ICloneable
{
    private readonly Game game;
    private readonly IPiece[] board;

    private Square enPassant;
    private Castling castling;

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

    public Castling Castling => this.castling;

    Square IBoard.EnPassant { get => this.enPassant; set => this.enPassant = value; }

    Castling IBoard.Castling { get => this.castling; set => this.castling = value; }

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
        var ep = this.enPassant;
        var cast = this.castling;

        // update en passant
        if (movedPiece.Type == PieceType.Pawn)
        {
            // detect two-square move
            var twoSquareMove = Piece.GetFile(move.From) == Piece.GetFile(move.To) &&
                Math.Abs(Piece.GetRank(move.From) - Piece.GetRank(move.To)) == 2;
            if (twoSquareMove)
            {
                this.enPassant = Piece.GetSquare(Piece.GetFile(move.To),
                    Piece.GetRank(move.To) + (movedPiece.Color == PieceColor.White ? -1 : +1));
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
                    this.board[(int)pawnSquare] = null!;
                    this.enPassant = Square.None;
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
        
        // update castling
        if (movedPiece.Type == PieceType.Rook)
        {
            if (move.From == Square.a1)
                this.castling &= ~Castling.WhiteQueenSide;
            else if (move.From == Square.h1)
                this.castling &= ~Castling.WhiteKingSide;
            else if (move.From == Square.a8)
                this.castling &= ~Castling.BlackQueenSide;
            else if (move.From == Square.h8)
                this.castling &= ~Castling.BlackKingSide;
        }
        else if (movedPiece.Type == PieceType.King)
        {
            if (CanCastleWith(move))
            {
                // move rook too
                (Square From, Square To) rookMove = move.Direction switch
                {
                    PieceMoveDirection.Left when movedPiece.Color == PieceColor.White => (Square.a1, Square.c1),
                    PieceMoveDirection.Left when movedPiece.Color == PieceColor.Black => (Square.a8, Square.c8),
                    PieceMoveDirection.Right when movedPiece.Color == PieceColor.White => (Square.h1, Square.f1),
                    PieceMoveDirection.Right when movedPiece.Color == PieceColor.Black => (Square.h8, Square.f8),
                    _ => (Square.None, Square.None),
                };
                if (rookMove.From != Square.None && rookMove.To != Square.None)
                {
                    var rook = this.board[(int)rookMove.From];
                    this.board[(int)rookMove.From] = null!;
                    this.board[(int)rookMove.To] = rook;
                }
            }

            if (move.From == Square.e1)
                this.castling &= ~(Castling.WhiteKingSide | Castling.WhiteQueenSide);
            else if (move.From == Square.e8)
                this.castling &= ~(Castling.BlackKingSide | Castling.BlackQueenSide);
        }

        if (cast != Castling.None)
            moveFlags |= MoveFlags.Castling;
        if (ep != Square.None)
            moveFlags |= MoveFlags.EnPassant;
        if (takenPiece is not null)
            moveFlags |= MoveFlags.Capture;

        return new Move(move, takenPiece?.Design ?? PieceDesign.None, moveFlags, ep, cast);
    }

    internal void ChangeBack(Move move)
    {
        this.board[(int)move.From] = this.board[(int)move.To];
        this.board[(int)move.To] = null!;

        var takenSquare = move.To;
        if (move.Flags.HasFlag(MoveFlags.EnPassant))
        {
            if (move.IsEnPassantCapture)
            {
                takenSquare = Piece.GetSquare(Piece.GetFile(move.To),
                   Piece.GetRank(move.To) + (Piece.GetColor(move.Design) == PieceColor.White ? -1 : +1));
            }
            this.enPassant = move.EnPassant;
        }
        if (move.Flags.HasFlag(MoveFlags.Capture))
        {
            this.board[(int)takenSquare] = this.game.FindSpare(move.DesignTaken);
        }
    }

    public bool CanChange(Move move)
    {
        if (move.IsValid)
        {
            foreach (var square in move.GetPath())
            {
                if (square == move.To)
                {
                    break;
                }

                if (this.board[(int)square] is not null)
                {
                    return false;
                }
            }

            var otherPiece = this.board[(int)move.To];
            if (Piece.GetType(move.Design) == PieceType.Pawn)
            {
                if (otherPiece is null)
                    return true;
            }
            else
            {
                if (otherPiece is null || otherPiece.Color != Piece.GetColor(move.Design))
                    return true;
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
                return true;
            }
        }
        else if (CanCastleWith(move))
        {
            foreach (var square in move.GetPath())
            {
                if (square == move.To)
                {
                    break;
                }

                //todo: also check if the square is under attack
                if (this.board[(int)square] is not null)
                {
                    return false;
                }
            }
            
            return !IsInCheck(Piece.GetColor(move.Design));
        }

        return false;
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
                if (attack.AsCastling == Castling.None && CanChange(attack))
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

    private bool IsLegal(Move move, IPiece piece, bool canSelfSacrifice = true)
    {
        if (CanChange(move))
        {
            move = Change(move);

            var inCheck = (piece.Type != PieceType.King && IsInCheck(piece.Color)) ||
                (!canSelfSacrifice && IsInCheckFor(piece));

            ChangeBack(move);

            if (!inCheck)
                return true;
        }

        return false;
    }

    private bool CanCastleWith(Move move)
    {
        var asCastling = move.AsCastling;

        return asCastling != Castling.None && this.Castling.HasFlag(asCastling);
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
                if (this.position.CanChange(this.move))
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
