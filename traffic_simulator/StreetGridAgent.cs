using ActressMas;
using Message = ActressMas.Message;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace Reactive
{
    public class StreetGridAgent : TurnBasedAgent
    {
        private IntersectionForm _formGui;
        public Dictionary<string, KeyValuePair<int, int>> CarPositions { get; set; }

        public StreetGridAgent()
        {
            CarPositions = new Dictionary<string, KeyValuePair<int, int>>();

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
                    // vezi exact flow-ul aici, cum trimite masina, cum ii trimite inapoi masinii
                    case "spawn":
                        HandleSpawn(message.Sender, parameters);
                        break;

                    case "change":
                        HandleChange(message.Sender, parameters);
                        break;
                    
                    case "moved":
                        HandleMoved(message.Sender, parameters);
                        break;

                    default:
                        break;
                }
                _formGui.UpdatePlanetGUI();
            }
        }

        private void HandleSpawn(string sender, string position)
        {
            string[] positionsList = position.Split(' ');
            CarPositions.Add(sender, new KeyValuePair<int, int>(int.Parse(positionsList[0]), int.Parse(positionsList[1])));
            Send(sender, "wait");
        }

        private void HandleChange(string sender, string position)
        {
            string[] positionsList = position.Split(' ');
            //test daca actually se poate misca unde vrea el

            if (int.Parse(positionsList[1]) == 6)
            {
                CarPositions[sender] = new KeyValuePair<int, int>(int.Parse(positionsList[0]), int.Parse(positionsList[1]));
                Send(sender, "done");
            }

            if (true)
            {
                CarPositions[sender] = new KeyValuePair<int, int>(int.Parse(positionsList[0]), int.Parse(positionsList[1]));
                Send(sender, "move");
            }
            else
            {
                Send(sender, "wait");
            }
            
            
        }

        private void HandleMoved(string sender, string position)
        {
            Send(sender, "wait");
        }
    }
}