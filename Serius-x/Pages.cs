using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using form = System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Windows.Threading;
using System.Net;
using Ionic.Zip;
using Ionic.Zlib;
namespace Serius
{
    class Node_page : Canvas
    {
        public static Info info;
        Button button_mongo = new Button() { Content = "start mongodb" };
        //Button button_node_inspector = new Button() { Content = "start node-inspector" };
        Button button_node_debug = new Button() { Content = "debug node app" };
        Button button_clear_text = new Button() { Content = "clear text" };
        TextBox console = new TextBox() { IsReadOnly = true};
        TextBox console_m = new TextBox() {IsReadOnly = true};
        static string input_initial_message = "input query. this is deleted with key-input";
        TextBox input_m = new TextBox() { Width = 600, Text = input_initial_message};
        ScrollViewer scroll = new ScrollViewer() {HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalScrollBarVisibility = ScrollBarVisibility.Auto};
        ScrollViewer scroll2 = new ScrollViewer() { Width = 600, Height = 405, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        static Node_page()
        {
            info = Info.read();
        }
        public Node_page()
        {
            this.Background = System.Windows.Media.Brushes.Black;
            GlobalEvent.add(new Simple_event(node_check));
            TabControl tab = new TabControl() { Width = 600, Height = 450 };
            this.Children.Add(tab);
            scroll.Content = console;
            tab.Items.Add(new TabItem() { Header = "node", Content = scroll });
            Canvas canvas = new Canvas();
            scroll2.Content = console_m;
            canvas.Children.Add(scroll2);
            TabItem item;
            tab.Items.Add(item = new TabItem() { Header = "mongo", Content = canvas });
            item.GotFocus += item_GotFocus;
            input_m.PreviewKeyDown += input_m_KeyDown;
            //input_m.a
            Canvas.SetTop(input_m, 408);
            canvas.Children.Add(input_m);
            this.Children.Add(button_mongo);
            button_mongo.Click += button_mongo_Click;
            Canvas.SetLeft(button_mongo, 0);
            //this.Children.Add(button_node_inspector);
            //button_node_inspector.Click += button_node_inspector_Click;
            //Canvas.SetLeft(button_node_inspector, 100);
            this.Children.Add(button_node_debug);
            button_node_debug.Click += button_node_debug_Click;
            GlobalEvent.add(new Simple_event(init_mongo));
            Canvas.SetLeft(button_node_debug, 250);
            button_clear_text.Click += button_clear_text_Click;
            Canvas.SetLeft(button_clear_text, 350);
            this.Children.Add(button_clear_text);
            Canvas.SetLeft(tab, 0); Canvas.SetTop(tab, 30);

        }
        void item_GotFocus(object sender, RoutedEventArgs e)
        {
            //input_m.Focus();
        }
        int n3 = info.shell_recomends.Count;
        void input_m_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (input_m.Text == input_initial_message) input_m.Text = "";
            switch(e.Key) 
            {
                case System.Windows.Input.Key.Enter:
                    info.add_shell_recomend(input_m.Text);
                    n3 = info.shell_recomends.Count;
                    mongo_input();
                    break;
                case System.Windows.Input.Key.Down:
                    if (n3 >= info.shell_recomends.Count - 1) return;
                    else
                    {
                        n3++;
                        input_m.Text = info.shell_recomends[n3];
                    }
                    break;
                case System.Windows.Input.Key.Up:
                    if (n3 == 0) input_m.Text = "";
                    else
                    {
                        n3--;
                        input_m.Text = info.shell_recomends[n3];
                    }
                    break;
            }
        }

        void button_clear_text_Click(object sender, RoutedEventArgs e)
        {
            console.Text = "";
            console_m.Text = "";
        }
        
