using ActressMas;
using System;
using System.Collections.Generic;

namespace TrafficSimulator
{
    public class CarAgent : TurnBasedAgent
    {
        private readonly int _id;
        private int _x, _y;
        private readonly int _startPos;
        private readonly int _skippedTurns;
        private int _turns;
        private int _finalX, _finalY;
        private bool[,] _unavailableCells;
        private Utils.CarPriorityState _carPriority;

        public CarAgent(int id, int skippedTurns, int startPos, Utils.CarPriorityState carPriority)
        {
            _id = id;
            _turns = 1;
            _skippedTurns = skippedTurns;
            _startPos = startPos;
            _carPriority = carPriority;
        }
        
        public override void Setup()
        {
            //_state = State.Free;
            _x = _startPos;
            _y = Utils.Size;
            _finalX = Utils.RandNoGen.Next(Utils.Size);
            _finalY = 0;

            _unavailableCells = new bool[Utils.Size,Utils.Size];

            for (int i = 1; i < Utils.Size; i+=2)
            {
                for (int j = 1; j < Utils.Size; j+=2)
                {
                     _unavailableCells[i,j] = true;
                }
            }

            Console.WriteLine("Starting " + Name + " - going to (" + _finalX + "," + _finalY + ")");

            Send("intersection", Utils.Str("position", _x, _y, _id));
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
                    string parameters;
                    Utils.ParseMessage(message.Content, out action, out parameters);

                    if (action == "block")
                    {
                        Console.WriteLine("\t[{0}]: waits at RED", this.Name);
                        Send("intersection", Utils.Str("change", _x, _y, _id));
                    }
                    else if (action == "move" && IsAtDestination())
                    {
                        //_state = State.Free;
                        Console.WriteLine("\t[{0}]: Arrived at destination", this.Name);
                        Send("intersection", Utils.Str("finish",  _x, _y, _id));
                        this.Stop();
                    }
                    else if (action == "move")
                    {
                        MoveToDestination(parameters);
                        Send("intersection", Utils.Str("change", _x, _y, _id));
                    }
                }
            }
            _turns++;
        }

        private void MoveToDestination(string position)
        {
            // Get number of cars from the possible next cells
            string[] t = position.Split();
            int leftCellNoCars = Convert.ToInt32(t[0]);
            int upCellNoCars = Convert.ToInt32(t[1]);
            int rightCellNoCars = Convert.ToInt32(t[2]);
            Utils.TrafficLightState leftCellLight = (Utils.TrafficLightState) Enum.Parse(typeof(Utils.TrafficLightState), t[3]);
            Utils.TrafficLightState upCellLight = (Utils.TrafficLightState) Enum.Parse(typeof(Utils.TrafficLightState), t[4]);
            Utils.TrafficLightState rightCellLight = (Utils.TrafficLightState) Enum.Parse(typeof(Utils.TrafficLightState), t[5]);

            if (_carPriority == Utils.CarPriorityState.NoPriority)
            {
                // Select next cell
                int dx = _x - _finalX;
                int dy = _y - _finalY;
                int newX = _x, newY = _y;

                if (Math.Abs(dx) > Math.Abs(dy))
                    newX -= Math.Sign(dx);
                else
                    newY -= Math.Sign(dy);

                if (_unavailableCells[newX, newY])
                {
                    if (newX == _x)
                    {
                        // unavailable cell is up => goes left or right
                        if (dx != 0)
                        {
                            if (_x - Math.Sign(dx) >= 0 && _x - Math.Sign(dx) < Utils.Size)
                            {
                                _x -= Math.Sign(dx);
                            }
                        }
                        else
                        {
                            if (_x - 1 >= 0) _x -= 1;
                            else if (_x + 1 < Utils.Size) _x += 1;
                        }
                    }
                    else if (newY == _y)
                    {
                        // unavailable cell is right or left => goes up
                        if (_y - 1 >= 0)
                            _y -= 1;
                    }
                }
                else
                {
                    _x = newX;
                    _y = newY;
                }
            }
            
            // !! TO Add - select the next cell accordingly to the car priority
            
            else if (_carPriority == Utils.CarPriorityState.GreenLight)
            {
                //
            }
            else if (_carPriority == Utils.CarPriorityState.LowerTraffic)
            {
                //
            }
        }
    }
}