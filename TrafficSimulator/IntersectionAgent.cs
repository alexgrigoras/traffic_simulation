using ActressMas;
using Message = ActressMas.Message;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace TrafficSimulator
{
    public class IntersectionAgent : TurnBasedAgent
    {
        private IntersectionForm _formGui;
        public Dictionary<string, string> CarPositions { get; set; }
        public Dictionary<string, string> TrafficLightPositions { get; set; }

        private Utils.TrafficLightState[,] _trafficLightStates;

        public int[,] NoCarsPerCell;
        
        public IntersectionAgent()
        {
            CarPositions = new Dictionary<string, string>();
            TrafficLightPositions = new Dictionary<string, string>();

            Thread t = new Thread(new ThreadStart(GUIThread));
            t.Start();
        }

        private void GUIThread()
        {
            _formGui = new IntersectionForm();
            _formGui.SetOwner(this);
            _formGui.ShowDialog();
            Application.Run();
        }

        public override void Setup()
        {
            Console.WriteLine("Starting " + Name);
            Thread.Sleep(1000);

            NoCarsPerCell = new int[Utils.Size, Utils.Size];
            
            _trafficLightStates = new Utils.TrafficLightState[Utils.Size,Utils.Size];

            for (int i = 0; i < Utils.Size; i++)
            {
                for (int j = 0; j < Utils.Size; j++)
                {
                    if (i % 2 != 0 && j % 2 != 0)
                    {
                        NoCarsPerCell[i, j] = -1;
                    }
                    _trafficLightStates[i, j] = Utils.TrafficLightState.Unavailable;
                }
            }
        }

        public override void Act(Queue<Message> messages)
        {
            while(messages.Count > 0)
            {
                Message message = messages.Dequeue();
                Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

                string action; string parameters;
                Utils.ParseMessage(message.Content, out action, out parameters);

                switch (action)
                {
                    case "position":
                        HandlePosition(message.Sender, parameters);
                        break;

                    case "trafficLight":
                        HandleTrafficLightPosition(message.Sender, parameters);
                        break;

                    case "changeLight":
                        HandleChangeLight(message.Sender, parameters);
                        break;
                    
                    case "noChangeLight":
                        HandleNoChangeLight(message.Sender, parameters);
                        break;

                    case "change":
                        HandleChange(message.Sender, parameters);
                        break;

                    case "finish":
                        RemoveCar(message.Sender, parameters);
                        break;
                }
                
                _formGui.UpdateIntersectionGUI();
            }
        }

        private void HandleTrafficLightPosition(string sender, string position)
        {
            TrafficLightPositions.Add(sender, position);
            
            string[] t = position.Split();
            int x = Convert.ToInt32(t[0]);
            int y = Convert.ToInt32(t[1]);
            Utils.TrafficLightState state = (Utils.TrafficLightState) Enum.Parse(typeof(Utils.TrafficLightState), t[2]);

            _trafficLightStates[x, y] = state;

            string message = Utils.CreateMessage(NoCarsPerCell, "change");

            Send(sender, message);
        }
        
        private void HandleChangeLight(string sender, string position)
        {
            TrafficLightPositions[sender] = position;
            
            string[] t = position.Split();
            int x = Convert.ToInt32(t[0]);
            int y = Convert.ToInt32(t[1]);
            Utils.TrafficLightState state = (Utils.TrafficLightState) Enum.Parse(typeof(Utils.TrafficLightState), t[2]);

            _trafficLightStates[x, y] = state;
            
            string message = Utils.CreateMessage(NoCarsPerCell, "change");
            
            Send(sender, message);
        }
        
        private void HandleNoChangeLight(string sender, string position)
        {
            string message = Utils.CreateMessage(NoCarsPerCell, "change");
            Send(sender, message);
        }

        private void HandlePosition(string sender, string position)
        {
            CarPositions.Add(sender, position);
            int leftCell = -1, upCell = 0, rightCell = -1;
            Utils.TrafficLightState leftCellLight = Utils.TrafficLightState.Green, 
                upCellLight = Utils.TrafficLightState.Green, 
                rightCellLight = Utils.TrafficLightState.Green;
            Send(sender, Utils.Str("move", leftCell, upCell, rightCell, 
                leftCellLight, upCellLight, rightCellLight));
        }
        
        private void RemoveCar(string sender, string position)
        {
            string[] t = position.Split();
            int x = Convert.ToInt32(t[0]);
            int y = Convert.ToInt32(t[1]);
            
            CarPositions.Remove(sender);
            
            NoCarsPerCell[x, y]--;
        }

        private void HandleChange(string sender, string position)
        {
            // Get old and new positions
            string[] oldT = CarPositions[sender].Split();
            int oldX = Convert.ToInt32(oldT[0]);
            int oldY = Convert.ToInt32(oldT[1]);
            string[] newT = position.Split();
            int newX = Convert.ToInt32(newT[0]);
            int newY = Convert.ToInt32(newT[1]);

            Utils.TrafficLightState lightForStop = Utils.TrafficLightState.Red;

            if (newY - oldY != 0)               // Traffic light is up
                lightForStop = Utils.TrafficLightState.Red;
            else if (newX - oldX != 0)          // Traffic light is left or right
                lightForStop = Utils.TrafficLightState.Green;

            if (_trafficLightStates[newX, newY] == lightForStop)
            {
                // Traffic light stop
                Send(sender, "block");
            }
            else if (NoCarsPerCell[newX, newY] < Utils.MaxNoCarsPerCell)
            {
                // Go to next position
                NoCarsPerCell[newX, newY]++;
                if (oldY != Utils.Size)
                {
                    NoCarsPerCell[oldX, oldY]--;
                }

                int leftCell = -1, upCell = -1, rightCell = -1;
                Utils.TrafficLightState leftCellLight = Utils.TrafficLightState.Unavailable, 
                    upCellLight = Utils.TrafficLightState.Unavailable, 
                    rightCellLight = Utils.TrafficLightState.Unavailable;
                if (newX - 2 > 0 && NoCarsPerCell[newX - 1, newY] != -1)
                {
                    leftCell = NoCarsPerCell[newX - 1, newY];
                    leftCellLight = _trafficLightStates[newX - 2, newY];
                }
                if (newY - 2 > 0 && NoCarsPerCell[newX, newY - 1] != -1)
                {
                    upCell = NoCarsPerCell[newX, newY - 1];
                    upCellLight = _trafficLightStates[newX, newY - 2];
                }
                if (newX + 2 < Utils.Size && NoCarsPerCell[newX + 1, newY] != -1)
                {
                    rightCell = NoCarsPerCell[newX + 1, newY];
                    rightCellLight = _trafficLightStates[newX + 2, newY];
                }

                CarPositions[sender] = position;
                Send(sender, Utils.Str("move", leftCell, upCell, rightCell, leftCellLight, upCellLight, rightCellLight));
            }
            else
            {
                // Too many cars
                Send(sender, "block");
            }
        }
    }
}