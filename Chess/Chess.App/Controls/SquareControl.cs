namespace Chess.App.Controls;
using System.Windows;
using System.Windows.Controls;

public class SquareControl : Control
{
    static SquareControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SquareControl),
            new FrameworkPropertyMetadata(typeof(SquareControl)));
    }
}
