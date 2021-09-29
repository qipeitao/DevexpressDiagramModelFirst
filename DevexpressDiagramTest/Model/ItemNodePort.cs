using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DevexpressDiagramTest
{
    public class ItemNodePort : BindableBase
    {
        #region 
        /// <summary>
        /// 序号
        /// </summary>
        public int Index { set; get; }
        public double X { set; get; }
        public double Y { set; get; }
        /// <summary>
        /// 位置节点
        /// </summary>
        public Point Point
        {
            get => new Point(X, Y);
        }
        private Boolean? isOuter = false;
        /// <summary>
        /// 是否是出节点
        /// </summary>
        public Boolean? IsOuter
        {
            set
            {
                SetProperty(ref isOuter, value);
                RaisePropertyChanged(nameof(IsOuter));
            }
            get => isOuter;
        }

        private string name;
        public string Name { get => name; set => SetProperty(ref name, value); }
        #endregion
    }
}
