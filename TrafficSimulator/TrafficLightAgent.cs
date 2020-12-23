using ActressMas;
using System;
using System.Collections.Generic;
using System.Threading;

namespace TrafficSimulator
{
    public class TrafficLightAgent : TurnBasedAgent
    {
        private int _id;
        private int _x, _y;
        private Utils.TrafficLightState _state;

        public TrafficLightAgent(int id, int posX, int posY)
        {
            this._id = id;
            this._x = posX;
            this._y = posY;
        }

        public override void Setup()
        {
            _state = Utils.TrafficLightState.Green;
            
            Console.WriteLine("Starting " + Name);
            
            Send("planet", Utils.Str("trafficLight", _x, _y, _state));
        }

        public override void Act(Queue<Message> messages)
        {
            while (messages.Count > 0)
            {
                Message message = messages.Dequeue();

                Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

                string action;
                List<string> parameters;
                Utils.ParseMessage(message.Content, out action, out parameters);
                
                if (action == "change")
                {
                    SwitchState();
                    Send("planet", Utils.Str("changeLight", _x, _y, _state));
                }
            }
        }

        private void SwitchState()
        {
            if (_state == Utils.TrafficLightState.Green)
                _state = Utils.TrafficLightState.Red;
            else
                _state = Utils.TrafficLightState.Green;
        }
    }
}