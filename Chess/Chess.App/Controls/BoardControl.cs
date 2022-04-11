namespace Chess.App.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

[TemplatePart(Name = TemplateParts.SquareGrid, Type = typeof(Grid))]
public class BoardControl : ItemsControl
{
    private static class TemplateParts
    {
        public const string SquareGrid = "SquareGrid";
    }

    public static readonly DependencyProperty SquareSizeProperty =
        DependencyProperty.Register(nameof(SquareSize), typeof(double), typeof(BoardControl), new PropertyMetadata(64d));

    public static readonly DependencyProperty DragTemplateProperty =
        DependencyProperty.Register(nameof(DragTemplate), typeof(DataTemplate), typeof(BoardControl), new PropertyMetadata(null));

    private Grid? squareGrid;
    private DragAdorner? dragAdorner;

    static BoardControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(BoardControl),
            new FrameworkPropertyMetadata(typeof(BoardControl)));
    }

    public BoardControl()
    {
    }

    public double SquareSize
    {
        get { return (double)GetValue(SquareSizeProperty); }
        set { SetValue(SquareSizeProperty, value); }
    }

    public DataTemplate DragTemplate
    {
        get { return (DataTemplate)GetValue(DragTemplateProperty); }
        set { SetValue(DragTemplateProperty, value); }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        this.squareGrid = this.GetTemplateChild(TemplateParts.SquareGrid) as Grid;
    }

    protected override bool IsItemItsOwnContainerOverride(object item) => item is PieceControl;

    protected override DependencyObject GetContainerForItemOverride() => new PieceControl();

    protected override void OnPreviewDragEnter(DragEventArgs e) => HandlePieceMove(e);

    protected override void OnPreviewDragOver(DragEventArgs e) => HandlePieceMove(e);

    protected override void OnPreviewDragLeave(DragEventArgs e)
    {
        if (e.Source == this)
        {
            //todo: we get here when we drag a piece over another piece
            this.dragAdorner?.Detach();
            this.dragAdorner = null;
            e.Handled = true;
        }
    }

    protected override void OnPreviewDrop(DragEventArgs e) => HandlePieceMove(e, drop: true);

    private bool HandlePieceMove(DragEventArgs e, bool drop = false)
    {
        if (e.Data.GetData(typeof(IPiece)) is IPiece gamePiece &&
            this.ItemContainerGenerator.ContainerFromItem(gamePiece) is PieceControl pieceControl &&
            this.squareGrid is not null)
        {
            var piecePosition = e.GetPosition(this.squareGrid);
            if (piecePosition.X < 0 || piecePosition.Y < 0 || piecePosition.X > this.squareGrid.ActualWidth || piecePosition.Y > this.squareGrid.ActualHeight)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return false;
            }
            else if (drop)
            {
                this.dragAdorner?.Detach();
                this.dragAdorner = null;
                
                var squareFile = (SquareFile)(piecePosition.X / this.SquareSize);
                var squareRank = (SquareRank)((int)SquareRank.Eight - piecePosition.Y / this.SquareSize + 1);
                var square = Game.GetSquare(squareFile, squareRank);

                if (this.ItemsSource is Game game && game.Move(gamePiece, square))
                {
                    pieceControl.Square = square;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                    e.Handled = true;
                    return false;
                }
            }
            else
            {
                if (this.dragAdorner is null)
                {
                    var adornerLayer = AdornerLayer.GetAdornerLayer(this.squareGrid);
                    this.dragAdorner = new DragAdorner(this.squareGrid, adornerLayer, e.GetPosition(pieceControl),
                        pieceControl, this.DragTemplate);
                }
                this.dragAdorner.UpdatePosition(piecePosition);
            }

            return true;
        }

        e.Effects = DragDropEffects.None;
        e.Handled = true;
        return false;
    }
}
