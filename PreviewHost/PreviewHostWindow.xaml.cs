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
using System.Windows.Shapes;

namespace PreviewHost {
    /// <summary>
    /// Interaktionslogik für PreviewHostWindow.xaml
    /// </summary>
    public partial class PreviewHostWindow : Window {
        public PreviewHostWindow(string file) {
            InitializeComponent();
            pvh.SourcePath = file;
        }
    }
}
