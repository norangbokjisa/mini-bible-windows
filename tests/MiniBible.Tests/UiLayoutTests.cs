using MiniBible.App;
using MiniBible.Core;
using System.Reflection;

namespace MiniBible.Tests;

public static class UiLayoutTests
{
    public static void Test_HeaderAndFooterMatchCompactNavigationDesign()
    {
        RunSta(() =>
        {
            using var form = new MainForm();
            form.CreateControl(); form.PerformLayout();
            var book = Find<ComboBox>(form, "BookSelector");
            var chapter = Find<HorizontalNumberSelector>(form, "ChapterSelector");
            var verse = Find<HorizontalNumberSelector>(form, "VerseSelector");
            var settings = Find<Button>(form, "SettingsButton");
            var range = Find<Label>(form, "VerseRange");
            AssertEx.True(book.Width >= 230, "book selector should show long names without clipping");
            AssertEx.True(chapter.Width >= 80 && verse.Width >= 80, "chapter and verse selectors should be wider");
            var go = Find<Button>(form, "GoButton");
            AssertEx.True(go.Margin.Left >= 10, "go button should have space after the verse selector");
            AssertEx.Equal("⚙", settings.Text);
            AssertEx.True(range.Font.Size > SystemFonts.DefaultFont.Size, "verse range should use larger text");
        });
    }

