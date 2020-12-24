using ActressMas;

namespace TrafficSimulator
{
    public class Program
    {
        private static void Main(string[] args)
        {
            TurnBasedEnvironment env = new TurnBasedEnvironment(0, 100);

            var planetAgent = new IntersectionAgent();

            for (int j = 2; j <= 4; j += 2)
            {
                for (int i = 0; i < 8; i += 2)
                {
                    var trafficLightAgent = new TrafficLightAgent(i * 2 + j, i, j);
                    env.Add(trafficLightAgent, "trafficLight" + i * 2 + j);
                }
            }
            
            for (int i = 0; i < Utils.NoExplorers; i++)
            {
                var explorerAgent = new CarAgent(i, i+1);
                env.Add(explorerAgent, "explorer" + i);
            }
            
            env.Add(planetAgent, "planet");
            
            env.Start();
        }
    }
}