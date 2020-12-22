using System;
using ActressMas;

namespace Reactive
{
    public class Program
    {
        private static void Main(string[] args)
        {
            Random rnd = new Random();
            //citesti fisieru ala de config si creeaza agentii cu parametri aia
            
            IntersectionEnv env = new IntersectionEnv(0, 100);

            var streetGridAgent = new StreetGridAgent();
            env.Add(streetGridAgent, "streetGridAgent");
            
            for (int i = 0; i < Utils.NoCars; i++)
            {
                var carAgent = new CarAgent(rnd.Next(8)/2, 0, rnd.Next(8)/2, 4, 1, 1, i);
                env.Add(carAgent, "car" + i);
            }
            
            //da pozitiile semafoarelor; (3,1), (3,3), (3,5)
            // for (int i = 0; i < Utils.NoSemaphores; i++)
            // {
            //     var semaphoreAgent = new SemaphoreAgent((3 + (i / 4) * 2), i % 4,  1);
            //     env.Add(semaphoreAgent, "explorer" + i);
            // }
            

            env.Start();
        }
    }
}