using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DevexpressDiagramTest
{
    public class DiagramLine :BindableBase, IDiagram
    {
        #region 属性 
        private ItemType itemType = ItemType.Item;
        /// <summary>
        /// 模型类型
        /// </summary>
        public ItemType ItemType { set { itemType = value; RaisePropertyChanged(nameof(ItemType)); } get => itemType; }
        private string nameId;
        /// <summary>
        /// id作用(dev需要)，
        /// 唯一性，Name会在新添加进图表区域时生成
        /// </summary>
        public virtual string NameId { set { nameId = value; RaisePropertyChanged(nameof(NameId)); } get => nameId; }

        private string name;
        /// <summary>
        /// 模型名  
        /// </summary>
        public virtual string Name { set { name = value; RaisePropertyChanged(nameof(Name)); } get => name; }


        private Boolean isSelected;
        /// <summary>
        /// 是否选中
        /// </summary>
        [JsonIgnore]
        public Boolean IsSelected { set { isSelected = value; RaisePropertyChanged(nameof(IsSelected)); } get => isSelected; }


        private string toolId;
        /// <summary>
        /// 工具id，dev生成图形时需要
        /// </summary>
        public string ToolId { set { toolId = value; RaisePropertyChanged(nameof(ToolId)); } get => toolId; }

        private string styleId;
        /// <summary>
        /// 样式id
        /// </summary>
        public string StyleId { set { styleId = value; RaisePropertyChanged(nameof(StyleId)); } get => styleId; }

    
        private double left = 100;
        public virtual double Left
        {
            set
            {
                if (left == Math.Round(value, 3)) return;
                left = Math.Round(value, 3);
                RaisePropertyChanged(nameof(Left));
                RaisePropertyChanged(nameof(Position));
            }
            get => left;
        }

        private double top = 100;
        public virtual double Top
        {
            set
            {
                if (top == Math.Round(value, 3)) return;
                top = Math.Round(value, 3);
                RaisePropertyChanged(nameof(Top));
                RaisePropertyChanged(nameof(Position));
            }
            get => top;
        }
        /// <summary>
        /// 位置
        /// left+top dev控件需要
        /// </summary>
        [JsonIgnore]
        public virtual Point Position
        {
            set
            {
                if (Left != value.X)
                {
                    Left = value.X;
                }
                if (Top != value.Y)
                {
                    Top = value.Y;
                }
                RaisePropertyChanged(nameof(Position));
            }
            get => new Point(Left, Top);
        }
        #endregion 
        #region DiagramLine
        public string SourcePortId { get => SourceItemName + "_" + SourceItemPortIndex; }
        public string TargetPortId { get => TargetItemName + "_" + TargetItemPortIndex; }

        private string sourceItemName;
        public string SourceItemName
        {
            set
            {
                SetProperty(ref sourceItemName, value);
                RaisePropertyChanged(nameof(SourcePortId));
            }
            get => sourceItemName;
        }

        private string targetItemName;
        public string TargetItemName
        {
            set
            {
                SetProperty(ref targetItemName, value);
                RaisePropertyChanged(nameof(TargetPortId));
            }
            get => targetItemName;
        }

        private int sourceItemPortIndex;
        public int SourceItemPortIndex
        {
            set
            {
                SetProperty(ref sourceItemPortIndex, value);
                RaisePropertyChanged(nameof(SourcePortId));
            }
            get => sourceItemPortIndex;
        }

        private int targetItemPortIndex;
        public int TargetItemPortIndex
        {
            set
            {
                SetProperty(ref targetItemPortIndex, value);
                RaisePropertyChanged(nameof(TargetPortId));
            }
            get => targetItemPortIndex;
        }
        #endregion
       
        public DiagramLine()
        { 
            ItemType = ItemType.Line;
        }

        public DiagramLine Copy()
        {
            var str = JsonConvert.SerializeObject(this);
            var s = JsonConvert.DeserializeObject(str, this.GetType()) as DiagramLine;
            return s;
        }
    }
}
