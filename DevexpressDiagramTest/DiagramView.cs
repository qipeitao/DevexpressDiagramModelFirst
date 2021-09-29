using DevExpress.Diagram.Core;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Diagram;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DevexpressDiagramTest
{
    /// <summary>
    /// 空间扩展
    /// 主要因为 源控件子项内不支持右击菜单
    /// 右击菜单为view控件创建。功能复写
    /// </summary>
    public class DiagramControlEx : DiagramControl
    {
        protected override IEnumerable<DevExpress.Xpf.Bars.IBarManagerControllerAction> CreateContextMenu()
        {
            if (SelectedItems != null && SelectedItems.Count > 0
                && SelectedItems[0] is DiagramContentItem item && item.DataContext is DiagramItem model)
            {
                if (this.TemplatedParent is DiagramView view)
                {
                    foreach (IBarManagerControllerAction action in model.CreateContextMenu(this, view.DiagramItemList))
                        yield return action;
                }
            }
            //foreach (IBarManagerControllerAction action in base.CreateContextMenu())
            //    yield return action;
        }
    }
    /// <summary>
    /// Interaction logic for Diagram.xaml
    /// </summary>
    [TemplatePart(Name = DiagramViewName, Type = typeof(DiagramControlEx))]
    public abstract class DiagramView : UserControl
    {
        static DiagramView()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
                typeof(DiagramView), new FrameworkPropertyMetadata(typeof(DiagramView)));
        }
        private const string DiagramViewName = "PART_DiagramView";
        public const string DiagramStencilId = "CustomTools";
        #region Item Property 
        public ListDiagramItemCollection DiagramItemList
        {
            get { return (ListDiagramItemCollection)GetValue(DiagramItemListProperty); }
            set { SetValue(DiagramItemListProperty, value); }
        }
        public static readonly DependencyProperty DiagramItemListProperty =
            DependencyProperty.Register("DiagramItemList",
                                       typeof(ListDiagramItemCollection),
                                       typeof(DiagramView),
                                       new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnItemSourceChanged)));

        private static async void OnItemSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = d as DiagramView;
            await Task.Run(() =>
            {
                Boolean isload = false;
                while (!isload)
                {
                    Thread.Sleep(50);
                    view.Dispatcher.Invoke(() => isload = view.IsLoaded);
                }
            });
            if (view.DiagramItemList == null)
            {
                view.Children.Clear();
                return;
            }
            view.DiagramItemList.CollectionChanged -= view.DiagramItemList_CollectionChanged;
            view.DiagramItemList.CollectionChanged += view.DiagramItemList_CollectionChanged;
            view.CreateChildren();
        } 

        /// <summary>
        /// 画布大小
        /// </summary>
        public Size PageSize
        {
            get { return (Size)GetValue(PageSizeProperty); }
            set { SetValue(PageSizeProperty, value); }
        }

        public static readonly DependencyProperty PageSizeProperty =
            DependencyProperty.Register("PageSize",
                                       typeof(Size),
                                       typeof(DiagramView),
                                       new FrameworkPropertyMetadata(new Size(800, 600)));
        #endregion
        #region 属性
        public DiagramStencil stencil = new DiagramStencil(DiagramStencilId, "Content Item Tools");
        public DiagramControlEx Diagram
        {
            get => control;
        }
        public DiagramItemCollection Children
        {
            get => (control?.Items);
        }
        #endregion
     
        private DiagramControlEx control;
        public DiagramView()
        {
            LoadTools();
        }

        /// <summary>
        /// 加载导航工具
        /// </summary>
        public virtual void LoadTools()
        {

        }
        private T GetTemplateChild<T>(string childName) where T : FrameworkElement, new()
        {
            return (GetTemplateChild(childName) as T) ?? new T();
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            control = GetTemplateChild<DiagramControlEx>(DiagramViewName);
            control.AddingNewItem += diagram_AddingNewItem;
            control.ItemsMoving += diagramControl_ItemsMoving;  
            Diagram.SelectedStencils.Clear();
            Diagram.SelectedStencils.Add(DiagramStencilId);
        }

        #region 方法
        /// <summary>
        /// 获取下一个子项 序号
        /// 用户不一定选择在连接点上
        /// </summary>
        /// <returns></returns>
        private int GetNextChildIndex()
        {
            if (Children.Count == 0) return 1;
            var max = Children.Select(p => p.Name.Substring(1)).Max(p => int.Parse(p));
            return max + 1;
        }
      
        /// <summary>
        /// 新增一项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void diagram_AddingNewItem(object sender, DiagramAddingNewItemEventArgs e)
        {

            if (e.Item is DiagramConnector connector && connector != null)
            {
                if ((connector.BeginItem == null || connector.EndItem == null))
                {
                    Diagram.ActiveTool = null;
                    e.Cancel = true;
                    return;
                }
                ///////可以新增 
                var s = connector.BeginPoint;
                var lineModel = CreateLineModel();
                lineModel.SourceItemName = ((DiagramContentItem)connector.BeginItem).Name;
                if (connector.BeginItemPointIndex == -1)
                {
                    lineModel.SourceItemPortIndex = GetIndexByContentPoint((DiagramContentItem)connector.BeginItem, connector.BeginPoint);
                }
                else
                {
                    lineModel.SourceItemPortIndex = connector.BeginItemPointIndex;
                }

                lineModel.TargetItemName = ((DiagramContentItem)connector.EndItem).Name;
                if (connector.EndItemPointIndex == -1)
                {
                    lineModel.TargetItemPortIndex = GetIndexByContentPoint((DiagramContentItem)connector.EndItem, connector.EndPoint);
                }
                else
                {
                    lineModel.TargetItemPortIndex = connector.EndItemPointIndex;
                }

                lineModel.NameId = "L" + GetNextChildIndex();
                if (CanCreateLine(lineModel))
                {
                    DiagramItemList.Add(lineModel);
                }
                //清尾操作
                Diagram.ActiveTool = null;
                e.Cancel = true;
            }
            if (e.Item is DiagramContentItem ct && ct != null && ct.Content is DiagramItem nodeModel)
            {
                ///////可以新增       
                if (CanCreateNode(nodeModel))
                {
                    var newModel = CreateNodeModel(nodeModel);
                    newModel.NameId = "N" + GetNextChildIndex();
                    DiagramItemList.Add(newModel);
                }
                //清尾操作
                Diagram.ActiveTool = null;
                e.Cancel = true;
            }
        }

        /// <summary>
        /// 根据点位获取点模型序号
        /// 用户连接节点时没有序号
        /// </summary>
        /// <param name="item"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        private int GetIndexByContentPoint(DiagramContentItem item, Point point)
        {
            var ps = item.DataContext as DiagramItem;
            var left = (point.X - item.Position.X) / item.ActualWidth;
            var top = (point.Y - item.Position.Y) / item.ActualHeight;
            var port = ps.Ports.OrderBy(p => (p.X - left) * (p.X - left) + (p.Y - top) * (p.Y - top));
            ////////////找不到连接点，采用默认位置 
            return port.FirstOrDefault().Index;
        }
        /// <summary>
        /// 监控列表变化了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DiagramItemList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (IDiagram item in e.NewItems)
                    {
                        if (item.ItemType == ItemType.Item)
                        {
                            AddOneNode(item as DiagramItem);
                        }
                        else if (item.ItemType == ItemType.Line)
                        {
                            AddOneLine(item as DiagramLine);
                        }
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (IDiagram item in e.OldItems)
                    {
                        var t = Children.FirstOrDefault(p => p.Name == item.NameId);
                        Children.Remove(t);
                        var node = item as IDiagram; 
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Reset)
                {
                    CreateChildren();
                }
            });
        }

        /// <summary>
        /// 创建项
        /// </summary>
        public void CreateChildren()
        {
            if (Children == null) return;
            Children.Clear();
            if (DiagramItemList == null || DiagramItemList.Count == 0) return;
            //先加载节点 
            DiagramItemList.Where(p => p.ItemType == ItemType.Item)
                   .OfType<DiagramItem>()
                   .ToList()
                   .ForEach(p =>
                   {
                       AddOneNode(p);
                   });
            DiagramItemList?
               .Where(p => p.ItemType == ItemType.Line)
               .OfType<DiagramLine>()
               .ToList()
               .ForEach(p =>
               {
                   AddOneLine(p);
               });
        }
        /// <summary>
        /// 添加线到view
        /// </summary>
        /// <param name="arrowLine"></param>
        /// <returns></returns>
        private FrameworkElement AddOneLine(DiagramLine arrowLine)
        {
            if (string.IsNullOrEmpty(arrowLine.NameId) || Children.FirstOrDefault(p => p.Name == arrowLine.NameId) != null)
            {
                arrowLine.NameId = "L" + GetNextChildIndex();
            }
            DiagramConnector control = new DiagramConnector() { Name = arrowLine.NameId };
            //手动创建无法自动引用样式
            if (!string.IsNullOrEmpty(arrowLine.StyleId))
            {
                try
                {
                    var s = this.FindResource(arrowLine.StyleId) as Style;
                    if (s != null)
                        control.Style = s;
                }
                catch (Exception ex)
                {

                }
            }
            control.DataContext = arrowLine;
            control.BeginItem = Children.FirstOrDefault(p => p.Name == arrowLine.SourceItemName);
            control.EndItem = Children.FirstOrDefault(p => p.Name == arrowLine.TargetItemName);
            control.BeginItemPointIndex = arrowLine.SourceItemPortIndex;
            control.EndItemPointIndex = arrowLine.TargetItemPortIndex;
            Children.Add(control); 
            return control;
        }
        /// <summary>
        /// 添加节点到view
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private FrameworkElement AddOneNode(DiagramItem node)
        {
            if (string.IsNullOrEmpty(node.NameId) || Children.FirstOrDefault(p => p.Name == node.NameId) != null)
            {
                node.NameId = "N" + GetNextChildIndex();
            }

            DiagramContentItem control = new DiagramContentItem()
            {
                Name = node.NameId,
                Content = node,
                Position = new Point(node.Left, node.Top),
            };
            //手动创建无法自动引用样式
            if (!string.IsNullOrEmpty(node.StyleId))
            {
                try
                {
                    var s = this.FindResource(node.StyleId) as Style;
                    if (s != null)
                        control.Style = s;
                }
                catch (Exception ex)
                {

                }
            }
            control.DataContext = node;
            Children.Add(control); 
            return control;
        }

        /// <summary>
        /// 内部拖动 限制运行范围
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void diagramControl_ItemsMoving(object sender, DiagramItemsMovingEventArgs e)
        {
            var item = e.Items.FirstOrDefault();
            if (item == null) return;
            var ct = item.Item as DiagramContentItem;
            var root = item.NewParent as DiagramRoot;
            if (item.Item != null && ct != null && root != null && ct.IsLoaded)
            {

                if (item.NewDiagramPosition.X < 10)
                {
                    item.NewDiagramPosition = new Point(10, item.NewDiagramPosition.Y);
                }
                if (item.NewDiagramPosition.Y < 10)
                {
                    item.NewDiagramPosition = new Point(item.NewDiagramPosition.X, 10);
                }
                if ((item.NewDiagramPosition.X + ct.ActualWidth + 10) > root.ActualWidth)
                {
                    item.NewDiagramPosition = new Point(root.ActualWidth - 10 - ct.ActualWidth, item.NewDiagramPosition.Y);

                }
                if ((item.NewDiagramPosition.Y + ct.ActualHeight + 10) > root.ActualHeight)
                {
                    item.NewDiagramPosition = new Point(item.NewDiagramPosition.X, root.ActualHeight - 10 - ct.ActualHeight);

                }
            }
        }
        /// <summary>
        /// 手动连接线 终点换位置
        /// 验证是否可以换，模型值设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void diagramControl_ConnectionChanging(object sender, DiagramConnectionChangingEventArgs e)
        {
            if (e.Connector == null) return;
            if (e.Connector != null && e.Connector.DataContext != null &&
                e.Connector.DataContext is DiagramLine lineModel)
            {
                if (e.NewItem == null || e.NewIndex == -1)
                {
                    e.Cancel = true; 
                    return;
                }
                //复制一份数据判断能否新增
                var copyLine = lineModel.Copy() as DiagramLine;
                if (e.ConnectorPointType == ConnectorPointType.Begin)
                {
                    copyLine.SourceItemName = e.NewItem?.Name;
                    copyLine.SourceItemPortIndex = e.NewIndex;
                }
                else if (e.ConnectorPointType == ConnectorPointType.End)
                {
                    copyLine.TargetItemName = e.NewItem?.Name;
                    copyLine.TargetItemPortIndex = e.NewIndex;
                }
                if (CanCreateLine(copyLine, false))
                {
                    if (e.ConnectorPointType == ConnectorPointType.Begin)
                    {
                        lineModel.SourceItemName = e.NewItem?.Name;
                        lineModel.SourceItemPortIndex = copyLine.SourceItemPortIndex;
                    }
                    else if (e.ConnectorPointType == ConnectorPointType.End)
                    {
                        lineModel.TargetItemName = e.NewItem?.Name;
                        lineModel.TargetItemPortIndex = copyLine.TargetItemPortIndex;
                    } 
                }
                else
                {
                    e.Cancel = true;
                    return;
                }
            }
        }
       
        #endregion
        #region 虚方法
        /// <summary>
        /// 创建线model
        /// </summary>
        /// <returns></returns>
        public abstract DiagramLine CreateLineModel();
        /// <summary>
        /// 创建节点model
        /// </summary>
        /// <returns></returns>
        public abstract DiagramItem CreateNodeModel(DiagramItem node);
        /// <summary>
        /// 验证是否能创建连接线
        /// </summary>
        /// <param name="lineModel"></param>
        /// <returns></returns>
        public virtual Boolean CanCreateLine(DiagramLine lineModel, Boolean isAdd = true)
        {
            var count = isAdd ? 0 : 1;
            if (string.IsNullOrEmpty(lineModel.SourceItemName) || string.IsNullOrEmpty(lineModel.TargetItemName)
                || lineModel.SourceItemPortIndex == -1 || lineModel.TargetItemPortIndex == -1)
            { 
                return false;
            }
            var lines = Children.OfType<DiagramConnector>().Select(p => p.DataContext as DiagramLine).ToList();
            //已经存在的 或相互的
            if (lines.Any(p => (p.SourcePortId == lineModel.SourcePortId && p.TargetPortId == lineModel.TargetPortId)
            || (p.TargetPortId == lineModel.SourcePortId && p.SourcePortId == lineModel.TargetPortId)))
            {
                return false;
            }
            var sModel = Children.OfType<DiagramContentItem>().FirstOrDefault(p => p.Name == lineModel.SourceItemName)?.Content as DiagramItem;
            var tModel = Children.OfType<DiagramContentItem>().FirstOrDefault(p => p.Name == lineModel.TargetItemName)?.Content as DiagramItem;
            if (sModel == null || tModel == null)
            { 
                return false;
            }
            var sPort = sModel.Ports.FirstOrDefault(p => p.Index == lineModel.SourceItemPortIndex);
            var tPort = tModel.Ports.FirstOrDefault(p => p.Index == lineModel.TargetItemPortIndex);
            if (sPort == null || tPort == null)
            { 
                return false;
            }
            if (lines.Where(p => p.SourcePortId == sPort.Name || p.TargetPortId == sPort.Name).Count() > count)
            { 
                return false;
            }
            if (lines.Where(p => p.SourcePortId == tPort.Name || p.TargetPortId == tPort.Name).Count() > count)
            { 
                return false;
            }
            if (sPort.IsOuter != true)
            { 
                return false;
            }
            if (tPort.IsOuter != false)
            { 
                return false;
            }
            return true;
        }
        /// <summary>
        /// 验证是否能创建节点
        /// </summary>
        /// <param name="lineModel"></param>
        /// <returns></returns>
        public virtual Boolean CanCreateNode(DiagramItem nodeModel)
        {
            return true;
        }
        #endregion

      
        #region 右击菜单

        #endregion
    }
}
