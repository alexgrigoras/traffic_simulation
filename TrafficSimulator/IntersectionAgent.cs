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
        public Dictionary<string, string> CarPositions { get; set; }
        public Dictionary<string, string> TrafficLightPositions { get; set; }
        
        private Utils.TrafficLightState[,] _trafficLightStates;
        
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
            
            _trafficLightStates = new Utils.TrafficLightState[Utils.Size,Utils.Size];
        }
        
        public static void Print2DArray<T>(T[,] matrix)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    Console.Write(matrix[i,j] + "\t");
                }
                Console.WriteLine();
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
                        RemoveCar(message.Sender);
                        break;
                }
                
                _formGui.UpdatePlanetGUI();
            }
        }

        private void HandlePosition(string sender, string position)
        {
            CarPositions.Add(sender, position);
            Send(sender, "move");
        }
        
        private void HandleTrafficLightPosition(string sender, string position)
        {
            TrafficLightPositions.Add(sender, position);
            Send(sender, "change");
        }
        
        private void HandleChangeLight(string sender, string position)
        {
            TrafficLightPositions[sender] = position; 
            
            string[] t = position.Split();
            int x = Convert.ToInt32(t[0]);
            int y = Convert.ToInt32(t[1]);
            Utils.TrafficLightState state = (Utils.TrafficLightState) Enum.Parse(typeof(Utils.TrafficLightState), t[2]);

            _trafficLightStates[x, y] = state;
            
            Send(sender, "change");
        }
        
        private void HandleNoChangeLight(string sender, string position)
        {
            Send(sender, "change");
        }
        
        private void RemoveCar(string sender)
        {
            CarPositions.Remove(sender);
        }

        private void HandleChange(string sender, string position)
        {
            CarPositions[sender] = position;

            foreach (string k in CarPositions.Keys)
            {
                if (k == sender)
                    continue;
                if (CarPositions[k] == position)
                {
                    Send(sender, "block");
                    return;
                }
            }
            
            string[] t = position.Split();
            int x = Convert.ToInt32(t[0]);
            int y = Convert.ToInt32(t[1]);
            int id = Convert.ToInt32(t[2]);

            if (_trafficLightStates[x, y] == Utils.TrafficLightState.Red)
            {
                Send(sender, "block");
            }
            else
            {
                Send(sender, "move");   
            }
        }
    }
}