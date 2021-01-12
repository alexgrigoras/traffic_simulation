using System;
using System.Collections.Generic;
using System.Text;

namespace TrafficSimulator
{
    public static class Utils
    {
        public const int Size = 7;
        public const int NoCars = 100;
        public const int NoMsPerTurn = 100;
        public const int NoCarsPerCell = 3;                             // grid with NoCarsPerCell*NoCarsPerCell cars
        public const int MaxNoCarsPerCell = NoCarsPerCell * NoCarsPerCell;
        public const int NoStartingPoints = (Size + 1) / 2;
        public const int LightSwitchingTime = 5;                        // number of turns to change light
        public const bool MultipleCarsPerTurn = true;                   // if there are multiple spawned cars per turn
        private static TrafficLightIntelligenceState _intelligenceState = TrafficLightIntelligenceState.L0;

        public enum TrafficLightState { Green, Red, Unavailable };
        public enum CarPriorityState { NoPriority, GreenLight, LowerTraffic };
        public enum TrafficLightIntelligenceState { L0, L1, L2, L3 };
        public static Random RandNoGen = new Random();

        public static void SetTrafficLightIntelligence(Utils.TrafficLightIntelligenceState state)
        {
            _intelligenceState = state;
        }
        
        public static void ParseMessage(string content, out string action, out List<string> parameters)
        {
            string[] t = content.Split();

            action = t[0];

            parameters = new List<string>();
            for (int i = 1; i < t.Length; i++)
                parameters.Add(t[i]);
        }
        
        public static void ParseMessage(string content, out string action, out string parameters)
        {
            string[] t = content.Split();

            action = t[0];

            parameters = "";

            if (t.Length > 1)
            {
                for (int i = 1; i < t.Length - 1; i++)
                    parameters += t[i] + " ";
                parameters += t[t.Length - 1];
            }
        }
        
        public static void ParseMessage(string str, int x, int y, out string action, out int[,] parameters)
        {
            string[] t = str.Split();
            int index = 1;

            action = t[0];
            parameters = new int[Size, Size];

            switch (_intelligenceState)
            {
                case TrafficLightIntelligenceState.L0:
                    break;
                case TrafficLightIntelligenceState.L1:
                    for (int i = 0; i < Size; i++)
                    {
                        for (int j = 0; j < Size; j++)
                        {
                            if (i >= x - 1 && i <= x + 1 && j >= y - 1 && j <= y + 1)
                            {
                                parameters[i, j] = Convert.ToInt32(t[index]);
                                index++;
                            }
                            else
                            {
                                parameters[i, j] = -1;
                            }
                        }
                    }
                    break;
                case TrafficLightIntelligenceState.L2:
                    for (int i = 0; i < Size; i++)
                    {
                        for (int j = 0; j < Size; j++)
                        {
                            if (i >= x - 2 && i <= x + 2 && j >= y - 2 && j <= y + 2)
                            {
                                parameters[i, j] = Convert.ToInt32(t[index]);
                                index++;
                            }
                            else
                            {
                                parameters[i, j] = -1;
                            }
                        }
                    }
                    break;
                case TrafficLightIntelligenceState.L3:
                    for (int i = 0; i < Size; i++)
                    {
                        for (int j = 0; j < Size; j++)
                        {
                            parameters[i, j] = Convert.ToInt32(t[index]);
                            index++;
                        }
                    }
                    break;
            }
        }

        public static string Str(object p1, object p2)
        {
            return $"{p1} {p2}";
        }

        public static string Str(object p1, object p2, object p3)
        {
            return $"{p1} {p2} {p3}";
        }
        
        public static string Str(object p1, object p2, object p3, object p4)
        {
            return $"{p1} {p2} {p3} {p4}";
        }
        
        public static string Str(object p1, object p2, object p3, object p4, object p5, object p6, object p7)
        {
            return $"{p1} {p2} {p3} {p4} {p5} {p6} {p7}";
        }
        
        public static string BuildMessage(int[,] matrix, string message, int x, int y)
        {
            var sb = new StringBuilder();

            sb.Append(message);
            sb.Append(" ");
            
            switch (_intelligenceState)
            {
                case TrafficLightIntelligenceState.L0:
                    break;
                case TrafficLightIntelligenceState.L1:
                    for (int i = x - 1; i <= x + 1; i++)
                    {
                        for (int j = y - 1; j <= y + 1; j++)
                        {
                            if (i >= 0 && i < Size && j >= 0 && j < Size)
                                sb.Append(matrix[i, j]);
                            else sb.Append(-1);
                            sb.Append(" ");
                        }
                    }
                    break;
                case TrafficLightIntelligenceState.L2:
                    for (int i = x - 2; i <= x + 2; i++)
                    {
                        for (int j = y - 2; j <= y + 2; j++)
                        {
                            if (i >= 0 && i < Size && j >= 0 && j < Size)
                                sb.Append(matrix[i, j]);
                            else sb.Append(-1);
                            sb.Append(" ");
                        }
                    }
                    break;
                case TrafficLightIntelligenceState.L3:
                    for (int i = 0; i < matrix.GetLength(0); i++)
                    {
                        for (int j = 0; j < matrix.GetLength(1); j++)
                        {
                            sb.Append(matrix[i, j]);
                            sb.Append(" ");
                        }
                    }
                    break;
            }

            return sb.ToString();
        }

        public static int Map(int x, int inMin, int inMax, int outMin, int outMax) {
            return (x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
        }
        
        public static void Print2DArray<T>(T[,] matrix)
        {
            for (var i = 0; i < matrix.GetLength(0); i++)
            {
                for (var j = 0; j < matrix.GetLength(1); j++)
                {
                    Console.Write(matrix[i, j] + @"	");
                }
                Console.WriteLine();
            }
        }
    }
}