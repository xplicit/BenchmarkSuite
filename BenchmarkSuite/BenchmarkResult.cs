﻿using System;
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

        /// <summary>
        /// Benchmark name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Number of measurements
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Mean value of benchmarks in ms
        /// </summary>
        public double Mean { get ; private set; }

        /// <summary>
        /// Minimum value of benchmarks in ms
        /// </summary>
        public double Min { get; private set; }

        /// <summary>
        /// Maximum value of benchmarks in ms
        /// </summary>
        public double Max { get; private set; }

        /// <summary>
        /// Standard deviation of benchmarks in ms
        /// </summary>
        public double StdDev {get; private set; }

        /// <summary>
        /// Relation of Standard Deviation to Mean in percents
        /// </summary>
        public double StdDevPercents { get { return (StdDev / Mean) * 100.0; } }

        /// <summary>
        /// List of benchmarking raw data
        /// </summary>
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
                    //we do not calculate the first result
                    //because JIT-ing the method at the first call 
                    //can influence on the results
                    if (isFirst)
                    {
                        br.Min = double.MaxValue;
                        br.Max = double.MinValue;
                        isFirst = false;
                        continue;
                    }
                    br.Benchmarks.Add(b);
                    br.Mean += (double)b.ElapsedTicks;

                    if (b.ElapsedTicks < br.Min)
                        br.Min = b.ElapsedTicks;
                    if (b.ElapsedTicks > br.Max)
                        br.Max = b.ElapsedTicks;
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
                br.Min = ToMilliseconds(br.Min);
                br.Max = ToMilliseconds(br.Max);
            }


            return results;
        }

        public static double ToMilliseconds(double ticks)
        {
            return ticks / (double)(Stopwatch.Frequency / 1000);
        }
    }
}

