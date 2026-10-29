using MiniBible.Core;
using System.Runtime.InteropServices;

namespace MiniBible.App;

public sealed class MainForm : Form
{
    private const int EmSetParaFormat = 0x447;
    private const uint PfmSpaceAfter = 0x00000080;

    [StructLayout(LayoutKind.Sequential)]
    private struct ParaFormat2
    {
        public uint Size;
        public uint Mask;
        public short Numbering;
        public short Reserved;
        public int StartIndent;
        public int RightIndent;
        public int Offset;
        public short Alignment;
        public short TabCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=32)] public int[] Tabs;
        public int SpaceBefore;
        public int SpaceAfter;
        public int LineSpacing;
        public short Style;
        public byte LineSpacingRule;
        public byte OutlineLevel;
        public short ShadingWeight;
        public short ShadingStyle;
        public short NumberingStart;
        public short NumberingStyle;
        public short NumberingTab;
        public short BorderSpace;
        public short BorderWidth;
        public short Borders;
    }

    [DllImport("user32.dll", CharSet=CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr window, int message, IntPtr parameter, ref ParaFormat2 format);

    private static readonly HashSet<string> OldTestamentBooks = new([
        "창세기","출애굽기","레위기","민수기","신명기","여호수아","사사기","룻기","사무엘상","사무엘하",
        "열왕기상","열왕기하","역대상","역대하","에스라","느헤미야","에스더","욥기","시편","잠언",
        "전도서","아가","이사야","예레미야","예레미야애가","에스겔","다니엘","호세아","요엘","아모스",
        "오바댜","요나","미가","나훔","하박국","스바냐","학개","스가랴","말라기"]);
    private readonly SettingsStore store = new();
    private AppSettings settings;
    private BibleNavigator? navigator;
    private VerseReference current = new("",1,1);
    private readonly RadioButton oldTestament = new() { Name="OldTestamentRadio", Text="구약", AutoSize=true };
    private readonly RadioButton newTestament = new() { Name="NewTestamentRadio", Text="신약", AutoSize=true };
    private readonly ComboBox books = new() { Name="BookSelector", DropDownStyle=ComboBoxStyle.DropDownList, Width=240, DropDownWidth=280 };
    private readonly HorizontalNumberSelector chapter = new("Chapter") { Minimum=1, Maximum=999 };
    private readonly HorizontalNumberSelector verse = new("Verse") { Minimum=1, Maximum=999 };
    private readonly RichTextBox text = new() { Name="BibleText", ReadOnly=true, BorderStyle=BorderStyle.None, Dock=DockStyle.Fill, Margin=new(10,9,10,3), ScrollBars=RichTextBoxScrollBars.Vertical };
    private readonly Label range = new() { Name="VerseRange", AutoSize=true, Anchor=AnchorStyles.Left };
    private readonly CheckBox top = new() { Name="AlwaysOnTopCheckBox", Text="항상 위", AutoSize=true };

    public MainForm()
    {
        AutoScaleMode=AutoScaleMode.Dpi;
        settings=store.Load(AppPaths.SettingsJson);
        Text="미니 성경 V2.10"; ClientSize=new(settings.WindowWidth,settings.WindowHeight); MinimumSize=new(760,260);
        StartPosition=FormStartPosition.CenterScreen; TopMost=settings.TopMost;
        if (settings.WindowX >= 0 && Screen.AllScreens.Any(s => s.WorkingArea.Contains(settings.WindowX,settings.WindowY)))
        { StartPosition=FormStartPosition.Manual; Location=new(settings.WindowX,settings.WindowY); }
        BuildUi(); ApplySettings();
        Load += (_,_) => LoadInstalledData();
        Shown += (_,_) => { if (navigator is null) PromptImport(); };
        FormClosing += (_,_) => SaveState();
    }

    private void BuildUi()
    {
        var root=new TableLayoutPanel { Dock=DockStyle.Fill, RowCount=3, ColumnCount=1, Padding=new(5) };
        root.RowStyles.Add(new(SizeType.Absolute,1)); root.RowStyles.Add(new(SizeType.Percent,100)); root.RowStyles.Add(new(SizeType.AutoSize));
        var header=new TableLayoutPanel { Name="MainHeader", Dock=DockStyle.Fill, AutoSize=false, ColumnCount=2, RowCount=1, Margin=Padding.Empty };
        header.RowStyles.Add(new(SizeType.Percent,100));
        header.ColumnStyles.Add(new(SizeType.Percent,100)); header.ColumnStyles.Add(new(SizeType.AutoSize));
        var navigation=new FlowLayoutPanel { Name="NavigationPanel", Dock=DockStyle.Top, AutoSize=true, AutoSizeMode=AutoSizeMode.GrowAndShrink, WrapContents=true, Margin=Padding.Empty };
        var firstChapter=new Button { Name="FirstChapterButton", Text="첫장 가기", AutoSize=true };
        var go=new Button { Name="GoButton", Text="이동", AutoSize=true, Margin=new(10,3,3,3) };
        navigation.Controls.AddRange([oldTestament,newTestament,books,firstChapter,new Label{Text="장",AutoSize=true,Margin=new(4,8,0,0)},chapter,new Label{Text="절",AutoSize=true,Margin=new(4,8,0,0)},verse,go]);
        var options=new Button { Name="SettingsButton", Text="⚙", AccessibleName="설정", Font=new Font("Segoe UI Symbol",12), AutoSize=true, Margin=new(3,0,0,0) };
        header.Controls.Add(navigation,0,0); header.Controls.Add(options,1,0);
        var footer=new TableLayoutPanel { Name="MainFooter", Dock=DockStyle.Fill, AutoSize=true, AutoSizeMode=AutoSizeMode.GrowAndShrink, ColumnCount=2, RowCount=1, Margin=Padding.Empty };
        footer.ColumnStyles.Add(new(SizeType.Percent,100)); footer.ColumnStyles.Add(new(SizeType.AutoSize));
        footer.Controls.Add(range,0,0); footer.Controls.Add(top,1,0); top.Anchor=AnchorStyles.Right;
        root.Controls.Add(header,0,0); root.Controls.Add(text,0,1); root.Controls.Add(footer,0,2); Controls.Add(root);
        books.SelectedIndexChanged += (_,_) => UpdateChapters();
        books.DropDown += (_,_) => ThemePalette.ApplyComboBoxDropDownTheme(books,settings.Theme);
        oldTestament.CheckedChanged += (_,_) => { if (oldTestament.Checked) RefreshBookFilter(); };
        newTestament.CheckedChanged += (_,_) => { if (newTestament.Checked) RefreshBookFilter(); };
        chapter.ValueChanged += (_,_) => UpdateVerses();
        go.Click += (_,_) => NavigateSelected();
        firstChapter.Click += (_,_) => NavigateFirstChapter();
        text.MouseWheel += (_,e) => MoveByWheel(e.Delta);
        top.CheckedChanged += (_,_) => TopMost=top.Checked;
        options.Click += (_,_) => OpenSettings();
        void UpdateHeaderRows()
        {
            navigation.SetFlowBreak(firstChapter,ClientSize.Width<1000);
            navigation.MinimumSize=Size.Empty;
            navigation.PerformLayout();
            var navigationHeight=navigation.Controls.Cast<Control>().Max(control=>control.Bottom)+2;
            navigation.MinimumSize=new Size(0,navigationHeight);
            root.RowStyles[0].Height=Math.Max(navigationHeight,options.PreferredSize.Height)+8;
            root.PerformLayout(); header.PerformLayout();
        }
        SizeChanged += (_,_) => UpdateHeaderRows();
        UpdateHeaderRows();
    }

    private void LoadInstalledData()
    {
        if (!File.Exists(AppPaths.BibleData)) return;
        try { SetData(JsonBibleImporter.Parse(File.ReadAllText(AppPaths.BibleData))); }
        catch (Exception ex) { MessageBox.Show($"저장된 CSV를 읽을 수 없습니다.\n{ex.Message}","미니 성경",MessageBoxButtons.OK,MessageBoxIcon.Warning); }
    }

    private void PromptImport()
    {
        if (MessageBox.Show("성경 본문이 없습니다. 권한 있는 CSV를 가져오시겠습니까?","미니 성경",MessageBoxButtons.YesNo,MessageBoxIcon.Information)==DialogResult.Yes) ImportCsv();
    }

    private void ImportCsv()
    {
        using var dialog=new OpenFileDialog { Filter="성경 데이터 (*.json;*.csv)|*.json;*.csv|JSON 파일 (*.json)|*.json|CSV 파일 (*.csv)|*.csv",Title="성경 JSON 또는 CSV 가져오기" };
        if (dialog.ShowDialog(this)!=DialogResult.OK) return;
        try { SetData(ImportService.ValidateAndInstall(dialog.FileName,AppPaths.BibleData)); MessageBox.Show("성경 파일을 연결했습니다.","미니 성경"); }
        catch (Exception ex) { MessageBox.Show($"가져오지 못했습니다. 기존 데이터는 유지됩니다.\n{ex.Message}","미니 성경",MessageBoxButtons.OK,MessageBoxIcon.Error); }
    }

    private void SetData(BibleData data)
    {
        navigator=new(data);
        var wanted=navigator.Clamp(new(settings.LastBook,settings.LastChapter,settings.LastVerse)); current=wanted;
        oldTestament.Checked=OldTestamentBooks.Contains(wanted.Book); newTestament.Checked=!oldTestament.Checked;
        RefreshBookFilter(wanted.Book);
        books.SelectedItem=wanted.Book; chapter.Value=wanted.Chapter; verse.Value=wanted.Verse; Render();
    }

    private void RefreshBookFilter(string? preferredBook=null)
    {
        if (navigator is null) return;
        preferredBook ??=books.SelectedItem as string;
        var filtered=navigator.GetBooks().Where(book=>oldTestament.Checked==OldTestamentBooks.Contains(book)).ToArray();
        books.BeginUpdate(); books.Items.Clear(); books.Items.AddRange(filtered.Cast<object>().ToArray());
        if (preferredBook is not null && filtered.Contains(preferredBook)) books.SelectedItem=preferredBook;
        else if (books.Items.Count>0) books.SelectedIndex=0;
        books.EndUpdate();
    }

    private void UpdateChapters()
    {
        if (navigator is null || books.SelectedItem is not string book) return;
        var values=navigator.GetChapters(book); chapter.Minimum=values.Min(); chapter.Maximum=values.Max();
        if (!values.Contains((int)chapter.Value)) chapter.Value=values[0]; UpdateVerses();
    }

    private void UpdateVerses()
    {
        if (navigator is null || books.SelectedItem is not string book) return;
        var values=navigator.GetVerses(book,(int)chapter.Value); if (values.Count==0) return;
        verse.Minimum=values.Min(); verse.Maximum=values.Max(); if (!values.Contains((int)verse.Value)) verse.Value=values[0];
    }

    private void NavigateSelected()
    {
        if (navigator is null || books.SelectedItem is not string book) return;
        current=navigator.Clamp(new(book,(int)chapter.Value,(int)verse.Value)); SyncAndRender();
    }

    private void NavigateFirstChapter()
    {
        if (navigator is null || books.SelectedItem is not string book) return;
        var firstChapter=navigator.GetChapters(book).Min();
        var firstVerse=navigator.GetVerses(book,firstChapter).Min();
        current=navigator.Clamp(new(book,firstChapter,firstVerse)); SyncAndRender();
    }

    private void MovePage(int direction) { if (navigator is null) return; current=navigator.MoveWindow(current,settings.VersesPerPage,direction); SyncAndRender(); }
    private void MoveByWheel(int delta) { if (navigator is null || delta==0) return; current=navigator.MoveWindow(current,1,delta<0?1:-1); SyncAndRender(); }
    private void SyncAndRender() { books.SelectedItem=current.Book; chapter.Value=current.Chapter; verse.Value=current.Verse; Render(); }

    private void Render()
    {
        if (navigator is null) { text.Text="설정에서 권한 있는 성경 CSV를 가져오세요."; range.Text="본문 없음"; return; }
        var items=navigator.GetWindow(current,settings.VersesPerPage);
        text.Clear();
        using (var boldFont=new Font(text.Font,FontStyle.Bold))
            for (var i=0;i<items.Count;i++)
            {
                var item=items[i];
                var beginsChapter=item.Number==1 || (i>0 && (items[i-1].Book!=item.Book || items[i-1].Chapter!=item.Chapter));
                if (beginsChapter)
                {
                    if (i>0) text.AppendText(Environment.NewLine+Environment.NewLine);
                    text.SelectionFont=boldFont; text.AppendText($"{item.Book} {item.Chapter}장");
                    text.AppendText(Environment.NewLine+Environment.NewLine);
                }
                else if (i>0) text.AppendText(Environment.NewLine);
                text.SelectionFont=text.Font;
                var verseStart=text.TextLength;
                text.AppendText($"{item.NumberLabel}. {item.Text}");
                var nextContinuesChapter=i+1<items.Count && items[i+1].Book==item.Book && items[i+1].Chapter==item.Chapter && items[i+1].Number!=1;
                var spaceAfter=nextContinuesChapter?(int)Math.Round(text.Font.SizeInPoints*10f):0;
                SetParagraphSpaceAfter(verseStart,text.TextLength-verseStart,spaceAfter);
            }
        range.Text=items.Count==0?"":$"{current.Book} {items[0].Chapter}:{items[0].NumberLabel}-{items[^1].Chapter}:{items[^1].NumberLabel}";
    }

    private void SetParagraphSpaceAfter(int start, int length, int twips)
    {
        text.Select(start,length);
        var format=new ParaFormat2
        {
            Size=(uint)Marshal.SizeOf<ParaFormat2>(),
            Mask=PfmSpaceAfter,
            Tabs=new int[32],
            SpaceAfter=Math.Max(0,twips)
        };
        SendMessage(text.Handle,EmSetParaFormat,IntPtr.Zero,ref format);
        text.Select(text.TextLength,0);
    }

    private void OpenSettings()
    {
        using var dialog=new SettingsForm(settings,ImportCsv); ThemePalette.Apply(dialog,settings.Theme);
        if (dialog.ShowDialog(this)!=DialogResult.OK) return;
        settings=dialog.Result.Normalize(); ClientSize=new(settings.WindowWidth,settings.WindowHeight); ApplySettings(); Render();
    }

    private void ApplySettings()
    {
        top.Checked=settings.TopMost; TopMost=settings.TopMost;
        var family=FontFamily.Families.Any(f=>f.Name==settings.FontFamily)?settings.FontFamily:"맑은 고딕";
        text.Font=new Font(family,settings.FontSize,FontStyle.Regular,GraphicsUnit.Point);
        range.Font=new Font(family,Math.Max(11f,settings.FontSize-1),FontStyle.Regular,GraphicsUnit.Point);
        ThemePalette.Apply(this,settings.Theme);
    }

    private void SaveState()
    {
        if (WindowState==FormWindowState.Normal) { settings.WindowWidth=ClientSize.Width; settings.WindowHeight=ClientSize.Height; settings.WindowX=Left; settings.WindowY=Top; }
        settings.TopMost=TopMost; settings.LastBook=current.Book; settings.LastChapter=current.Chapter; settings.LastVerse=current.Verse;
        try { store.Save(AppPaths.SettingsJson,settings); } catch { }
    }
}

