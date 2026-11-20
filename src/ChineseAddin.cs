using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using MarkdownMonster.AddIns;

public class ChineseAddin : MarkdownMonsterAddin
{
    static Dictionary<string,string> words = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
    static int changed;
    static string folder = Path.GetDirectoryName(typeof(ChineseAddin).Assembly.Location);
    public ChineseAddin(){Id="ChineseLocalization";Name="简体中文界面";}
    public override System.Threading.Tasks.Task OnWindowLoaded()
    {
        var dictionaryPath = Path.Combine(folder,"translations.tsv");
        if (!File.Exists(dictionaryPath)) return System.Threading.Tasks.Task.CompletedTask;
        foreach(var line in File.ReadAllLines(dictionaryPath)) {
            var p=line.Split('\t'); if(p.Length==2)words[p[0]]=p[1];
        }
        EventManager.RegisterClassHandler(typeof(FrameworkElement),FrameworkElement.LoadedEvent,new RoutedEventHandler(Loaded),true);
        EventManager.RegisterClassHandler(typeof(MenuItem),MenuItem.SubmenuOpenedEvent,new RoutedEventHandler(Loaded),true);
        EventManager.RegisterClassHandler(typeof(ContextMenu),ContextMenu.OpenedEvent,new RoutedEventHandler(Loaded),true);
        foreach(Window w in Application.Current.Windows) Scan(w,new HashSet<DependencyObject>());
        // Diagnostic output is optional: installation folders may be read-only.
        try { File.WriteAllText(Path.Combine(folder,"loaded.txt"),DateTime.Now.ToString("s")+"\nTranslated literal UI labels: "+changed+"\nDictionary entries: "+words.Count); } catch (IOException) {} catch (UnauthorizedAccessException) {}
        return System.Threading.Tasks.Task.CompletedTask;
    }
    static void Loaded(object sender,RoutedEventArgs e) {
        try{if(sender is DependencyObject d) Scan(d,new HashSet<DependencyObject>());}catch{}
    }
    static string Translate(string s) {
        if(string.IsNullOrWhiteSpace(s))return s;
        var key=s.Trim().Replace("_","");
        string suffix="";
        var accelerator=s.IndexOf('_');
        if(accelerator>=0&&accelerator+1<s.Length&&char.IsLetter(s[accelerator+1]))suffix=" (_"+char.ToUpperInvariant(s[accelerator+1])+")";
        if(words.TryGetValue(key,out var value))return value+suffix;
        string clean=key.TrimEnd('.','…',':').Trim();
        if(words.TryGetValue(clean,out value))return value+(key.EndsWith("...")?"…":key.EndsWith(":")?"：":"")+suffix;
        // Preserve keyboard hints while translating the known UI label.
        int paren=key.LastIndexOf(" (");
        if(paren>0&&words.TryGetValue(key.Substring(0,paren),out value))return value+key.Substring(paren)+suffix;
        return s;
    }
    static void Property(DependencyObject d,DependencyProperty p) {
        // Preserve bindings and translate only literal UI labels.
        if(BindingOperations.IsDataBound(d,p))return;
        if(d.GetValue(p) is string s) {
            var translated=Translate(s); if(s!=translated){d.SetCurrentValue(p,translated);changed++;}
        }
    }
    static void Scan(DependencyObject d,HashSet<DependencyObject> seen) {
        if(d==null||!seen.Add(d))return;
        // Editable text, browser content and document models are never modified.
        if(d is TextBox || d is PasswordBox || d.GetType().Name.Contains("WebView"))return;
        if(d is FrameworkElement)Property(d,FrameworkElement.ToolTipProperty);
        if(d is TextBlock)Property(d,TextBlock.TextProperty);
        if(d is AccessText)Property(d,AccessText.TextProperty);
        if(d is HeaderedItemsControl)Property(d,HeaderedItemsControl.HeaderProperty);
        if(d is HeaderedContentControl)Property(d,HeaderedContentControl.HeaderProperty);
        if(d is ContentControl && !(d is Window))Property(d,ContentControl.ContentProperty);
        if(d is MenuItem mi)foreach(var item in mi.Items)if(item is DependencyObject c)Scan(c,seen);
        if(d is Visual)for(int i=0;i<VisualTreeHelper.GetChildrenCount(d);i++)Scan(VisualTreeHelper.GetChild(d,i),seen);
        foreach(var child in LogicalTreeHelper.GetChildren(d))if(child is DependencyObject c)Scan(c,seen);
        if(d is FrameworkElement fe && fe.ContextMenu!=null)Scan(fe.ContextMenu,seen);
    }
}
