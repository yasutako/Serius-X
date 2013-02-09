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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using form = System.Windows.Forms;
using System.Diagnostics;
namespace Serius
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Project.init();
            if (Node_page.info.latest_project != null) Project.open(Node_page.info.latest_project + @"\a", this);
            this.Closing += MainWindow_Closed;
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            Node_page.info.write();
            init.stop_mongo(false);
            Process[] ps = Process.GetProcessesByName("node");
            if (ps.Length != 0)
            {
                foreach (Process p in ps) p.Kill();
            }
        }
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Tag_element.init();
        }
        protected override void OnClosed(EventArgs e)
        {
            //init.stop_mongo();
            //base.OnClosed(e);
        }
        private void new_file_click(object sender, RoutedEventArgs e)
        {
            new_file new_file_dialog = new new_file();
            if (new_file_dialog.ShowDialog() == true)
            {
                Filea file = new Filea(new_file_dialog.file_name);
                Project.project.files.Add(file);
                tab.Items.Add(file.item);
            }
        }
        private void new_project_click(object sender, RoutedEventArgs e)
        {
            new_project new_project_dialog = new new_project();
            if (new_project_dialog.ShowDialog() == true)
            {
                Project.create(new_project_dialog.name, this);
            }
        }

        private void open_project_click(object sender, RoutedEventArgs e)
        {
            form.OpenFileDialog dialog = new form.OpenFileDialog();
            dialog.Filter = "プロジェクトファイル|*.prj;package.json";
            if (dialog.ShowDialog() == form.DialogResult.OK)
            {
                Project.open(dialog.FileName, this);
            }
        }

        private void save_project_click(object sender, RoutedEventArgs e)
        {
            Project.save();
        }
    }
    class Project
    {

        public static String main_directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Serius-x";
        public static Project project;
        public String address;
        public String name;
        public List<Filea> files = new List<Filea>();
        public static void init()
        {
            DirectoryInfo di = new DirectoryInfo(Project.main_directory);
            if (di.Exists == false) di.Create();
        }
        public Project() {}
        public Project(String dir, String name)
        {
            if (dir == null) dir = Project.main_directory;
            address = dir + "/" + name;
            this.name = name;
        }
        public static void create(String name, MainWindow window)
        {
            String path = main_directory + "/" + name;
            Node_page.info.latest_project = path;
            Node_page.info.add_project(path);
            DirectoryInfo di = new DirectoryInfo(path);
            if (di.Exists == false)
            {
                di.Create();
                Project.project = new Project(null, name);
                window.Title = Project.project.address;
            }
            else MessageBox.Show("既に同名のフォルダが存在しています。");
        }
        private void object_create()
        {
            files.Add(new Filea("package.json"));
            files.Add(new Filea("node_modules"));
            files.Add(new Filea("public"));
            files.Add(new Filea("views"));
            files.Add(new Filea("controllers"));
            files.Add(new Filea("models"));
            files.Add(new Filea("containers"));
            files.Add(new Filea("server.js"));
            files.Add(new Filea("@express"));
            files.Add(new Filea("@mongoose"));
        }
        public static void save()
        {
            Project.project.object_save();
        }
        private void object_save()
        {
            StreamWriter sw = new StreamWriter(address + "/" + name + ".prj");
            sw.WriteLine(name);
            sw.Close();
            foreach (Filea file in files)
            {
                sw = new StreamWriter(address + "/" + file.name + "." + file.extention);
                sw.Write(file.text_canvas.text.text);
                sw.Close();
            }
        }
        public static void include(String name)
        {
        }
        public static void open(String path, MainWindow window)
        {
            path = path.Substring(0, path.LastIndexOf('\\'));
            Node_page.info.latest_project = path;
            Node_page.info.add_project(path);
            DirectoryInfo di = new DirectoryInfo(path);
            Project prj = new Project(di.Parent.FullName, di.Name);
            Project.project = prj;
            window.Title = Project.project.address;
            foreach (FileInfo fi in di.GetFiles())
            {
                switch (fi.Extension)
                {
                    case ".htm": case".html":
                        StreamReader sr = new StreamReader(fi.OpenRead());
                        prj.files.Add(new Filea(fi.Name, sr.ReadToEnd()));
                        sr.Close();
                        break;
                    case ".js":
                        sr = new StreamReader(fi.OpenRead());
                        prj.files.Add(new Filea(fi.Name, sr.ReadToEnd()));
                        sr.Close();
                        break;
                    case ".ejs":
                        sr = new StreamReader(fi.OpenRead());
                        prj.files.Add(new Filea(fi.Name, sr.ReadToEnd()));
                        sr.Close();
                        break;
                    case ".prj":
                        sr = new StreamReader(fi.OpenRead());
                        //prj.files.Add(new Filea(fi.Name, sr.ReadToEnd()));
                        sr.Close();
                        break;
                }
            }
            foreach (DirectoryInfo di2 in di.GetDirectories())
            {
                foreach (FileInfo fi in di2.GetFiles())
                {
                    switch (fi.Extension)
                    {
                        case ".htm":
                        case ".html":
                            StreamReader sr = new StreamReader(fi.OpenRead());
                            prj.files.Add(new Filea(fi.Name, sr.ReadToEnd()));
                            sr.Close();
                            break;
                        case ".js":
                            sr = new StreamReader(fi.OpenRead());
                            prj.files.Add(new Filea(fi.Name, sr.ReadToEnd()));
                            sr.Close();
                            break;
                        case ".ejs":
                            sr = new StreamReader(fi.OpenRead());
                            prj.files.Add(new Filea(fi.Name, sr.ReadToEnd()));
                            sr.Close();
                            break;
                        case ".prj":
                            sr = new StreamReader(fi.OpenRead());
                            //prj.files.Add(new Filea(fi.Name, sr.ReadToEnd()));
                            sr.Close();
                            break;
                    }
                }
            }
            foreach (Filea file in Project.project.files)
            {
                window.tab.Items.Add(new TabItem() {Header = file.name, Content = file.scroll});
            }
        }
        public void open()
        {
        }
    }
    class Filea
    {
        public String name;
        public String extention;
        public Text_page text_canvas;
        public TabItem item = new TabItem();
        public ScrollViewer scroll = new ScrollViewer() { HorizontalScrollBarVisibility = ScrollBarVisibility.Auto };
        public Filea(String file_name)
        {
            int n = file_name.LastIndexOf('.');
            this.name = file_name.Substring(0, n);
            n++;
            this.extention = file_name.Substring(n, file_name.Length - n);
            item.Header = new TextBlock() { Text = name };
            text_canvas = new Text_page(scroll);
            item.Content = scroll;
            scroll.Content = text_canvas;
            scroll.VerticalAlignment = VerticalAlignment.Top;
            scroll.HorizontalAlignment = HorizontalAlignment.Left;
        }
        public Filea(String file_name, String text) : this(file_name)
        {
            text_canvas.text.text = text;
        }
    }
}
class Map<type_key, type_value> : Dictionary<type_key, type_value>
{
}