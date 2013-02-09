using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace Serius
{
    enum Kind_select
    {
        Class, Function, Field,
        Tag,
        Tag_attribute,
        Tag_value,
        Name,
        Id,
        Css_class,
        Style_value
    }
    class Compiler
    {
        public static Color Tag = Colors.Blue;
        public List<Text> texts = new List<Text>();
        public List<Tag_element> tags = new List<Tag_element>();
        public Map<String, Tag_element> ids = new Map<String, Tag_element>();
        public Map<String, Tag_element> names = new Map<String, Tag_element>();
        public Map<String, Tag_element> classes = new Map<string, Tag_element>();
        public Tag_element tag { get { return tags[tags.Count - 1]; } }
        public Compiler(Word now)
        {
            this.now = now;
        }
        public void html_compile()
        {
            nex();
            tags.Add(new Tag_element() {name="window"});
            for (; ; )
            {
                if (now.kind == Kind_word.Compare_Left)
                {
                    now.color = Brushes.Blue;
                    if (now.next.kind == Kind_word.Slash)//</
                    {
                        nex();
                        now.color = Brushes.Blue;
                        neox();
                        if (now.kind == Kind_word.Letter)
                        {
                            now.type = Type_value.Tag_close_name;
                            now.color = Brushes.Aqua;
                            now.item = tags.Last();
                            neox();
                            if (now.kind == Kind_word.Compare_Right)
                            {
                                now.color = Brushes.Blue;
                                tags.Remove(tags.Last());
                                nex();
                            }
                            else throw new Exception_error();
                        }
                        else throw new Exception_error();
                    }
                    else if (now.next.kind == Kind_word.Percent)//<%
                    {
                        for (; ; )
                        {
                            if (now.kind == Kind_word.Percent)
                            {
                                nex();
                                if (now.kind == Kind_word.Compare_Right)
                                {
                                    nex();
                                    break;
                                }
                                nex();
                            }
                            else nex();
                        }
                    }
                    else
                    {
                        neox();
                        tag_compile();
                        nex();
                    }
                }
                else
                {
                    nex();
                }
            }
        }
        public void tag_compile()
        {
            Tag_element here = null;
            Tag_attribute tag_attribute = null;
            Type_value check_type = Type_value.Tag_name;
            String letter = null;
            for (; ; )
            {
                if ((letter = html_name_compile(check_type, tags.Last(), Brushes.Aqua)) != null)
                {
                    if (here == null)
                    {
                        here = new Tag_element() { name = letter };
                        check_type = Type_value.Tag_attribute;
                    }
                    else
                    {
                        switch(letter) {
                            case "id":
                                here.id = now.str;
                                if (ids.ContainsKey(now.str) == false) ids.Add(letter, here);
                                else throw new Exception_error();
                                break;
                            case "name":
                                here.nm = now.str;
                                if (names.ContainsKey(now.str) == false) names.Add(letter, here);
                                break;
                            case "class":
                                here.cls = now.str;
                                if (classes.ContainsKey(now.str) == false) classes.Add(letter, here);
                                break;
                            default:
                                tag_attribute = new Tag_attribute() { name = now.str };
                                here.attributes.Add(tag_attribute);
                                break;
                        }
                        now.item = here;
                    }
                    neox();
                    if (now.kind == Kind_word.Equal)
                    {
                        neox();
                        if (now.kind == Kind_word.Double_quote || now.kind == Kind_word.Single_quote)
                        {
                            string_compile(now.kind, Type_value.Tag_attribute_value, tag_attribute);
                            neox();
                        }
                    }
                }
                else if (now.kind == Kind_word.Slash)
                {
                    now.color = Brushes.Blue;
                    nex();
                    if (now.kind == Kind_word.Compare_Right)
                    {
                        now.color = Brushes.Blue;
                        //now programing
                        return;
                    }
                    else throw new Exception_error();
                }
                else if (now.kind == Kind_word.Compare_Right)
                {
                    now.color = Brushes.Blue;
                    tags.Add(here);
                    if (here.name == "style")
                    {
                        css_comile(Kind_word.Compare_Left);
                        now = now.before;
                    }
                    else if (here.name == "script")
                    {
                        //++js_compile();
                    }
                    return;
                }
                else throw new Exception_error();
            }
        }
        public String html_name_compile(Type_value type, Item item, Brush color)
        {
            String ret = "";
            if (now.kind == Kind_word.Letter || now.kind == Kind_word.Minus)
            {
                now.type = type;
                now.item = item;
                now.color = color;
                ret += now.str;
            loop:
                if (now.next.kind == Kind_word.Letter || now.next.kind == Kind_word.Minus)
                {
                    nex();
                    now.type = type;
                    now.item = item;
                    now.color = color;
                    ret += now.str;
                    goto loop;
                }
                return ret;
            }
            return null;
        }
        public void string_compile(Kind_word start, Type_value type, Item item)
        {
            for (; ; )
            {
                nex();
                if (now.kind == start)
                {
                    return;
                }
                else if (now.kind == Kind_word.End || now.kind == Kind_word.Break)
                {
                    return;
                }
                else if (now.kind == Kind_word.Yen)
                {
                    now.type = type;
                    now.item = item;
                    nex();
                }
                now.type = type;
                now.item = item;
            }
        }
        public void css_comile(Kind_word end)
        {
            neox();
            for (; ; )
            {
            head:
                if (now.kind == end) return;
                String letter = null;
                Type_value type = Type_value.Style_attribute_tag;
                if (now.kind == Kind_word.Atmark)
                {
                    nex();
                    if ((letter = html_name_compile(Type_value.Style_attribute_option, null, Brushes.Brown)) != null)
                    {
                        neox();
                    }
                    else throw new Exception_error();
                    Kind_word kind = css_value_compile(Kind_word.Semicolon, Kind_word.Brace_right, Kind_word.Brace_left);
                    if (kind == Kind_word.Brace_left)
                    {
                        css_comile(Kind_word.Brace_right);
                        neox();
                    }
                    continue;
                }
            loop:
                if (now.kind == Kind_word.Dot)
                {
                    type = Type_value.Style_attribute_class;
                    now.type = type;
                    nex();
                }
                else if (now.kind == Kind_word.Sharp)
                {
                    type = Type_value.Style_attribute_id;
                    now.type = type;
                    nex();
                }
                if ((letter = html_name_compile(type, null, Brushes.Brown)) != null)
                {
                    if (type == Type_value.Style_attribute_option)
                    {
                        neox();
                        if (now.kind == Kind_word.Paren_left)
                        {
                            neox();
                            if (now.kind == Kind_word.Letter)
                            {
                                neox();
                            }
                            else throw new Exception_error();
                            if (now.kind == Kind_word.Paren_right)
                            {
                                neox();
                            }
                            else throw new Exception_error();
                        }
                        else if (now.kind == Kind_word.Brace_left)
                        {
                        }
                    }
                    else
                    {
                        neox();
                        if (type == Type_value.Style_attribute_tag)
                        {
                            Tag_element tag = new Tag_element() {name = letter};
                        loop2:
                            if (now.kind == Kind_word.Bracket_left)
                            {
                                neox();
                                if ((letter = html_name_compile(Type_value.Style_attribute_att, tag, Brushes.Brown)) != null)
                                {
                                    neox();
                                    if (now.kind == Kind_word.Or)
                                    {
                                        nex();
                                        if (now.kind == Kind_word.Equal)
                                        {
                                            neox();
                                            if (now.kind == Kind_word.Double_quote)
                                            {
                                                string_compile(now.kind, Type_value.Style_tag_attribute_value, null);
                                                neox();
                                            }
                                            else throw new Exception_error();
                                        }
                                        else throw new Exception_error();
                                    }
                                    else if (now.kind == Kind_word.Nyoro)
                                    {
                                        nex();
                                        if (now.kind == Kind_word.Equal)
                                        {
                                            neox();
                                            if (now.kind == Kind_word.Double_quote)
                                            {
                                                string_compile(now.kind, Type_value.Style_tag_attribute_value, null);
                                                neox();
                                            }
                                            else throw new Exception_error();
                                        }
                                        else throw new Exception_error();
                                    }
                                    else if (now.kind == Kind_word.Equal)
                                    {
                                        neox();
                                        if (now.kind == Kind_word.Double_quote)
                                        {
                                            string_compile(now.kind, Type_value.Style_tag_attribute_value, null);
                                            neox();
                                        }
                                        else throw new Exception_error();
                                    }
                                    if (now.kind == Kind_word.Bracket_right)
                                    {
                                        neox();
                                        goto loop2;
                                    }
                                    else throw new Exception_error();
                                }
                            }
                        }
                    }
                    if (now.kind == Kind_word.Letter)
                    {
                        type = Type_value.Style_attribute_tag;
                        goto loop;
                    }
                    else if (now.kind == Kind_word.Comma || now.kind == Kind_word.Plus || now.kind == Kind_word.Compare_Left)
                    {
                        type = Type_value.Style_attribute_tag;
                        neox();
                        goto loop;
                    }
                    else if (now.kind == Kind_word.Colon)
                    {
                        type = Type_value.Style_attribute_option;
                        neox();
                        goto loop;
                    }
                }
                else throw new Exception_error();
                if (now.kind == Kind_word.Brace_left)
                {
                    neox();
                    for (; ; )
                    {
                        if (now.kind == Kind_word.Brace_right)
                        {
                            neox();
                            goto head;
                        }
                        else if ((letter = html_name_compile(Type_value.Style_attribute_value, null, Brushes.Orange)) != null)
                        {
                            neox();
                            if (now.kind == Kind_word.Colon)
                            {
                                neox();
                            }
                            else throw new Exception_error();
                            css_value_compile(Kind_word.Semicolon, Kind_word.Brace_right, Kind_word.None);
                            if (now.kind == Kind_word.Brace_right)
                            {
                                neox();
                                goto head;
                            }
                            neox();
                        }
                        else throw new Exception_error();
                    }
                }
            }
        }
        public Kind_word css_value_compile(Kind_word end, Kind_word sub, Kind_word option)
        {
            for (; ; )
            {
                if (now.kind == Kind_word.Letter || now.kind == Kind_word.Minus)
                {
                head2:
                    html_name_compile(Type_value.Style_value, null, Brushes.Black);
                    neox();
                    if (now.kind == Kind_word.Dot)
                    {
                        nex();
                        goto head2;
                    }
                    else if (now.kind == Kind_word.Paren_left)
                    {
                        neo();
                        for (; ; )
                        {
                            Kind_word kind = css_value_compile(Kind_word.Paren_right, Kind_word.None, Kind_word.None);
                            if (kind == Kind_word.Paren_right) break;
                            neox();
                        }
                        neox();
                    }
                }
                else if (now.kind == Kind_word.Number || now.kind == Kind_word.Dot || now.kind == Kind_word.Minus)
                {
                    if (now.kind == Kind_word.Minus) neo();
                    bool dot = false;
                    for (; ; )
                    {
                        if (now.kind == Kind_word.Number)
                        {
                            nex();
                            if (now.kind != Kind_word.Dot) break;
                        }
                        else if (now.kind == Kind_word.Dot)
                        {
                            if (dot) throw new Exception_error();
                            else dot = true;
                            nex();
                        }
                        else throw new Exception_error();
                    }
                    if (now.kind == Kind_word.Letter)
                    {
                        if (now.str == "em" || now.str == "px" || now.str == "%" || now.str == "pt")
                        {
                            neo();
                        }
                        else throw new Exception_error();
                    }
                    if (now.kind == Kind_word.Space) neo();
                }
                else if (now.kind == Kind_word.Sharp)
                {
                    nex();
                    if (now.kind == Kind_word.Letter || now.kind == Kind_word.Number)//16進数だから、数字と名前
                    {
                        if (now.length != 3 && now.length != 6) { /*error*/}
                        neo();
                    }
                    else throw new Exception_error();
                }
                else if (now.kind == Kind_word.Single_quote || now.kind == Kind_word.Double_quote)
                {
                    string_compile(now.kind, Type_value.Style_attribute_value, null);
                    neox();
                }
                else throw new Exception_error();
                if (now.kind == Kind_word.Comma) neox();
                else if (now.kind == end) return end;
                else if (now.kind == sub) return sub;
                else if (now.kind == option) return option;
            }
        }
        public void js_compile()
        {
            for (; ; )
            {
                js_line_compile(Kind_word.Semicolon, Kind_word.None);
            }
        }
        public void js_line_compile(Kind_word end, Kind_word finish)
        {
            if (now.kind == Kind_word.Letter)
            {
                if (now.str == "var")
                {
                }
                else if (now.str == "function")
                {
                }
                else if (now.str == "for")
                {
                }
                else if (now.str == "switch" || now.str == "while")
                {
                }
                else if (now.str == "if")
                {
                }
                else if (now.str == "do")
                {
                }
            }
            else
            {
                for (; ; )
                {
                    if (now.kind == finish || now.kind == Kind_word.End)
                    {
                        return;
                    }
                    if (now.kind == Kind_word.Plus || now.kind == Kind_word.Minus)
                    {
                        if (now.next.kind == now.kind)
                        {
                            nex(); neo();
                        }
                        else throw new Exception();
                    }
                    else if (now.kind == Kind_word.Not)
                    {
                    }
                    if (now.kind == Kind_word.Letter)
                    {
                        neo();
                        for (; ; )
                        {
                            if (now.kind == Kind_word.Paren_left)
                            {
                            }
                            else if (now.kind == Kind_word.Bracket_left)
                            {
                            }
                            else if (now.kind == Kind_word.Dot)
                            {
                            }
                            else break;
                        }
                    }
                    else if (now.kind == Kind_word.Paren_left)
                    {
                    }
                    else if (now.kind == Kind_word.Bracket_left)
                    {
                    }
                    else if (now.kind == Kind_word.Brace_left)
                    {
                    }
                operate_head:
                    if (now.kind == Kind_word.Equal) { }
                    else if (now.kind == Kind_word.Plus || now.kind == Kind_word.Minus)
                    {
                        if (now.next.kind == now.kind) goto operate_head;
                    }
                    else if (now.kind == Kind_word.Slash || now.kind == Kind_word.Astarisk)
                    {
                    }
                    else if (now.kind == Kind_word.Compare_Left || now.kind == Kind_word.Compare_Right) { }
                    else if (now.kind == end || now.kind == Kind_word.Comma || now.kind == Kind_word.Break) { break; }
                }
            }
        }
        public void js_var_compile() { }
        public void js_function_compile() { }
        public void js_for_compile() { }
        public void js_if_compile() { }
        public void js_do_compile() { }
        public void js_switch_compile() { }
        public void js_while_compile() { }
        Word now;
        //改行とスペース飛ばし
        void neox()
        {
        head:
            now = now.next_over_space;
            if (now.kind == Kind_word.Break) goto head;
            if (now.kind == Kind_word.End) throw new Exception_end();
        }
        //スペース飛ばし
        void neo()
        {
            now = now.next_over_space;
            if (now.kind == Kind_word.End) throw new Exception_end();
        }
        void nex()
        {
            now = now.next;
            if (now.kind == Kind_word.End) throw new Exception_end();
        }
    }
    class Exception_end : Exception { }
    class Exception_error : Exception { }
    class VBlob
    {
    }
    class VFolder : VBlob
    {
    }
    class VFile : VBlob
    {
    }
    class Blob
    {
    }
    class Folder : Blob
    {
    }
    class File : Blob
    {
        public String name;
    }
    class Item
    {
        public String name;
        public virtual String output() { return null; }
    }
    class Selection : Item
    {
        public String explain;
        public Kind_select select;
    }
    partial class Tag_element
    {
        public static Map<String, Tag_element> Base_tags = new Map<String,Tag_element>();
        public static Map<String, Tag_attribute> Base_tag_attributes = new Map<String, Tag_attribute>();
        public static Map<String, String[]> replaces = new Map<String, String[]>();
        public static void init()
        {
            Tag_element tag;
            foreach (String name in new String[] { "window", "html", "head", "body", "frameset", "title", "meta", "base", "link", "style", "li", "dt", "dd", "area", "param", "caption", "thead", "tfoot", "tbody", "colgroup", "col", "tr", "td", "th", "frame", "option", "optgroup", "legend", "del", "ins",
                "span", "em", "strong", "abbr", "acronym", "dfn", "q", "cite", "sup", "sub", "code", "var", "kbd", "samp", "bdo", "font", "big", "small", "b", "i", "s", "strike", "u", "tt", "a", "label", "object", "applet", "iframe", "button", "textarea", "select", "basefont", "img", "br", "input", "script", "map",
                "div", "fieldset", "center", "blockquote", "form", "noscript", "h1", "h2", "h3", "h4", "h5", "h6", "address", "p", "pre", "ul", "ol", "dl", "dir", "menu", "table", "hr", "isindex", "noframes"})
            {
                tag = new Tag_element() { name = name, select = Kind_select.Tag };
                Base_tags.Add(name, tag);
            }
            Tag_attribute att;
            foreach (String name in new String[] { "lang", "dir", "name", "name#1", "name#2", "name#3", "http-equiv", "content", "rel", "href", "href#1", "hreflang", "type", "type#1", "type#2", "type#3", "type#4", "type#5", "media", "style", "class", "id", "title", "accesskey", "tabindex",
                "src", "src#1", "charset", "defer", "language", "data", "width", "height", "usemap", "cite", "cite#1", "target", "for", "value", "value#1", "value#2", "disabled", "cols", "rows", "readonly", "size", "size#1", "multiple", "alt", "ismap", "border", "maxlength",
                "checked", "accept", "action", "method", "enctype", "accept-charset", "shape", "coords", "span", "colspan", "rowspan", "headers", "scope", "selected", "label", "disable", "datetime", "start", "summary", "boarder",
                "onclick", "ondblclick", "onmousedown", "onmouseup", "onmouseover", "onmouseout", "onmousemove", "onkeypress", "onkeydown", "onkeyup", "onload", "onunload", "onfocus", "onblur", "onselect", "onchange"})
            {
                String id = "";
                for (int i = 0; ; i++)
                {
                    if (i == name.Length || name[i] == '#')
                    {
                        id = name.Substring(0, i);
                        break;
                    }
                }
                att = new Tag_attribute() { name = id, select = Kind_select.Tag_attribute };
                Base_tag_attributes.Add(name, att);
            }
            replaces.Add("@inline", new String[] { "span", "em", "strong", "abbr", "acronym", "dfn", "q", "cite", "sup", "sub", "code", "var", "kbd", "samp", "bdo", "font", "big", "small", "b", "i", "s", "strike", "u", "tt", "a", "label", "object", "applet", "iframe", "button", "textarea", "select", "basefont", "img", "br", "input", "map", "script" });
            replaces.Add("@block", new String[] {"div", "fieldset", "blockquote", "form", "noscript", "h1", "h2", "h3", "h4", "h5", "h6", "address", "p", "pre", "ul", "ol", "dl", "menu", "table", "hr"});
            replaces.Add("@basic", new String[] { "style", "class", "id", "title", "lang", "dir" });
            replaces.Add("@event", new String[] { "onclick", "ondblclick", "onmousedown", "onmouseup", "onmouseover", "onmouseout", "onmousemove", "onkeypress", "onkeydown", "onkeyup" });
            tag = Base_tags["window"]; tag.explain = "";
            tag.set_children("html");
            tag = Base_tags["html"]; tag.explain = "ホームページ開始タグです。";
            tag.set_children("head", "body", "script"); tag.set_attributes("lang", "dir");
            tag = Base_tags["head"]; tag.explain = "中にヘッダ情報を記述するタグです。\ntitleは必須です。";
            tag.set_children("title", "base", "meta", "link", "script", "style", "object"); tag.set_attributes("lang", "dir");
            tag = Base_tags["body"]; tag.explain = "中身を記述します。";
            tag.set_children("@block", "@inline"); tag.set_attributes("@basic", "@event", "onload", "onunload");
            tag = Base_tags["title"]; tag.explain = "ページのタイトルを記述します。\n検索キーワードに大きな影響を与えます。";
            tag = Base_tags["meta"]; tag.explain = "付加情報を付与します。\nname属性とhttp-equiv属性は、どちらかを必ず指定する必要があります。contentは必須です。";
            tag.set_attributes("name#1", "http-equiv", "content");
            tag = Base_tags["base"]; tag.explain = "相対URIの基準となるURIを指定します。";
            tag.set_attributes("href#1", "target");
            tag = Base_tags["link"]; tag.explain = "外部文書との連携に使います。前や次の文書を指定したり、外部スタイルシートを引用したりできます。";
            tag.set_attributes("rel", "href", "hreflang", "type", "media", "@basic", "onclick", "ondblclick", "onmousedown", "onmouseup", "onmouseover", "onmouseout", "onkeypress", "onkeydown", "onkeyup");
            tag = Base_tags["li"]; tag.explain = "リストの項目をあらわす。";
            tag.set_children("@inline", "@block", "ins", "del"); tag.set_attributes("value#1", "@basic", "@event");
            tag = Base_tags["dt"]; tag.explain = "dl内で定義する用語をあらわす。";
            tag.set_children("@inline", "ins", "del"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["dd"]; tag.explain = "dl内で定義された用語に対する説明を表す。";
            tag.set_children("@inline", "@block", "ins", "del"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["area"]; tag.explain = "imgのリンク領域を設定する。";
            tag.set_attributes("alt", "shape", "coords", "href", "target", "accesskey", "tabindex", "@basic", "@event", "onfocus", "onblur");
            tag = Base_tags["param"]; tag.explain = "objectに対する引数を指定する。";
            tag.set_attributes("name", "value#2", "id");
            tag = Base_tags["caption"]; tag.explain = "表にタイトルをつける要素です。\ntableの最初の部分に配置します。";
            tag.set_children("@inline", "ins", "del"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["thead"]; tag.explain = "表の行をヘッダ部分としてグループ化します。";
            tag.set_children("tr"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["tfoot"]; tag.explain = "表の行をフッタ部分としてグループ化します。";
            tag.set_children("tr"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["tbody"]; tag.explain = "表の行を本体部分としてグループ化します。";
            tag.set_children("tr"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["colgroup"]; tag.explain = "表の列を構造的にグループ化します。";
            tag.set_children("col"); tag.set_attributes("span", "@basic", "@event");
            tag = Base_tags["col"]; tag.explain = "表の列の属性をまとめて設定します。";
            tag.set_attributes("span", "@basic", "@event");
            tag = Base_tags["tr"]; tag.explain = "表の行をあらわします。\n中にtd,thを配置します。";
            tag.set_children("td", "th"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["td"]; tag.explain = "表のセルをあらわします。";
            tag.set_children("@inline", "@block", "ins", "del"); tag.set_attributes("colspan", "rowspan", "headers", "@basic", "@event");
            tag = Base_tags["th"]; tag.explain = "表のセルをあらわいます。";
            tag.set_children("@inline", "@block", "ins", "del"); tag.set_attributes("colspan", "rowspan", "headers", "scope", "@basic", "@event");
            tag = Base_tags["option"]; tag.explain = "メニューの選択肢を作成します。";
            tag.set_attributes("value", "selected", "label", "disable", "@basic", "@event");
            tag = Base_tags["optgroup"]; tag.explain = "メニューの選択肢をグループ化します。";
            tag.set_children("option"); tag.set_attributes("label", "disabled", "@basic", "@event");
            tag = Base_tags["legend"]; tag.explain = "フォーム部品のグループにラベルをつける要素です。\nfieldset内で使用します。";
            tag.set_children("@inline", "ins", "del"); tag.set_attributes("accesskey", "@basic", "@event");
            tag = Base_tags["del"]; tag.explain = "削除された部分を示します。";
            /*?*/tag.set_children("@inline", "@block", "ins", "del"); tag.set_attributes("cite#1", "datetime", "@basic", "@event");
            tag = Base_tags["ins"]; tag.explain = "追加された部分を示します。";
            /*?*/tag.set_children("@inline", "@block", "ins", "del"); tag.set_attributes("cite#1", "datetime", "@basic", "@event");
            //style,script
            tag = Base_tags["style"]; tag.explain = "中にスタイルシートを記述します。\ntypeは必須です。";
            tag.set_attributes("type#4", "media", "title", "lang", "dir");
            tag = Base_tags["script"]; tag.explain = "中にjavascriptコードを記述します。\ntypeは必須です。";
            tag.set_attributes("type#5", "src", "charset", "defer", "language");
            //inline
            tag = Base_tags["span"]; tag.explain = "内部の要素をインラインとしてまとめることができます。";
            tag.set_children("@inline", "ins", "del"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["em"]; tag.explain = "内部の要素を強調して表示できます。関連としてより強く強調するstrongがあります。";
            tag.set_children("@inline", "ins", "del"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["strong"]; tag.explain = "内部の要素をより強く強調して表示できます。関連として中程度に強調するemがあります。";
            tag.set_children("@inline", "ins", "del"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["abbr"]; tag.explain = "title属性をしていすることで、省略する前の文字を示すことができます。\n<abbr title='World Wide Web'>WWW</abbr>";
            tag.set_children("@inline", "ins", "del"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["dfn"]; tag.explain = "用語の意味を解説する際、その用語の部分に対してこの要素を使います。";
            tag.set_children("@inline", "ins", "del"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["q"]; tag.explain = "短いテキストを引用する際にに、この要素を使用します。\n長い文章の引用にはblockqouteを、出展や参照先を示すのにciteを使用します。";
            tag.set_children("@inline", "ins", "del"); tag.set_attributes("cite", "@basic", "@event");
            tag = Base_tags["cite"]; tag.explain = "出展や参照先を示す要素です。\n短いテキストの引用にはciteを、長い文章の引用にはblockquoteを使います。";
            tag.set_children("@inline", "ins", "del"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["sup"]; tag.explain = "上付き文字を示す要素です。\n下付き文字を示すのにはsubがあります。";
            tag.set_children("@inline", "ins", "del"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["sub"]; tag.explain = "下付き文字を示す要素です。\n上付き文字を示すのにはsupを使います。";
            tag.set_children("@inline", "ins", "del"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["code"]; tag.explain = "コードであることを示す要素です。変数や引数のみを示す場合varを使います。";
            tag.set_children("@inline", "ins", "del"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["var"]; tag.explain = "コードの変数や引数を示す要素です。\nコード全体を示す場合codeを使います。";
            tag.set_children("@inline", "ins", "del"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["kbd"]; tag.explain = "ユーザーが入力するテキストを示します。\n<kbd>guest</kbd>と入力するとログインできます。";
            tag.set_attributes("@basic", "@event");
            tag = Base_tags["samp"]; tag.explain = "プログラムなどの出力サンプルを示す要素です。";
            tag.set_children("@inline", "ins", "del"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["bdo"]; tag.explain = "書字方向を指定する要素です。";
            tag.set_children("@inline", "ins", "del"); tag.set_attributes("@basic");
            tag = Base_tags["small"]; tag.explain = "内部のテキストを一回り小さいサイズで表示します。";
            tag.set_children("@inline", "ins", "del"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["b"]; tag.explain = "内部のテキストを、太字で表示します。\n強調を示す場合、strongまたはemを使います。";
            tag.set_children("@inline", "ins", "del"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["i"]; tag.explain = "内部のテキストを、イタリック体で表示します。";
            tag.set_children("@inline", "ins", "del"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["s"]; tag.explain = "内部のテキストに取り消し線を引いて表示します。\n削除された部分を示す場合はdelを使います。";
            tag.set_attributes("@basic", "@event");
            tag = Base_tags["u"]; tag.explain = "内部のテキストを下線付きで表示します。";
            tag.set_children("@inline", "ins", "del"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["a"]; tag.explain = "リンクの出発点、または到達点を表します。\nhrefが指定された場合、hrefへリンクし、nameやidが指定された場合到達点を意味します。";
            tag.set_children("@inline", "ins", "del"); tag.set_attributes("href", "name#2", "hreflang", "type", "rel", "target", "accesskey", "tabindex", "@basic", "@event", "onfocus", "onblur");
            tag = Base_tags["label"]; tag.explain = "フォーム部品とラベルの関連付けをします。\n方法としては、inputにidを指定してlabelのforにid名を指定するのと、labael要素の中にinputを配置（ひとつまで）する二つの方法があります。";
            tag.set_children("@inline", "ins", "del"); tag.set_attributes("for", "accesskey", "@basic", "@event", "onfocus", "onblur");
            //
            tag = Base_tags["object"]; tag.explain = "動画、サウンド、画像、HTML文書、Javaアプレット等を埋め込むことができます。";
            tag.set_children("@inline", "@block", "ins", "del", "param"); tag.set_attributes("data", "type#2", "width", "height", "usemap", "name", "@basic", "tabindex", "onclick", "ondblclick", "onmousedown", "onmouseup", "onmouseover", "onmouseout", "onkeypress", "onkeydown", "onkeyup");
            tag = Base_tags["iframe"]; tag.explain = "文書内にインラインのフレームを配置します。\nこのフレームを利用できない環境ではこの要素の内容が大体として仕様されることになります。";
            tag.set_children("@inline", "@block", "ins", "del"); tag.set_attributes("src", "name", "width", "height", "style", "class", "id", "title");
            tag = Base_tags["button"]; tag.explain = "汎用的なボタンを作成する要素です。\n内部要素をもてるので画像や強調表示ができる点がinputと違います。";
            tag.set_children("@inline", "@block", "-a", "-iframe", "-form", "-input", "-textarea", "-select", "-fieldset", "-label", "-button", "-isindex", "ins", "del"); tag.set_attributes("type#3", "name", "value", "disabled", "accesskey", "tabindex", "@basic", "@event", "onfocus", "onblur");
            //
            tag = Base_tags["textarea"]; tag.explain = "複数行のテキスト入力欄を提供します。";
            tag.set_attributes("cols", "rows", "name", "disabled", "readonly", "accesskey", "tabindex", "@basic", "@event", "onfocus", "onblur", "onselect", "onchange");
            tag = Base_tags["select"]; tag.explain = "メニューを作成します。";
            tag.set_children("option", "optgroup");  tag.set_attributes("name", "size#1", "multiple", "disabled", "tabindex", "@basic", "@event", "onfocus", "onblur", "onchange");
            tag = Base_tags["img"]; tag.explain = "画像を表示する。";
            tag.set_attributes("src", "alt", "width", "height", "usemap", "ismap", "border", "@basic", "@event");
            tag = Base_tags["br"]; tag.explain = "改行します。";
            tag.set_attributes("style", "class", "id", "title");
            tag = Base_tags["input"]; tag.explain = "各種入力フォーム提供します。";
            tag.set_attributes("type", "name", "value", "size", "maxlength", "checked", "disabled", "readonly", "accept", "src", "alt", "accesskey", "tabindex", "@basic", "@event");
            tag = Base_tags["map"]; tag.explain = "ひとつの画像に複数のリンク先を設定します。";
            tag.set_children("area");  tag.set_attributes("name#3", "@basic", "@event");
            //block
            tag = Base_tags["div"]; tag.explain = "任意の範囲をブロック要素としてまとめることができます。";
            tag.set_children("@inline", "@block", "ins", "del"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["fieldset"]; tag.explain = "フォームの部品をグループ化する要素です。\n中身はlegendから始まらなければいけません。\n<fieldset><legend>申込者</legend>お名前<input type='text' name='name'></fieldset>";
            tag.set_children("@inline", "@block", "ins", "del", "legend"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["blockquote"]; tag.explain = "長い文章を引用します。\n短文の引用はqを、出展や参照の表示にはciteを使います。";
            tag.set_children("@block", "ins", "del", "script"); tag.set_attributes("cite", "@basic", "@event");
            tag = Base_tags["form"]; tag.explain = "入力フォームを作成します。内部の入力情報はnameを指定することで転送されます。";
            tag.set_children("@inline", "@block", "ins", "del", "script"); tag.set_attributes("action", "method", "enctype", "accept-charset", "name", "target", "@basic", "@event");
            tag = Base_tags["noscript"]; tag.explain = "スクリプトを利用できないブラウザに対して異なる内容を提供します。スクリプトを利用できるブラウザでは無視されます。";
            tag.set_attributes("@basic", "@event");
            tag = Base_tags["h1"]; tag.explain = "巨大見出しを示す要素です。";
            tag.set_children("@inline", "ins", "del"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["h2"]; tag.explain = "大見出しを示す要素です。";
            tag.set_children("@inline", "ins", "del"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["h3"]; tag.explain = "中見出しを示す要素です。";
            tag.set_children("@inline", "ins", "del"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["h4"]; tag.explain = "大見出しを示す要素です。";
            tag.set_children("@inline", "ins", "del"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["h5"]; tag.explain = "大見出しを示す要素です。";
            tag.set_children("@inline", "ins", "del"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["h6"]; tag.explain = "大見出しを示す要素です。";
            tag.set_children("@inline", "ins", "del"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["address"]; tag.explain = "連絡先を示す要素です。";
            tag.set_children("@inline", "ins", "del"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["p"]; tag.explain = "段落を示す要素です。";
            tag.set_children("@inline", "ins", "del"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["pre"]; tag.explain = "整形済みのテキストであることを示す要素です。\n半角スペースや開業はそのままの形で表示されます。また、自動的な折り返しが行われなくなります。";
            tag.set_children("@inline", "ins", "del", "-sup", "-sub", "-font", "-basefont", "-big", "-small", "-img", "-object", "-iframe"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["ul"]; tag.explain = "順序のない箇条書きリストをさくせいします。\nリストの項目はliで作成します。";
            tag.set_children("li"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["ol"]; tag.explain = "番号つきのリストを作成します。\nリストの項目はliで作成します。";
            tag.set_children("li"); tag.set_attributes("type", "start", "@basic", "@event");
            tag = Base_tags["dl"]; tag.explain = "定義型リストを作成します。\n定義する用語をdtで、その定義に対する説明をddで示します。";
            tag.set_children("dt", "dd"); tag.set_attributes("@basic", "@event");
            tag = Base_tags["table"]; tag.explain = "表の大枠を示します。\ncolgroupとcolは同時に配置できません。";
            tag.set_children("caption", "thead", "tfoot", "tbody", "colgroup", "col"); tag.set_attributes("summary", "border", "@basic", "@event");
            tag = Base_tags["hr"]; tag.explain = "水平線を引きます。";
            tag.set_attributes("@basic", "@event");
            //attribute
            att = Base_tag_attributes["lang"]; att.explain = "要素の言語を指定します。";
            att.set_values("ja", "en-US", "zh", "de", "it", "pt", "pl", "ru", "hi", "la", "en", "en-GB", "ko", "fr", "nl", "es", "el", "he", "ar", "sw");
            att = Base_tag_attributes["dir"]; att.explain = "要素の所持方向を指定することができます。";
            att.set_values("ltr", "rtl");
            att = Base_tag_attributes["style"]; att.explain = "要素にスタイルシートを適用します。";
            att = Base_tag_attributes["class"]; att.explain = "要素のクラス名を指定します。";
            att = Base_tag_attributes["id"]; att.explain = "要素のIDを指定します。";
            att = Base_tag_attributes["title"]; att.explain = "要素に補足情報を記述します。";
            att = Base_tag_attributes["name"]; att.explain = "部品名を指定します。データ送信時の名前にもなります。";
            att = Base_tag_attributes["name#1"]; att.explain = "meta名を指定します。大文字と小文字は区別されます";
            att = Base_tag_attributes["name#2"]; att.explain = "到達点にするための名前";
            att.set_values("Content-Type", "keywords", "author", "description", "ROBOTS", "viewport");
            att = Base_tag_attributes["http-equiv"]; att.explain = "HTTPヘッダ名を指定します。";
            att.set_values("Content-Type", "Content-Style-Type", "Content-Script-Type");
            att = Base_tag_attributes["content"]; att.explain = "metaの内容を記述します。大文字と小文字は区別されます。";
            att = Base_tag_attributes["cite"]; att.explain = "引用元のURIを記述します";
            att.set_values("@URI");
            att = Base_tag_attributes["cite#1"]; att.explain = "変更理由の参照URI";
            att.set_values("@URI");
            att = Base_tag_attributes["datetime"]; att.explain = "変更日時を記述します。\nYYYY-MM-DDhh:mm:ssTZD";
            att.set_values("@DATE");
            att = Base_tag_attributes["href"]; att.explain = "リンク先の参照";//**
            att.set_values("@URI");
            att = Base_tag_attributes["href#1"]; att.explain = "基準となるURL";
            att.set_values("@URIA");
            att = Base_tag_attributes["hreflang"]; att.explain = "リンク先の基本言語";
            att.set_values("@LANG");
            att = Base_tag_attributes["type"]; att.explain = "リンク先のMIMEタイプ";
            att.set_values("text/plain", "text/html", "text/xml", "application/xhtml+xml", "text/css", "text/javascript", "text/vbscript", "application/x-httpd-cgi",
                "image/gif", "image/jpeg", "image/png", "image/vnd.microsoft.ico", "application/x-shockwave-flash", "video/mpeg", "video/quicktime", "video/x-msvideo",
                "audio/mpeg", "audio/midi", "audio/vnd.rn-realaudio", "audio/wav", "application/pdf", "application/msword", "application/msexcel");
            att = Base_tag_attributes["rel"]; att.explain = "この文書から見たリンク先の関係";
            att.set_values("alternate", "stylesheet", "start", "next", "prev", "contents", "index", "glossary", "copyright", "chapter", "section", "subsection", "appendix", "help", "bookmark");
            att = Base_tag_attributes["target"]; att.explain = "リンク先の表示方法";
            att.set_values("_blank", "_self", "_parent", "_top", "@FLAME", "@WINDOW");
            att = Base_tag_attributes["type#1"]; att.explain = "番号の種類を指定します。";
            att.set_values("1", "A", "a", "l", "i");
            att = Base_tag_attributes["start"]; att.explain = "開始番号。初期値は１。";
            att = Base_tag_attributes["value#1"]; att.explain = "番号の変更。(ol要素内)。";//-?img
            att = Base_tag_attributes["src"]; att.explain = "参照先の指定";//**
            att.set_values("@URI");
            att = Base_tag_attributes["alt"]; att.explain = "代わりになるテキスト";//**
            att = Base_tag_attributes["width"]; att.explain = "横幅。ピクセル数またはパーセント。";
            att = Base_tag_attributes["height"]; att.explain = "高さ。ピクセル数またはパーセント。";
            att = Base_tag_attributes["usemap"]; att.explain = "イメージマップの関連付け。#マップ名。";
            att.set_values("@MAP");
            att = Base_tag_attributes["ismap"]; att.explain = "サーバーサイド・イメージマップ";
            att.set_values("ismap");
            att = Base_tag_attributes["boarder"]; att.explain = "境界線の太さ。ピクセル数。";
            att = Base_tag_attributes["name#3"]; att.explain = "イメージマップの名前";
            att = Base_tag_attributes["shape"]; att.explain = "領域の形状";
            att.set_values("rect", "circle", "poly", "default");
            att = Base_tag_attributes["coords"]; att.explain = "領域の座標。\nrect 左下と右下の座標　X,Y,X,Y\ncircl 中心点の座標と半径　X,Y,R\npoly　すべての角の座標　X,Y,X,Y...\ndefault 指定不要";
            att = Base_tag_attributes["accesskey"]; att.explain = "要素に対してアクセスキーを割り当てます。";
            att = Base_tag_attributes["tabindex"]; att.explain = "Tabキーによるフォーカスの移動順序を指定することができます。";
            att = Base_tag_attributes["data"]; att.explain = "オブジェクトデータの指定。";
            att.set_values("@URI");
            att = Base_tag_attributes["type#2"]; att.explain = "オブジェクトデータののMIMEタイプ";
            att.set_values("text/plain", "text/html", "text/xml", "application/xhtml+xml", "text/css", "text/javascript", "text/vbscript", "application/x-httpd-cgi",
                "image/gif", "image/jpeg", "image/png", "image/vnd.microsoft.ico", "application/x-shockwave-flash", "video/mpeg", "video/quicktime", "video/x-msvideo",
                "audio/mpeg", "audio/midi", "audio/vnd.rn-realaudio", "audio/wav", "application/pdf", "application/msword", "application/msexcel");
            att = Base_tag_attributes["value#2"]; att.explain = "パラメーターの値。";
            att = Base_tag_attributes["summary"]; att.explain = "表の説明";
            att = Base_tag_attributes["span"]; att.explain = "グループ化する列数。初期値は1。";
            att = Base_tag_attributes["colspan"]; att.explain = "水平方向のセルの結合数。初期値は1。";
            att = Base_tag_attributes["rowspan"]; att.explain = "垂直方向のセルの結合数。初期値は1。";
            att = Base_tag_attributes["headers"]; att.explain = "見出しセルのID名。半角スペース区切りで複数指定可能";
            att = Base_tag_attributes["scope"]; att.explain = "見出しの対象範囲。";
            att.set_values("row", "col", "rowgroup", "colgroup");
            att = Base_tag_attributes["action"]; att.explain = "送信先のURI";
            att.set_values("@URI");
            att = Base_tag_attributes["method"]; att.explain = "データの送信方法の指定。";
            att.set_values("get", "post");
            att = Base_tag_attributes["enctype"]; att.explain = "データの送信形式の指定。";
            att.set_values("application/x-www-form-urlencoded", "multipart/form-data", "text/plain");
            att = Base_tag_attributes["accept-charset"]; att.explain = "プログラム側が受け入れる文字コード。";
            att.set_values("ISO-8859-1", "ISO-2022-JP", "UTF-8", "EUC-JP", "Shift-JIS");
            att = Base_tag_attributes["type"]; att.explain = "部品の形式。";
            att.set_values("text", "password", "radio", "checkbox", "file", "hidden", "submit", "reset", "image", "button");//**
            att = Base_tag_attributes["size"]; att.explain = "部品の幅。textまたはpasswordの場合、文字数。ほかはピクセル数。";
            att = Base_tag_attributes["maxlength"]; att.explain = "入力できる最大文字数。初期値は無制限。textまたはpasswordの場合有効。";
            att = Base_tag_attributes["checked"]; att.explain = "選択されている状態にする。radio1またはcheckboxのとき有効。";
            att.set_values("checked");
            att = Base_tag_attributes["disabled"]; att.explain = "部品の無効化。送信もされなくなります。";
            att.set_values("disabled");
            att = Base_tag_attributes["readonly"]; att.explain = "書き換えを禁止。";
            att.set_values("readonly");
            att = Base_tag_attributes["type#3"]; att.explain = "部品の種類";
            att.set_values("submit", "reset", "button");
            att = Base_tag_attributes["accept"]; att.explain = "プログラム側が受け入れるMIMEタイプ。カンマ区切りで複数指定可能。";
            att.set_values("@MIME");
            att = Base_tag_attributes["cols"]; att.explain = "入力欄の幅。文字数。";
            att = Base_tag_attributes["rows"]; att.explain = "入力欄の高さ。行数。";
            att = Base_tag_attributes["size#1"]; att.explain = "表示する行数。";//**
            att = Base_tag_attributes["multiple"]; att.explain = "複数選択を可能にする。";
            att.set_values("multiple");
            att = Base_tag_attributes["selected"]; att.explain = "選択されている状態にする。";
            att.set_values("selected");
            att = Base_tag_attributes["label"]; att.explain = "選択肢として表示するテキスト。";
            att = Base_tag_attributes["type"]; att.explain = "ボタンの形式。";
            att.set_values("submit", "reset", "button");
            att = Base_tag_attributes["for"]; att.explain = "関連付ける部品の指定。";
            att.set_values("@ID");
            att = Base_tag_attributes["type#4"]; att.explain = "スタイルシート言語の指定";
            att.set_values("text/css");//**
            att = Base_tag_attributes["media"]; att.explain = "出力メディア。カンマ区切りで複数指定可能";
            att.set_values("screen", "tty", "tv", "projection", "handheld", "print", "braille", "aural", "all");
            att = Base_tag_attributes["type#5"]; att.explain = "スクリプト言語の指定。";
            att.set_values("text/javascript");
            att = Base_tag_attributes["src#1"]; att.explain = "外部スクリプトの指定。";
            att.set_values("@URI");
            att = Base_tag_attributes["charset"]; att.explain = "外部スクリプトの文字コード。";
            att.set_values("@CHARSET");
            att = Base_tag_attributes["defer"]; att.explain = "内容を生成しないことを示す。";
            att.set_values("defer");
            att = Base_tag_attributes["language"]; att.explain = "スクリプト言語の指定";
            att.set_values("JavaScript");
            att = Base_tag_attributes["onclick"]; att.explain = "要素の上でクリックされたときのイベント。";
            
            Css_attribute.init();
        }
        public void set_children(params String[] names)
        {
            foreach (String name in names)
            {
                if (name[0] == '@')
                {
                    foreach (String name2 in replaces[name]) tags.Add(Base_tags[name2]);
                }
                else if (name[0] == '-')
                {
                    tags.Remove(Base_tags[name.Substring(1)]);
                }
                else tags.Add(Base_tags[name]);
            }
        }
        public void set_attributes(params String[] names)
        {
            foreach (String name in names)
            {
                if (name[0] == '@')
                {
                    foreach (String name2 in replaces[name]) attributes.Add(Base_tag_attributes[name2]);
                }
                else attributes.Add(Base_tag_attributes[name]);
            }
        }
    }
    partial class Tag_element : Selection
    {
        public List<Tag_element> tags = new List<Tag_element>();
        public List<Tag_attribute> attributes = new List<Tag_attribute>();
        public String id;
        public String nm;//nameかぶってます。
        public String cls;
        public bool single;
        public override String output()
        {
            String output = "";
            output = "<" + name;
            if (id != null) output += " id='" + id + "'";
            if (nm != null) output += " name='" + nm + "'";
            if (cls != null) output += " cls='" + cls + "'";
            if (single) { output += "/>"; return output; }
            else output += ">";
            for (int i = 0; i < tags.Count; i++) output += tags[i].ToString();
            output += "</" + name + ">";
            return output;
        }
    }
    class Tag_attribute : Selection
    {
        public List<Tag_value> values = new List<Tag_value>();
        public void set_values(params String[] names)
        {
            foreach (String name in names) values.Add(new Tag_value() { name = name });
        }
    }
    class Tag_value : Selection
    {
    }
    class Css_attribute : Selection
    {
        public Css_type type;
        public static Map<String, Css_attribute> attributes = new Map<String, Css_attribute>();
        public static Map<String, Css_type> types = new Map<String, Css_type>();
        public static void init()
        {
            add("font", "font-size", "font-weight", "font-style", "font-family", "font-variant", "text-align", "vertical-align", "line-height", "text-decoration", "text-indent", "text-transform", "letter-spacing", "word-spacing", "white-space", 
                "color", "background", "background-color", "background-image", "background-repeat", "background-position", "background-attachment",
                "width", "height", "max-width", "min-width", "max-height", "min-height",
                "margin", "margin-top", "margin-right", "margin-bottom", "margin-left", "padding", "padding-top", "padding-right", "padding-bottom", "padding-left",
                "border", "border-top", "border-right", "border-bottom", "border-left", "border-width", "border-top-width", "border-right-width", "border-bottom-width", "border-left-width",
                "border-color", "border-top-color", "border-right-color", "border-bottom-color", "border-left-color", "border-style", "border-top-style", "border-right-style", "border-bottom-style", "border-left-style",
                "overflow", "display", "visibility", "clip", "float", "clear", "position", "top", "right", "bottom", "left", "z-index", "direction", "unicode-bidi",
                "list-style", "list-style-type", "list-style-position", "list-style-image",
                "table-layout", "border-collapse", "border-spacing", "empty-cells", "caption-side", "content", "quotes", "outline", "outline-width", "outline-color", "outline-style",
                "cursor", "page-break-before", "page-break-after", "page-break-inside");
            Css_attribute att;
            att = attributes["background-color"]; att.explain = "背景色を指定します。";
            att = attributes["background-image"]; att.explain = "背景画像を指定します。";
            att = attributes["background-repeat"]; att.explain = "背景画像の繰り返し方を指定します。";
            add_type("url", "color", "size", "number", "list");
        }
        public static void add(params String[] names)
        {
            foreach(String name in names) {
                attributes.Add(name, new Css_attribute() {name = name});
            }
        }
        public static void add_type(params String[] names)
        {
            foreach (String name in names)
            {
                types.Add(name, new Css_type() { name = name });
            }
        }
    }
    class Css_type : Selection
    {
    }
    class Text_content : Tag_element
    {
        public String value;
        public override string output()
        {
            return value;
        }
    }
    class Tag_js : Tag_element
    {
        public override string output()
        {
            return "<script type='text/javascript'>" + "</script>";
        }
    }
    class Tag_cache : Tag_element
    {
        public List<String> files = new List<string>();
    }
    class Tag_wordl : Tag_element
    {
        public override string output()
        {
            return "<script type='text/javascript'>" + "</script>";
        }
    }
    class Tag_style : Tag_element
    {
    }
    class JS : Tag_element
    {
    }
    class Attribute
    {
    }
    class Block : Variable
    {
        public Block parent;
        protected Map<String, Variable> values = new Map<String, Variable>();
        public bool add(String key, Variable value)
        {
            if (values.ContainsKey(key)) return false;
            else
            {
                if (value is Block) (value as Block).parent = this;
                values.Add(key, value);
                return true;
            }
        }
        public Variable get(String key)
        {
            if (values.ContainsKey(key)) return values[key];
            else if (parent == null) return null;
            else return parent.get(key);
        }
    }
    class Style
    {
    }
    class Variable : Selection
    {
    }
    class Class : Block
    {
    }
    class Function : Block
    {
        List<Variable> draws = new List<Variable>();
    }
    class For : Block
    {
        List<Variable> draws = new List<Variable>();
    }
}
