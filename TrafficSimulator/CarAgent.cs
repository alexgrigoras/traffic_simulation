using ActressMas;
using System;
using System.Collections.Generic;

namespace TrafficSimulator
{
    public class CarAgent : TurnBasedAgent
    {
        private readonly int _id;
        private int _x, _y;
        private int _finalX, _finalY;
        private readonly int _startPos;
        private readonly int _skippedTurns;
        private int _turns;
        private bool[,] _unavailableCells;
        private readonly Utils.CarPriorityState _carPriority;

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
            _x = _startPos;
            _y = Utils.Size;
            _finalX = Utils.RandNoGen.Next(Utils.Size/2) * 2;
            _finalY = 0;
            _unavailableCells = new bool[Utils.Size,Utils.Size];
            
            for (int i = 1; i < Utils.Size; i+=2)
            {
                for (int j = 1; j < Utils.Size; j+=2)
                {
                     _unavailableCells[i,j] = true;
                }
            }

            Console.WriteLine(@"Starting " + Name + @" - going to (" + _finalX + "," + _finalY + ")");

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

                    Console.WriteLine(@"	[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

                    Utils.ParseMessage(message.Content, out var action, out List<string> parameters);

                    if (action == "block")
                    {
                        Console.WriteLine(@"	[{0}]: waits at RED", this.Name);
                        Send("intersection", Utils.Str("change", _x, _y, _id));
                    }
                    else if (action == "move" && IsAtDestination())
                    {
                        Console.WriteLine(@"	[{0}]: Arrived at destination", this.Name);
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

        private void MoveToDestination(IReadOnlyList<string> t)
        {
            // Get number of cars from the possible next cells
            int leftCellNoCars = Convert.ToInt32(t[0]);
            int upCellNoCars = Convert.ToInt32(t[1]);
            int rightCellNoCars = Convert.ToInt32(t[2]);
            Utils.TrafficLightState leftCellLight = (Utils.TrafficLightState) Enum.Parse(typeof(Utils.TrafficLightState), t[3]);
            Utils.TrafficLightState upCellLight = (Utils.TrafficLightState) Enum.Parse(typeof(Utils.TrafficLightState), t[4]);
            Utils.TrafficLightState rightCellLight = (Utils.TrafficLightState) Enum.Parse(typeof(Utils.TrafficLightState), t[5]);
            
            //Console.WriteLine(@"{0} {1} {2} {3} {4} {5}", leftCellNoCars, upCellNoCars, rightCellNoCars, leftCellLight, upCellLight, rightCellLight);
            
            int dx = _x - _finalX;
            int dy = _y - _finalY;
            int newX = _x, newY = _y;
            
            // If no priority or the cell isn't an intersection, get the next cell without any priority
            if (!IsIntersection(_x, _y) || _carPriority == Utils.CarPriorityState.NoPriority)
            {
                // Select next cell
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
                                newX = _x - Math.Sign(dx);
                                newY = _y;
                            }
                        }
                        else if (_x - 1 >= 0)
                        {
                            newX = _x - 1;
                            newY = _y;
                        }
                        else if (_x + 1 < Utils.Size)
                        {
                            newX = _x + 1;
                            newY = _y;
                        }
                    }
                    else if (newY == _y)
                    {
                        // unavailable cell is right or left => goes up
                        if (_y - 1 >= 0)
                        {
                            newY = _y - 1;
                            newX = _x;
                        }
                    }
                }
                
                _x = newX;
                _y = newY;
            }
            // If the cell is an intersection and the priority is over the green light of the traffic light
            else if (_carPriority == Utils.CarPriorityState.GreenLight)
            {
                // Select next cell accordingly to the priority
                dx = _x - _finalX;

                if (dx > 0) // if next direction is left
                {
                    if (leftCellLight == Utils.TrafficLightState.Green && leftCellNoCars != -1)
                        _x -= 1;
                    else if (upCellLight == Utils.TrafficLightState.Green && upCellNoCars != -1)
                        _y -= 1;
                    else if (rightCellLight == Utils.TrafficLightState.Green && rightCellNoCars != -1)
                        _x += 1;
                }
                else if (dx < 0) // if next direction is right
                {
                    if (rightCellLight == Utils.TrafficLightState.Green && rightCellNoCars != -1)
                        _x += 1;
                    else if (upCellLight == Utils.TrafficLightState.Green && upCellNoCars != -1)
                        _y -= 1;
                    else if (leftCellLight == Utils.TrafficLightState.Green && leftCellNoCars != -1)
                        _x -= 1;
                }
                else // if next direction is up
                {
                    if (upCellLight == Utils.TrafficLightState.Green && upCellNoCars != -1)
                        _y -= 1;
                    else if (leftCellLight == Utils.TrafficLightState.Green && leftCellNoCars != -1)
                        _x -= 1;
                    else if (rightCellLight == Utils.TrafficLightState.Green && rightCellNoCars != -1)
                        _x += 1;
                }
            }
            // If the cell is an intersection and the priority is over the number of cars from the cell
            else if (_carPriority == Utils.CarPriorityState.LowerTraffic)
            {
                // Select next cell accordingly to the priority
                dx = _x - _finalX;
                int minIndex = GetMinCarsDirection(leftCellNoCars, upCellNoCars, rightCellNoCars);
                
                if (dx > 0) // if next direction is left
                {
                    if (leftCellNoCars != -1 && minIndex == 1)
                        _x -= 1;
                    else if (upCellNoCars != -1 && minIndex == 2)
                        _y -= 1;
                    else if (rightCellNoCars != -1 && minIndex == 3)
                        _x += 1;
                }
                else if (dx < 0) // if next direction is right
                {
                    if (rightCellNoCars != -1 && minIndex == 3)
                        _x += 1;
                    else if (upCellNoCars != -1 && minIndex == 2)
                        _y -= 1;
                    else if (leftCellNoCars != -1 && minIndex == 1)
                        _x -= 1;
                }
                else // if next direction is up
                {
                    if (upCellNoCars != -1 && minIndex == 2)
                        _y -= 1;
                    else if (leftCellNoCars != -1 && minIndex == 1)
                        _x -= 1;
                    else if (rightCellNoCars != -1 && minIndex == 3)
                        _x += 1;
                }
            }
        }

        private int GetMinCarsDirection(int left, int up, int right)
        {
            int min = Int32.MaxValue;
            int minIndex = 0;

            if (left != -1)
            {
                min = left;
                minIndex = 1;
            }
            if (up != -1 && up < min)
            {
                min = up;
                minIndex = 2;
            }

            if (right != -1 && right < min)
            {
                minIndex = 3;
            }

            return minIndex;
        }
        
        private bool IsIntersection(int x, int y)
        {
            return (y > 3 && y < Utils.Size - 3 && x % 2 == 0);
        }
    }
}