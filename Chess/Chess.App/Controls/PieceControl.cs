namespace Chess.App.Controls;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public class PieceControl : Control, IPiece
{
    public static readonly DependencyProperty DesignProperty =
        DependencyProperty.Register(nameof(Design), typeof(PieceDesign),
            typeof(PieceControl), new PropertyMetadata(PieceDesign.WhitePawn));

    public static readonly DependencyProperty SquareProperty =
        DependencyProperty.Register(nameof(Square), typeof(Square),
            typeof(PieceControl), new PropertyMetadata(Square.None, (d, e) => ((PieceControl)d).OnSquareChanged(e)));

    public static readonly DependencyProperty IsMovingProperty =
        DependencyProperty.Register(nameof(IsMoving), typeof(bool),
            typeof(PieceControl), new PropertyMetadata(false));

    private bool isTouched;

    static PieceControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(PieceControl),
            new FrameworkPropertyMetadata(typeof(PieceControl)));
    }

    public PieceDesign Design
    {
        get => (PieceDesign)GetValue(DesignProperty);
        set => SetValue(DesignProperty, value);
    }

    public Square Square
    {
        get { return (Square)GetValue(SquareProperty); }
        set { SetValue(SquareProperty, value); }
    }

    public bool IsMoving
    {
        get { return (bool)GetValue(IsMovingProperty); }
        private set { SetValue(IsMovingProperty, value); }
    }

    private void OnSquareChanged(DependencyPropertyChangedEventArgs e)
    {
        var square = (Square)e.NewValue;

        Grid.SetRow(this, (int)Game.GetFile(square));
        Grid.SetColumn(this, (int)Game.GetRank(square));
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        this.isTouched = this.Square != Square.None;
    }

    protected override void OnPreviewMouseMove(MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && this.isTouched)
        {
            try
            {
                this.IsMoving = true;
                // this will block untill the drop
                DragDrop.DoDragDrop(this, this, DragDropEffects.Move);
            }
            finally
            {
                this.IsMoving = false;
                this.isTouched = false;
            }
        }
    }

    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        this.isTouched = false;
    }
}
