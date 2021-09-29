using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DevexpressDiagramTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            list.ItemsSource = new List<DiagramItem>() {
              new DiagramItem(){ ToolId="tool1"},
              new DiagramItem(){ ToolId="tool2"},
              new DiagramItem(){ ToolId="tool3"},
             };
            view.DiagramItemList = new ListDiagramItemCollection() {
              new DiagramItem(){ ToolId="tool1",NameId="N0",Left=100,Top=100,Width=50,Height=50},
              new DiagramItem(){ ToolId="tool2",NameId="N1",Left=300,Top=300,Width=100,Height=50},
              new DiagramLine(){ SourceItemName="N0", SourceItemPortIndex=1,TargetItemName="N1",TargetItemPortIndex=0}
            };
        }
    }
}
