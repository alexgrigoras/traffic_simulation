using ActressMas;

namespace Reactive
{
    public class Program
    {
        private static void Main(string[] args)
        {
            //citesti fisieru ala de config si creeaza agentii cu parametri aia
            
            IntersectionEnv env = new IntersectionEnv(0, 100);

            var streetGridAgent = new StreetGridAgent();
            env.Add(streetGridAgent, "streetGridAgent");
            
            for (int i = 1; i <= Utils.NoCars; i++)
            {
                var explorerAgent = new CarAgent();
                env.Add(explorerAgent, "car" + i);
            }
            
            for (int i = 1; i <= Utils.NoSemaphores; i++)
            {
                var explorerAgent = new SemaphoreAgent();
                env.Add(explorerAgent, "explorer" + i);
            }
            

            env.Start();
        }
    }
}