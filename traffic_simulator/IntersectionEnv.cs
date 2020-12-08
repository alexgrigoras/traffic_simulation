using System;
using System.Collections.Generic;
using ActressMas;

namespace Reactive

{
    public class IntersectionEnv : TurnBasedEnvironment
    {
        // voiam sa folosim asta in ideea ca refolosim agentii, dar dupa cum am discutat putem sa pornim o gramada
        // si le dam un delay la inceput pana cand sa porneasca
        public List<Agent> activeCarAgents;
        public List<Agent> inactiveCarAgents;

        public IntersectionEnv(
            int numberOfTurns = 0,
            int delayAfterTurn = 0,
            bool randomOrder = true,
            Random rand = null)
        {
            //trebuie sa ne folosim de constructoru parinte
            //TurnBasedEnvironment(numberOfTurns, delayAfterTurn, randomOrder, rand);
        }
    }
}