        public void node_check()
        {
            if (info.node_version == null)
            {
                GlobalEvent.add(new Simple_event(node_version));
            }
            else node_inspector_check();
        }
        public void node_version()
        {
            Process p = e(System.Environment.GetEnvironmentVariable("ComSpec"), "/c node -v", null, get_data, null);
        }
        String ver;
        public void get_data(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null && e.Data.StartsWith("v"))
            {
                info.node_version = e.Data;
            }
            else if (e.Data == null)
            {
                if (info.node_version == null)
                {
                    if (MessageBox.Show("node.jsをインストールしますか", "node.js installer", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        if (IntPtr.Size == 4) execute(AppDomain.CurrentDomain.BaseDirectory + "/node-v0.8.12-x86.msi", "", 1);
                        else if (IntPtr.Size == 8) execute(AppDomain.CurrentDomain.BaseDirectory + "/node-v0.8.12-x64.msi", "", 1);
                    }
                }
                GlobalEvent.add(new Simple_event(node_inspector_check));
            }
        }
        public void node_inspector_check()
        {
            if (info.node_inspector_exist == null)
            {
                e(System.Environment.GetEnvironmentVariable("ComSpec"), "/c node-inspector", null, node_inspector_get, null);
                GlobalEvent.add(new Simple_event(mongodb_check));
            }
            else mongodb_check();
        }
        public void node_inspector_get(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null && e.Data.StartsWith("info"))
            {
                info.node_inspector_exist = "true";
            }
            else if (e.Data == null) {
                if (info.node_inspector_exist == null)
                {
                    if (MessageBox.Show("node-inspectorをインストールしますか。", "node-inspector install") == MessageBoxResult.OK)
                    {
                        node_inspector_install();
                    }
                }
            }
        }
        public void node_inspector_install()
        {
            execute(System.Environment.GetEnvironmentVariable("ComSpec"), "/c npm install -g node-inspector", 1000);
        }
        public void chrome_check()
        {
            if (info.chrome_address == null)
            {
                FileInfo fi = new FileInfo(@"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe");
            head:
                if (fi.Exists)
                {
                    info.chrome_address = fi.FullName;
                    return;
                }
                fi = new FileInfo(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Google\Chrome\Application\chrome.exe");
                if (fi.Exists)
                {
                    info.chrome_address = fi.FullName;
                    return;
                }
                if (MessageBox.Show("Chromeをインストールしますか？", "Chrome install?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    execute(AppDomain.CurrentDomain.BaseDirectory + "ChromeSetup.exe", "", 1000);
                    goto head;
                }
            }
        }
        public void mongodb_check()
        {
            if (info.mongo_address == null)
            {
                DirectoryInfo di = new DirectoryInfo("c:/data");
                if (di.Exists == false) di.Create();
                di = new DirectoryInfo("c:/data/db");
                if (di.Exists == false) di.Create();
                switch(MessageBox.Show("mognodbをインストールしますか、引用しますか", "mongodb installer", MessageBoxButton.YesNoCancel)) {
                    case MessageBoxResult.Yes:
                        GlobalEvent.add( new Simple_event(down_load_mongodb) { status = Simple_event_staus.Thread });
                        GlobalEvent.add( new Simple_event(get_info_of_mongo) { status = Simple_event_staus.Loop, loop_count = -1 });
                        break;
                    case MessageBoxResult.No:
                        form.OpenFileDialog dialog = new form.OpenFileDialog();
                        dialog.Filter = "mongod.exe|mongod.exe";
                        if (dialog.ShowDialog() == form.DialogResult.OK) {
                            info.mongo_address = dialog.FileName.Substring(0, dialog.FileName.LastIndexOf('\\'));
                        }
                        break;
                }
            }
            chrome_check();
        }
        private String file_name;
        private String address2 = null;
        public void down_load_mongodb()
        {
            if (IntPtr.Size == 4) { file_name = "mongodb-win32-i386-2.2.1"; }
            else if (IntPtr.Size == 8) { file_name = "mongodb-win32-x86_64-2.2.1"; }
            else return;
            String address = null;
            address = "http://downloads.mongodb.org/win32/" + file_name + ".zip";
            WebClient client = new WebClient();
            str_m += "download start mongodb\n";
            client.DownloadFileCompleted += client_DownloadFileCompleted;
            client.DownloadProgressChanged += client_DownloadProgressChanged;
            client.DownloadFileAsync(new Uri(address), address2 = AppDomain.CurrentDomain.BaseDirectory + "mongo.zip");
        }
        int n2 = 0;
        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (n2 == e.ProgressPercentage) return;
            str_m += "downloaded" + e.ProgressPercentage + "%";
            n2 = e.ProgressPercentage;
            if (n2 % 5 == 0) str_m += "\n";
            else str_m += " ";
        }

        void client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            str_m += "download end and extract mongodb zip\n";
            ZipFile zip = new ZipFile(address2);
            zip.ExtractProgress += zip_ExtractProgress;
            zip.ExtractAll(AppDomain.CurrentDomain.BaseDirectory, ExtractExistingFileAction.OverwriteSilently);
            zip.Dispose();
            info.mongo_address = AppDomain.CurrentDomain.BaseDirectory + "/" + file_name + "/bin";
            str_m += "extracte end and mongodb setting finish\n";
        }