    public static void Test_MouseWheelMovesOneVerseAtATime()
    {
        RunSta(() =>
        {
            using var form = new MainForm();
            var data = new BibleData(Enumerable.Range(1, 8).Select(n => new Verse("가상권", 1, n, $"본문 {n}")).ToList());
            typeof(MainForm).GetMethod("SetData", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, [data]);
            var body = Find<RichTextBox>(form, "BibleText");
            typeof(Control).GetMethod("OnMouseWheel", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(body, [new MouseEventArgs(MouseButtons.None, 0, 0, 0, -120)]);
            AssertEx.True(Find<Label>(form, "VerseRange").Text.StartsWith("가상권 1:2-"), "wheel down should move the start by one verse");
        });
    }

    public static void Test_FirstChapterButtonMovesSelectedBookToItsBeginning()
    {
        RunSta(() =>
        {
            using var form=new MainForm();
            var data=new BibleData([new Verse("가상권",3,1,"셋째 장"),new Verse("가상권",1,1,"첫 장")]);
            typeof(MainForm).GetMethod("SetData",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(form,[data]);
            var first=Find<Button>(form,"FirstChapterButton");
            typeof(Button).GetMethod("OnClick",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(first,[EventArgs.Empty]);
            AssertEx.True(Find<Label>(form,"VerseRange").Text.StartsWith("가상권 1:1-"),"first chapter button should move to the first chapter and verse");
        });
    }

    public static void Test_ChapterAndVerseArrowsAreHorizontal()
    {
        RunSta(() =>
        {
            using var form=new MainForm(); form.CreateControl(); form.PerformLayout();
            foreach (var prefix in new[]{"Chapter","Verse"})
            {
                var value=Find<TextBox>(form,$"{prefix}Value");
                var previous=Find<Button>(form,$"{prefix}PreviousButton");
                var next=Find<Button>(form,$"{prefix}NextButton");
                AssertEx.True(previous.Right<=value.Left && next.Left>=value.Right,$"{prefix} arrows should sit left and right of the value");
            }
        });
    }

    public static void Test_SettingsDialogFitsAndOffersFontAndOneToTenVerses()
    {
        RunSta(() =>
        {
            using var dialog = new SettingsForm(new AppSettings(), () => { });
            dialog.CreateControl(); dialog.PerformLayout();
            var fontFamily = Find<ComboBox>(dialog, "FontFamilySelector");
            var count = Find<NumericUpDown>(dialog, "VerseCountSelector");
            AssertEx.Equal(1m, count.Minimum); AssertEx.Equal(10m, count.Maximum);
            AssertEx.True(fontFamily.Items.Count > 0, "installed fonts should be selectable");
            foreach (Control child in Descendants(dialog))
                AssertEx.True(child.Bottom <= dialog.ClientSize.Height, $"{child.Name} is clipped below the settings window");
        });
    }

    public static void Test_DarkThemeColorsInteractiveControls()
    {
        RunSta(() =>
        {
            using var form = new MainForm();
            ThemePalette.Apply(form, ThemeMode.Dark);
            var body = Find<RichTextBox>(form, "BibleText");
            var button = Find<Button>(form, "SettingsButton");
            var go = Find<Button>(form, "GoButton");
            AssertEx.True(body.BackColor.GetBrightness() < .35f, "Bible body should be dark");
            AssertEx.True(button.BackColor.GetBrightness() < .35f && !button.UseVisualStyleBackColor, "button should render dark theme");
            AssertEx.True(go.BackColor.GetBrightness() is > .15f and < .4f, "go button should use a distinct dark gray");
            AssertEx.True(go.ForeColor.GetBrightness() > .8f, "go button text should be white in dark mode");
        });
    }

    public static void Test_ChapterHeadingAppearsOnlyAtChapterBoundary()
    {
        RunSta(() =>
        {
            using var form = new MainForm();
            var data = new BibleData([new Verse("창세기",1,31,"첫 본문"),new Verse("창세기",2,1,"둘째 본문"),new Verse("창세기",2,2,"셋째 본문")]);
            typeof(MainForm).GetMethod("SetData", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, [data]);
            var body = Find<RichTextBox>(form, "BibleText");
            AssertEx.True(body.Text.StartsWith("31. 첫 본문"), "same-chapter verse should use number and text only");
            AssertEx.True(!body.Text.Contains("창세기 1장"), "a heading should not repeat before a non-first verse");
            var heading = body.Text.IndexOf("창세기 2장", StringComparison.Ordinal);
            var verseOne = body.Text.IndexOf("1. 둘째 본문", StringComparison.Ordinal);
            AssertEx.True(heading > 0 && verseOne > heading, "new chapter heading should appear above verse one");
            AssertEx.Equal(1, body.Text.Split("창세기 2장").Length-1);
            body.Select(heading, "창세기 2장".Length);
            AssertEx.True(body.SelectionFont?.Bold == true, "chapter heading should be bold");
        });
    }

    public static void Test_SettingsButtonsAreTallEnoughForTheirText()
    {
        RunSta(() =>
        {
            using var dialog = new SettingsForm(new AppSettings(), () => { });
            dialog.CreateControl(); dialog.PerformLayout();
            foreach (var button in Descendants(dialog).OfType<Button>())
                AssertEx.True(button.Height >= button.PreferredSize.Height,
                    $"button '{button.Text}' height {button.Height} is below preferred {button.PreferredSize.Height}");
        });
    }

    public static void Test_TestamentRadioButtonsFilterTheBookSelector()
    {
        RunSta(() =>
        {
            using var form = new MainForm();
            var data = new BibleData([
                new Verse("창세기",1,1,"구약1"), new Verse("시편",1,1,"구약2"),
                new Verse("마태복음",1,1,"신약1"), new Verse("요한계시록",1,1,"신약2")]);
            typeof(MainForm).GetMethod("SetData", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, [data]);
            var oldTestament = Find<RadioButton>(form, "OldTestamentRadio");
            var newTestament = Find<RadioButton>(form, "NewTestamentRadio");
            var books = Find<ComboBox>(form, "BookSelector");
            AssertEx.True(oldTestament.Checked, "Old Testament should be selected for Genesis");
            AssertEx.True(books.Items.Cast<string>().SequenceEqual(["창세기","시편"]), "old selection should show only Old Testament books");
            newTestament.Checked=true;
            AssertEx.True(books.Items.Cast<string>().SequenceEqual(["마태복음","요한계시록"]), "new selection should show only New Testament books");
        });
    }

    public static void Test_HeaderBookWidthBodySpacingAndDarkColorMatchDesign()
    {
        RunSta(() =>
        {
            using var form = new MainForm();
            form.CreateControl(); form.PerformLayout();
            var books = Find<ComboBox>(form, "BookSelector");
            var body = Find<RichTextBox>(form, "BibleText");
            var header = Find<TableLayoutPanel>(form, "MainHeader");
            AssertEx.True(books.Width >= 170, "book selector should show long book names");
            AssertEx.True(body.Margin.Top >= 8 && body.Margin.Left >= 10 && body.Margin.Right >= 10, "body should have larger top and side spacing");
            ThemePalette.Apply(form, ThemeMode.Dark);
            AssertEx.True(header.BackColor.GetBrightness() < .35f, "header should follow dark theme");
        });
    }

    public static void Test_HeaderWrapsAtMinimumSizeAndCannotShrinkFurther()
    {
        RunSta(() =>
        {
            using var form = new MainForm();
            form.Size=form.MinimumSize; form.CreateControl(); form.PerformLayout();
            var navigation=Find<FlowLayoutPanel>(form,"NavigationPanel");
            var books=Find<ComboBox>(form,"BookSelector");
            AssertEx.True(form.MinimumSize.Width>=520 && form.MinimumSize.Height>=250,"form should enforce a usable minimum size");
            var firstChapter=Find<Button>(form,"FirstChapterButton");
            AssertEx.True(navigation.GetFlowBreak(firstChapter),"navigation should switch to two rows at minimum width");
            foreach (Control child in navigation.Controls)
                AssertEx.True(child.Bottom<=navigation.ClientSize.Height,$"{child.Name} should remain visible in wrapped header");
            form.Width=1200; form.PerformLayout();
            AssertEx.True(!navigation.GetFlowBreak(firstChapter),"navigation should return to one row when wide enough");
        });
    }

    public static void Test_HeaderShrinksToOneControlRowWhenWide()
    {
        RunSta(() =>
        {
            using var form=new MainForm();
            form.Width=1200; form.CreateControl(); form.PerformLayout();
            var header=Find<TableLayoutPanel>(form,"MainHeader");
            var navigation=Find<FlowLayoutPanel>(form,"NavigationPanel");
            var settings=Find<Button>(form,"SettingsButton");
            var expected=Math.Max(navigation.PreferredSize.Height,settings.PreferredSize.Height);
            AssertEx.True(header.Height<=expected+10,$"single-row header {header.Height} should match controls {expected}");
            var wideHeight=header.Height;
            form.Size=form.MinimumSize; form.PerformLayout();
            AssertEx.True(header.Height>wideHeight,"wrapped navigation should be taller than one-row navigation");
        });
    }

    public static void Test_ScrollbarNativeThemeFollowsAppTheme()
    {
        AssertEx.Equal("DarkMode_Explorer",ThemePalette.NativeScrollbarTheme(ThemeMode.Dark));
        AssertEx.Equal("Explorer",ThemePalette.NativeScrollbarTheme(ThemeMode.Light));
        RunSta(() =>
        {
            using var combo=new ComboBox(); combo.CreateControl();
            ThemePalette.ApplyComboBoxDropDownTheme(combo,ThemeMode.Dark);
        });
    }

    public static void Test_AlwaysOnTopCheckboxIsRightAligned()
    {
        RunSta(() =>
        {
            using var form = new MainForm();
            form.CreateControl(); form.PerformLayout();
            var footer=Find<TableLayoutPanel>(form,"MainFooter");
            var alwaysOnTop=Find<CheckBox>(form,"AlwaysOnTopCheckBox");
            var range=Find<Label>(form,"VerseRange");
            AssertEx.True(range.Left<=range.Margin.Left+2,"verse range should sit at the left edge");
            AssertEx.True(footer.ClientSize.Width-alwaysOnTop.Right<=alwaysOnTop.Margin.Right+2,"always-on-top checkbox should sit at the right edge");
            AssertEx.True(!range.Font.Bold,"verse range should use regular font weight");
        });
    }

    public static void Test_MainChromeControlsFitTheirRowsAtCurrentDpi()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                using var form = new MainForm();
                form.CreateControl();
                form.PerformLayout();
                var strips = Descendants(form).OfType<FlowLayoutPanel>().ToList();
                AssertEx.Equal(1, strips.Count);
                foreach (var strip in strips)
                {
                    AssertEx.True(strip.PreferredSize.Height <= strip.ClientSize.Height,
                        $"menu preferred height {strip.PreferredSize.Height} exceeds row {strip.ClientSize.Height}");
                    foreach (Control child in strip.Controls)
                        AssertEx.True(child.Bottom <= strip.ClientSize.Height, $"{child.GetType().Name} bottom {child.Bottom} exceeds row {strip.ClientSize.Height}");
                }
            }
            catch (Exception ex) { failure = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure is not null) throw failure;
    }

    private static IEnumerable<Control> Descendants(Control root)
    {
        foreach (Control child in root.Controls)
        {
            yield return child;
            foreach (var nested in Descendants(child)) yield return nested;
        }
    }

    private static T Find<T>(Control root, string name) where T : Control =>
        Descendants(root).OfType<T>().Single(c => c.Name == name);

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() => { try { action(); } catch (Exception ex) { failure=ex; } });
        thread.SetApartmentState(ApartmentState.STA); thread.Start(); thread.Join();
        if (failure is not null) throw failure;
    }
}
