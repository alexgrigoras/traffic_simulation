using ActressMas;
using System;
using System.Collections.Generic;

namespace Reactive
{
    //TODO: modifica clasa
    public class SemaphoreAgent : TurnBasedAgent
    {
        // pozitia in matrice; (0;0)
        private int _x, _y;
        private State _state;
        private string _resourceCarried;
        private int intelligence;
        private int priority;

        private enum State { Free, Carrying };

        public SemaphoreAgent(int pos_x, int pos_y, int intelligence, int priority)
        {
            this._x = pos_x;
            this._y = pos_y;
            this.intelligence = intelligence;
            this.priority = priority;
        }
        
        public override void Setup()
        {
            Console.WriteLine("Starting " + Name);

            _x = Utils.Size / 2;
            _y = Utils.Size / 2;
            _state = State.Free;

            while (IsAtBase())
            {
                _x = Utils.RandNoGen.Next(Utils.Size);
                _y = Utils.RandNoGen.Next(Utils.Size);
            }

            Send("planet", Utils.Str("position", _x, _y));
        }

        private bool IsAtBase()
        {
            return (_x == Utils.Size / 2 && _y == Utils.Size / 2); // the position of the base
        }

        public override void Act(Queue<Message> messages)
        {
            while(messages.Count > 0)
            {
                Message message = messages.Dequeue();
            
                Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

                string action;
                List<string> parameters;
                Utils.ParseMessage(message.Content, out action, out parameters);

                if (action == "block")
                {
                    // R1. If you detect an obstacle, then change direction
                    MoveRandomly();
                    Send("planet", Utils.Str("change", _x, _y));
                }
                else if (action == "move" && _state == State.Carrying && IsAtBase())
                {
                    // R2. If carrying samples and at the base, then unload samples
                    _state = State.Free;
                    Send("planet", Utils.Str("unload", _resourceCarried));
                }
                else if (action == "move" && _state == State.Carrying && !IsAtBase())
                {
                    // R3. If carrying samples and not at the base, then travel up gradient
                    MoveToBase();
                    Send("planet", Utils.Str("carry", _x, _y));
                }
                else if (action == "rock")
                {
                    // R4. If you detect a sample, then pick sample up
                    _state = State.Carrying;
                    _resourceCarried = parameters[0];
                    Send("planet", Utils.Str("pick-up", _resourceCarried));
                }
                else if (action == "move")
                {
                    // R5. If (true), then move randomly
                    MoveRandomly();
                    Send("planet", Utils.Str("change", _x, _y));
                }
            }
        }

        private void MoveRandomly()
        {
            int d = Utils.RandNoGen.Next(4);
            switch (d)
            {
                case 0: if (_x > 0) _x--; break;
                case 1: if (_x < Utils.Size - 1) _x++; break;
                case 2: if (_y > 0) _y--; break;
                case 3: if (_y < Utils.Size - 1) _y++; break;
            }
        }

        private void MoveToBase()
        {
            int dx = _x - Utils.Size / 2;
            int dy = _y - Utils.Size / 2;

            if (Math.Abs(dx) > Math.Abs(dy))
                _x -= Math.Sign(dx);
            else
                _y -= Math.Sign(dy);
        }
    }
}