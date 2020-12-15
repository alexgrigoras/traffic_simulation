using System;
using System.Collections.Generic;
using ActressMas;

namespace Reactive

{
    public class IntersectionEnv : TurnBasedEnvironment
    {
        public IntersectionEnv(
            int numberOfTurns = 0,
            int delayAfterTurn = 0,
            bool randomOrder = true,
            Random rand = null) : base(numberOfTurns, delayAfterTurn, randomOrder, rand)
        {
            //in caz ca vrem sa-l folosim
        }
    }
}