﻿namespace Chess;

using System.Runtime.CompilerServices;
using static Movement;

public enum PieceMoveDirection
{
    Up,         //  8
    Down,       // -8
    Left,       // -1
    Right,      //  1
    UpLeft,     //  7
    UpRight,    //  9
    DownLeft,   // -9
    DownRight   // -7
}

[Flags]
public enum MoveFlags : byte
{
    None = 0,
    Capture = 1 << 0,
    EnPassant = 1 << 1,
    Castling = 1 << 2,
    Promotion = 1 << 3,
    Check = 1 << 4,
    Checkmate = 1 << 5,
    //Stalemate = 1 << 6,
    //Draw = 1 << 7
}

public readonly struct Move : IEquatable<Move>
{
    private const int SquareMask = 0b111111;
    private const int FromShift = 0;
    private const int ToShift = 6;
    private const int DesignMask = 0b1111;
    private const int DesignShift = 12;
    private const int DesignTakenShift = 16;
    private const int FlagsMask = 0b111111;
    private const int FlagsShift = 20;
    private const int EnPassantShift = 26;

    // 0..5 : from
    // 6..11 : to
    // 12..15 : design
    // 16..19 : design taken
    // 20..25 : flags
    // 26..31 : en passant
    private readonly int value;

    public Move(PieceDesign design, Square from, Square to)
    {
        this.value = ((int)from << FromShift) | ((int)to << ToShift) | ((int)design << DesignShift);
    }

    public Move(IPiece piece, Square to)
        : this(piece.Design, piece.Square, to)
    {
    }

    public Move(Move move, PieceDesign taken, 
        MoveFlags flags = MoveFlags.None, Square enPassant = Square.None, Castling castling = Castling.None)
        : this(move.Design, move.From, move.To)
    {
        //todo: store castling
        this.value |= ((int)taken << DesignTakenShift) | ((int)flags << FlagsShift) | ((int)enPassant << EnPassantShift);
    }

    public PieceDesign Design => (PieceDesign)((this.value >> DesignShift) & DesignMask);
    public PieceDesign DesignTaken => (PieceDesign)((this.value >> DesignTakenShift) & DesignMask);
    public Square From => (Square)((this.value >> FromShift) & SquareMask);
    public Square To => (Square)((this.value >> ToShift) & SquareMask);
    public MoveFlags Flags => (MoveFlags)((this.value >> FlagsShift) & FlagsMask);
    public Square EnPassant => (Square)((this.value >> EnPassantShift) & SquareMask);
    public bool IsValid => CanMove(this.Design, this.From, this.To);

    public bool IsCaptureByPawn
    {
        get
        {
            var direction = GetDirection(this.From, this.To);
            if (CanOffset(this.From, direction) && Offset(this.From, direction) == this.To)
            {
                return this.Design switch
                {
                    PieceDesign.WhitePawn => direction == PieceMoveDirection.UpLeft || direction == PieceMoveDirection.UpRight,
                    PieceDesign.BlackPawn => direction == PieceMoveDirection.DownLeft || direction == PieceMoveDirection.DownRight,
                    _ => false
                };
            }

            return false;
        }
    }

    public bool IsEnPassantCapture => this.IsCaptureByPawn &&
        this.Flags.HasFlag(MoveFlags.Capture | MoveFlags.EnPassant) && this.To == this.EnPassant;

    public Castling AsCastling
    {
        get
        {
            if (Piece.GetType(this.Design) != PieceType.King)
            {
                return Castling.None;
            }

            var direction = GetDirection(this.From, this.To);
            if (direction != PieceMoveDirection.Left && direction != PieceMoveDirection.Right)
            {
                return Castling.None;
            }

            if (!CanOffset(this.From, direction, 2) || Offset(this.From, direction, 2) != this.To)
            {
                return Castling.None;
            }

            if (direction == PieceMoveDirection.Right)
            {
                return this.Design == PieceDesign.WhiteKing ? Castling.WhiteKingSide : Castling.BlackKingSide;
            }

            return this.Design == PieceDesign.WhiteKing ? Castling.WhiteQueenSide : Castling.BlackQueenSide;
        }
    }

    public PieceMoveDirection Direction => GetDirection(this.From, this.To);

    public SquareEnumerator GetPath() => new(this);

    public override string ToString() => $"{this.Design} {this.From} -> {this.To}";

    public override bool Equals(object? obj) => obj is Move move && Equals(move);

    public bool Equals(Move other) => this.value == other.value;

    public override int GetHashCode() => this.value;

    public static bool operator ==(Move left, Move right) => left.Equals(right);

    public static bool operator !=(Move left, Move right) => !(left == right);

    public struct SquareEnumerator
    {
        private readonly ulong moves;
        private readonly Square target;
        private readonly int step;
        private Square square;

        public SquareEnumerator(Move move)
        {
            this.moves = move.IsValid || move.IsCaptureByPawn || move.AsCastling != Castling.None ? GetMap(move.Design, move.From) : EmptyMap;
            this.target = move.To;
            this.step = GetDirectionOffset(move.From, move.To);
            this.square = Piece.GetType(move.Design) == PieceType.Knight ? move.To - this.step : move.From;
        }

        public bool MoveNext()
        {
            if (this.moves == EmptyMap || this.square == this.target)
                return false;

            this.square += this.step;
            if (this.square == this.target)
                return true;

            while ((this.moves & (1UL << (int)this.square)) == 0UL)
            {
                this.square += this.step;
                if (this.square == this.target)
                    return true;
            }

            return this.square >= Square.First && this.square <= Square.Last;
        }

        public Square Current => this.square;

        public SquareEnumerator GetEnumerator() => this;
    }
}

