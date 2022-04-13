namespace Chess;

using System;

sealed class Position : ICloneable
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

    public Square Find(IPiece piece)
    {
        if (piece is null)
            return Square.None;

        var index = Array.IndexOf(this.board, piece);
        return index > -1 ? (Square)index : Square.None;
    }

    public IPiece Change(Square from, Square to)
    {
        var movedPiece = this.board[(int)from];
        var takenPiece = this.board[(int)to];

        this.board[(int)from] = null!;
        this.board[(int)to] = movedPiece;

        return takenPiece;
    }

    public void Reset(IEnumerable<(Square, IPiece)> pieces)
    {
        Array.Clear(this.board);

        foreach (var (square, piece) in pieces)
        {
            this.board[(int)square] = piece;
        }
    }

    public Position Clone()
    {
        var boardCopy = new IPiece[this.board.Length];
        Array.Copy(this.board, boardCopy, this.board.Length);
        return new Position(this.game, boardCopy);
    }

    object ICloneable.Clone() => Clone();
}
