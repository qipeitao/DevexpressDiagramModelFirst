using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Xpf.Diagram;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DevexpressDiagramTest
{
    /// <summary>
    /// 顶层显示
    /// </summary>
    public class ZoomBehavior : Behavior<FrameworkElement>
    {
        public double ZoomFactor
        {
            get { return (double)GetValue(ZoomFactorProperty); }
            set { SetValue(ZoomFactorProperty, value); }
        }

        public static readonly DependencyProperty ZoomFactorProperty =
            DependencyProperty.Register("ZoomFactor", typeof(double), typeof(ZoomBehavior), new PropertyMetadata(1.0));


        protected override void OnAttached()
        {
            base.OnAttached();
            ChangeFactor();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == ZoomFactorProperty)
            {
                ChangeFactor();
            }
        }

        public void ChangeFactor()
        {
            if (this.AssociatedObject != null)
                this.AssociatedObject.RenderTransform = new ScaleTransform(1 / ZoomFactor, 1 / ZoomFactor, this.AssociatedObject.ActualWidth / 2, this.AssociatedObject.ActualHeight / 2);
        }
        protected override void OnDetaching()
        {
            base.OnDetaching();
        }
    }
    /// <summary>
    /// 实现是否按压属性
    /// </summary>
    public class IsPressedBehavior : Behavior<FrameworkElement>
    {
        public DiagramControl Diagram
        {
            get { return (DiagramControl)GetValue(DiagramProperty); }
            set { SetValue(DiagramProperty, value); }
        }
        public static readonly DependencyProperty DiagramProperty =
            DependencyProperty.Register("Diagram", typeof(DiagramControl),
                typeof(IsPressedBehavior), new PropertyMetadata(null));
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseLeftButtonDown -= AssociatedObject_MouseLeftButtonDown;
            AssociatedObject.MouseLeftButtonUp -= AssociatedObject_MouseLeftButtonUp;
            AssociatedObject.MouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;
            AssociatedObject.MouseLeftButtonUp += AssociatedObject_MouseLeftButtonUp;
        }

        private void AssociatedObject_LostMouseCapture(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Trace.WriteLine("==========");
        }

        private void AssociatedObject_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Diagram.ActiveTool = null;
            // Diagram.Commands.Execute(DiagramCommandsBase.ChangeConnectorTypeCommand ,null);
        }

        private void AssociatedObject_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Diagram.Commands.Execute(DiagramCommandsBase.SelectConnectorToolCommand);
            Diagram.ActiveTool = Diagram.ConnectorTool;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseLeftButtonDown -= AssociatedObject_MouseLeftButtonDown;
            AssociatedObject.MouseLeftButtonUp -= AssociatedObject_MouseLeftButtonUp;
        }
    }

    /// <summary>
    /// 实现图形点击 启动连线功能
    /// </summary>
    public abstract class DiagramDragBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.AllowDrop = true;
            AssociatedObject.IsEnabled = true;
            AssociatedObject.MouseLeftButtonDown -= AssociatedObject_MouseLeftButtonDown;
            AssociatedObject.MouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;
        }

        private void AssociatedObject_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (AssociatedObject.DataContext is DiagramItem model)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    var ist = ConvertToItem(model);
                    DragDrop.DoDragDrop(this, new DataObject(DataFormats.Serializable, ist), DragDropEffects.Link);
                }
            }
        }

        public abstract IDiagram ConvertToItem(DiagramItem model);

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }
    }

    /// <summary>
    /// 定位容器
    /// </summary>
    public class ArrangablePanel : Canvas
    {
        protected override Size ArrangeOverride(Size arrangeSize)
        {

            foreach (FrameworkElement child in Children)
                if (child.DataContext is ItemNodePort pi)
                {
                    child.Arrange(new Rect(new Point(pi.X * arrangeSize.Width - child.DesiredSize.Width / 2,
                        (arrangeSize.Height) * pi.Y - child.DesiredSize.Height / 2), child.DesiredSize));
                }
            return arrangeSize;
        }
    }   /// <summary>
        /// 是否选中辅助类
        /// dev中IsSelected 是只读的。
        /// </summary>
    public class IsSelectedHelper
    {
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.RegisterAttached("IsSelected", typeof(Boolean), typeof(IsSelectedHelper), new FrameworkPropertyMetadata(IsSelectedPropertyChanged));
        public static void SetIsSelected(UIElement element, Boolean value)
        {
            element.SetValue(IsSelectedProperty, value);
        }
        public static Boolean GetIsSelected(UIElement element)
        {
            return (Boolean)element.GetValue(IsSelectedProperty);
        }

        private static void IsSelectedPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            if (source is DiagramContentItem item)
            {
                item.Dispatcher.Invoke(() =>
                {
                    ((IDiagram)item.DataContext).IsSelected = ((Boolean)e.NewValue);
                });
            }
            else if (source is DiagramConnector con)
            {
                con.Dispatcher.Invoke(() =>
                {
                    ((IDiagram)con.DataContext).IsSelected = ((Boolean)e.NewValue);
                });
            }
        }
    }
    /// <summary>
    /// 节点位置属性需要通过依赖属性通知
    /// </summary>
    public class PositionHelper
    {
        public static readonly DependencyProperty PositionProperty = DependencyProperty.RegisterAttached("Position", typeof(Point), typeof(PositionHelper), new FrameworkPropertyMetadata(PositionPropertyChanged));
        public static void SetPosition(UIElement element, Point value)
        {
            element.SetValue(PositionProperty, value);
        }
        public static Point GetPosition(UIElement element)
        {
            return (Point)element.GetValue(PositionProperty);
        }

        private static void PositionPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            DiagramContentItem item = source as DiagramContentItem;
            item.Dispatcher.Invoke(() =>
            {
                ((IDiagram)item.Content).Position = ((Point)e.NewValue);
                Trace.WriteLine($"====p{((IDiagram)item.Content).NameId} :{((Point)e.NewValue).X} {((Point)e.NewValue).Y}");
            });
        }
    }
}
