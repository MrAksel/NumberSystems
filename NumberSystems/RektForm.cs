using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NumberSystems
{
    public partial class RektForm : Form
    {
        Random r = new Random();
        Bitmap[] bms;

        public RektForm()
        {
            InitializeComponent();
            bms = new Bitmap[5];

            bms[0] = Properties.Resources.congruence; //bms[0].MakeTransparent(Color.White);
            bms[1] = Properties.Resources.ex; //bms[1].MakeTransparent(Color.White);
            bms[2] = Properties.Resources.pi; //bms[2].MakeTransparent(Color.Black);
            bms[3] = Properties.Resources.sigma;
            bms[4] = Properties.Resources.Z;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //base.OnPaintBackground(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            for (int y = 0; y < e.ClipRectangle.Height; y += e.ClipRectangle.Height / 9)
            {
                for (int x = 0; x < e.ClipRectangle.Width; x += e.ClipRectangle.Width / 16)
                {
                    Rectangle rect = new Rectangle(x, y, e.ClipRectangle.Width / 16, e.ClipRectangle.Height / 9);
                    Color c = ColorFromHSV(r.NextDouble() * 360.0, 1.0, 1.0);
                    e.Graphics.FillRectangle(new SolidBrush(c), rect);

                    if (r.Next() % 10 == 0)
                    {
                        Bitmap img = bms[r.Next(5)];
                        e.Graphics.DrawImage(img, rect);
                    }
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            base.OnClosing(e);
        }

        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, v, t, p);
            else if (hi == 1)
                return Color.FromArgb(255, q, v, p);
            else if (hi == 2)
                return Color.FromArgb(255, p, v, t);
            else if (hi == 3)
                return Color.FromArgb(255, p, q, v);
            else if (hi == 4)
                return Color.FromArgb(255, t, p, v);
            else
                return Color.FromArgb(255, v, p, q);
        }

        private void RektForm_MouseMove(object sender, MouseEventArgs e)
        {
            Invalidate();
        }
    }
}
