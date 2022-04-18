namespace Chess;

using System.Runtime.CompilerServices;
using static Movement;

enum PieceMoveDirection
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

public readonly struct Move : IEquatable<Move>
{
    private const int SquareMask = 0b111111;
    private const int DesignMask = 0b1111;
    private const int DesignShift = 12;
    private const int FromShift = 0;
    private const int ToShift = 6;

    private readonly int value;

    public Move(PieceDesign design, Square from, Square to)
    {
        this.value = ((int)from << FromShift) | ((int)to << ToShift) | ((int)design << DesignShift);
    }

    public Move(IPiece piece, Square to)
        : this(piece.Design, piece.Square, to)
    {
    }

    public PieceDesign Design => (PieceDesign)((this.value >> DesignShift) & DesignMask);
    public Square From => (Square)((this.value >> FromShift) & SquareMask);
    public Square To => (Square)((this.value >> ToShift) & SquareMask);
    public bool IsValid => CanMove(this.Design, this.From, this.To);

    public SquareEnumerator GetPath() => new(this);

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
            this.moves = move.IsValid ? GetMap(move.Design, move.From) : EmptyMap;
            this.target = move.To;
            this.step = GetDirectionOffset(move.From, move.To);
            this.square = Piece.GetType(move.Design) == PieceType.Knight ? move.To - this.step : move.From;
        }

        public bool MoveNext()
        {
            if (this.moves == EmptyMap || this.square == this.target)
                return false;

            this.square += this.step;
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

        private static int GetDirectionOffset(Square from, Square to)
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
    }
}

static class Movement
{
    public const ulong EmptyMap = 0UL;

    private static readonly int[] directionOffsets = { 8, -8, -1, 1, 7, 9, -9, -7 };
    private static readonly ulong[][] moves;

    static Movement()
    {
        var pieceDesignCount = PieceDesign.BlackKing - PieceDesign.WhitePawn + 1;
        var squareCount = Square.Last - Square.First + 1;

        moves = new ulong[pieceDesignCount][];
        for (var design = PieceDesign.WhitePawn; design <= PieceDesign.BlackKing; ++design)
        {
            moves[(int)design] = new ulong[squareCount];
            for (var square = Square.First; square <= Square.Last; ++square)
            {
                moves[(int)design][(int)square] = GetMoves(design, square);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong GetMap(PieceDesign design, Square square)
    {
        return moves[(int)design][(int)square];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CanMove(PieceDesign design, Square from, Square to)
    {
        return (moves[(int)design][(int)from] & (1UL << (int)to)) != 0UL;
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

        TrySetMove(ref moves, square +
            directionOffsets[(int)PieceMoveDirection.UpLeft] +
            directionOffsets[(int)PieceMoveDirection.Left]);
        TrySetMove(ref moves, square +
            directionOffsets[(int)PieceMoveDirection.UpLeft] +
            directionOffsets[(int)PieceMoveDirection.Up]);

        TrySetMove(ref moves, square +
            directionOffsets[(int)PieceMoveDirection.UpRight] +
            directionOffsets[(int)PieceMoveDirection.Right]);
        TrySetMove(ref moves, square +
            directionOffsets[(int)PieceMoveDirection.UpRight] +
            directionOffsets[(int)PieceMoveDirection.Up]);

        TrySetMove(ref moves, square +
            directionOffsets[(int)PieceMoveDirection.DownLeft] +
            directionOffsets[(int)PieceMoveDirection.Left]);
        TrySetMove(ref moves, square +
            directionOffsets[(int)PieceMoveDirection.DownLeft] +
            directionOffsets[(int)PieceMoveDirection.Down]);

        TrySetMove(ref moves, square +
            directionOffsets[(int)PieceMoveDirection.DownRight] +
            directionOffsets[(int)PieceMoveDirection.Right]);
        TrySetMove(ref moves, square +
            directionOffsets[(int)PieceMoveDirection.DownRight] +
            directionOffsets[(int)PieceMoveDirection.Down]);

        return moves;
    }

    private static ulong GetPawnMoves(PieceColor color, Square square)
    {
        var moves = EmptyMap;

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

    private static bool CanOffset(Square from, PieceMoveDirection direction)
    {
        return direction switch
        {
            PieceMoveDirection.Up when Piece.GetRank(from) == SquareRank.Eight => false,
            PieceMoveDirection.Down when Piece.GetRank(from) == SquareRank.One => false,
            PieceMoveDirection.Left when Piece.GetFile(from) == SquareFile.A => false,
            PieceMoveDirection.Right when Piece.GetFile(from) == SquareFile.H => false,
            PieceMoveDirection.UpLeft when Piece.GetRank(from) == SquareRank.Eight || Piece.GetFile(from) == SquareFile.A => false,
            PieceMoveDirection.UpRight when Piece.GetRank(from) == SquareRank.Eight || Piece.GetFile(from) == SquareFile.H => false,
            PieceMoveDirection.DownLeft when Piece.GetRank(from) == SquareRank.One || Piece.GetFile(from) == SquareFile.A => false,
            PieceMoveDirection.DownRight when Piece.GetRank(from) == SquareRank.One || Piece.GetFile(from) == SquareFile.H => false,
            _ => true
        };
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
