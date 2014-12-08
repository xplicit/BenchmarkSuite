using System;
using System.Diagnostics;
using BenchmarkSuite.Framework.Internal.Commands;

namespace BenchmarkSuite
{
	public class Benchmark
	{
		Stopwatch sw = new Stopwatch();
		long elapsed;

		public string Name { get; set; }

		public long ElapsedTicks { get { return elapsed; } }

		public Benchmark ()
		{
			Name = new StackFrame (1, true).GetMethod ().Name;
			TestCommand.Benchmarks.Add (this);
		}

		public Benchmark(string name)
		{
			Name = name;
			TestCommand.Benchmarks.Add (this);
		}


		public void Start()
		{
			GC.Collect ();
			GC.WaitForPendingFinalizers ();

			sw.Start ();
		}

		public void Stop()
		{
			sw.Stop ();

			elapsed = sw.ElapsedTicks;
		}

	}
}

