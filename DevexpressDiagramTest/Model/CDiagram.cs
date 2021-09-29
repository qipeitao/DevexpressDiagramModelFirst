using DevExpress.Diagram.Core;
using DevExpress.Xpf.Diagram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevexpressDiagramTest
{
    public class CDiagram : DiagramView
    {
        public CDiagram()
        {
        }
        /// <summary>
        /// 加载全局工具栏
        /// </summary>
        public override void LoadTools()
        {
            if (DiagramToolboxRegistrator.GetStencil(DiagramStencilId) == null)
            {
                DiagramToolboxRegistrator.RegisterStencil(stencil);
            }

            var model = new DiagramItem();
            stencil.RegisterTool(new FactoryItemTool(
                     "tool1",
                     () => "tool1",
                    diagram => new DiagramContentItem()
                    {
                        CustomStyleId = model.StyleId, //////该样式为拖动自动引用
                       DataContext = model,
                        Content = model
                    }));

            var model2 = new DiagramItem()  ;
            stencil.RegisterTool(new FactoryItemTool(
                     "tool2",
                     () => "tool2",
                    diagram => new DiagramContentItem()
                    {
                        CustomStyleId = model2.StyleId, //////该样式为拖动自动引用
                        DataContext = model2,
                        Content = model2
                    }));
            var model3 = new DiagramItem() { };
            stencil.RegisterTool(new FactoryItemTool(
                     "tool3",
                     () => "tool3",
                    diagram => new DiagramContentItem()
                    {
                        CustomStyleId = model3.StyleId, //////该样式为拖动自动引用
                        DataContext = model3,
                        Content = model3
                    }));

        }
        /// <summary>
        /// 创建线model
        /// </summary>
        /// <returns></returns>
        public override DiagramLine CreateLineModel()
        {
            return new DiagramLine();
        }
        /// <summary>
        /// 创建节点model
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override DiagramItem CreateNodeModel(DiagramItem lookItem)
        {
            return lookItem.Copy();
        }
    }
}
