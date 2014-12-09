using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace BenchmarkSuite
{
    public class BenchmarkResult
    {
        public BenchmarkResult()
        {
        }

        public string Name { get; private set; }

        public int Count { get; private set; }

        public double Mean { get ; private set; }

        public double StdDev {get; private set; }

        public IList<Benchmark> Benchmarks { get; private set; }

        public static IList<BenchmarkResult> CalculateResults(IList<Benchmark> benchmarks)
        {
            List<BenchmarkResult> results = new List<BenchmarkResult>();

            var groups = benchmarks.GroupBy(b => b.Name);

            foreach (var gr in groups)
            {
                BenchmarkResult br = new BenchmarkResult() { Name = gr.Key, Count = gr.Count() - 1, Benchmarks = new List<Benchmark>() };
                results.Add(br);
                bool isFirst = true;

                foreach (Benchmark b in gr)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                        continue;
                    }
                    br.Benchmarks.Add(b);
                    br.Mean += (double)b.ElapsedTicks;
                }
                br.Mean /= (double)(br.Count);

                isFirst = true;

                foreach (Benchmark b in gr)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                        continue;
                    }
                    br.StdDev += ((double)b.ElapsedTicks - br.Mean) * ((double)b.ElapsedTicks - br.Mean);
                }
                br.StdDev = Math.Sqrt((br.StdDev / (double)(br.Count)));

                //convert final results to milliseconds
                br.Mean = ToMilliseconds(br.Mean);
                br.StdDev = ToMilliseconds(br.StdDev);
            }


            return results;
        }

        public static double ToMilliseconds(double ticks)
        {
            return ticks / (double)(Stopwatch.Frequency / 1000);
        }
    }
}

