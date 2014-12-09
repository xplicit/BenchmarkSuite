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

        public long ElapsedMilleseconds { get { return elapsed * 1000 / Stopwatch.Frequency; } }

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

        public static Benchmark StartNew()
        {
            string name = new StackFrame (1, true).GetMethod ().Name;
            return Benchmark.StartNew(name);
        }

        public static Benchmark StartNew(string name)
        {
            Benchmark b = new Benchmark(name);
            b.Start();
            return b;
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

