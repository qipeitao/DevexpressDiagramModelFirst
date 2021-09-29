using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevexpressDiagramTest
{
    public enum ItemType : int
    {
        Item,
        Line,
    }
    public sealed class ListDiagramItemCollection : ListItemCollection<IDiagram>
    {
        public void AddRange(List<DiagramItem> models)
        {
            models.Where(p => p.ItemType == ItemType.Item).ToList().ForEach(p => Add(p));
            models.Where(p => p.ItemType == ItemType.Line).ToList().ForEach(p => Add(p));
        }
    }
}
