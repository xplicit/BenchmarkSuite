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

            Benchmark b1 = new Benchmark (); 

            x = 0;

            b1.Start ();
            for (int i = 0; i < 1000000; i++) {
                x += 1;
            }
//            b1.Stop ();

            PrintBenchmarks(result);
			Console.WriteLine ("Benchmark b '{0}' ms='{1}'", b.Name, b.ElapsedTicks);
//            Console.WriteLine ("Benchmark b1 '{0}' ms='{1}'", b1.Name, b1.ElapsedTicks);
		}

        public static void PrintBenchmarks(ITestResult result)
        {
            foreach (BenchmarkResult br in result.BenchmarkResults)
            {
                Console.WriteLine("Name={0}, Mean={1}, Min={2}, Max={3}, StdDev={4}, StdDev%={5}", br.Name, br.Mean, br.Min, br.Max, br.StdDev, br.StdDevPercents);
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
