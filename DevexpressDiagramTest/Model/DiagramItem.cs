using DevExpress.Diagram.Core;
using DevExpress.Utils.Design;
using DevExpress.Xpf.Bars;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Diagram;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DevexpressDiagramTest
{
    public interface IDiagram
    {
        ItemType ItemType {  get; set; }
        string ToolId { get; set; }
        string StyleId { get; set; }
        string NameId { get; set; }
        Boolean IsSelected { set; get; }
        Point Position { set; get; }
    }
    public class DiagramItem : BindableBase, IDiagram
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

        private string styleId= "LabwareDiagramItemNode";
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
        public DiagramItem()
        { 
            ToolId = this.GetType().Name;
            Name  = ToolId; 
            Ports = new ObservableCollection<ItemNodePort>() {
            new ItemNodePort{ X=0,Y=0.5, IsOuter=false, Index=0 },
            new ItemNodePort{ X=1,Y=0.5, IsOuter=true, Index=1 },
            };
        }
        #region 页面刷新 
        #endregion 
        #region 右键菜单 
        /// <summary>
        /// 创建新的右击菜单
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<BarButtonItem> CreateContextMenu(DiagramControl control, ListDiagramItemCollection list)
        { 
            yield return new BarButtonItem()
            {
                Glyph = DXImageHelper.GetImageSource("Delete", ImageSize.Size16x16),
                Content = "删除",
                Command = DeleteCommand,
                CommandParameter = list,
            }; 
        }
        #region DeleteCommand
        private ICommand deleteCommand;
        [JsonIgnore]
        public ICommand DeleteCommand
        {
            get
            {
                if (deleteCommand == null)
                {
                    deleteCommand = new DelegateCommand<ListDiagramItemCollection>(OnDeleteCommand);
                }
                return deleteCommand;
            }
        }

        private double width = 100.0;
        public double Width { set { width = Math.Round(value, 2); RaisePropertyChanged(nameof(Width)); } get => width; }

        private double height = 200.0;
        public double Height { set { height = Math.Round(value, 2); RaisePropertyChanged(nameof(Height)); } get => height; }


        private ObservableCollection<ItemNodePort> ports;
        /// <summary>
        /// 连接点
        /// </summary>
        [JsonIgnore]
        public virtual ObservableCollection<ItemNodePort> Ports
        {
            get => ports;
            set
            {
                ports = value;
                RaisePropertyChanged(nameof(Ports));
            }
        }
        /// <summary>
        /// 本节点的图 连接点 
        /// 位置列表 用于定位
        /// </summary>
        [JsonIgnore]
        public DiagramPointCollection DiagramPoints
        {
            get
            {
                return new DiagramPointCollection(Ports.Select(p => new Point(p.X, p.Y)));
            }
        }
        public virtual void OnDeleteCommand(ListDiagramItemCollection list)
        {
            list.Remove(this);
        }
        #endregion
        #endregion
        public DiagramItem Copy()
        {
            var str = JsonConvert.SerializeObject(this);
            var s = JsonConvert.DeserializeObject(str, this.GetType()) as DiagramItem;
            return s;
        }
    }
}
