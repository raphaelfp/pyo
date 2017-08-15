using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MyoSharp.Util
{
    public class Util
    {
        #region Pyo

        public static Bitmap ColoreBotao(int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                Rectangle r = new Rectangle(0, 0, bmp.Width, bmp.Height);
                using (LinearGradientBrush br = new LinearGradientBrush(
                                                    r,
                                                    Color.Red,
                                                    Color.DarkRed,
                                                    LinearGradientMode.Vertical))
                {
                    g.FillRectangle(br, r);
                }
            }
            return bmp;
        }

        public static bool EsperaSegundos(DateTime hora, int segundos)
        {
            if (hora <= DateTime.Now.AddSeconds(2))
                return false;
            else
            {
                hora = DateTime.Now;
                return true;
            }
        }

        #endregion

        #region TP1CG
        public static int CorrigeX(int x) { return x + 20; }
        public static int CorrigeY(int y) { return (y * -1) + 500; }
        public static Point CorrigePonto(int x, int y)
        {
            return new Point
            {
                X = x + 20,
                Y = (y * (-1)) + 500
            };
        }
        public static Bitmap GeraPlanoCartesiano()
        {
            Bitmap bmp = new Bitmap(520, 520);
            Graphics g = Graphics.FromImage(bmp);
            g.DrawLine(new Pen(Color.Black, 1), Util.CorrigePonto(0, 0), Util.CorrigePonto(0, 500));
            g.DrawLine(new Pen(Color.Black, 1), Util.CorrigePonto(0, 0), Util.CorrigePonto(500, 0));

            for (int i = 0; i < 500; i += 10)
            {
                int marcador = 1;
                int marcador100 = 3;
                if (i % 100 == 0)
                {
                    g.DrawLine(new Pen(Color.Black, 1), Util.CorrigePonto(i, marcador100), Util.CorrigePonto(i, -marcador100));
                    g.DrawLine(new Pen(Color.Black, 1), Util.CorrigePonto(marcador100, i), Util.CorrigePonto(-marcador100, i));
                    g.DrawString(i.ToString(), new Font("Calibri", 9), new SolidBrush(Color.Black), Util.CorrigePonto(i - 10, -marcador100));
                    g.DrawString(i.ToString(), new Font("Calibri", 9), new SolidBrush(Color.Black), Util.CorrigePonto(-21, i + 6));
                }
                else
                {
                    g.DrawLine(new Pen(Color.Black, 1), Util.CorrigePonto(i, marcador), Util.CorrigePonto(i, -marcador));
                    g.DrawLine(new Pen(Color.Black, 1), Util.CorrigePonto(marcador, i), Util.CorrigePonto(-marcador, i));
                }

                if (i != 0 && i % 50 == 0)
                {
                    g.DrawLine(new Pen(Color.White, 1), Util.CorrigePonto(i, marcador100), Util.CorrigePonto(i, 500));
                    g.DrawLine(new Pen(Color.White, 1), Util.CorrigePonto(marcador100, i), Util.CorrigePonto(500, i));
                }
            }
            g.DrawString("X", new Font("Calibri", 12), new SolidBrush(Color.Black), Util.CorrigeX(480), Util.CorrigeY(0));
            g.DrawString("Y", new Font("Calibri", 12), new SolidBrush(Color.Black), Util.CorrigeX(-12), Util.CorrigeY(500));

            return bmp;
        }

        public static void DesenhaUmPonto(Graphics g, int x, int y, int size = 1, Color? cor = null)
        {
            g.FillRectangle(new SolidBrush(cor != null ? cor.Value : Color.Black), x - size / 2, y - size / 2, size, size);
        }

        public static void PlotaRetaDDA(Graphics g, Point p1, Point p2,int size, Color Cor)
        {
            int dx = p2.X - p1.X,
                dy = p2.Y - p1.Y;

            float x = p1.X,
                y = p1.Y,
                passos;

            float xincr, yincr;

            if (Math.Abs(dx) > Math.Abs(dy))
                passos = Math.Abs(dx);
            else
                passos = Math.Abs(dy);

            xincr = dx / passos;
            yincr = dy / passos;

            DesenhaUmPonto(g, Convert.ToInt32(Math.Round(x)), Convert.ToInt32(Math.Round(y)),size,Cor);

            for (int i = 0; i <= passos; i++)
            {
                x += xincr;
                y += yincr;
                DesenhaUmPonto(g, Convert.ToInt32(Math.Round(x)), Convert.ToInt32(Math.Round(y)), size, Cor);
            }
        }

        public static void PlotaRetaBresenham(Graphics g, Point p1, Point p2, int size, Color Cor)
        {
            int dx = p2.X - p1.X,
                dy = p2.Y - p1.Y,
                x = p1.X,
                y = p1.Y,
                p, xincr, yincr, const1, const2;

            DesenhaUmPonto(g, x, y,size,Cor);

            if (dx < 0)
            {
                dx = -dx;
                xincr = -1;
            }
            else
                xincr = 1;

            if (dy < 0)
            {
                dy = -dy;
                yincr = -1;
            }
            else
                yincr = 1;

            if (dx > dy)
            {
                p = 2 * dy - dx;
                const1 = 2 * dy;
                const2 = 2 * (dy - dx);
                for (int i = 0; i < dx; i++)
                {
                    x += xincr;
                    if (p < 0)
                        p += const1;
                    else
                    {
                        p += const2;
                        y += yincr;
                    }

                    DesenhaUmPonto(g, x, y, size, Cor);
                }
            }
            else
            {
                p = 2 * dx - dy;
                const1 = 2 * dx;
                const2 = 2 * (dx - dy);
                for (int i = 0; i < dy; i++)
                {
                    y += yincr;
                    if (p < 0)
                        p += const1;
                    else
                    {
                        p += const2;
                        x += xincr;
                    }

                    DesenhaUmPonto(g, x, y, size, Cor);
                }
            }
        }

        public static void PlotaCircunferenciaBresenham(Graphics g, Point centro, int raio, int size, Color Cor)
        {
            int xc = centro.X,
                yc = centro.Y,
                x = 0,
                y = raio,
                p = 3 - 2 * raio;

            ColoreSimetricos(g, x, y, xc, yc, size, Cor);
            while (x < y)
            {
                if (p < 0)
                    p += 4 * x + 6;
                else
                {
                    p += 4 * (x - y) + 10;
                    y--;
                }
                x++;

                ColoreSimetricos(g, x, y, xc, yc, size, Cor);
            }
        }

        public static void ColoreSimetricos(Graphics g, int x, int y, int xc, int yc, int size, Color Cor)
        {
            DesenhaUmPonto(g, xc + x, yc - y, size, Cor);
            DesenhaUmPonto(g, xc + x, yc + y, size, Cor);
            DesenhaUmPonto(g, xc - x, yc - y, size, Cor);
            DesenhaUmPonto(g, xc - x, yc + y, size, Cor);
            DesenhaUmPonto(g, xc + y, yc - x, size, Cor);
            DesenhaUmPonto(g, xc + y, yc + x, size, Cor);
            DesenhaUmPonto(g, xc - y, yc - x, size, Cor);
            DesenhaUmPonto(g, xc - y, yc + x, size, Cor);
        }

        public static double CalculaRaio(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow((p2.X - p1.X), 2) + Math.Pow((p2.Y - p1.Y), 2));
        }

        public static void PreencheTexto(TextBox textBox, string texto)
        {
            if (String.IsNullOrEmpty(textBox.Text))
                textBox.Text = texto;
        }
        #endregion
    }
}
