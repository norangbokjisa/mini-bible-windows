using MiniBible.Core;

namespace MiniBible.App;

public sealed class SettingsForm : Form
{
    private readonly NumericUpDown width = new() { Minimum=300, Maximum=1600 };
    private readonly NumericUpDown height = new() { Minimum=220, Maximum=1200 };
    private readonly NumericUpDown count = new() { Name="VerseCountSelector", Minimum=1, Maximum=10 };
    private readonly NumericUpDown font = new() { Minimum=9, Maximum=28, DecimalPlaces=1, Increment=.5m };
    private readonly ComboBox fontFamily = new() { Name="FontFamilySelector", DropDownStyle=ComboBoxStyle.DropDownList };
    private readonly ComboBox theme = new() { DropDownStyle=ComboBoxStyle.DropDownList };
    private readonly Action import;
    public AppSettings Result { get; private set; }

    public SettingsForm(AppSettings source, Action importAction)
    {
        AutoScaleMode = AutoScaleMode.Dpi;
        import = importAction;
        Result = new AppSettings {
            WindowWidth=source.WindowWidth, WindowHeight=source.WindowHeight, WindowX=source.WindowX, WindowY=source.WindowY,
            VersesPerPage=source.VersesPerPage, FontSize=source.FontSize, FontFamily=source.FontFamily, Theme=source.Theme, TopMost=source.TopMost,
            LastBook=source.LastBook, LastChapter=source.LastChapter, LastVerse=source.LastVerse
        };
        Text = "설정"; ClientSize = new(380, 460); MinimumSize=new(380,460); FormBorderStyle = FormBorderStyle.FixedDialog;
        AutoScroll=true;
        MaximizeBox = false; MinimizeBox = false; StartPosition = FormStartPosition.CenterParent;
        theme.Items.AddRange(["시스템", "라이트", "다크"]);
        fontFamily.Items.AddRange(FontFamily.Families.Select(f => f.Name).OrderBy(n => n).Cast<object>().ToArray());
        width.Value=Result.WindowWidth; height.Value=Result.WindowHeight; count.Value=Result.VersesPerPage;
        font.Value=(decimal)Result.FontSize; fontFamily.SelectedItem=Result.FontFamily;
        if (fontFamily.SelectedIndex<0 && fontFamily.Items.Count>0) fontFamily.SelectedIndex=0;
        theme.SelectedIndex=(int)Result.Theme;

        var grid = new TableLayoutPanel { Dock=DockStyle.Fill, Padding=new(12), ColumnCount=2, RowCount=8 };
        grid.ColumnStyles.Add(new(SizeType.Percent,45)); grid.ColumnStyles.Add(new(SizeType.Percent,55));
        for (var row=0; row<8; row++) grid.RowStyles.Add(new(SizeType.AutoSize));
        AddRow(grid, "창 너비", width, 0); AddRow(grid, "창 높이", height, 1);
        AddRow(grid, "표시 절 수", count, 2); AddRow(grid, "글꼴", fontFamily, 3);
        AddRow(grid, "글꼴 크기", font, 4); AddRow(grid, "테마", theme, 5);
        var importButton = new Button { Text="JSON/CSV 연결…", Dock=DockStyle.Fill, AutoSize=true, Padding=new(8,4,8,4) };
        importButton.Click += (_,_) => import();
        grid.Controls.Add(importButton, 0, 6); grid.SetColumnSpan(importButton, 2);
        var defaults = new Button { Text="기본값", AutoSize=true, Padding=new(8,4,8,4) }; defaults.Click += (_,_) => { width.Value=340; height.Value=240; count.Value=5; fontFamily.SelectedItem="맑은 고딕"; font.Value=12; theme.SelectedIndex=0; };
        var ok = new Button { Text="저장", AutoSize=true, Padding=new(8,4,8,4), DialogResult=DialogResult.OK }; ok.Click += Save;
        var cancel = new Button { Text="취소", AutoSize=true, Padding=new(8,4,8,4), DialogResult=DialogResult.Cancel };
        var buttons = new FlowLayoutPanel { Dock=DockStyle.Fill, AutoSize=true, AutoSizeMode=AutoSizeMode.GrowAndShrink, FlowDirection=FlowDirection.RightToLeft, WrapContents=false };
        buttons.Controls.AddRange([ok,cancel,defaults]); grid.Controls.Add(buttons,0,7); grid.SetColumnSpan(buttons,2);
        Controls.Add(grid); AcceptButton=ok; CancelButton=cancel;
    }

    private static void AddRow(TableLayoutPanel grid, string label, Control control, int row)
    {
        grid.Controls.Add(new Label { Text=label, AutoSize=true, Anchor=AnchorStyles.Left },0,row);
        control.Dock=DockStyle.Fill; grid.Controls.Add(control,1,row);
    }

    private void Save(object? sender, EventArgs e)
    {
        Result.WindowWidth=(int)width.Value; Result.WindowHeight=(int)height.Value;
        Result.VersesPerPage=(int)count.Value; Result.FontFamily=fontFamily.SelectedItem?.ToString() ?? "맑은 고딕";
        Result.FontSize=(float)font.Value; Result.Theme=(ThemeMode)theme.SelectedIndex;
    }
}
