using ActressMas;
using System;
using System.Configuration;

namespace TrafficSimulator
{
    public class Program
    {
        private static void Main(string[] args)
        {
            // Read control parameters
            // Traffic Lights Intelligence
            string sAttr;
            sAttr = ConfigurationManager.AppSettings.Get("TrafficLightIntelligence");
            Utils.TrafficLightIntelligenceState trafficLightIntelligence = 
                (Utils.TrafficLightIntelligenceState) Enum.Parse(typeof(Utils.TrafficLightIntelligenceState), sAttr);

            // Cars Rate
            string[] carsRateName = {"CarsRateA", "CarsRateB", "CarsRateC", "CarsRateD"};
            int[] carsRate = new int[Utils.NoStartingPoints];  // Nr of cars per each entry point
            int indexCarRate = 0;
            foreach(string s in carsRateName)
            {
                sAttr = ConfigurationManager.AppSettings.Get(s);
                carsRate[indexCarRate] = Int32.Parse(sAttr);
                indexCarRate++;
            }
            
            // Cars Priority
            sAttr = ConfigurationManager.AppSettings.Get("CarsPriority");
            Utils.CarPriorityState carsPriority = 
                (Utils.CarPriorityState) Enum.Parse(typeof(Utils.CarPriorityState), sAttr);

            int lightSwitchingTime = 10; // Number of turns
        
            // Build environment
            TurnBasedEnvironment env = new TurnBasedEnvironment(0, 300);

            var planetAgent = new IntersectionAgent();
        
            for (int j = 2; j <= 4; j += 2)
            {
                for (int i = 0; i < 8; i += 2)
                {
                    int index = i * 2 + j;
                    var trafficLightAgent = new TrafficLightAgent(index, i, j, lightSwitchingTime, 
                        trafficLightIntelligence);
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
                        var explorerAgent = new CarAgent(carIndex, carIndex+1, i*2, carsPriority);
                        env.Add(explorerAgent, "explorer" + carIndex);
                        noCarsLeft--;
                        carIndex++;
                    }
                }
            }
        
            env.Add(planetAgent, "planet");
        
            // Start Environment
            env.Start();
        }
    }
}