﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using BenchmarkSuite.Framework.Interfaces;
using System.Xml;
using BenchmarkSuite.Common;

namespace BenchmarkSuite.Framework.Internal
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
        /// Number of iterations in one measure
        /// </summary>
        public int Iterations { get; private set; }

        /// <summary>
        /// Gets the ops per second.
        /// </summary>
        /// <value>The ops per second.</value>
        public double OpsPerSecond { get { return (Iterations > 0) ? 1000.0 * ((double)Iterations) / Mean : double.NaN; } } 

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
        /// Relation of Standard Error to Mean in percents
        /// </summary>
        public double StdErrPercents { get { return (StdDev / (Mean * Math.Sqrt((double)Count))) * 100.0; } }

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
                BenchmarkResult br = new BenchmarkResult() { Iterations = -1, Name = gr.Key, Count = gr.Count() - 1, Benchmarks = new List<Benchmark>() };
                results.Add(br);
                bool isFirst = true;

                foreach (Benchmark b in gr)
                {
                    //we do not calculate the first result
                    //because JIT-ing the method at the first call 
                    //can influence on the results
                    if (isFirst)
                    {
                        //TODO: we are using static property 'Iter'.  
                        //Not so good, because not obvious behaviour. 
                        //Currently we're assuming, that the CalculateResults 
                        //is called after benchmarking
                        br.Iterations = Benchmark.Iter;
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

        public virtual XmlNode AddToXml(XmlNode parentNode, bool recursive)
        {
            XmlNode thisNode = parentNode.AddElement("benchmark-result");

            thisNode.AddAttribute("name", this.Name);
            thisNode.AddAttribute("count", this.Count.ToString());
            thisNode.AddAttribute("min", this.Min.ToString());
            thisNode.AddAttribute("max", this.Max.ToString());
            thisNode.AddAttribute("mean", this.Mean.ToString());
            thisNode.AddAttribute("ops-per-sec", this.OpsPerSecond.ToString());
            thisNode.AddAttribute("stddev", this.StdDev.ToString());
            thisNode.AddAttribute("stderr-percent", this.StdErrPercents.ToString());

            if (this.Benchmarks != null)
            {
                XmlNode bmNode = thisNode.AddElement("benchmarks");

                foreach(Benchmark b in Benchmarks)
                {
                    XmlNode bNode = bmNode.AddElement("benchmark");
                    bNode.AddAttribute("name", b.Name);
                    bNode.AddAttribute("elapsed-ms", b.ElapsedMilleseconds.ToString());
                    bNode.AddAttribute("elapsed-ticks", b.ElapsedTicks.ToString());
                }
            }

            return thisNode;
        }
    }
}

