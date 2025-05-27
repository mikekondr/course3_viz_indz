using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace HanoiTower
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private DragAdorner _adorner;
        private AdornerLayer _adornerLayer;

        private int _total;
        public int Total {
            get => _total;
            set {
                if (_total != value) {
                    _total = value;
                    OnPropertyChanged(nameof(Total));
                    OnPropertyChanged(nameof(MinMoves));
                }
            }
        }

        private int _moves;
        public int Moves
        {
            get => _moves;
            set
            {
                if (_moves != value)
                {
                    _moves = value;
                    OnPropertyChanged(nameof(Moves));
                }
            }
        }

        public int MinMoves
        {
            get { return (int)Math.Pow(2, Total) - 1; }
            set { }
        }

        private int _time;
        public int Time
        {
            get => _time;
            set
            {
                if (_time != value)
                {
                    _time = value;
                    OnPropertyChanged(nameof(Time));
                }
            }
        }

        private Timer _timer;

        private static List<Color> colors = new List<Color>
        {
            Color.FromRgb(255, 0, 0),      // Яскраво-червоний
            Color.FromRgb(0, 255, 0),      // Яскраво-зелений
            Color.FromRgb(0, 0, 255),      // Яскраво-синій
            Color.FromRgb(255, 255, 0),    // Жовтий
            Color.FromRgb(0, 255, 255),    // Бірюзовий
            Color.FromRgb(255, 0, 255),    // Пурпурний
            Color.FromRgb(255, 128, 0),    // Оранжевий
            Color.FromRgb(128, 0, 255),    // Фіолетовий
            Color.FromRgb(0, 255, 128),    // Салатовий
            Color.FromRgb(255, 0, 128)     // Малиновий
        };

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Total = new Random().Next(5, 10);
            _timer = new Timer(1000);
            _timer.AutoReset = true;
            _timer.Elapsed += (s, e) => { Time++; };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NewGame();
        }

        private void NewGame()
        {
            panel1.Children.Clear();
            panel2.Children.Clear();
            panel3.Children.Clear();
            _adorner = null;
            _adornerLayer = null;
            mainGrid.Tag = null;
            _timer.Stop();
            Finish.Visibility = Visibility.Collapsed;   

            Moves = 0;
            Time = 0;

            int maxWidth = (int)panel1.ActualWidth - 20;
            int minWidth = 40;
            int stepWidth = (maxWidth - minWidth) / Total;
            for (int i = 1; i <= Total; i++)
            {
                var disk = new VolDisk
                {
                    Name = $"disk{i}",
                    Margin = new Thickness((stepWidth * i) / 2, 0, (stepWidth * i) / 2, 10),
                    Height = 25,
                    BaseColor = colors[i - 1]
                };
                disk.MouseDown += disk_MouseDown;
                disk.MouseEnter += VolDisk_MouseEnter;
                disk.VerticalAlignment = VerticalAlignment.Bottom;
                DockPanel.SetDock(disk, Dock.Bottom);
                panel1.Children.Add(disk);
            }
        }

        private void disk_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var disk = sender as VolDisk;
                if (disk.Parent is DockPanel panel && panel.Children.IndexOf(disk) == panel.Children.Count - 1)
                {
                    disk.Visibility = Visibility.Hidden;
                    DragDrop.DoDragDrop(disk, new DataObject(typeof(VolDisk), disk), DragDropEffects.Move);
                    disk.Visibility = Visibility.Visible;
                }
            }
        }

        private Panel GetPanel(object source)
        {
            var panel = source as Panel;
            if (panel == null)
            {
                var element = source as FrameworkElement;
                if (element != null)
                    panel = element.Parent as Panel;
            }
            return panel;
        }

        private void MoveDisk(VolDisk disk, Panel panel)
        {
            if (disk == null || panel == null)
                return;

            var old_panel = disk.Parent as Panel;
            if (old_panel == panel)
                return;
            old_panel.Children.Remove(disk);
            panel.Children.Add(disk);

            Moves++;
            if (_timer != null && !_timer.Enabled)
            {
                _timer.Start();
            }

            if (panel2.Children.Count == Total || panel3.Children.Count == Total)
            {
                _timer.Stop();
                Finish.Visibility = Visibility.Visible;
            }
        }

        private void panel_DragEnter(object sender, DragEventArgs e)
        {
            var disk = e.Data.GetData(typeof(VolDisk)) as VolDisk;

            if (_adorner == null)
            {
                _adornerLayer = AdornerLayer.GetAdornerLayer(this.Content as Visual);
                if (_adornerLayer != null && disk != null)
                {
                    _adorner = new DragAdorner(sender as UIElement, disk);
                    var pos = e.GetPosition(sender as IInputElement);
                    _adorner.SetPosition(pos.X - disk.ActualWidth / 2, pos.Y - disk.ActualHeight / 2);

                    _adornerLayer.Add(_adorner);
                }
            }
        }

        private void panel_Drop(object sender, DragEventArgs e)
        {
            var panel = GetPanel(e.Source);
            if (panel == null || !(panel is DockPanel))
                return;

            RemoveAdorner();
            mainGrid.Tag = null;

            var disk = e.Data.GetData(typeof(VolDisk)) as VolDisk;
            if (panel == disk.Parent)
                return;

            if (disk != null)
                disk.Visibility = Visibility.Visible;

            MoveDisk(disk, panel);
        }

        private void panel_DragOver(object sender, DragEventArgs e)
        {
            var disk = e.Data.GetData(typeof(VolDisk)) as VolDisk;
            if (_adorner != null && disk != null)
            {
                var pos = e.GetPosition(sender as IInputElement);
                _adorner.SetPosition(pos.X - disk.ActualWidth / 2, pos.Y - disk.Height / 2);
            }

            var panel = GetPanel(e.Source);
            if (panel == null || !(panel is DockPanel))
            {
                e.Effects = DragDropEffects.None;
                return;
            }
            var c = panel.Children.Count;
            var k = c > 0 ? (panel.Children[c - 1] as VolDisk).ActualWidth : double.MaxValue;
            if (disk.ActualWidth <= k)
            {
                e.Effects = DragDropEffects.Move;
                if (sender == panel1)
                    mainGrid.Tag = "Highlight0";
                else if (sender == panel2)
                    mainGrid.Tag = "Highlight1";
                else if (sender == panel3)
                    mainGrid.Tag = "Highlight2";
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void panel_DragLeave(object sender, DragEventArgs e)
        {
            RemoveAdorner();
            mainGrid.Tag = null;
        }

        private void RemoveAdorner()
        {
            if (_adorner != null && _adornerLayer != null)
            {
                _adornerLayer.Remove(_adorner);
                _adorner = null;
            }
        }

        private void VolDisk_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is VolDisk disk)
                if (disk.Parent is DockPanel panel && panel.Children.Count - 1 == panel.Children.IndexOf(disk))
                    disk.Cursor = Cursors.Hand;
                else
                    disk.Cursor = Cursors.Arrow;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            int maxWidth = (int)panel1.ActualWidth - 20;
            int minWidth = 40;
            int stepWidth = (maxWidth - minWidth) / Total;
            for (int i = 1; i <= Total; i++)
            {
                var disk = FindDiskByName(this, $"disk{i}");
                if (disk == null)
                    continue;
                disk.Margin = new Thickness((stepWidth * i) / 2, 0, (stepWidth * i) / 2, 10);
            }

        }
        private VolDisk FindDiskByName(DependencyObject parent, string name)
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is VolDisk disk && disk.Name == name)
                    return disk;

                var result = FindDiskByName(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }

        private void NewGame_Click(object sender, RoutedEventArgs e)
        {
            NewGame();
        }

        private void TotalButton(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Name == "plus")
            {
                if (Total < 10)
                {
                    Total++;
                    NewGame();
                }
            }
            else if ((sender as Button).Name == "minus")
            {
                if (Total > 3)
                {
                    Total--;
                    NewGame();
                }
            }
        }
    }
}
