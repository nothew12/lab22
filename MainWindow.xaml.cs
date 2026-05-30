using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace WpfTextEditor;

public partial class MainWindow : Window
{
    
    private string _lang = "uk";

    private static readonly Dictionary<string, Dictionary<string, string>> L = new()
    {
        ["title"] = new() { ["uk"] = "WPF Текстовий Редактор", ["en"] = "WPF Text Editor", ["pl"] = "Edytor tekstu WPF" },
        ["open"] = new() { ["uk"] = "Відкрити", ["en"] = "Open", ["pl"] = "Otwórz" },
        ["save"] = new() { ["uk"] = "Зберегти", ["en"] = "Save", ["pl"] = "Zapisz" },
        ["bold"] = new() { ["uk"] = "Жирний (Ctrl+B)", ["en"] = "Bold (Ctrl+B)", ["pl"] = "Pogrubienie (Ctrl+B)" },
        ["italic"] = new() { ["uk"] = "Курсив (Ctrl+I)", ["en"] = "Italic (Ctrl+I)", ["pl"] = "Kursywa (Ctrl+I)" },
        ["underline"] = new() { ["uk"] = "Підкреслений (Ctrl+U)", ["en"] = "Underline (Ctrl+U)", ["pl"] = "Podkreślenie (Ctrl+U)" },
        ["alignleft"] = new() { ["uk"] = "По лівому краю", ["en"] = "Align Left", ["pl"] = "Do lewej" },
        ["aligncenter"] = new() { ["uk"] = "По центру", ["en"] = "Center", ["pl"] = "Środek" },
        ["alignright"] = new() { ["uk"] = "По правому краю", ["en"] = "Align Right", ["pl"] = "Do prawej" },
        ["fontfamily"] = new() { ["uk"] = "Сімейство шрифту", ["en"] = "Font Family", ["pl"] = "Rodzina czcionek" },
        ["fontsize"] = new() { ["uk"] = "Розмір шрифту", ["en"] = "Font Size", ["pl"] = "Rozmiar czcionki" },
        ["color"] = new() { ["uk"] = "Колір тексту", ["en"] = "Text Color", ["pl"] = "Kolor tekstu" },
        ["language"] = new() { ["uk"] = "Мова інтерфейсу", ["en"] = "Interface Language", ["pl"] = "Język interfejsu" },
        ["ready"] = new() { ["uk"] = "Готово", ["en"] = "Ready", ["pl"] = "Gotowy" },
        ["modified"] = new() { ["uk"] = "Змінено", ["en"] = "Modified", ["pl"] = "Zmieniono" },
        ["words"] = new() { ["uk"] = "Слів:", ["en"] = "Words:", ["pl"] = "Słów:" },
        ["filter_rtf"] = new()
        {
            ["uk"] = "RTF файли (*.rtf)|*.rtf|Текстові файли (*.txt)|*.txt|Всі файли (*.*)|*.*",
            ["en"] = "RTF files (*.rtf)|*.rtf|Text files (*.txt)|*.txt|All files (*.*)|*.*",
            ["pl"] = "Pliki RTF (*.rtf)|*.rtf|Pliki tekstowe (*.txt)|*.txt|Wszystkie pliki (*.*)|*.*"
        },
    };

    private string T(string key) =>
        L.TryGetValue(key, out var d) && d.TryGetValue(_lang, out var v) ? v : key;

    public MainWindow()
    {
        InitializeComponent();

        cmbFontFamily.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
        cmbFontSize.ItemsSource = new List<double> { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };

        cmbFontFamily.SelectedItem = Fonts.SystemFontFamilies
            .FirstOrDefault(f => f.Source == "Segoe UI") ?? Fonts.SystemFontFamilies.First();
        cmbFontSize.Text = "14";

        ApplyLanguage();
    }

    private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var dlg = new OpenFileDialog { Filter = T("filter_rtf") };
        if (dlg.ShowDialog() != true) return;

        using var fs = new FileStream(dlg.FileName, FileMode.Open);
        var range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
        string fmt = dlg.FileName.EndsWith(".rtf", System.StringComparison.OrdinalIgnoreCase)
            ? DataFormats.Rtf : DataFormats.Text;
        range.Load(fs, fmt);

