using System;
using BenchmarkSuite.Framework;


namespace BenchmarkSuite.Tests
{
	[BenchFixture]
	public class MyClass
	{
        const int nIter=100000000;

		public MyClass ()
		{
		}

		[Bench]
		public void Bench1()
		{
			Console.WriteLine ("Bench1");
			Benchmark b = new Benchmark (); 

			int x = 0;

			b.Start ();
            for (int i = 0; i < nIter; i++) {
				x += 1;
			}
			b.Stop ();

		}

		[Bench]
        [Iterations(100000)]
		public void Bench2()
		{
            Console.WriteLine ("Bench2 {0}",Benchmark.Iter);
			Benchmark b = new Benchmark (); 

			int x = 0;

			b.Start ();
            for (int i = 0; i < nIter; i++) {
				x += 1;
			}
			b.Stop ();
		}
	}
}

