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
        public Dictionary<string, string> ResourcePositions { get; set; }
        public Dictionary<string, string> Loads { get; set; }
        private string _basePosition;

        public IntersectionAgent()
        {
            CarPositions = new Dictionary<string, string>();
            TrafficLightPositions = new Dictionary<string, string>();
            ResourcePositions = new Dictionary<string, string>();
            Loads = new Dictionary<string, string>();
            _basePosition = Utils.Str(Utils.Size / 2, Utils.Size / 2);

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

                    case "change":
                        HandleChange(message.Sender, parameters);
                        break;

                    case "finish":
                        RemoveCar(message.Sender);
                        break;

                    default:
                        break;
                }
                                            
                _formGui.UpdatePlanetGUI();
            }
            
            //_formGui.UpdatePlanetGUI();
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

            Send(sender, "move");
        }

        /*
        private void HandlePickUp(string sender, string position)
        {
            Loads[sender] = position;
            Send(sender, "move");
        }

        private void HandleCarry(string sender, string position)
        {
            ExplorerPositions[sender] = position;
            string res = Loads[sender];
            ResourcePositions[res] = position;
            Send(sender, "move");
        }

        private void HandleUnload(string sender)
        {
            Loads.Remove(sender);
            Send(sender, "move");
        }
        */
    }
}