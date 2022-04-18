namespace Chess.App.Presentation;

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Relay.PresentationModel;

public class MainPresenter : Presenter, IGame
{
    private readonly Game game;
    
    public MainPresenter()
    {
        this.game = new Game();
        this.Game = new ObservableCollection<IPiece>(this.game);

        this.game.BoardReset += (sender, e) =>
        {
            this.Game.Clear();
            foreach (var piece in this.game)
            {
                this.Game.Add(piece);
            }
        };
        this.game.PieceTaken += (sender, e) => this.Game.Remove(e.Piece);
        this.game.PieceMoved += (sender, e) => RaisePropertyChanged(nameof(this.Fen));

        this.Fen = Chess.Game.FenStartingPosition;
    }

    public ObservableCollection<IPiece> Game { get; }

    public string? Fen
    {
        get => this.game.ToString();
        set
        {
            this.game.Reset(value);
            RaisePropertyChanged();
        }
    }

    Square IGame.Find(IPiece piece) => this.game.Find(piece);

    bool IGame.Move(IPiece piece, Square square) => this.game.Move(piece, square);

    IEnumerator<IPiece> IEnumerable<IPiece>.GetEnumerator() => this.game.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.game.GetEnumerator();
}
