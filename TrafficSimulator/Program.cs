using ActressMas;

namespace TrafficSimulator
{
    public class Program
    {
        private static void Main(string[] args)
        {
            // Control parameters
            Utils.TrafficLightIntelligenceState trafficLightIntelligence = Utils.TrafficLightIntelligenceState.L0;
            int[] carsRate = {1, 1, 1, 1};  // Nr of cars per each entry point
            Utils.CarPriorityState carsPriority = Utils.CarPriorityState.GreenLight;

            int lightSwitchingTime = 5; // Number of turns
            
            TurnBasedEnvironment env = new TurnBasedEnvironment(0, 100);

            var planetAgent = new IntersectionAgent();
            
            for (int j = 2; j <= 4; j += 2)
            {
                for (int i = 0; i < 8; i += 2)
                {
                    int index = i * 2 + j;
                    var trafficLightAgent = new TrafficLightAgent(index, i, j, lightSwitchingTime);
                    env.Add(trafficLightAgent, "trafficLight" + index);
                }
            }

            int carIndex = 0;
            int noCarsLeft = Utils.NoCars;
            
            while (noCarsLeft != 0)
            {
                for (int i = 0; i < Utils.NoStartingPoints && noCarsLeft != 0; i++)
                {
                    for (int j = 0; j < carsRate[i] && noCarsLeft != 0; j++)
                    {
                        var explorerAgent = new CarAgent(carIndex, carIndex+1, i*2);
                        env.Add(explorerAgent, "explorer" + carIndex);
                        noCarsLeft--;
                        carIndex++;
                    }
                }
            }
            
            env.Add(planetAgent, "planet");
            
            env.Start();
        }
    }
}