namespace Chess.App.Controls;

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

public class Checkered : Decorator
{
    private static readonly Typeface typeface = new Typeface("Georgia");

    public Brush Foreground
    {
        get { return (Brush)GetValue(ForegroundProperty); }
        set { SetValue(ForegroundProperty, value); }
    }

    public static readonly DependencyProperty ForegroundProperty =
        DependencyProperty.Register(nameof(Foreground), typeof(Brush), typeof(Checkered), new PropertyMetadata(null));

    public Brush Background
    {
        get { return (Brush)GetValue(BackgroundProperty); }
        set { SetValue(BackgroundProperty, value); }
    }

    public static readonly DependencyProperty BackgroundProperty =
        DependencyProperty.Register(nameof(Background), typeof(Brush), typeof(Checkered), new PropertyMetadata(null));

    public Thickness BorderThickness
    {
        get { return (Thickness)GetValue(BorderThicknessProperty); }
        set { SetValue(BorderThicknessProperty, value); }
    }

    public static readonly DependencyProperty BorderThicknessProperty =
        DependencyProperty.Register(nameof(BorderThickness), typeof(Thickness), typeof(Checkered), new PropertyMetadata(new Thickness(1d)));

    public double SquareSize
    {
        get { return (double)GetValue(SquareSizeProperty); }
        set { SetValue(SquareSizeProperty, value); }
    }

    public static readonly DependencyProperty SquareSizeProperty =
        DependencyProperty.Register(nameof(SquareSize), typeof(double), typeof(Checkered), new PropertyMetadata(36d));

    protected override Size MeasureOverride(Size constraint)
    {
        // Compute the chrome size added by the various elements
        var borders = CollapseThicknessTwice(this.BorderThickness);
        var padding = new Size(this.SquareSize * 10d, this.SquareSize * 10d);

        var child = this.Child;
        if (child is null)
        {
            // Combine into total decorating size
            return new Size(borders.Width + padding.Width, borders.Height + padding.Height);
        }

        // Remove size of border only from child's reference size.
        var childConstraint = new Size(Math.Max(0d, constraint.Width - borders.Width),
                                        Math.Max(0d, constraint.Height - borders.Height));


        child.Measure(childConstraint);
        var childDesiredSize = child.DesiredSize;
        var childSize = new Size(Math.Max(padding.Width, childDesiredSize.Width),
                                 Math.Max(padding.Height, childDesiredSize.Height));

        // Now use the returned size to drive our size, by adding back the margins, etc.
        return new Size(childSize.Width + borders.Width, childSize.Height + borders.Height);
    }

