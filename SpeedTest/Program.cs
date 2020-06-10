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
            Console.WriteLine("This will show how much internets is being used on a certain Netowrk Interface Controller");

            var nics = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
            int nicNo = 0;
            foreach (var networkInterface in nics)
            {
                Console.WriteLine(nicNo + ": " + networkInterface.Name);
                nicNo++;
            }

            Console.Write("\r\nSelect a NIC: ");
            var nicNumber = Console.ReadLine();
            var nicName = nics[Int32.Parse(nicNumber)].Name;

            Console.Write("\r\nNumber of test cycles to run: ");
            var cycles = Console.ReadLine();

            for (var t = 1; t <= Int32.Parse(cycles); t++)
            {
                var nic = nics.Single(n => n.Name == nicName);
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
                        Console.WriteLine("Result " + t + "." + j + ": " + mbs.ToString("N4") + " mbps");
                        j++;
                    }
                }
                timer.Stop();
                var elapsedTime = timer.ElapsedMilliseconds;

                var average = (results.Sum() / results.Count());

                var lowestValue = results.Min().ToString("N4");
                var highestValue = results.Max().ToString("N4");

                var stdDev = StdDev(results, average);

                ResultsModel resultsModel = new ResultsModel
                {
                    AverageValue = average.ToString("N4"),
                    LowestValue = lowestValue,
                    HighestValue = highestValue,
                    StdDeviation = stdDev.ToString("N4"),
                    TimeCompleted = DateTime.Now
                };

                WriteAwayData(resultsModel);

                Console.WriteLine("");
                Console.WriteLine("Time taken: " + elapsedTime + " milliseconds");
                Console.WriteLine("Lowest download speed: " + lowestValue + "mbps");
                Console.WriteLine("Highest download speed: " + highestValue + "mbps");
                Console.WriteLine("Average download speed: " + average.ToString("N4") + "mbps");
                Console.WriteLine("Standard deviation: " + stdDev.ToString("0.00"));
                Console.WriteLine("\r\n");
            }

            //Open the results
            Console.WriteLine("\r\nOpening results...");
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            string _path = FileName();
            startInfo.Arguments = string.Format("/C start {0}", _path);
            process.StartInfo = startInfo;
            process.Start();
            
            Console.WriteLine("\r\nPress any key to quit");
            Console.ReadKey();
        }

        private static double StdDev(List<double> values, double mean)
        {
            // Get the sum of the squares of the differences between the values and the mean.
            var squaresQuery = from double value in values select (value - mean) * (value - mean);
            double sumOfSquares = squaresQuery.Sum();

            return Math.Sqrt(sumOfSquares / values.Count());
        }

        private static string FileName()
        {
            string folderName = @"c:\SpeedTests";
            var today = DateTime.Now.Date.ToShortDateString();
            var todayFormatted = today.Replace("/", "_");
            string pathString = Path.Combine(folderName, todayFormatted);
            System.IO.Directory.CreateDirectory(pathString);
            string fileName = "Output.csv";
            pathString = System.IO.Path.Combine(pathString, fileName);
            return pathString;
        }

        private static void WriteAwayData(ResultsModel model)
        {
            string pathString = FileName();
            if (!File.Exists(pathString))
            {
                using (FileStream fileStream = File.Create(pathString))
                {
                    StreamWriter streamWriter = new StreamWriter(fileStream);
                    streamWriter.AutoFlush = true;
                    streamWriter.WriteLine(
                        "Test completed at, Min, Max, Avg, StdDev");
                    streamWriter.WriteLine("{0},{1},{2},{3},{4}",
                        model.TimeCompleted, model.LowestValue, model.HighestValue, model.AverageValue, model.StdDeviation);
                }
            }
            else
            {
                using (FileStream fileStream = new FileStream(pathString, FileMode.Append))
                {
                    StreamWriter streamWriter = new StreamWriter(fileStream);
                    streamWriter.AutoFlush = true;
                    streamWriter.WriteLine(
                        "{0},{1},{2},{3},{4}",
                        model.TimeCompleted, model.LowestValue, model.HighestValue, model.AverageValue, model.StdDeviation);
                }
            }
        }
    }
}
