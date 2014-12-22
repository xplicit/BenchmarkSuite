using System;
using BenchmarkSuite.Framework;

namespace BenchmarkSuite.Tests
{
	[BenchFixture]
	public class MyClass2
	{
        const int nIter = 10000000;

		public MyClass2 ()
		{
		}

		[Bench]
		public void Bench1()
		{
            string s = "this is the test string";
            int h;

            var b = Benchmark.StartNew();
            for (int i = 0; i < nIter; i++)
            {
                h = s.GetHashCode();
            }
            b.Stop();
		}

	}
}

