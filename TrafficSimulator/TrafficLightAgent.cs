using ActressMas;
using System;
using System.Collections.Generic;

namespace TrafficSimulator
{
    public class TrafficLightAgent : TurnBasedAgent
    {
        private int _id;
        private readonly int _x, _y;
        private Utils.TrafficLightState _state;
        private readonly int _noTurns;
        private int _currentNoTurns;

        public TrafficLightAgent(int id, int posX, int posY, int noTurns, Utils.TrafficLightState initialState)
        {
            _id = id;
            _x = posX;
            _y = posY;
            _noTurns = noTurns;
            _state = initialState;
        }

        public override void Setup()
        {
            Console.WriteLine(@"Starting {0} with state {1}", Name, _state);
            Send("intersection", Utils.Str("trafficLight", _x, _y, _state));
        }

        public override void Act(Queue<Message> messages)
        {
            while (messages.Count > 0)
            {
                Message message = messages.Dequeue();

                Console.WriteLine(@"	[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

                Utils.ParseMessage(message.Content, _x, _y, out var action, out int[,] parameters);

                if (action == "change")
                {
                    ParseState(parameters);
                    
                    if (_currentNoTurns == _noTurns)
                    {
                        SwitchState();
                        _currentNoTurns = 0;
                        Send("intersection", Utils.Str("changeLight", _x, _y, _state));
                    }
                    else
                    {
                        Send("intersection", Utils.Str("noChangeLight", _x, _y));
                    }
                    
                    _currentNoTurns++;
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

        private void ParseState(int[,] values) {}
    }
}