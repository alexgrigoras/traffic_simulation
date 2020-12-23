using ActressMas;
using System;
using System.Collections.Generic;
using System.Threading;

namespace TrafficSimulator
{
    public class CarAgent : TurnBasedAgent
    {
        private int _id;
        private int _x, _y;
        private State _state;
        private string _resourceCarried;
        private readonly int _skippedTurns;
        private int _turns;
        private int _finalX, _finalY;

        public CarAgent(int id, int skippedTurns)
        {
            this._id = id;
            this._turns = 1;
            this._skippedTurns = skippedTurns;
        }

        private enum State { Free, Carrying };

        public override void Setup()
        {
            _state = State.Free;
            _x = Utils.RandNoGen.Next(Utils.Size);
            _y = Utils.Size;
            _finalX = Utils.RandNoGen.Next(Utils.Size);
            _finalY = 0;
            
            Console.WriteLine("Starting " + Name + " - going to (" + _finalX + "," + _finalY + ")");
            
            Send("planet", Utils.Str("position", _x, _y, _id));
        }

        private bool IsAtDestination()
        {
            return (_x == _finalX && _y == _finalY);
        }

        public override void Act(Queue<Message> messages)
        {
            if (_turns > _skippedTurns)
            {
                while (messages.Count > 0)
                {
                    Message message = messages.Dequeue();

                    Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

                    string action;
                    List<string> parameters;
                    Utils.ParseMessage(message.Content, out action, out parameters);

                    if (action == "block")
                    {
                        MoveToDestination();
                        Send("planet", Utils.Str("change", _x, _y, _id));
                    }
                    else if (action == "move" && IsAtDestination())
                    {
                        _state = State.Free;
                        Console.WriteLine("\t[{0}]: Arrived at destination", this.Name);
                        Send("planet", Utils.Str("finished", _resourceCarried));
                        this.Stop();
                    }
                    else if (action == "move")
                    {
                        MoveToDestination();
                        Send("planet", Utils.Str("change", _x, _y, _id));
                    }
                }
            }
            _turns++;
        }

        private void MoveToDestination()
        {
            int dx = _x - _finalX;
            int dy = _y - _finalY;

            if (Math.Abs(dx) > Math.Abs(dy))
                _x -= Math.Sign(dx);
            else
                _y -= Math.Sign(dy);
        }
    }
}