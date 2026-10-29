using MiniBible.Core;
using System.Runtime.InteropServices;

namespace MiniBible.App;

public static class ThemePalette
{
    public static void Apply(Control root, ThemeMode mode)
    {
        var dark = mode == ThemeMode.Dark;
        var back = dark ? Color.FromArgb(30, 32, 36) : SystemColors.Control;
        var surface = dark ? Color.FromArgb(39, 42, 47) : SystemColors.Window;
        var fore = dark ? Color.FromArgb(235, 235, 235) : SystemColors.ControlText;
        ApplyRecursive(root, back, surface, fore, dark);
        if (root is Form form)
        {
            if (form.IsHandleCreated) ApplyTitleBar(form,mode);
            else form.HandleCreated += (_,_) => ApplyTitleBar(form,mode);
        }
    }

    public static void ApplyTitleBar(Form form, ThemeMode mode)
    {
        if (!OperatingSystem.IsWindows() || !form.IsHandleCreated) return;
        var dark=mode==ThemeMode.Dark ? 1 : 0;
        try { DwmSetWindowAttribute(form.Handle,20,ref dark,sizeof(int)); }
        catch { try { DwmSetWindowAttribute(form.Handle,19,ref dark,sizeof(int)); } catch { } }
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd,int attribute,ref int value,int size);

    public static string NativeScrollbarTheme(ThemeMode mode) => mode==ThemeMode.Dark ? "DarkMode_Explorer" : "Explorer";

    public static void ApplyComboBoxDropDownTheme(ComboBox comboBox,ThemeMode mode)
    {
        if (!OperatingSystem.IsWindows() || !comboBox.IsHandleCreated) return;
        var info=new ComboBoxInfo { Size=Marshal.SizeOf<ComboBoxInfo>() };
        try
        {
            if (!GetComboBoxInfo(comboBox.Handle,ref info) || info.ListHandle==IntPtr.Zero) return;
            SetWindowTheme(info.ListHandle,NativeScrollbarTheme(mode),null);
            SendMessage(info.ListHandle,0x031A,IntPtr.Zero,IntPtr.Zero);
        }
        catch { }
    }

    private static void ApplyNativeScrollbarTheme(Control control,ThemeMode mode)
    {
        if (!OperatingSystem.IsWindows()) return;
        void ApplyTheme()
        {
            try
            {
                SetWindowTheme(control.Handle,NativeScrollbarTheme(mode),null);
                SendMessage(control.Handle,0x031A,IntPtr.Zero,IntPtr.Zero);
            }
            catch { }
        }
        if (control.IsHandleCreated) ApplyTheme(); else control.HandleCreated += (_,_) => ApplyTheme();
    }

    private static void ApplyRecursive(Control control, Color back, Color surface, Color fore, bool dark)
    {
        control.ForeColor = fore;
        control.BackColor = control is TextBoxBase or ComboBox or NumericUpDown ? surface : back;
        if (control is Button button)
        {
            button.UseVisualStyleBackColor=false; button.FlatStyle=FlatStyle.Flat;
            if (button.Name=="GoButton" && dark) { button.BackColor=Color.FromArgb(65,68,74); button.ForeColor=Color.White; }
        }
        if (control is CheckBox checkBox) checkBox.UseVisualStyleBackColor=false;
        if (control is ComboBox comboBox) comboBox.FlatStyle=FlatStyle.Flat;
        if (control is TextBoxBase) ApplyNativeScrollbarTheme(control,fore.GetBrightness()>.5f?ThemeMode.Dark:ThemeMode.Light);
        foreach (Control child in control.Controls) ApplyRecursive(child, back, surface, fore, dark);
    }

    [DllImport("uxtheme.dll",CharSet=CharSet.Unicode)]
    private static extern int SetWindowTheme(IntPtr hwnd,string? subAppName,string? subIdList);

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hwnd,uint message,IntPtr wParam,IntPtr lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetComboBoxInfo(IntPtr hwnd,ref ComboBoxInfo info);

    [StructLayout(LayoutKind.Sequential)]
    private struct ComboBoxInfo
    {
        public int Size;
        public Rectangle ItemRectangle;
        public Rectangle ButtonRectangle;
        public int ButtonState;
        public IntPtr ComboHandle;
        public IntPtr EditHandle;
        public IntPtr ListHandle;
    }
}
