BenchmarkSuite
==============

Benchmark framework for .NET and Mono

###Downloads:
- BenchmarkSuite.ConsoleRunner [![ConsoleRunner](https://img.shields.io/nuget/v/BenchmarkSuite.ConsoleRunner.svg)](http://www.nuget.org/packages/BenchmarkSuite.ConsoleRunner/)
- BenchmarkSuite.Framework [![NuGet](https://img.shields.io/nuget/v/BenchmarkSuite.Framework.svg)](http://www.nuget.org/packages/BenchmarkSuite.Framework/)


Introduction
--------------

Benchmark Suite allows to write benchmarks in NUnit-style, when one can mark the methods with [Bench] attributes and framework cares about running and collecting the statistics. The sample of writing benchmarking

	[BenchFixture]
	public class MyClass
	{
        const int nIter=100000000;

		[Bench]
		public void BenchmarkAdding()
		{
			int x = 0;

			var b = b.StartNew ();
            for (int i = 0; i < nIter; i++) {
            	//benchmarking adding operation
				x += 1;
			}
			b.Stop ();

		}

	}

To start benchmarking run `bench-console.exe <your-assembly-name>` from command line and output with benchmarks and statistical data will be generated into the file BenchmarkResults.xml



  
