using System.ComponentModel;
using System.Drawing;

public class SeperatorLine : System.Windows.Forms.Control
{

    private System.ComponentModel.Container components = null;

    public SeperatorLine()
    {
        //This call is required by the Windows.Forms Form Designer.
        InitializeComponent();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if ((components != null))
            {
                components.Dispose();
            }
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.Name = "SeperatorLine";
        this.Size = new System.Drawing.Size(100, 2);
    }

    protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.DrawLines(new Pen(SystemColors.ControlDark, 1), new Point[] { new Point(0, 1), new Point(0, 0), new Point(this.Width, 0) });
        e.Graphics.DrawLines(new Pen(SystemColors.ControlLightLight, 1), new Point[] { new Point(0, 1), new Point(this.Width, 1) });
    }

    protected override void OnResize(System.EventArgs e)
    {
        base.OnResize(e);
        this.Height = 2;
        this.Invalidate();
    }

}
