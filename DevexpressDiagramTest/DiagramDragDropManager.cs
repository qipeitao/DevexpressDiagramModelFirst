using DevExpress.Diagram.Core;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Diagram;
using DevExpress.Xpf.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DevexpressDiagramTest
{
    /// <summary>
    /// dev图形控件接收拖拽数据
    /// </summary>
    public class DiagramDragDropManager : DragDropManagerBase
    {
        private DiagramControl Diagram { get { return (DiagramControl)this.AssociatedObject; } }
        protected override void OnAttached()
        {
            base.OnAttached();
            DragManager.SetDropTargetFactory(Diagram, new MyFactory());
            DragDropManagerBase.SetDragDropManager(Diagram, this);
        }
        public override void OnDragOver(DragDropManagerBase sourceManager, UIElement source, Point pt)
        {
            if (sourceManager.DraggingRows[0] is IDiagram dTool && !string.IsNullOrEmpty(dTool.ToolId))
            {
                var tool = DiagramToolboxRegistrator.GetStencil(DiagramView.DiagramStencilId)
                                                    .GetTool(dTool.ToolId) as FactoryItemTool;
                Diagram.Commands.Execute(DiagramCommandsBase.StartDragToolCommand, tool, null);
            }
        }
        protected override System.Collections.IList CalcDraggingRows(IndependentMouseEventArgs e)
        {
            return null;
        }
        protected override bool CanStartDrag(MouseButtonEventArgs e)
        {
            return false;
        }
        protected override DevExpress.Xpf.Grid.DragDrop.StartDragEventArgs RaiseStartDragEvent(IndependentMouseEventArgs e)
        {
            return null;
        }
        protected override System.Collections.IList ItemsSource
        {
            get { return null; }
        }
    }
    public class MyFactory : DragDropManagerBase.DragDropManagerDropTargetFactory
    {
    }
}
