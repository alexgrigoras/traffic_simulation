using ActressMas;
using System;
using System.Collections.Generic;

namespace Reactive
{
    public class CarAgent : TurnBasedAgent
    {
        private int _x, _y;
        private int _x_end, _y_end;
        private string _resourceCarried;
        private int intelligence;
        private int priority;
        //cu parametrul asta setam cate sa porneasca deodata, o sa fie dat la constructor
        private int turnsBeforeSpawning;

        private int wantToMoveHere_X;
        private int wantToMoveHere_Y;

        //private enum State { Free, Carrying };

        public CarAgent(int pos_X, int pos_Y, int pos_X_end, int pos_Y_end, int intelligence, int priority, int turnsBeforeSpawning)
        {
            this._x = pos_X;
            this._y = pos_Y;
            this._x_end = pos_X_end;
            this._y_end = pos_Y_end;
            
            this.intelligence = intelligence;
            this.priority = priority;
            this.turnsBeforeSpawning = turnsBeforeSpawning;
        }
        
        public override void Setup()
        {
            Console.WriteLine("Starting " + Name);

            Send("streetGridAgent", Utils.Str("spawn", _x, _y));
        }

        // 
        public override void Act(Queue<Message> messages)
        {
            while(messages.Count > 0)
            {
                Message message = messages.Dequeue();
            
                Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

                string action;
                List<string> parameters;
                Utils.ParseMessage(message.Content, out action, out parameters);

                if (action == "wait")
                {
                    thinkAboutNextMove();
                }
                else if (action == "move")
                {
                    this._x = wantToMoveHere_X;
                    this._y = wantToMoveHere_Y;
                    Send("streetGridAgent", Utils.Str("change", _x, _y));
                }
                else if (action == "info")
                {
                    //some code
                    //send ("i'm trying to move here", x, y)
                }
                else if (action == "done")
                {
                    //destination reached
                }
                else
                {
                    Console.WriteLine("I'M DONE LUL");
                    break;
                }
            }
        }

        private void thinkAboutNextMove()
        {
            wantToMoveHere_X = _x;
            wantToMoveHere_Y = _y + 1;
        }
    }
}