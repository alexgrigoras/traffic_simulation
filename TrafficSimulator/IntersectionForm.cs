using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace TrafficSimulator
{
    public partial class IntersectionForm : Form
    {
        private IntersectionAgent _ownerAgent;
        private Bitmap _doubleBufferImage;

        public IntersectionForm()
        {
            InitializeComponent();
        }

        public void SetOwner(IntersectionAgent a)
        {
            _ownerAgent = a;
        }

        private void pictureBox_Paint(object sender, PaintEventArgs e)
        {
            DrawPlanet();
        }

        public void UpdatePlanetGUI()
        {
            DrawPlanet();
        }

        private void pictureBox_Resize(object sender, EventArgs e)
        {
            DrawPlanet();
        }
        
        private Brush PickBrush()
        {
            Brush result = Brushes.Transparent;

            Random rnd = new Random();

            Type brushesType = typeof(Brushes);

            PropertyInfo[] properties = brushesType.GetProperties();

            int random = rnd.Next(properties.Length);
            result = (Brush)properties[random].GetValue(null, null);

            return result;
        }
        
        int map(int x, int in_min, int in_max, int out_min, int out_max) {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }
        
        private Brush PickBrush(int value)
        {
            Brush result = Brushes.Transparent;
            Type brushesType = typeof(Brushes);
            PropertyInfo[] properties = brushesType.GetProperties();
            int selectedValue = map(value, 0, Utils.NoExplorers - 1, 0, properties.Length-1);
            
            result = (Brush)properties[selectedValue].GetValue(null, null);

            return result;
        }

        private void DrawPlanet()
        {
            int w = pictureBox.Width;
            int h = pictureBox.Height;

            if (_doubleBufferImage != null)
            {
                _doubleBufferImage.Dispose();
                GC.Collect(); // prevents memory leaks
            }

            _doubleBufferImage = new Bitmap(w, h);
            Graphics g = Graphics.FromImage(_doubleBufferImage);
            g.Clear(Color.White);

            int minXY = Math.Min(w, h);
            int cellSize = (minXY - 40) / Utils.Size;

            for (int i = 0; i <= Utils.Size; i++)
            {
                g.DrawLine(Pens.DarkGray, 20, 20 + i * cellSize, 20 + Utils.Size * cellSize, 20 + i * cellSize);
                g.DrawLine(Pens.DarkGray, 20 + i * cellSize, 20, 20 + i * cellSize, 20 + Utils.Size * cellSize);
            }

            if (_ownerAgent != null)
            {
                int[] nrCarsPerCell = new int[Utils.Size * Utils.Size + 1];
                int offsetX = 0, offsetY = 0;

                for (int i = 0; i < nrCarsPerCell.Length; i++)
                    nrCarsPerCell[i] = 0;

                foreach (string v in _ownerAgent.TrafficLightPositions.Values)
                {
                    string[] t = v.Split();
                    int x = Convert.ToInt32(t[0]);
                    int y = Convert.ToInt32(t[1]);
                    string st = t[2];
                    Utils.TrafficLightState state = (Utils.TrafficLightState) Enum.Parse(typeof(Utils.TrafficLightState),st);

                    Brush b1, b2;
                    switch(state)
                    {
                        case Utils.TrafficLightState.Green:
                            b1 = Brushes.Green;
                            b2 = Brushes.Red;
                            break;
                        case  Utils.TrafficLightState.Red:
                            b1 = Brushes.Red;
                            b2 = Brushes.Green;
                            break;
                        default:
                            b1 = Brushes.Red;
                            b2 = Brushes.Green;
                            break;
                    }

                    RectangleF[] rects1 = {
                        new Rectangle(20 + x * cellSize, 20 + y * cellSize, cellSize / 10, cellSize),
                        new Rectangle(20 + x * cellSize + cellSize, 20 + y * cellSize, cellSize / 10, cellSize)
                    };
                    RectangleF[] rects2 = {
                        new Rectangle(20 + x * cellSize, 20 + y * cellSize, cellSize, cellSize / 10),
                        new Rectangle(20 + x * cellSize, 20 + y * cellSize + cellSize, cellSize, cellSize / 10)
                    };
                    
                    g.FillRectangles(b1, rects1);
                    g.FillRectangles(b2, rects2);
                }

                foreach (string v in _ownerAgent.CarPositions.Values)
                {
                    string[] t = v.Split();
                    int x = Convert.ToInt32(t[0]);
                    int y = Convert.ToInt32(t[1]);
                    string id = t[2];
                    int idInt = Convert.ToInt32(id);
                    int nrExplorersCellCount = nrCarsPerCell[x * Utils.Size + y];
                    
                    if (nrExplorersCellCount > 0 && nrExplorersCellCount < 9)
                    {
                        offsetX = (nrExplorersCellCount / 3) * (cellSize/3);
                        offsetY = (nrExplorersCellCount % 3) * (cellSize/3);
                    }
                    
                    Rectangle rect = new Rectangle(20 + x * cellSize + offsetX, 20 + y * cellSize + offsetY, cellSize/3, cellSize/3);

                    g.FillRectangle(PickBrush(idInt), rect);
                    g.DrawString(id, new Font("Arial", 13f), Brushes.White, rect);

                    nrCarsPerCell[x * Utils.Size + y]++;
                }
            }

            Graphics pbg = pictureBox.CreateGraphics();
            pbg.DrawImage(_doubleBufferImage, 0, 0);
        }
    }
}