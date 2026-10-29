using System.ComponentModel;

namespace MiniBible.App;

public sealed class HorizontalNumberSelector : UserControl
{
    private readonly TextBox valueBox;
    private int minimum=1;
    private int maximum=999;
    private int value=1;

    public HorizontalNumberSelector(string prefix)
    {
        Name=$"{prefix}Selector";
        AutoSize=true; AutoSizeMode=AutoSizeMode.GrowAndShrink; Margin=new(3);
        var previous=new Button { Name=$"{prefix}PreviousButton", Text="◀", AutoSize=true, Margin=Padding.Empty };
        var next=new Button { Name=$"{prefix}NextButton", Text="▶", AutoSize=true, Margin=Padding.Empty };
        valueBox=new TextBox { Name=$"{prefix}Value", Width=64, TextAlign=HorizontalAlignment.Center, Text="1", Margin=new(3,0,3,0) };
        var layout=new TableLayoutPanel { Dock=DockStyle.Fill, AutoSize=true, AutoSizeMode=AutoSizeMode.GrowAndShrink, ColumnCount=3, RowCount=1, Margin=Padding.Empty };
        layout.ColumnStyles.Add(new(SizeType.AutoSize)); layout.ColumnStyles.Add(new(SizeType.AutoSize)); layout.ColumnStyles.Add(new(SizeType.AutoSize));
        layout.Controls.Add(previous,0,0); layout.Controls.Add(valueBox,1,0); layout.Controls.Add(next,2,0); Controls.Add(layout);
        previous.Click+=(_,_)=>Value--;
        next.Click+=(_,_)=>Value++;
        valueBox.Leave+=(_,_)=>CommitText();
        valueBox.KeyDown+=(_,e)=> { if (e.KeyCode==Keys.Enter) { CommitText(); e.SuppressKeyPress=true; } };
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Minimum { get=>minimum; set { minimum=value; Value=this.value; } }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Maximum { get=>maximum; set { maximum=value; Value=this.value; } }
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Value
    {
        get=>value;
        set
        {
            var next=Math.Clamp(value,minimum,maximum);
            if (this.value==next) { valueBox.Text=next.ToString(); return; }
            this.value=next; valueBox.Text=next.ToString(); ValueChanged?.Invoke(this,EventArgs.Empty);
        }
    }
    public event EventHandler? ValueChanged;

    private void CommitText()
    {
        if (int.TryParse(valueBox.Text,out var parsed)) Value=parsed;
        else valueBox.Text=value.ToString();
    }
}
