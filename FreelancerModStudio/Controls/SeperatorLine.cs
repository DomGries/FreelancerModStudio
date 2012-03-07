using System;
using System.Drawing;
using System.Windows.Forms;

namespace FreelancerModStudio.Controls
{
    public class SeperatorLine : Control
    {
        public SeperatorLine()
        {
            //This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
        }

        void InitializeComponent()
        {
            Name = "SeperatorLine";
            Size = new Size(100, 2);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.DrawLines(new Pen(SystemColors.ControlDark, 1), new[] { new Point(0, 1), new Point(0, 0), new Point(Width, 0) });
            e.Graphics.DrawLines(new Pen(SystemColors.ControlLightLight, 1), new[] { new Point(0, 1), new Point(Width, 1) });
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Height = 2;
            Invalidate();
        }

    }
}