    protected override Size ArrangeOverride(Size arrangeSize)
    {
        var child = this.Child;
        if (child is not null)
        {
            var borderThickness = this.BorderThickness;
            var borders = CollapseThicknessTwice(this.BorderThickness);
            var squareSize = this.SquareSize;

            var paddingWidth = borders.Width + squareSize * 2d;
            var paddingHeight = borders.Height + squareSize * 2d;

            var finalRect = new Rect();
            if ((arrangeSize.Width >= paddingWidth) && (arrangeSize.Height >= paddingHeight))
            {
                finalRect.X = borderThickness.Left * 2d + squareSize;
                finalRect.Y = borderThickness.Top * 2d + squareSize;
                finalRect.Width = arrangeSize.Width - paddingWidth;
                finalRect.Height = arrangeSize.Height - paddingHeight;
            }
            child.Arrange(finalRect);
        }

        return arrangeSize;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        var bounds = new Rect(0.0, 0.0, base.ActualWidth, base.ActualHeight);
        var background = this.Background;
        var foreground = this.Foreground;
        var squareSize = this.SquareSize;
        var borderThickness = this.BorderThickness;

        if (background is not null)
        {
            drawingContext.DrawRectangle(background, null, bounds);
        }
        if (foreground is not null)
        {
            DrawBorder(foreground, borderThickness, drawingContext, ref bounds);

            // draw the checkered pattern within the border

            // draw letters
            for (var letter = 'a'; letter <= 'h'; ++letter)
            {
                var letterStr = letter.ToString();
                var letterCol = letter - 'a' + 1;
                var text = new FormattedText(
                    letterStr, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                    typeface, squareSize * 0.7, foreground, 1.25)
                {
                    TextAlignment = TextAlignment.Center,
                    MaxTextWidth = squareSize,
                    MaxTextHeight = squareSize,
                };

                // top row letters
                drawingContext.DrawText(text, new Point(bounds.Left + squareSize * letterCol, bounds.Top));
                // bottom row letters
                drawingContext.DrawText(text, new Point(bounds.Left + squareSize * letterCol, bounds.Bottom - squareSize));
            }

            // draw numbers
            for (var number = 1; number <= 8; ++number)
            {
                var numberStr = number.ToString();
                var numberRow = 9 - number;
                var text = new FormattedText(
                    numberStr, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                    typeface, squareSize * 0.7, foreground, 1.25)
                {
                    TextAlignment = TextAlignment.Center,
                    MaxTextWidth = squareSize,
                    MaxTextHeight = squareSize,
                };

                // left column numbers
                drawingContext.DrawText(text, new Point(bounds.Left, bounds.Top + squareSize * numberRow));
                // right column numbers
                drawingContext.DrawText(text, new Point(bounds.Right - squareSize, bounds.Top + squareSize * numberRow));
            }

            bounds = DeflateRect(bounds, new Thickness(squareSize));
            DrawBorder(foreground, borderThickness, drawingContext, ref bounds);

            // draw checkers
            for (var i = 0; i != 8; ++i)
            {
                for (var j = 0; j != 8; ++j)
                {
                    if (((i % 2 == 0) && (j % 2 != 0)) || ((i % 2 != 0) && (j % 2 == 0)))
                    {
                        drawingContext.DrawRectangle(foreground, null,
                            new Rect(bounds.Left + squareSize * i, bounds.Top + squareSize * j, squareSize, squareSize));
                    }
                }
            }
        }
    }

    protected void DrawBorder(Brush borderBrush, Thickness borderThickness, DrawingContext dc, ref Rect bounds)
    {
        var size = CollapseThickness(borderThickness);
        if ((size.Width > 0.0) || (size.Height > 0.0))
        {
            if ((size.Width > bounds.Width) || (size.Height > bounds.Height))
            {
                if ((borderBrush != null) && (bounds.Width > 0.0) && (bounds.Height > 0.0))
                {
                    dc.DrawRectangle(borderBrush, null, bounds);
                }
                bounds = Rect.Empty;
            }
            else
            {
                if (borderThickness.Top > 0.0)
                {
                    dc.DrawRectangle(borderBrush, null, new Rect(bounds.Left, bounds.Top, bounds.Width, borderThickness.Top));
                }
                if (borderThickness.Left > 0.0)
                {
                    dc.DrawRectangle(borderBrush, null, new Rect(bounds.Left, bounds.Top, borderThickness.Left, bounds.Height));
                }
                if (borderThickness.Right > 0.0)
                {
                    dc.DrawRectangle(borderBrush, null, new Rect(bounds.Right - borderThickness.Right, bounds.Top, borderThickness.Right, bounds.Height));
                }
                if (borderThickness.Bottom > 0.0)
                {
                    dc.DrawRectangle(borderBrush, null, new Rect(bounds.Left, bounds.Bottom - borderThickness.Bottom, bounds.Width, borderThickness.Bottom));
                }
                bounds = DeflateRect(bounds, borderThickness);
            }
        }
    }

    private static Rect DeflateRect(Rect rt, Thickness thick)
    {
        return new Rect(rt.Left + thick.Left, rt.Top + thick.Top,
            Math.Max(0.0, rt.Width - thick.Left - thick.Right),
            Math.Max(0.0, rt.Height - thick.Top - thick.Bottom));
    }

    private static Size CollapseThickness(Thickness borderThickness)
    {
        return new Size(Math.Max(0.0, borderThickness.Left) + Math.Max(0.0, borderThickness.Right),
                        Math.Max(0.0, borderThickness.Top) + Math.Max(0.0, borderThickness.Bottom));
    }

    private static Size CollapseThicknessTwice(Thickness th)
    {
        return new Size((th.Left + th.Right) * 2d, (th.Top + th.Bottom) * 2d);
    }
}
