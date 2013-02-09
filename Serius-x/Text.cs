using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
namespace Serius
{
    class TextContextMenu : ContextMenu
    {
        public Text_page canvas;
        public TextContextMenu(Text_page canvas)
        {
            this.canvas = canvas;
            MenuItem menu_cut = new MenuItem() { Header = "Cut"};
            menu_cut.Click += menu_cut_Click;
            Items.Add(menu_cut);
            MenuItem menu_paste = new MenuItem() { Header = "Paste" };
            menu_paste.Click += menu_paste_Click;
            Items.Add(menu_paste);
        }

        void menu_paste_Click(object sender, RoutedEventArgs e)
        {
            canvas.text.input(Clipboard.GetData(DataFormats.Text).ToString());
            canvas.InvalidateVisual();
        }

        void menu_cut_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetData(DataFormats.Text, canvas.text.cut());
            canvas.InvalidateVisual();
        }
    }
    class Text_page : Canvas{
        public ScrollViewer scroll;
        public Text text;
        public Selects selects = new Selects();
        public Text_page(ScrollViewer scroll) {
            this.scroll = scroll;
            Background = Brushes.White;
            text = new Text(this);
            this.Focusable = true;
            this.ContextMenu = new TextContextMenu(this);
            Children.Add(selects = new Selects() { Visibility = System.Windows.Visibility.Hidden });
            this.FocusVisualStyle = null;
        }
        protected override void OnRender(DrawingContext dc)
        {
 	        base.OnRender(dc);
            text.draw(dc);
        }
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            if (selects.visible == true) selects.Focus();
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            Key key = e.Key;
            bool shift = ((Keyboard.Modifiers & ModifierKeys.Shift) > 0);
            if (Key.A <= key && key <= Key.Z) {
                if (shift) text.input("" + (char)('A' + key - Key.A));
                else text.input("" + (char)('a' + key - Key.A));
            }
            else if (Key.D0 <= key && key <= Key.D9) {
                if (shift) {
                    switch (key) {
                        case Key.D0:
                            break;
                        case Key.D1:
                            text.input("!");
                            break;
                        case Key.D2:
                            text.input("\"");
                            break;
                        case Key.D3:
                            text.input("#");
                            break;
                        case Key.D4:
                            text.input("$");
                            break;
                        case Key.D5:
                            text.input("%");
                            break;
                        case Key.D6:
                            text.input("&");
                            break;
                        case Key.D7:
                            text.input("'");
                            break;
                        case Key.D8:
                            text.input("(");
                            break;
                        case Key.D9:
                            text.input(")");
                            break;
                    }
                }
                else {
                    text.input(((char)('0' + key - Key.D0)).ToString());
                }
            }
            else {
                switch (key) {
                    case Key.Space:
                        text.input(" ");
                        break;
                    case Key.Enter:
                        if (shift) { }
                        else
                        {
                            if (selects.visible) { }
                            text.enter();
                        }
                        break;
                    case Key.Tab:
                        e.Handled = true;
                        if (shift) { }
                        else
                        {
                            if (selects.visible)
                            {
                                goto finish;
                            }
                            text.tab();
                        }
                        break;
                    case Key.Delete:
                        if (shift) { }
                        else text.delete();
                        break;
                    case Key.Back:
                        if (shift) { }
                        else
                        {
                            text.back_space();
                            if (selects.visible)
                            {
                                text.back_space();
                                goto finish;
                            }
                        }
                        break;
                    case Key.Left:
                        if (shift) text.shift_horizontal_move(-1);
                        else text.horizontal_move(-1);
                        e.Handled = true;
                        break;
                    case Key.Right:
                        if (shift) text.shift_horizontal_move(1);
                        else text.horizontal_move(1);
                        e.Handled = true;
                        break;
                    case Key.Up:
                        e.Handled = true;
                        if (shift) text.shift_vertical_move(-1);
                        else text.vertical_move(-1);
                        break;
                    case Key.Down:
                        if (shift) text.shift_vertical_move(1);
                        else text.vertical_move(1);
                        e.Handled = true;
                        break;
                    case Key.OemMinus:
                        if (shift) text.input("=");
                        else text.input("-");
                        break;
                    case Key.OemQuotes:
                        if (shift) text.input("^");
                        else text.input("~");
                        break;
                    case Key.Oem5:
                        if (shift) text.input("|");
                        else text.input("\\");
                        break;
                    case Key.Oem3:
                        if (shift) text.input("`");
                        else text.input("@");
                        break;
                    case Key.OemOpenBrackets:
                        if (shift) text.input("{");
                        else text.input("[");
                        break;
                    case Key.OemPlus:
                        if (shift) text.input("+");
                        else text.input(";");
                        break;
                    case Key.Oem1:
                        if (shift) text.input("*");
                        else text.input(":");
                        break;
                    case Key.Oem6:
                        if (shift) text.input("}");
                        else text.input("]");
                        break;
                    case Key.OemComma:
                        if (shift) text.input("<");
                        else text.input(",");
                        break;
                    case Key.OemPeriod:
                        if (shift) text.input(">");
                        else text.input(".");
                        break;
                    case Key.OemQuestion:
                        if (shift) text.input("?");
                        else text.input("/");
                        break;
                    case Key.OemBackslash:
                        if (shift) text.input("_");
                        else text.input("\\");
                        break;
                }
                if (selects.visible) selects.visible = false;
            }
          finish:
            this.InvalidateVisual();
        }
        bool clicked;
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            clicked = true;
            base.OnMouseDown(e);
            if (selects.visible) selects.visible = false;
            this.Focus();
            Point p = e.GetPosition(this);
            if (e.ChangedButton == MouseButton.Left) {
                if (e.ClickCount == 2) {
                }
                else {
                    text.click(p);
                    InvalidateVisual();
                }
            }
            else if (e.ChangedButton == MouseButton.Right) {
            }
            e.Handled = true;
            //MessageBox.Show(Clipboard.GetData(DataFormats.Text).ToString());
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            this.Focus();
            Point p = e.GetPosition(this);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (selects.visible) selects.visible = false;
                text.shift_click(p);
                InvalidateVisual();
            }
            base.OnMouseMove(e);
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            if (selects.visible) selects.visible = false;
            Point p = e.GetPosition(this);
            if (e.ChangedButton == MouseButton.Left) {
            }
            else if (e.ChangedButton == MouseButton.Right) {
            }
            /*Point p2 = text.from.get_position();
            Canvas.SetLeft(selects, p2.X);
            Canvas.SetTop(selects, p2.Y);
            selects.Visibility = System.Windows.Visibility.Visible;*/
        }
    }
    enum Kind_input
    {
        Input, Delete, Backspace, Auto_indent, Replace, Refercter, Create
    }
    class Input
    {
        public int pos;
        public Kind_input kind;
        public String text;
        public String replace;
        public int count;
        public long time;
        public Input()
        {
            time = DateTime.Now.Ticks;
        }
        public Input input(int pos)
        {
            kind = Kind_input.Input;
            return this;
        }
        public Input delete(int pos)
        {
            kind = Kind_input.Delete;
            return this;
        }
    }
    class Text
    {
        public Text_page canvas;
        public Word start;
        public int word_count;
        public Target from, to;
        public Text(Text_page canvas)
        {
            this.canvas = canvas;
            start = new Word(this);
            from = new Target(0, this); to = null;
        }
        public String text
        {
            get
            {
                String ret = "";
                for (Word now = start.next; now.kind != Kind_word.End; now = now.next) ret += now.str;
                return ret;
            }
            set
            {
                from.input(value, false);
                from.compile();
                from = new Target(0, this);
            }
        }
        public void draw(DrawingContext dc) {
            double x = 0, y = 0;
            Target begin, end;
            if (to == null) {
                begin = from; end = from;
            }
            else if (from.word == to.word) {
                if (from.pos <= to.pos) {
                    begin = from; end = to;
                }
                else {
                    begin = to; end = from;
                }
            }
            else if (from.word.order < to.word.order) {
                begin = from; end = to;
            }
            else {
                begin = to; end = from;
            }
            bool selected = false;
            if (begin.word == start) {
                dc.DrawRectangle(Brushes.Gray, null, new Rect(new Point(0, 0), new Size(1, start.next.format.Height)));
                if (begin.word == end.word) { }
                else selected = true;
            }
            double width = 0;
            for (Word now = start.next; ; ) {
                if (now == begin.word) {
                    if (now.kind == Kind_word.Break) {
                        if (begin.word == end.word) {
                            if (begin.pos == 0) dc.DrawRectangle(Brushes.Gray, null, new Rect(new Point(x, y), new Size(1, now.format.Height)));
                            if (end.pos == 1) dc.DrawRectangle(Brushes.Gray, null, new Rect(new Point(0, y + now.format.Height), new Size(1, now.format.Height)));
                        }
                        else {
                            if (begin.pos == 0) dc.DrawRectangle(Brushes.Gray, null, new Rect(new Point(x, y), new Size(1, now.format.Height)));
                            dc.DrawRectangle(Brushes.Gray, null, new Rect(new Point(0, y + now.format.Height), new Size(1, now.format.Height)));
                            selected = true;
                        }
                    }
                    else if (begin.word == end.word) dc.DrawRectangle(Brushes.Gray, null, now.get_rect(begin.pos, end.pos, x, y));
                    else {
                        dc.DrawRectangle(Brushes.Gray, null, now.get_rect(begin.pos, now.length, x, y));
                        selected = true;
                    }
                }
                else if (selected) {
                    if (now == end.word) {
                        if (now.kind == Kind_word.Break) {
                            dc.DrawRectangle(Brushes.Gray, null, new Rect(new Point(x, y), new Size(1, now.format.Height)));
                            if (end.pos == 1) dc.DrawRectangle(Brushes.Gray, null, new Rect(new Point(0, y + now.format.Height), new Size(1, now.format.Height)));
                        }
                        else dc.DrawRectangle(Brushes.Gray, null, now.get_rect(0, end.pos, x, y));
                        selected = false;
                    }
                    else dc.DrawRectangle(Brushes.Gray, null, new Rect(new Point(x, y), new Size(now.format.WidthIncludingTrailingWhitespace + 1, now.format.Height)));
                }
                if (now.kind == Kind_word.End || now.kind == Kind_word.Break)
                {
                    if (x > width) width = x;
                    y += now.format.Height;
                    if (now.kind == Kind_word.End)
                    {
                        if (width > canvas.scroll.ActualWidth) canvas.Width = width;
                        else canvas.Width = canvas.scroll.ActualWidth;
                        if (y > canvas.scroll.ActualHeight) canvas.Height = y;
                        else canvas.Height = canvas.scroll.ActualHeight;
                        break;
                    }
                    else if (now.kind == Kind_word.Break)
                    {
                        x = 0;
                    }
                }
                dc.DrawText(now.format, new Point(x, y));
                x += now.format.WidthIncludingTrailingWhitespace;
                now = now.next; 
            }
        }
        public void input(String text)
        {
            if (to != null) {
                from.delete(to);
                to = null;
                return;
            }
            from.input(text, true);
        }
        public void click(Point p)
        {
            if (to != null) to = null;
            from.click(p.X, p.Y);
        }
        public void shift_click(Point p)
        {
            if (to == null) to = new Target(from);
            to.click(p.X, p.Y);
        }
        internal void enter()
        {
            if (to != null) {
                from.delete(to);
                to = null;
                return;
            }
            from.input("\n", false);
        }

        internal void tab()
        {
            if (to != null) {
                from.delete(to);
                to = null;
                return;
            }
            from.input("  ", false);
        }
        public String cut()
        {
            if (to == null) return "";
            String ret = from.delete(to);
            to = null;
            return ret;
        }
        internal void delete()
        {
            if (canvas.selects.visible) canvas.selects.visible = false;
            if (to != null) {
                from.delete(to);
                to = null;
                return;
            }
            from.delete();
        }

        internal void back_space()
        {
            if (to != null) {
                from.delete(to);
                to = null;
                return;
            }
            from.backspace();
        }
        public void horizontal_move(int p)
        {
            if (canvas.selects.visible) canvas.selects.visible = false;
            if (to != null) {
                to = null;
                return;
            }
            from.move_horizontal(p);
        }
        public void shift_horizontal_move(int p)
        {
            if (canvas.selects.visible) canvas.selects.visible = false;
            if (to == null) to = new Target(from);
            to.move_horizontal(p);
        }
        public void vertical_move(int p)
        {
            if (canvas.selects.visible) canvas.selects.visible = false;
            if (to != null) {
                to = null;
                return;
            }
            from.move_vertical(p);
        }
        public void shift_vertical_move(int p)
        {
            if (canvas.selects.visible) canvas.selects.visible = false;
            if (to == null) to = new Target(from);
            to.move_vertical(p);
        }
    }
    class Target
    {
        public Text text;
        int x;
        public Word word;
        public int pos;
        public int count;
        public Compile compile;
        public Target(int count, Text text)
        {
            this.text = text;
            word = text.start;
            pos = count + 1;
            this.count = count;
            move(true);
            compile = html_compile;
        }
        public Target(Target origin)
        {
            word = origin.word;
            pos = origin.pos;
            text = origin.text;
            compile = origin.compile;
        }
        public void input(String text, bool call_select)
        {
            if (pos == 0) word = word.before;
            else if (pos < word.length) {
                word.split(pos);
            }
            List<Word> new_words = split(text);
            int i = 0;
            if ((word.kind == Kind_word.Letter || word.kind == Kind_word.Number || word.kind == Kind_word.Space || word.kind == Kind_word.MultiByte) && word.kind == new_words[0].kind) {
                word.Str += new_words[0].str;
                i = 1;
            }
            for ( ; i < new_words.Count; i++) {
                word.connect(new_words[i]);
                word = new_words[i];
            }
            pos = word.length;
            if ((word.kind == Kind_word.Letter || word.kind == Kind_word.Number || word.kind == Kind_word.Space || word.kind == Kind_word.MultiByte) && word.kind == word.next.kind)
            {
                word.Str += word.next.str;
                word.next.remove();
            }
            else
            {
              //now programing
            }
            if (call_select)select();
            count += text.Length;
        }
        Compiler compiler;
        public delegate void Compile();
        public void html_compile()
        {
            compiler = new Compiler(text.start);
            try
            {
                compiler.html_compile();
            }
            catch (Exception_end) { }
            catch (Exception_error) { }
            switch (word.type)
            {
                case Type_value.Tag_name:

                    foreach (Tag_element tag in Tag_element.Base_tags[word.item.name].tags)
                    {
                        text.canvas.selects.add(new Select(tag.name, tag.explain, Kind_select.Tag));
                    }
                    break;
                case Type_value.Tag_attribute:
                    foreach (Tag_attribute attribute in Tag_element.Base_tags[word.item.name].attributes)
                    {
                        text.canvas.selects.add(new Select(attribute.name, attribute.explain, Kind_select.Tag_attribute));
                    }
                    break;
                case Type_value.Tag_attribute_value:
                    foreach (Tag_value value in Tag_element.Base_tag_attributes[word.item.name].values)
                    {
                        text.canvas.selects.add(new Select(value.name, value.explain, Kind_select.Tag_value));
                    }
                    break;
                case Type_value.Tag_close_name:
                    text.canvas.selects.add(new Select(word.item.name, "", Kind_select.Tag));
                    break;
                default:
                    css_check(text, compiler);
                    break;
            }
            text.canvas.selects.show(get_position());
        }
        public void css_compile()
        {
            try
            {
                compiler = new Compiler(text.start);
                compiler.css_comile(Kind_word.End);
            }
            catch (Exception_end) { }
            catch (Exception_error) { }
            css_check(text, compiler);
            text.canvas.selects.show(get_position());
        }
        public void css_check(Text text, Compiler compiler)
        {
            switch (word.type)
            {
                case Type_value.Style_attribute_tag:
                    foreach (Tag_element element in Tag_element.Base_tags.Values)
                    {
                        text.canvas.selects.add(new Select(element.name, "", Kind_select.Tag));
                    }
                    break;
                case Type_value.Style_attribute_id:
                    foreach (Tag_element element in compiler.ids.Values)
                    {
                        text.canvas.selects.add(new Select(element.id, "", Kind_select.Id));
                    }
                    break;
                case Type_value.Style_attribute_name:
                    foreach (Tag_element element in compiler.names.Values)
                    {
                        text.canvas.selects.add(new Select(element.nm, "", Kind_select.Name));
                    }
                    break;
                case Type_value.Style_attribute_class:
                    foreach (Tag_element element in compiler.classes.Values)
                    {
                        text.canvas.selects.add(new Select(element.cls, "", Kind_select.Css_class));
                    }
                    break;
                case Type_value.Style_attribute_att:
                    foreach (Tag_attribute att in Tag_element.Base_tags[word.item.name].attributes)
                    {
                        text.canvas.selects.add(new Select(att.name, "", Kind_select.Tag_attribute));
                    }
                    break;
                case Type_value.Style_tag_attribute_value:
                    foreach (Tag_value value in Tag_element.Base_tag_attributes[word.item.name].values) {
                        text.canvas.selects.add(new Select(value.name, "", Kind_select.Tag_value));
                    }
                    break;
                case Type_value.Style_attribute_value:
                    foreach(Css_attribute att in Css_attribute.attributes.Values) {
                        text.canvas.selects.add(new Select(att.name, "", Kind_select.Style_value));
                    }
                    break;
            }
        }
        public virtual void select()
        {
            if (word.kind == Kind_word.Letter)
            {
                text.canvas.selects.clear();
                compile();
            }
            else if (word.kind == Kind_word.Minus) {
            }
            else if (word.kind == Kind_word.Dot) {
            }
            else if (word.kind == Kind_word.Compare_Left) {
                if (word.type == Type_value.Tag_begin)
                {
                    text.canvas.selects.add(new Select("html", "開始タグ", Kind_select.Tag));
                }
            }
            else if (word.kind == Kind_word.Slash) {
            }
            else if (word.kind == Kind_word.Sharp) {
            }
        }
        public Point get_position()
        {
            Point ret = new Point(0, 0);
            if (word.kind == Kind_word.Break) {
                if (pos == 0) {
                    word = word.before;
                    pos = word.length;
                }
            }
            for (Line now = text.start.line; ; now = now.next) {
                ret.Y += now.start.next.format.Height;
                if (now == word.line) {
                    if (word.kind == Kind_word.Break || word.kind == Kind_word.Start) return ret;
                    for (Word w = now.start.next; w != word; w = w.next) {
                        ret.X += w.format.WidthIncludingTrailingWhitespace;
                    }
                    return ret;
                }
            }
        }
        public List<Word> split(String text)
        {
            List<Word> words = new List<Word>();
            for (int i = 0; i < text.Length;) {
                char c = text[i];
                if (('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z') || c == '_') {
                    for (int j = i + 1; ; j++) {
                        char c2;
                        if (j >= text.Length || !(('a' <= (c2 = text[j]) &&  c2 <= 'z') || ('A' <= c2 && c2 <= 'Z') || ('0' <= c2 && c2 <= '9') || c2 == '_')) {
                            words.Add(new Word(text.Substring(i, j - i), Kind_word.Letter));
                            i = j;
                            break;
                        }
                    }
                    continue;
                }
                else if ('0' <= c && c <= '9')
                {
                    for (int j = i + 1; ; j++)
                    {
                        char c2;
                        if (j >= text.Length || !(('0' <= (c2 = text[j]) && c2 <= '9')))
                        {
                            words.Add(new Word(text.Substring(i, j - i), Kind_word.Number));
                            i = j;
                            break;
                        }
                    }
                    continue;
                }
                else if (256 <= c)
                {
                    for (int j = i + 1; ; j++)
                    {
                        if (j >= text.Length || text[j] < 256)
                        {
                            words.Add(new Word(text.Substring(i, j - i), Kind_word.MultiByte));
                            i = j;
                            break;
                        }
                    }
                    continue;
                }
                else
                {
                    switch (c)
                    {
                        case '\r':
                            break;
                        case '\n':
                            words.Add(new Word(c, Kind_word.Break));
                            break;
                        case ' ':
                            for (int j = i + 1; ; j++)
                            {
                                if (j >= text.Length || text[j] != ' ')
                                {
                                    words.Add(new Word(text.Substring(i, j - i), Kind_word.Space));
                                    i = j;
                                    break;
                                }
                            }
                            continue;
                        case '+':
                            words.Add(new Word(c, Kind_word.Plus));
                            break;
                        case '-':
                            words.Add(new Word(c, Kind_word.Minus));
                            break;
                        case '&':
                            words.Add(new Word(c, Kind_word.And));
                            break;
                        case '%':
                            words.Add(new Word(c, Kind_word.Percent));
                            break;
                        case '|':
                            words.Add(new Word(c, Kind_word.Or));
                            break;
                        case '!':
                            words.Add(new Word(c, Kind_word.Not));
                            break;
                        case '*':
                            words.Add(new Word(c, Kind_word.Astarisk));
                            break;
                        case '/':
                            words.Add(new Word(c, Kind_word.Slash));
                            break;
                        case '<':
                            words.Add(new Word(c, Kind_word.Compare_Left));
                            break;
                        case '>':
                            words.Add(new Word(c, Kind_word.Compare_Right));
                            break;
                        case '.':
                            words.Add(new Word(c, Kind_word.Dot));
                            break;
                        case ':':
                            words.Add(new Word(c, Kind_word.Colon));
                            break;
                        case '#':
                            words.Add(new Word(c, Kind_word.Sharp));
                            break;
                        case '=':
                            words.Add(new Word(c, Kind_word.Equal));
                            break;
                        case '\'':
                            words.Add(new Word(c, Kind_word.Single_quote));
                            break;
                        case '"':
                            words.Add(new Word(c, Kind_word.Double_quote));
                            break;
                        case '(':
                            words.Add(new Word(c, Kind_word.Paren_left));
                            break;
                        case ')':
                            words.Add(new Word(c, Kind_word.Paren_right));
                            break;
                        case '[':
                            words.Add(new Word(c, Kind_word.Bracket_left));
                            break;
                        case ']':
                            words.Add(new Word(c, Kind_word.Bracket_right));
                            break;
                        case '{':
                            words.Add(new Word(c, Kind_word.Brace_left));
                            break;
                        case '}':
                            words.Add(new Word(c, Kind_word.Brace_right));
                            break;
                        case '@':
                            words.Add(new Word(c, Kind_word.Atmark));
                            break;
                        case ',':
                            words.Add(new Word(c, Kind_word.Comma));
                            break;
                        case ';':
                            words.Add(new Word(c, Kind_word.Semicolon));
                            break;
                        case '\\':
                            words.Add(new Word(c, Kind_word.Yen));
                            break;
                        default:
                            words.Add(new Word("???", Kind_word.Symbol));
                            break;
                    }
                }
                i++;
            }
            return words;
        }
        public void click(double x, double y)
        {
            Line now = text.start.line;
            for (; ; ) {
                y -= now.start.next.format.Height;
                if (y < 0) break;
                now = now.next;
                if (now.start.kind == Kind_word.Start) {
                    now = now.before;
                    break;
                }
            }
            for (word = now.start.next; ; word = word.next) {
                if (word.kind == Kind_word.Break || word.kind == Kind_word.End) {
                    pos = 0;
                    break;
                }
                if (x < word.format.WidthIncludingTrailingWhitespace) {
                    pos = word.get_pos(x);
                    break;
                }
                x -= word.format.WidthIncludingTrailingWhitespace;
            }
        }
        public void delete()
        {
            if (pos == word.length) {
                word = word.next;
                pos = 0;
            }
            if (word.kind == Kind_word.End) return;
            del();
        }
        public void backspace()
        {
            if (pos == 0) {
                word = word.before;
                pos = word.length;
            }
            if (word.kind == Kind_word.Start) return;
            pos--;
            del();
        }
        public void del()
        {
            if (pos == 0 && word.length == 1) {
                Word removed_word = word;
                word = word.before;
                pos = word.length;
                removed_word.remove();
                if ((word.kind == Kind_word.Letter || word.kind == Kind_word.Space || word.kind == Kind_word.MultiByte) && word.next.kind == word.kind) {
                    word.Str += word.next.str;
                    word.next.remove();
                }
            }
            else {
                word.Str = word.Str.Remove(pos, 1);
            }
        }
        public String delete(Target to)
        {
            String ret = "";
            Target start, end;
            if (word == to.word) {
                if (pos == to.pos) return "";
                else if (pos < to.pos) {
                    ret += word.Str.Substring(pos, to.pos - pos);
                    word.Str = word.Str.Remove(pos, to.pos - pos);
                }
                else {
                    ret += word.Str.Substring(to.pos, pos - to.pos);
                    word.Str = word.Str.Remove(to.pos, pos - to.pos);
                    pos = to.pos;
                }
                if (word.Str.Length == 0) {
                    word.remove();
                    word = word.before;
                    pos = word.length;
                }
                goto finish;
            }
            else if (word.order < to.word.order) {
                start = this; end = to;
            }
            else {
                start = to; end = this;
            }
            ret += start.word.Str.Substring(start.pos);
            start.word.Str = start.word.Str.Substring(0, start.pos);
            if (start.word.Str.Length == 0) {
                start.word.remove();
                if (start == this) {
                    word = word.before;
                    pos = word.length;
                }
            }
            for (Word now = start.word.next; ; ) {
                if (now == end.word) {
                    ret += now.Str.Substring(0, end.pos);
                    end.word.Str = now.Str.Substring(end.pos, now.length - end.pos);
                    if (now.Str.Length == 0) {
                        if (end == this) {
                            word = now.next;
                            pos = 0;
                        }
                        now.remove();
                    }
                    else if (end == this) pos = 0;
                    break;
                }
                ret += now.Str;
                now.remove();
                now = now.next;
            }
        finish:
            if (word.kind == Kind_word.Letter || word.kind == Kind_word.Space || word.kind == Kind_word.MultiByte) {
                if (pos == 0 && word.kind == word.before.kind) {
                    word = word.before;
                    pos = word.length;
                    word.Str += word.next.Str;
                    word.next.remove();
                }
                else if (pos == word.length && word.kind == word.next.kind) {
                    word.Str += word.next.Str;
                    word.next.remove();
                }
            }
        return ret;
        }
        public void move_horizontal(int count)
        {
            pos += count;
            move(true);
        }
        public void move_vertical(int count)
        {
            Line line = word.line;
            for (Word now = line.start; now != word; now = now.next) {
                pos += now.length;
            }
            if (count > 0) {
                for (int i = 0; i < count; i++) {
                    line = line.next;
                    if (line.start.kind == Kind_word.Start) {
                        word = line.start.before; pos = 0;
                        line = line.before;
                        return;
                    }
                }
            }
            else if (count < 0) {
                for (int i = 0; i < -count; i++) {
                    if (line.start.kind == Kind_word.Start) {
                        word = line.start; pos = 1;
                        return;
                    }
                    line = line.before;
                }
            }
            word = line.start;
            move(false);
        }
        public void move(bool can_over)
        {
        loop:
            if (pos >= word.length) {
                move_next(can_over);
            }
            else if (pos < 0) {
                move_back();
            }
        }
        public void move_next(bool can_over)
        {
            if (word.kind == Kind_word.End) {
                pos = 0;
                return;
            }
        loop:
            if (pos > word.length) {
                pos -= word.length;
                word = word.next;
                if (word.kind == Kind_word.Break) {
                    if (can_over == false) {
                        pos = 0;
                        return;
                    }
                }
                else if (word.kind == Kind_word.End) {
                    pos = 0;
                    return;
                }
                goto loop;
            }
        }
        public void move_back()
        {
            if (word.kind == Kind_word.Start) {
                pos = 1; return;
            }
        loop:
            if (pos < 0) {
                word = word.before;
                pos += word.length;
                goto loop;
            }
        }
    }
    enum Kind_word
    {
        None, Start, End, Letter, Plus, Minus, Semicolon, Slash,
        Compare_Left, Compare_Right, Question, Break, Space, Symbol,
        MultiByte,
        Dot,
        Sharp,
        And,
        Brace_left,
        Brace_right,
        Colon,
        Equal,
        Double_quote,
        Yen,
        Quote,
        Single_quote,
        Bracket_left,
        Not,
        Paren_left,
        Astarisk,
        Comma,
        Number,
        Paren_right,
        Bracket_right,
        Or,
        Nyoro,
        Atmark,
        Percent,
    }
    enum Type_value
    {
        None,
        Tag_begin,
        Call_start, Tag_name, Tag_attribute, Tag_close_name, Tag_attribute_value, Style_attribute_id, Style_attribute_tag, Style_attribute_name, Style_attribute_value,
        Style_attribute_class,
        Style_attribute_option,
        Style_attribute_att,
        Style_tag_attribute_value, Call_end,
        Style_value

    }
    class Line
    {
        public Line next, before;
        public Word start;
        public Line(Word start)
        {
            next = before = this;
            this.start = start;
        }
        public void connect(Line item)
        {
            item.before = this;
            item.next = next;
            next.before = item;
            this.next = item;
        }
        public void remove()
        {
            if (next == this) return;
            before.next = next;
            next.before = before;
            next = before = this;
        }
    }
    class Word
    {
        public Text text;
        public String str;
        public Word next;
        public Word before;
        public Line line;
        public Kind_word kind;
        public FormattedText format;
        public int order;
        public Type_value type;
        public Item item;
        public static Typeface FONT_FAMILY = new Typeface("MS Gothic");
        public Word(Text text)
        {
            this.text = text;
            next = before = this;
            line = new Line(this);
            this.str = "\0";
            this.order = 0;
            this.kind = Kind_word.Start;
            text.word_count++;
            this.connect(new Word("\0", Kind_word.End));
        }
        public Brush color
        {
            set
            {
                format.SetForegroundBrush(value);
            }
        }
        public Word(String str, Kind_word kind)
        {
            this.str = str;
            this.kind = kind;
            this.format = new FormattedText(str, new System.Globalization.CultureInfo("ja"), FlowDirection.LeftToRight, FONT_FAMILY, 15.0, Brushes.Black);
            next = before = this;
        }
        public String Str
        {
            get
            {
                return str;
            }
            set
            {
                str = value;
                this.format = new FormattedText(str, new System.Globalization.CultureInfo("ja"), FlowDirection.LeftToRight, FONT_FAMILY, 15.0, Brushes.Black);
            }

        }
        public Word(char c, Kind_word kind) : this(c.ToString(), kind) { }
        public void connect(Word item)
        {
            item.before = this;
            item.next = next;
            next.before = item;
            this.next = item;
            //
            item.text = this.text;
            item.line = this.line;
            text.word_count++;
            if (item.next.order - this.order <= 1) {
                Word now = text.start;
                if (now == null) now = this;
                int n = Int32.MaxValue / text.word_count;
                for (int i = 0; i < text.word_count; i++) {
                    now.order = n * i;
                    now = now.next;
                }
            }
            else item.order = (this.order + item.next.order) / 2;
            if (item.kind == Kind_word.Break) {
                Line new_line = new Line(item);
                line.connect(new_line);
                for(Word now = item; ; ) {
                    now.line = new_line;
                    now = now.next;
                    if (now.kind == Kind_word.Break || now.kind == Kind_word.End) break;
                }
            }
        }
        public void split(int pos)
        {
            this.connect(new Word(Str.Substring(pos), Kind_word.Letter));
            Str = Str.Substring(0, pos);
        }
        public void remove()
        {
            if (next == this) return;
            before.next = next;
            next.before = before;
            text.word_count--;
            text = null;
        }
        public void add(int pos, String text)
        {
        }
        public void draw(Drawing dc, int x, int y)
        {
        }
        public Word next_over_space
        {
            get
            {
                Word now = this;
                for (; ; ) {
                    now = now.next;
                    if (now.kind == Kind_word.Space) continue;
                    break;
                }
                return now;
            }
        }
        public Word before_over_space
        {
            get
            {
                Word now = this;
                for (; ; ) {
                    now = now.before;
                    if (now.kind == Kind_word.Space) continue;
                    break;
                }
                return now;
            }
        }
        public int length
        {
            get { return str.Length; }
        }

        internal Rect get_rect(int p1, int p2, double x, double y)
        {
            FormattedText i_format = new FormattedText(str.Substring(0, p1), new System.Globalization.CultureInfo("ja"), FlowDirection.LeftToRight, FONT_FAMILY, 15.0, Brushes.Black);
            if (p1 == p2) return new Rect(new Point(x + i_format.WidthIncludingTrailingWhitespace, y), new Size(1, format.Height));
            else {
                if (p2 == length) return new Rect(new Point(x + i_format.WidthIncludingTrailingWhitespace, y), new Size(format.WidthIncludingTrailingWhitespace - i_format.WidthIncludingTrailingWhitespace + 1, format.Height));
                FormattedText i_format2 = new FormattedText(str.Substring(p1, p2 - p1), new System.Globalization.CultureInfo("ja"), FlowDirection.LeftToRight, FONT_FAMILY, 15.0, Brushes.Black);
                return new Rect(new Point(x + i_format.WidthIncludingTrailingWhitespace, y), new Size(i_format2.WidthIncludingTrailingWhitespace + 1, format.Height));
            }
        }
        public int get_pos(double x)
        {
            for (int i = 1; i < length; i++) {
                FormattedText format = new FormattedText(str.Substring(0, i), new System.Globalization.CultureInfo("ja"), FlowDirection.LeftToRight, FONT_FAMILY, 15.0, Brushes.Black);
                if (format.WidthIncludingTrailingWhitespace > x) return i - 1;
            }
            return length - 1;
        }
        public static bool In_call(Word now)
        {
            if (Type_value.Call_start <= now.type && now.type <= Type_value.Call_end) return true;
            else return false;
        }
    }
}
