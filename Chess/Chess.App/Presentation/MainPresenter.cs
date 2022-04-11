namespace Chess.App.Presentation;

using System;
using Relay.PresentationModel;

public class MainPresenter : Presenter
{
    private Game? game;

    public MainPresenter()
    {
        this.Fen = Game.FenStartingPosition;
    }

    public string? Fen
    {
        get => this.game?.ToFen();
        set
        {
            if (this.game is not null)
                this.game.Moved -= HandleGameMoved;
            var gameChanged = Set(ref this.game, Game.FromFen(value));
            if (this.game is not null)
                this.game.Moved += HandleGameMoved;
            if (gameChanged)
            {
                RaisePropertyChanged(nameof(this.Game));
            }
        }
    }

    public Game? Game => this.game;

    private void HandleGameMoved(object? sender, EventArgs e)
    {
        RaisePropertyChanged(nameof(this.Fen));
    }
}
