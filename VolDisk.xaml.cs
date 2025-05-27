using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HanoiTower
{
    public partial class VolDisk : UserControl
    {
        public static readonly DependencyProperty BaseColorProperty =
            DependencyProperty.Register(
                "BaseColor",
                typeof(Color),
                typeof(VolDisk),
                new PropertyMetadata(Colors.SteelBlue, OnBaseColorChanged));

        public static readonly DependencyProperty StrokeBrushProperty =
            DependencyProperty.Register(
                "StrokeBrush",
                typeof(Brush),
                typeof(VolDisk),
                new PropertyMetadata(new SolidColorBrush(Colors.SteelBlue)));

        public static readonly DependencyProperty DisableTriggerProperty =
            DependencyProperty.Register(
                "DisableTrigger",
                typeof(Boolean),
                typeof(VolDisk),
                new PropertyMetadata(false));

        public Color BaseColor
        {
            get { return (Color)GetValue(BaseColorProperty); }
            set { SetValue(BaseColorProperty, value); }
        }

        public Brush StrokeBrush
        {
            get { return (Brush)GetValue(StrokeBrushProperty); }
            set { SetValue(StrokeBrushProperty, value); }
        }

        public Boolean DisableTrigger
        {
            get { return (Boolean)GetValue(DisableTriggerProperty); }
            set { SetValue(DisableTriggerProperty, value); }
        }

        private static void OnBaseColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as VolDisk;
            if (control != null)
            {
                control.UpdateGradient();
                control.StrokeBrush = new SolidColorBrush(control.BaseColor);
                control.UpdateGradient();
            }
        }

        public VolDisk()
        {
            InitializeComponent();
            StrokeBrush = new SolidColorBrush(BaseColor);
            UpdateGradient();
        }

        private void UpdateGradient()
        {
            // Формуємо градієнт на основі BaseColor
            var baseColor = BaseColor;
            var light = ChangeColorBrightness(baseColor, 0.4);
            var mid = ChangeColorBrightness(baseColor, 0.0);
            var dark = ChangeColorBrightness(baseColor, -0.3);

            var brush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1)
            };
            brush.GradientStops.Add(new GradientStop(light, 0.0));
            brush.GradientStops.Add(new GradientStop(mid, 0.5));
            brush.GradientStops.Add(new GradientStop(dark, 1.0));

            MainRect.Fill = brush;
        }

        // Допоміжний метод для зміни яскравості кольору
        private static Color ChangeColorBrightness(Color color, double correctionFactor)
        {
            double red = color.R;
            double green = color.G;
            double blue = color.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }

            return Color.FromArgb(color.A, (byte)red, (byte)green, (byte)blue);
        }
    }
}
