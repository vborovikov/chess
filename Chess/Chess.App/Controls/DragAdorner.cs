namespace Chess.App.Controls;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

public class DragAdorner : Adorner
{
    private readonly AdornerLayer adornerLayer;
    private readonly ContentPresenter contentPresenter;
    private readonly Point offset;
    private Point position;

    public DragAdorner(UIElement adornedElement, AdornerLayer adornerLayer, Point offset,
        object dragDropData, DataTemplate? dragDropTemplate = null)
        : base(adornedElement)
    {
        this.adornerLayer = adornerLayer;
        if (dragDropData is FrameworkElement element)
        {
            // check the offset for not being too far
            if (Math.Abs(offset.X) > element.ActualWidth / 2)
                offset.X = Math.Sign(offset.X) * element.ActualWidth / 2;
            if (Math.Abs(offset.Y) > element.ActualHeight / 2)
                offset.Y = Math.Sign(offset.Y) * element.ActualHeight / 2;
        }
        this.offset = offset;
        this.contentPresenter = new ContentPresenter
        {
            Content = dragDropData,
            ContentTemplate = dragDropTemplate,
            //Opacity = 0.7d,
        };

        this.adornerLayer.Add(this);
    }

    public void UpdatePosition(Point position)
    {
        this.position = (Point)(position - this.offset);
        try
        {
            this.adornerLayer?.Update(this.AdornedElement);
        }
        catch { }
    }

    protected override Size MeasureOverride(Size constraint)
    {
        this.contentPresenter.Measure(constraint);
        return this.contentPresenter.DesiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        this.contentPresenter.Arrange(new Rect(finalSize));
        return finalSize;
    }

    protected override Visual GetVisualChild(int index) => this.contentPresenter;

    protected override int VisualChildrenCount => 1;

    public override GeneralTransform GetDesiredTransform(GeneralTransform transform) => new GeneralTransformGroup
    {
        Children =
        {
            base.GetDesiredTransform(transform),
            new TranslateTransform(this.position.X, this.position.Y)
        }
    };

    public void Detach()
    {
        this.adornerLayer.Remove(this);
    }
}
