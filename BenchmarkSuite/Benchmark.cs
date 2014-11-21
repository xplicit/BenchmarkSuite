using System;
using System.Diagnostics;

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
		}

		public Benchmark(string name)
		{
			Name = name;
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

