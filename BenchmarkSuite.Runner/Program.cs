using System;
using BenchmarkSuite.Framework.Api;
using BenchmarkSuite.Framework.Interfaces;
using BenchmarkSuite.ConsoleRunner.Options;
using System.Collections.Generic;
using BenchmarkSuite.Framework.Internal;

namespace BenchmarkSuite.Runner
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			ConsoleOptions options = new ConsoleOptions();

			options.Parse(args);

			var settings = new Dictionary<string, object>();

			DefaultTestAssemblyBuilder builder = new DefaultTestAssemblyBuilder ();
			NUnitTestAssemblyRunner runner = new NUnitTestAssemblyRunner (builder);

			ITest test = runner.Load ("BenchmarkSuite.Tests.dll", settings);

			ITestResult result = runner.Run (TestListener.NULL, TestFilter.Empty);

//			ITest test = builder.Build ("BenchmarkSuite.Tests.dll", settings);

			int c = test.TestCaseCount;

			ITest t1 = test.Tests [0];

			Benchmark b = new Benchmark (); 

			int x = 0;

			b.Start ();
			for (int i = 0; i < 1000000; i++) {
				x += 1;
			}
			b.Stop ();

            PrintBenchmarks(result);
			Console.WriteLine ("Benchmark '{0}' ms='{1}'", b.Name, b.ElapsedTicks);
		}

        public static void PrintBenchmarks(ITestResult result)
        {
            foreach (BenchmarkResult br in result.BenchmarkResults)
            {
                Console.WriteLine("Name={0}, Mean={1}, StdDev={2}", br.Name, br.Mean, br.StdDev);
                foreach (Benchmark b in br.Benchmarks)
                {
                    Console.WriteLine("Name={0}, Elapsed={1}", b.Name, b.ElapsedTicks);
                }
            }

            if (result.HasChildren)
            {
                foreach (ITestResult child in result.Children)
                {
                    PrintBenchmarks(child);
                }
            }
        }
	}
}
