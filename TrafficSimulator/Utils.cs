using System;
using System.Collections.Generic;
using System.Text;

namespace TrafficSimulator
{
    public static class Utils
    {
        public static int Size = 7;
        public static int NoCars = 100;
        public static int NoCarsPerCell = 3;        // grid with NoCarsPerCell*NoCarsPerCell cars
        public static int MaxNoCarsPerCell = NoCarsPerCell * NoCarsPerCell;
        public static int NoStartingPoints = (Size+1) / 2;
        public static int LightSwitchingTime = 10;
        public enum TrafficLightState { Green, Red, Unavailable };
        public enum CarPriorityState { NoPriority, GreenLight, LowerTraffic };
        public enum TrafficLightIntelligenceState { L0, L1, L2, L3 };
        public static Random RandNoGen = new Random();

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
        
        public static void ParseMessage(string str, out string action, out int[,] parameters)
        {
            string[] t = str.Split();
            int index = 1;

            action = t[0];
            parameters = new int[Size, Size];

            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    parameters[i, j] = Convert.ToInt32(t[index]);
                    index++;
                }
            }
        }

        public static string Str(object p1, object p2)
        {
            return string.Format("{0} {1}", p1, p2);
        }

        public static string Str(object p1, object p2, object p3)
        {
            return string.Format("{0} {1} {2}", p1, p2, p3);
        }
        
        public static string Str(object p1, object p2, object p3, object p4)
        {
            return string.Format("{0} {1} {2} {3}", p1, p2, p3, p4);
        }
        
        public static string Str(object p1, object p2, object p3, object p4, object p5, object p6, object p7)
        {
            return string.Format("{0} {1} {2} {3} {4} {5} {6}", p1, p2, p3, p4, p5, p6, p7);
        }

        public static void Print2DArray<T>(T[,] matrix)
        {
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    Console.Write(matrix[i, j] + "\t");
                }

                Console.WriteLine();
            }
        }

        public static string CreateMessage(int[,] matrix, string message)
        {
            var sb = new StringBuilder();

            sb.Append(message);
            sb.Append(" ");
            
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    sb.Append(matrix[i, j]);
                    sb.Append(" ");
                }
            }

            return sb.ToString();
        }
    }
}