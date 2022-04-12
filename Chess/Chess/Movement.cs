namespace Chess;

using System.Runtime.CompilerServices;

enum PieceMoveDirection
{
    Up,
    Down,
    Left,
    Right,
    UpLeft,
    UpRight,
    DownLeft,
    DownRight
}

static class Movement
{
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

    public static bool CanMove(PieceDesign design, Square from, Square to)
    {
        return (moves[(int)design][(int)from] & (1UL << (int)to)) != 0UL;
    }

    public static MoveEnumerator GetPath(PieceDesign design, Square from, Square to)
    {
        return new MoveEnumerator(design, from, to);
    }

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
            _ => 0UL,
        };
    }

    private static ulong GetKingMoves(Square square)
    {
        var moves = 0UL;

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
        var moves = 0UL;

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
        var moves = 0UL;

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
        var moves = 0UL;

        for (var direction = first; direction <= last; ++direction)
        {
            var offset = directionOffsets[(int)direction];
            var to = square + offset;
            while (TrySetMove(ref moves, to))
            {
                to += offset;
                
                if (direction == PieceMoveDirection.Left && Piece.GetFile(to) == SquareFile.H)
                    break;
                if (direction == PieceMoveDirection.Right && Piece.GetFile(to) == SquareFile.A)
                    break;
            }
        }

        return moves;
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

    public struct MoveEnumerator
    {
        private readonly ulong moves;
        private readonly Square target;
        private readonly int step;
        private Square square;
        
        public MoveEnumerator(PieceDesign design, Square from, Square to)
        {
            this.moves = Movement.moves[(int)design][(int)from];
            this.target = to;
            this.step = GetDirectionOffset(from, to);
            this.square = Piece.GetType(design) == PieceType.Knight ? to - step : from;
        }

        public bool MoveNext()
        {
            if (this.square == this.target)
                return false;

            this.square += this.step;
            while ((this.moves & (1UL << (int)this.square)) == 0UL)
            {
                this.square += this.step;
                if (this.square == this.target)
                    break;
            }

            return true;
        }

        public Square Current => this.square;

        public MoveEnumerator GetEnumerator() => this;
    }
}
