namespace Chess.App.Controls;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

public class PieceControl : ContentControl, IPiece
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
    private DragAdorner? dragAdorner;

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

        if (square == Square.None)
        {
            ClearValue(Grid.RowProperty);
            ClearValue(Grid.ColumnProperty);
        }
        else
        {
            Grid.SetRow(this, SquareRank.Eight - Piece.GetRank(square));
            Grid.SetColumn(this, (int)Piece.GetFile(square));
        }
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        this.isTouched = this.Square != Square.None;
    }

    protected override void OnPreviewMouseMove(MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && this.isTouched && this.Content is IPiece gamePiece)
        {
            try
            {
                this.IsMoving = true;
                // this will block untill the drop
                DragDrop.DoDragDrop(this, new DataObject(typeof(IPiece), gamePiece), DragDropEffects.Move);
            }
            finally
            {
                StopDragging();
                this.IsMoving = false;
                this.isTouched = false;
            }
        }
    }

    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        this.isTouched = false;
    }

    internal void StartDragging(UIElement dropTarget, Point position, Point offset, DataTemplate dragTemplate)
    {
        if (this.dragAdorner is null)
        {
            var adornerLayer = AdornerLayer.GetAdornerLayer(dropTarget);
            this.dragAdorner = new DragAdorner(dropTarget, adornerLayer, offset, this, dragTemplate);
        }
        this.dragAdorner.UpdatePosition(position);
    }

    internal void StopDragging()
    {
        this.dragAdorner?.Detach();
        this.dragAdorner = null;
    }
}
