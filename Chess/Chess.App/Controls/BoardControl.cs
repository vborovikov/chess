namespace Chess.App.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

[TemplatePart(Name = TemplateParts.ItemGrid, Type = typeof(Grid))]
public class BoardControl : Control
{
    private static class TemplateParts
    {
        public const string ItemGrid = "ItemGrid";
    }

    public static readonly DependencyProperty SquareSizeProperty =
        DependencyProperty.Register(nameof(SquareSize), typeof(double), typeof(BoardControl), new PropertyMetadata(36d));

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
}