        Title = $"{T("title")} — {System.IO.Path.GetFileName(dlg.FileName)}";
        tbStatus.Text = T("ready");
    }

    private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        var dlg = new SaveFileDialog { Filter = T("filter_rtf") };
        if (dlg.ShowDialog() != true) return;

        using var fs = new FileStream(dlg.FileName, FileMode.Create);
        var range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
        string fmt = dlg.FileName.EndsWith(".rtf", System.StringComparison.OrdinalIgnoreCase)
            ? DataFormats.Rtf : DataFormats.Text;
        range.Save(fs, fmt);

        Title = $"{T("title")} — {System.IO.Path.GetFileName(dlg.FileName)}";
        tbStatus.Text = T("ready");
    }

    private void rtbEditor_SelectionChanged(object sender, RoutedEventArgs e)
    {         
        object temp = rtbEditor.Selection.GetPropertyValue(Inline.FontWeightProperty);
        btnBold.IsChecked = temp != DependencyProperty.UnsetValue && temp.Equals(FontWeights.Bold);

        
        temp = rtbEditor.Selection.GetPropertyValue(Inline.FontStyleProperty);
        btnItalic.IsChecked = temp != DependencyProperty.UnsetValue && temp.Equals(FontStyles.Italic);

        temp = rtbEditor.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
        btnUnderline.IsChecked = temp != DependencyProperty.UnsetValue && temp.Equals(TextDecorations.Underline);

       
        temp = rtbEditor.Selection.GetPropertyValue(Inline.FontFamilyProperty);
        if (temp != DependencyProperty.UnsetValue)
            cmbFontFamily.SelectedItem = temp;

       
        temp = rtbEditor.Selection.GetPropertyValue(Inline.FontSizeProperty);
        if (temp != DependencyProperty.UnsetValue)
            cmbFontSize.Text = temp.ToString();

        
        temp = rtbEditor.Selection.GetPropertyValue(Paragraph.TextAlignmentProperty);
        if (temp != DependencyProperty.UnsetValue)
        {
            var align = (TextAlignment)temp;
            btnAlignLeft.IsChecked = align == TextAlignment.Left;
            btnAlignCenter.IsChecked = align == TextAlignment.Center;
            btnAlignRight.IsChecked = align == TextAlignment.Right;
        }
    }

    
    private void cmbFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cmbFontFamily.SelectedItem != null)
            rtbEditor.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, cmbFontFamily.SelectedItem);
    }

    private void cmbFontSize_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (double.TryParse(cmbFontSize.Text, out double size) && size > 0)
            rtbEditor.Selection.ApplyPropertyValue(Inline.FontSizeProperty, size);
    }

    private void AlignLeft_Click(object sender, RoutedEventArgs e)
    {
        rtbEditor.Selection.ApplyPropertyValue(Paragraph.TextAlignmentProperty, TextAlignment.Left);
        UpdateAlignButtons(TextAlignment.Left);
    }

    private void AlignCenter_Click(object sender, RoutedEventArgs e)
    {
        rtbEditor.Selection.ApplyPropertyValue(Paragraph.TextAlignmentProperty, TextAlignment.Center);
        UpdateAlignButtons(TextAlignment.Center);
    }

    private void AlignRight_Click(object sender, RoutedEventArgs e)
    {
        rtbEditor.Selection.ApplyPropertyValue(Paragraph.TextAlignmentProperty, TextAlignment.Right);
        UpdateAlignButtons(TextAlignment.Right);
    }

    private void UpdateAlignButtons(TextAlignment a)
    {
        btnAlignLeft.IsChecked = a == TextAlignment.Left;
        btnAlignCenter.IsChecked = a == TextAlignment.Center;
        btnAlignRight.IsChecked = a == TextAlignment.Right;
    }

    private void ColorPicker_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new System.Windows.Forms.ColorDialog();
        if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            var c = dlg.Color;
            var brush = new SolidColorBrush(Color.FromRgb(c.R, c.G, c.B));
            rtbEditor.Selection.ApplyPropertyValue(Inline.ForegroundProperty, brush);
        }
    }

    private void rtbEditor_TextChanged(object sender, TextChangedEventArgs e)
    {
        var text = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd).Text;
        int words = string.IsNullOrWhiteSpace(text)
            ? 0
            : text.Split(new char[] { ' ', '\n', '\r', '\t' }, System.StringSplitOptions.RemoveEmptyEntries).Length;

        tbWordCount.Text = $"{T("words")} {words}";
        tbStatus.Text = T("modified");
    }

    private void cmbLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return;

        if (cmbLanguage.SelectedItem is ComboBoxItem item && item.Tag is string tag)
        {
            _lang = tag;
            ApplyLanguage();
        }
    }

    private void ApplyLanguage()
    {
        Title = T("title");

        
        btnBold.ToolTip = T("bold");
        btnItalic.ToolTip = T("italic");
        btnUnderline.ToolTip = T("underline");
        btnAlignLeft.ToolTip = T("alignleft");
        btnAlignCenter.ToolTip = T("aligncenter");
        btnAlignRight.ToolTip = T("alignright");
        cmbFontFamily.ToolTip = T("fontfamily");
        cmbFontSize.ToolTip = T("fontsize");
        cmbLanguage.ToolTip = T("language");

        tbStatus.Text = T("ready");
        tbWordCount.Text = $"{T("words")} 0";
    }
}