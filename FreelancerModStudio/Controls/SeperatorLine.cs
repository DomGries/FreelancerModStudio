namespace FreelancerModStudio.Controls
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public class SeperatorLine : Control
    {
        public SeperatorLine()
        {
            // This call is required by the Windows.Forms Form Designer.
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Name = "SeperatorLine";
            this.Size = new Size(100, 2);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.DrawLines(new Pen(SystemColors.ControlDark, 1), new[] { new Point(0, 1), new Point(0, 0), new Point(this.Width, 0) });
            e.Graphics.DrawLines(new Pen(SystemColors.ControlLightLight, 1), new[] { new Point(0, 1), new Point(this.Width, 1) });
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Height = 2;
            this.Invalidate();
        }
    }
}
