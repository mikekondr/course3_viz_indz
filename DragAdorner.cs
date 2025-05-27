using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace HanoiTower
{
    public class DragAdorner : Adorner
    {
        private readonly VolDisk _visual;
        private double _left;
        private double _top;

        public DragAdorner(UIElement adornedElement, VolDisk disk)
            : base(adornedElement)
        {
            _visual = new VolDisk
            {
                Width = disk.ActualWidth,
                Height = disk.ActualHeight,
                BaseColor = disk.BaseColor,
                Opacity = 0.7,
                IsHitTestVisible = false
            };
            AddVisualChild(_visual);
        }
        public void SetPosition(double left, double top)
        {
            _left = left;
            _top = top;
            InvalidateVisual();
        }
        protected override int VisualChildrenCount => 1;
        protected override Visual GetVisualChild(int index) => _visual;
        protected override Size MeasureOverride(Size constraint)
        {
            _visual.Measure(constraint);
            return _visual.DesiredSize;
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            _visual.Arrange(new Rect(new Point(_left, _top), _visual.DesiredSize));
            return finalSize;
        }
    }
}