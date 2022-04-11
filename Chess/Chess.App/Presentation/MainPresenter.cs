namespace Chess.App.Presentation;

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
            if (Set(ref this.game, Game.FromFen(value)))
            {
                RaisePropertyChanged(nameof(this.Game));
            }
        }
    }

    public Game? Game => this.game;
}
