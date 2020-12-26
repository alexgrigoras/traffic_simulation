using ActressMas;
using Message = ActressMas.Message;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace TrafficSimulator
{
    public class IntersectionAgent : TurnBasedAgent
    {
        private IntersectionForm _formGui;
        public Dictionary<string, List<string>> CarPositions { get; }
        public Dictionary<string, List<string>> TrafficLightPositions { get; }
        private Utils.TrafficLightState[,] _trafficLightStates;
        private int[,] _noCarsPerCell;
        
        public IntersectionAgent()
        {
            CarPositions = new Dictionary<string, List<string>>();
            TrafficLightPositions = new Dictionary<string, List<string>>();

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
            Console.WriteLine(@"Starting {0}", Name);
            Thread.Sleep(1000);

            _noCarsPerCell = new int[Utils.Size, Utils.Size];
            _trafficLightStates = new Utils.TrafficLightState[Utils.Size,Utils.Size];

            for (int i = 0; i < Utils.Size; i++)
            {
                for (int j = 0; j < Utils.Size; j++)
                {
                    if (i % 2 != 0 && j % 2 != 0)
                        _noCarsPerCell[i, j] = -1;
                    _trafficLightStates[i, j] = Utils.TrafficLightState.Unavailable;
                }
            }
        }

        public override void Act(Queue<Message> messages)
        {
            while(messages.Count > 0)
            {
                var message = messages.Dequeue();
                Console.WriteLine(@"	[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

                Utils.ParseMessage(message.Content, out var action, out List<string> parameters);

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
                        HandleNoChangeLight(message.Sender);
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

        private void HandleTrafficLightPosition(string sender, List<string> position)
        {
            TrafficLightPositions.Add(sender, position);
            
            int x = Convert.ToInt32(position[0]);
            int y = Convert.ToInt32(position[1]);
            Utils.TrafficLightState state = 
                (Utils.TrafficLightState) Enum.Parse(typeof(Utils.TrafficLightState), position[2]);

            _trafficLightStates[x, y] = state;

            string message = Utils.BuildMessage(_noCarsPerCell, "change");

            Send(sender, message);
        }
        
        private void HandleChangeLight(string sender, List<string> position)
        {
            TrafficLightPositions[sender] = position;

            int x = Convert.ToInt32(position[0]);
            int y = Convert.ToInt32(position[1]);
            Utils.TrafficLightState state = 
                (Utils.TrafficLightState) Enum.Parse(typeof(Utils.TrafficLightState), position[2]);

            _trafficLightStates[x, y] = state;
            
            string message = Utils.BuildMessage(_noCarsPerCell, "change");
            
            Send(sender, message);
        }
        
        private void HandleNoChangeLight(string sender)
        {
            string message = Utils.BuildMessage(_noCarsPerCell, "change");
            
            Send(sender, message);
        }

        private void HandlePosition(string sender, List<string> position)
        {
            const int leftCell = -1;
            const int upCell = 0;
            const int rightCell = -1;
            const Utils.TrafficLightState leftCellLight = Utils.TrafficLightState.Green;
            const Utils.TrafficLightState upCellLight = Utils.TrafficLightState.Green;
            const Utils.TrafficLightState rightCellLight = Utils.TrafficLightState.Green;
            
            CarPositions.Add(sender, position);
            Send(sender, Utils.Str("move", leftCell, upCell, rightCell, 
                leftCellLight, upCellLight, rightCellLight));
        }
        
        private void RemoveCar(string sender, List<string> position)
        {
            int x = Convert.ToInt32(position[0]);
            int y = Convert.ToInt32(position[1]);
            
            CarPositions.Remove(sender);
            
            _noCarsPerCell[x, y]--;
        }

        private void HandleChange(string sender, List<string> position)
        {
            // Get old and new positions
            int oldX = Convert.ToInt32(CarPositions[sender][0]);
            int oldY = Convert.ToInt32(CarPositions[sender][1]);
            int newX = Convert.ToInt32(position[0]);
            int newY = Convert.ToInt32(position[1]);

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
            else if (_noCarsPerCell[newX, newY] < Utils.MaxNoCarsPerCell)
            {
                // Go to next position
                _noCarsPerCell[newX, newY]++;
                if (oldY != Utils.Size)
                {
                    _noCarsPerCell[oldX, oldY]--;
                }

                int leftCell = -1, upCell = -1, rightCell = -1;
                Utils.TrafficLightState leftCellLight = Utils.TrafficLightState.Unavailable, 
                    upCellLight = Utils.TrafficLightState.Unavailable, 
                    rightCellLight = Utils.TrafficLightState.Unavailable;
                if (newX - 2 > 0 && _noCarsPerCell[newX - 1, newY] != -1)
                {
                    leftCell = _noCarsPerCell[newX - 1, newY];
                    leftCellLight = _trafficLightStates[newX - 2, newY];
                }
                if (newY - 2 > 0 && _noCarsPerCell[newX, newY - 1] != -1)
                {
                    //Console.WriteLine("-- {0} {1}", _noCarsPerCell[newX, newY - 1], _trafficLightStates[newX, newY - 2]);
                    upCell = _noCarsPerCell[newX, newY - 1];
                    upCellLight = _trafficLightStates[newX, newY - 2];
                }
                if (newX + 2 < Utils.Size && _noCarsPerCell[newX + 1, newY] != -1)
                {
                    rightCell = _noCarsPerCell[newX + 1, newY];
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