static class Movement
{
    public const ulong EmptyMap = 0UL;

    private static readonly int[] directionOffsets = { 8, -8, -1, 1, 7, 9, -9, -7 };
    private static readonly ulong[][] moves;
    private static readonly ulong[][] attacks;

    static Movement()
    {
        var pieceDesignCount = PieceDesign.BlackKing - PieceDesign.WhitePawn + 1;
        var squareCount = Square.Last - Square.First + 1;

        moves = new ulong[pieceDesignCount][];
        attacks = new ulong[pieceDesignCount][];
        for (var design = PieceDesign.WhitePawn; design <= PieceDesign.BlackKing; ++design)
        {
            moves[(int)design - 1] = new ulong[squareCount];
            attacks[(int)design - 1] = new ulong[squareCount];
            for (var square = Square.First; square <= Square.Last; ++square)
            {
                moves[(int)design - 1][(int)square] = GetMoves(design, square);
                if (design == PieceDesign.WhitePawn || design == PieceDesign.BlackPawn)
                {
                    attacks[(int)design - 1][(int)square] = GetPawnAttacks(Piece.GetColor(design), square);
                }
                else
                {
                    attacks[(int)design - 1][(int)square] = moves[(int)design - 1][(int)square];
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong GetAttackMap(PieceDesign design, Square square)
    {
        return attacks[(int)design - 1][(int)square];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong GetMap(PieceDesign design, Square square)
    {
        return moves[(int)design - 1][(int)square];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CanMove(PieceDesign design, Square from, Square to)
    {
        return (moves[(int)design - 1][(int)from] & (1UL << (int)to)) != 0UL;
    }

    public static int GetDirectionOffset(Square from, Square to)
    {
        var orientation = from < to ? 1 : -1;
        var distance = from - to;

        if (distance % 9 == 0)
            return orientation * 9;
        if (distance % 8 == 0)
            return orientation * 8;
        if (distance % 7 == 0)
            return orientation * 7;

        return orientation;
    }

    public static PieceMoveDirection GetDirection(Square from, Square to)
    {
        var directionOffset = GetDirectionOffset(from, to);
        return (PieceMoveDirection)Array.IndexOf(directionOffsets, directionOffset);
    }

    private static ulong GetMoves(PieceDesign design, Square square)
    {
        return design switch
        {
            PieceDesign.WhitePawn or PieceDesign.BlackPawn => GetPawnMoves(Piece.GetColor(design), square),
            PieceDesign.WhiteKnight or PieceDesign.BlackKnight => GetKnightMoves(square),
            PieceDesign.WhiteBishop or PieceDesign.BlackBishop => GetBishopMoves(square),
            PieceDesign.WhiteRook or PieceDesign.BlackRook => GetRookMoves(square),
            PieceDesign.WhiteQueen or PieceDesign.BlackQueen => GetQueenMoves(square),
            PieceDesign.WhiteKing or PieceDesign.BlackKing => GetKingMoves(square),
            _ => EmptyMap,
        };
    }

    private static ulong GetKingMoves(Square square)
    {
        var moves = EmptyMap;

        for (var d = PieceMoveDirection.Up; d <= PieceMoveDirection.DownRight; ++d)
        {
            if (!CanOffset(square, d))
                continue;

            var offset = directionOffsets[(int)d];
            var to = square + offset;
            TrySetMove(ref moves, to);
        }

        return moves;
    }

    private static ulong GetQueenMoves(Square square) =>
        GetSlidingMoves(square, PieceMoveDirection.Up, PieceMoveDirection.DownRight);

    private static ulong GetRookMoves(Square square) =>
        GetSlidingMoves(square, PieceMoveDirection.Up, PieceMoveDirection.Right);

    private static ulong GetBishopMoves(Square square) =>
        GetSlidingMoves(square, PieceMoveDirection.UpLeft, PieceMoveDirection.DownRight);

    private static ulong GetKnightMoves(Square square)
    {
        var moves = EmptyMap;

        TrySetKnightMove(ref moves, square,
            PieceMoveDirection.UpLeft, PieceMoveDirection.Left);
        TrySetKnightMove(ref moves, square,
            PieceMoveDirection.UpLeft, PieceMoveDirection.Up);

        TrySetKnightMove(ref moves, square,
            PieceMoveDirection.UpRight, PieceMoveDirection.Right);
        TrySetKnightMove(ref moves, square,
            PieceMoveDirection.UpRight, PieceMoveDirection.Up);

        TrySetKnightMove(ref moves, square,
            PieceMoveDirection.DownLeft, PieceMoveDirection.Left);
        TrySetKnightMove(ref moves, square,
            PieceMoveDirection.DownLeft, PieceMoveDirection.Down);

        TrySetKnightMove(ref moves, square,
            PieceMoveDirection.DownRight, PieceMoveDirection.Right);
        TrySetKnightMove(ref moves, square,
            PieceMoveDirection.DownRight, PieceMoveDirection.Down);

        return moves;
    }

    private static bool TrySetKnightMove(ref ulong moves, Square square, PieceMoveDirection direction1, PieceMoveDirection direction2)
    {
        if (CanOffset(square, direction1, direction2))
        {
            return TrySetMove(ref moves, square +
                directionOffsets[(int)direction1] + directionOffsets[(int)direction2]);
        }

        return false;
    }

    private static ulong GetPawnMoves(PieceColor color, Square square)
    {
        var moves = EmptyMap;

        if (color == PieceColor.White)
        {
            if (Piece.GetRank(square) == SquareRank.One)
                return moves;
        }
        else
        {
            if (Piece.GetRank(square) == SquareRank.Eight)
                return moves;
        }

        var offset = color == PieceColor.White ?
            directionOffsets[(int)PieceMoveDirection.Up] : directionOffsets[(int)PieceMoveDirection.Down];
        var to = square + offset;
        TrySetMove(ref moves, to);

        if (color == PieceColor.White)
        {
            if (square >= Square.a2 && square <= Square.h2)
            {
                to = square + offset * 2;
                TrySetMove(ref moves, to);
            }
        }
        else
        {
            if (square >= Square.a7 && square <= Square.h7)
            {
                to = square + offset * 2;
                TrySetMove(ref moves, to);
            }
        }

        return moves;
    }

    private static ulong GetPawnAttacks(PieceColor color, Square square)
    {
        if (color == PieceColor.White)
        {
            if (Piece.GetRank(square) == SquareRank.One)
                return EmptyMap;
        }
        else
        {
            if (Piece.GetRank(square) == SquareRank.Eight)
                return EmptyMap;
        }

        var attacks = EmptyMap;

        var leftCaptureOffset = color == PieceColor.White ?
            directionOffsets[(int)PieceMoveDirection.UpLeft] : directionOffsets[(int)PieceMoveDirection.DownLeft];
        TrySetMove(ref attacks, square + leftCaptureOffset);

        var rightCaptureOffset = color == PieceColor.White ?
            directionOffsets[(int)PieceMoveDirection.UpRight] : directionOffsets[(int)PieceMoveDirection.DownRight];
        TrySetMove(ref attacks, square + rightCaptureOffset);

        return attacks;
    }

    private static ulong GetSlidingMoves(Square square,
        PieceMoveDirection first, PieceMoveDirection last)
    {
        var moves = EmptyMap;

        for (var direction = first; direction <= last; ++direction)
        {
            if (!CanOffset(square, direction))
                continue;

            var offset = directionOffsets[(int)direction];
            var to = square + offset;
            while (TrySetMove(ref moves, to))
            {
                if (CanOffset(to, direction))
                {
                    to += offset;
                }
                else
                {
                    break;
                }
            }
        }

        return moves;
    }

    internal static bool CanOffset(Square from, PieceMoveDirection direction, int times = 1)
    {
        if (times <= 0 || times > 7)
            return false;

        var firstRank = SquareRank.One + times - 1;
        var lastRank = SquareRank.Eight - times + 1;
        var firstFile = SquareFile.A + times - 1;
        var lastFile = SquareFile.H - times + 1;

        return direction switch
        {
            PieceMoveDirection.Up when Piece.GetRank(from) == lastRank => false,
            PieceMoveDirection.Down when Piece.GetRank(from) == firstRank => false,
            PieceMoveDirection.Left when Piece.GetFile(from) == firstFile => false,
            PieceMoveDirection.Right when Piece.GetFile(from) == lastFile => false,
            PieceMoveDirection.UpLeft when Piece.GetRank(from) == lastRank || Piece.GetFile(from) == firstFile => false,
            PieceMoveDirection.UpRight when Piece.GetRank(from) == lastRank || Piece.GetFile(from) == lastFile => false,
            PieceMoveDirection.DownLeft when Piece.GetRank(from) == firstRank || Piece.GetFile(from) == firstFile => false,
            PieceMoveDirection.DownRight when Piece.GetRank(from) == firstRank || Piece.GetFile(from) == lastFile => false,
            _ => true
        };
    }

    internal static Square Offset(Square from, PieceMoveDirection direction, int times = 1)
    {
        return from + directionOffsets[(int)direction] * times;
    }

    private static bool CanOffset(Square from, params PieceMoveDirection[] directions)
    {
        foreach (var direction in directions)
        {
            if (!CanOffset(from, direction))
                return false;
            from += directionOffsets[(int)direction];
        }

        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TrySetMove(ref ulong moves, Square to)
    {
        if (to >= Square.First && to <= Square.Last)
        {
            moves |= 1UL << (int)to;
            return true;
        }
        return false;
    }

    [System.Diagnostics.Conditional("DEBUG")]
    private static void PrintMoves(PieceDesign design, Square square)
    {
        System.Diagnostics.Debug.WriteLine($"Moves for {design} at {square}");

        var pieceSymbol = Piece.GetSymbol(design);
        var moves = GetMoves(design, square);
        Span<char> fileStr = stackalloc char[8];
        var f = 7;
        for (var i = 63; i >= 0; --i)
        {
            fileStr[f--] = (moves & (1UL << i)) != 0UL || (i == (int)square) ? pieceSymbol : '\u2610';
            if (i % 8 == 0)
            {
                f = 7;
                System.Diagnostics.Debug.WriteLine(fileStr.ToString());
            }
        }

        System.Diagnostics.Debug.WriteLine("");
    }
}
