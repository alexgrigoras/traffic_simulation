using System;
using System.Collections.Generic;
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
            DrawIntersection();
        }

        public void UpdateIntersectionGUI()
        {
            DrawIntersection();
        }

        private void pictureBox_Resize(object sender, EventArgs e)
        {
            DrawIntersection();
        }
        
        private Brush PickBrush(int value)
        {
            Type brushesType = typeof(Brushes);
            PropertyInfo[] properties = brushesType.GetProperties();
            int selectedValue = Utils.Map(value, 0, Utils.NoCars, 0, properties.Length-1);
            
            return (Brush)properties[selectedValue].GetValue(null, null);
        }

        private void DrawIntersection()
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

                RectangleF[] rects = new RectangleF[(Utils.Size / 2) * (Utils.Size / 2)];

                int index = 0;
                Brush b = Brushes.Black;
                
                for (int j = 1; j <= Utils.Size - 2; j += 2)
                {
                    for (int i = 1; i <= Utils.Size - 2; i += 2)
                    {
                        rects[index] = new Rectangle(20 + i * cellSize, 20 + j * cellSize, cellSize, cellSize); 
                        index++;
                    }
                }
                
                g.FillRectangles(b, rects);

                foreach (List<string> t in _ownerAgent.TrafficLightPositions.Values)
                {
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

                    g.FillRectangles(b2, rects1);
                    g.FillRectangles(b1, rects2);
                }

                RectangleF[] startingPointRects = new RectangleF[Utils.NoStartingPoints];
                
                for (int i = 0; i < Utils.NoStartingPoints; i++)
                {
                    startingPointRects[i] = new Rectangle(20 + (i * 2) * cellSize, 20 + Utils.Size * cellSize, cellSize, cellSize);
                }
                
                g.FillRectangles(Brushes.LightSteelBlue, startingPointRects);

                foreach (List<string> t in _ownerAgent.CarPositions.Values)
                {
                    int x = Convert.ToInt32(t[0]);
                    int y = Convert.ToInt32(t[1]);
                    string id = t[2];
                    int idInt = Convert.ToInt32(id);
                    int nrCarsCellCount = nrCarsPerCell[x * Utils.Size + y];

                    if (nrCarsCellCount > 0 && nrCarsCellCount < Utils.MaxNoCarsPerCell)
                    {
                        offsetX = (nrCarsCellCount / Utils.NoCarsPerCell) * (cellSize / Utils.NoCarsPerCell);
                        offsetY = (nrCarsCellCount % Utils.NoCarsPerCell) * (cellSize / Utils.NoCarsPerCell);
                    }
                    else
                    {
                        offsetX = 0;
                        offsetY = 0;
                    }
                    
                    Rectangle rect = new Rectangle(20 + x * cellSize + offsetX, 20 + y * cellSize + offsetY, 
                        cellSize / Utils.NoCarsPerCell, cellSize / Utils.NoCarsPerCell);

                    g.FillRectangle(PickBrush(idInt), rect);
                    g.DrawString(id, new Font("Arial", 9f), Brushes.Black, rect);

                    nrCarsPerCell[x * Utils.Size + y]++;
                }
            }

            Graphics pbg = pictureBox.CreateGraphics();
            pbg.DrawImage(_doubleBufferImage, 0, 0);
        }
    }
}