        void zip_ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            if (n2 == e.EntriesExtracted) return;
            if (e.EntriesTotal == 0) return;
            if (e.EntriesExtracted % 5 != 0) return;
            str_m += e.EntriesExtracted + "/" + e.EntriesTotal + "\n";
            n2 = e.EntriesExtracted;
        }
        void button_node_debug_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Process[] ps = Process.GetProcessesByName("node");
            if (ps.Length != 0)
            {
                foreach(Process p in ps) p.Kill();
            }
            GlobalEvent.add(new Simple_event(start_node_inspector));
            GlobalEvent.add(new Simple_event(start_node_debug));
        }
        public void void_function() { }

        void button_node_inspector_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            start_node_inspector();
        }
        int n = 0;
        Process p_inspector;
        public void start_node_inspector()
        {
            p_inspector = execute(System.Environment.GetEnvironmentVariable("ComSpec"), @"/c node-inspector", 10);
        }
        public Process execute(String file_name, String arguments, int loop_count)
        {
            return e(file_name, arguments, new Simple_event(get_info_of_node) { status = Simple_event_staus.Loop, loop_count = loop_count }, p_OutputDataReceived, p_OutputDataReceived);
        }
        public Process execute_mongo(String file_name, String arguments, int loop_count)
        {
            return e(file_name, arguments, new Simple_event(get_info_of_mongo) { status = Simple_event_staus.Loop, loop_count = loop_count }, pm_OutputDataReceived, pm_OutputDataReceived);
        }
        public Process e(String file_name, String arguments, Simple_event checker, DataReceivedEventHandler output, DataReceivedEventHandler error)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            Process p = new Process();
            psi.FileName = file_name;
            psi.RedirectStandardInput = true;
            if (output != null)
            {
                psi.RedirectStandardOutput = true;
                psi.StandardOutputEncoding = Encoding.UTF8;
                p.OutputDataReceived += output;
            }
            else psi.RedirectStandardOutput = false;
            if (error != null)
            {
                psi.RedirectStandardError = true;
                psi.StandardErrorEncoding = Encoding.UTF8;
                p.ErrorDataReceived += error;
            }
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.Arguments = arguments;
            p.StartInfo = psi;
            p.Start();
            if (output != null) p.BeginOutputReadLine();
            if (error != null) p.BeginErrorReadLine();
            GlobalEvent.add(checker);
            return p;
        }
        void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            str += e.Data + "\n";
        }
        void pm_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            str_m += e.Data + "\n";
        }
        Process p_node;
        public void start_node_debug()
        {
            p_node = execute(System.Environment.GetEnvironmentVariable("ComSpec"), @"/c cd " + info.latest_project +" & node --debug server.js", -1);//, new Simple_event(get_info_node) { status = Simple_event_staus.Loop, loop_count = -1 }, null, p_OutputDataReceived);
            addresses.Add("http://localhost:8080/debug?port=5858");
            GlobalEvent.add(new Simple_event(open_browse));
            addresses.Add("http://localhost:5963");
            GlobalEvent.add(new Simple_event(open_browse));
        }
        public List<String> addresses = new List<string>();
        public void open_browse()
        {
            execute(info.chrome_address, addresses[0], 1);
            addresses.RemoveAt(0);
        }
        void button_mongo_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (info.mongo_address == null)
            {
                mongodb_check();
                return;
            }
            stop_mongo(true);
            if ((String)button_mongo.Content == "start mongodb")
            {
                GlobalEvent.add(new Simple_event(start_mongo));
                button_mongo.Content = "stop mongodb";
            }
            else
            {
                button_mongo.Content = "start mongodb";
            }
        }
        Process p_mongo;
        public void init_mongo()
        {
            Process[] ps = Process.GetProcessesByName("mongod");
            if (ps.Length == 0)
            {
                if (info.mongo_address == null) return;
                GlobalEvent.add(new Simple_event(start_mongo));
                button_mongo.Content = "stop mongodb";
            }
            GlobalEvent.add(new Simple_event(start_mongoq));
        }
        public void stop_mongo(bool wait)
        {
            
            Process[] ps = Process.GetProcessesByName("mongod");
            if (ps.Length != 0)
            {
                Process[] ps2 = Process.GetProcessesByName("mongo");
                if (ps2.Length != 0)
                {
                    foreach(Process p2 in ps2) p2.Kill();
                }
                Process p = execute(AppDomain.CurrentDomain.BaseDirectory + "stop_mongo.exe", info.mongo_address, 10);
                if (wait) p.WaitForExit();
            }
        }
        public void start_mongo()
        {
            execute(info.mongo_address + "/mongod.exe", "", -1);
            //psi.Arguments = "--repair";
        }
        public void start_mongoq()
        {
            if (info.mongo_address != null) p_mongo = execute_mongo(info.mongo_address + "/mongo.exe", "", -1);
        }
        public void mongo_input()
        {
            if (p_mongo != null)
            {
                if (p_mongo.HasExited == true)
                {
                    str_m += "mongo shell has exited\n";
                    return;
                }
                str_m += ">>" + input_m.Text + "\n";
                StreamWriter sw = new StreamWriter(p_mongo.StandardInput.BaseStream, Encoding.UTF8);
                sw.WriteLine(input_m.Text);
                sw.Flush();
                input_m.Text = "";
            }
        }
        String str_m = "";
        public void get_info_of_mongo()
        {
            console_m.Text += str_m;
            if (str_m.IndexOf('\n') != - 1) GlobalEvent.add(new Simple_event(scroll2.ScrollToEnd));
            str_m = "";
        }
        String str = "";
        public void get_info_of_node()
        {
            if (console.Text.Length >= 65536) console.Text = "";
            console.Text += str;
            //if (str.IndexOf('\n') != -1) GlobalEvent.add(new Simple_event(scroll.ScrollToEnd));
            str = "";
        }
    }
    enum Simple_event_staus
    {
        Disposable, Loop, Thread
    }
    class Simple_event
    {
        public Simple_event_staus status = Simple_event_staus.Disposable;
        public sev run;
        public int loop_count = 1;
        public Simple_event(sev run)
        {
            this.run = run;
        }
    }
    delegate void sev();
    class GlobalEvent
    {
        public static DispatcherTimer timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, 300)};
        public static List<Simple_event> simple_events = new List<Simple_event>();
        static GlobalEvent()
        {
            timer.Tick += timer_Tick;
        }
        static Object o = new Object();
        static void timer_Tick(object sender, EventArgs e)
        {
            if (simple_events.Count == 0) return;
            Simple_event eve = simple_events[0];
            simple_events.RemoveAt(0);
            switch (eve.status)
            {
                case Simple_event_staus.Disposable:
                    eve.run();
                    break;
                case Simple_event_staus.Loop:
                    eve.run();
                    eve.loop_count--;
                    if (eve.loop_count != 0) simple_events.Add(eve);
                    break;
                case Simple_event_staus.Thread:
                    new Thread(new ThreadStart(eve.run)).Start();
                    break;
            }
            if (simple_events.Count == 0) timer.Stop();
        }
        public static void add(Simple_event eve)
        {
            if (eve == null || eve.run == null) return;
            simple_events.Add(eve);
            if (timer.IsEnabled == false)
            {
                timer.Tick += timer_Tick;
                timer.Start();
            }

        }
    }
}
