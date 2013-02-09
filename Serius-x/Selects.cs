using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;

namespace Serius
{
    class Selects : StackPanel
    {
        public Select now;
        private bool selected = true;
        public StackPanel lists = new StackPanel() { Orientation = Orientation.Vertical, Width = 100};
        public TextBlock explain = new TextBlock() { Width = 100, Text = "", TextWrapping = TextWrapping.Wrap };
        public ScrollViewer scroll = new ScrollViewer() { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden };
        public Border boarder = new Border() { BorderThickness = new Thickness(0, 0, 1, 0), BorderBrush = Brushes.Gray };
        public String selected_word
        {
            get { return ""; }
        }
        public Selects()
        {
            Background = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
            Width = 200; Height = 140;
            Orientation = Orientation.Horizontal;
            Children.Add(boarder); boarder.Child = scroll; scroll.Content = lists;
            Children.Add(explain);
            this.Focusable = true;
            this.FocusVisualStyle = null;
        }
        public void show(Point p)
        {
            Canvas.SetLeft(this, p.X);
            Canvas.SetTop(this, p.Y);
            show();
            this.Focus();
        }
        public void show()
        {
            selects.Sort(compare);
            lists.Children.Clear();
            foreach (Select s1 in selects) lists.Children.Add(s1);
            if (selects.Count != 0)
            {
                visible = true;
                check_word(((Text_page)Parent).text);
            }
        }
        public bool visible
        {
            get { return Visibility.Visible == Visibility; }
            set
            {
                if (value == true)
                {
                    if (Canvas.GetLeft(this) + this.ActualWidth >= ((Canvas)this.Parent).ActualWidth) Canvas.SetLeft(this, ((Canvas)this.Parent).ActualWidth - this.ActualWidth);
                    if (Canvas.GetTop(this) + this.ActualHeight >= ((Canvas)this.Parent).ActualHeight) Canvas.SetTop(this, Canvas.GetTop(this) - this.ActualHeight - 10);
                    Visibility = Visibility.Visible;
                }
                else
                {
                    Visibility = Visibility.Collapsed;
                    lists.Children.Clear();
                }
            }
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            e.Handled = true;
            bool shift = ((Keyboard.Modifiers & ModifierKeys.Shift) > 0);
            Text_page parent = ((Text_page)Parent);
            if (Key.A <= e.Key && e.Key <= Key.Z)
            {
                char c;
                if (shift) c = (char)('A' + e.Key - Key.A);
                else c = (char)('a' + e.Key - Key.A);
                parent.text.from.input(c.ToString(), false);
                check_word(parent.text);
                
            }
            else switch (e.Key)
                {
                    case Key.Up: up();
                        break;
                    case Key.Down: down();
                        break;
                    case Key.Right:
                        visible = false;
                        break;
                    case Key.Left:
                        visible = false;
                        break;
                    case Key.Enter:
                        if (now != null) set_word(parent.text, now.str);
                        visible = false;
                        break;
                    case Key.OemMinus:
                        if (shift) visible = false;
                        else {
                            parent.text.from.input("-", false);
                        }
                        break;
                    case Key.OemBackslash:
                        if (shift)
                        {
                            parent.text.from.input("_", false);
                        }
                        else visible = false;
                        break;
                    default:
                        visible = false;
                        break;
                }
            parent.InvalidateVisual();
        }
        public void check_word(Text text)
        {
            if (selects.Count == 0) return;
            Word start, end;
            start = end = text.from.word;
            if (Word.In_call(start))
            {
                for (; ; start = start.before)
                {
                    if (Word.In_call(start.before) == false) break;
                }
                for (; ; end = end.next)
                {
                    if (Word.In_call(end.next) == false) break;
                }
                String str = "";
                for (Word now = start; ; now = now.next)
                {
                    str += now.str;
                    if (now == end) break;
                }
                foreach (Select s in selects)
                {
                    if (s.str.StartsWith(str))
                    {
                        select(s);
                        return;
                    }
                }
                select(null);
            }
            else select(selects[0]);
        }
        public void set_word(Text text, String input)
        {
            if (selected == false) return;
            Word start, end;
            start = end = text.from.word;
            if (Word.In_call(start))
            {
                for (; ; start = start.before) {
                    if (Word.In_call(start.before) == false) break;
                }
                for (; ; end = end.next)
                {
                    if (Word.In_call(end.next) == false) break;
                }
                text.from.word = start; text.from.pos = 0;
                Target to = new Target(text.from); to.word = end; to.pos = to.word.length;
                text.from.delete(to);
            }
            text.from.input(input, false);
        }
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            e.Handled = true;
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            e.Handled = true;
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
        public void up()
        {
            int n = selects.IndexOf(now);
            if (n == 0) return;
            n--;
            select(selects[n]);
        }
        public void down()
        {
            int n = selects.IndexOf(now);
            if (n == selects.Count - 1) return;
            n++;
            select(selects[n]);
        }
        public void select(Select target)
        {
            if (target == null)
            {
                selected = false;
                if (now != null) now.Background = Brushes.LightGray;
                return;
            }
            else selected = true;
            if (now != null) now.Background = new SolidColorBrush(Color.FromArgb(255, 200, 200, 200));
            now = target;
            now.Background = Brushes.Blue;
            explain.Text = now.explain;
            scroll.ScrollToVerticalOffset(18.0 * selects.IndexOf(now));
        }
        public void select_width_string(String str)
        {
            foreach (UIElement _ in lists.Children)
            {
                Select now = (Select)_;
                if (now.str.ToLower().StartsWith(str.ToLower()))
                {
                    select(now);
                    return;
                }
            }
        }
        List<Select> selects = new List<Select>();
        public void clear()
        {
            selects.Clear();
            now = null;
        }
        public void add(Select select)
        {
            selects.Add(select);
            select.parent = this;
        }
        public static int compare(Select s1, Select s2)
        {
            String str1 = s1.str, str2 = s2.str;
            for (int i = 0; ; i++) {
                if (i == str1.Length || i == str2.Length) return 0;
                else if (i >= str1.Length) return -1;
                else if (i >= str2.Length) return 1;
                char c1 = str1[i], c2 = str2[i];
                if (char.ToLower(c1) > char.ToLower(c2)) return 1;
                else if (char.ToLower(c2) > char.ToLower(c1)) return -1;
                else if (c1 > c2) return -1;
            }
        }
    }
    class Select : StackPanel
    {
        Kind_select kind;
        public String str;
        public String explain;
        public Selects parent;
        public Select(String str, String explain, Kind_select kind)
        {
            this.str = str; this.explain = explain; this.kind = kind;
            this.Height = 18.0;
            Children.Add(new TextBlock() { Text = str });
        }
        protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            parent.select(this);
        }
    }
}
