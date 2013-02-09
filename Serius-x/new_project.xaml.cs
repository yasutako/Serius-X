using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Serius
{
    /// <summary>
    /// new_project.xaml の相互作用ロジック
    /// </summary>
    public partial class new_project : Window
    {
        public new_project()
        {
            InitializeComponent();
        }

        private void ok_click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
        private void cancel_click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        public String name
        {
            get
            {
                return name_box.Text;
            }
        }
    }
}
