using System;
using BenchmarkSuite.Framework;

namespace BenchmarkSuite.Tests
{
	[BenchFixture]
	public class MyClass2
	{
		public MyClass2 ()
		{
		}

		[Bench]
		public void Bench1()
		{
            string s = "this is the test string";
            int h;

            var b = Benchmark.StartNew();
            for (int i = 0; i < 100000; i++)
            {
                h = s.GetHashCode();
            }
            b.Stop();
		}

	}
}

