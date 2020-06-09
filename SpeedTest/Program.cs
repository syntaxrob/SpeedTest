using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace SpeedTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var nics = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            var nic = nics.Single(n => n.Name == "Ethernet");
            
            var reads = Enumerable.Empty<double>();
            
            var sw = new Stopwatch();
            var timer = Stopwatch.StartNew();

            var lastBr = nic.GetIPv4Statistics().BytesReceived;
            
            List<double> results = new List<double>();
            int j = 1;
            
            for (var i = 0; i < 1000; i++)
            {
                sw.Restart();
                Thread.Sleep(100);
                var elapsed = sw.Elapsed.TotalSeconds;
                var br = nic.GetIPv4Statistics().BytesReceived;

                var local = (br - lastBr) / elapsed;
                lastBr = br;

                // Keep last 20, ~2 seconds
                reads = new[] { local }.Concat(reads).Take(20);
                
                if (i % 10 == 0)
                { // ~1 second
                    var bSec = reads.Sum() / reads.Count();
                    var kbs = (bSec * 8) / 1024;
                    var mbs = kbs / 1000;
                    results.Add(mbs);
                    //Console.WriteLine("Kb/s ~ " + kbs);
                    Console.WriteLine("Result " + j + ": " + mbs.ToString("N4") + " mbps");
                    j++;
                }
            }
            timer.Stop();
            var elapsedTime = timer.ElapsedMilliseconds;

            var average = (results.Sum() / results.Count());

            var lowestValue = results.Min().ToString("N4");
            var highestValue = results.Max().ToString("N4");

            var stdDev = StdDev(results, average);

            Console.WriteLine("");
            Console.WriteLine("Time taken: " + elapsedTime + " milliseconds");
            Console.WriteLine("Lowest download speed: " + lowestValue + "mbps");
            Console.WriteLine("Highest download speed: " + highestValue + "mbps");
            Console.WriteLine("Average download speed: " + average.ToString("N4") + "mbps");
            Console.WriteLine("Standard deviation: " + stdDev.ToString("0.00"));
            Console.ReadLine();
        }

        private static double StdDev(List<double> values, double mean)
        {
            // Get the sum of the squares of the differences between the values and the mean.
            var squaresQuery = from double value in values select (value - mean) * (value - mean);
            double sumOfSquares = squaresQuery.Sum();

            return Math.Sqrt(sumOfSquares / values.Count());
        }
    }
}
