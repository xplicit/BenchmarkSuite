using System;
using NUnit.Framework;


namespace BenchmarkSuite.Tests
{
	[BenchFixtureAttribute]
	public class MyClass
	{
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
			for (int i = 0; i < 1000000; i++) {
				x += 1;
			}
			b.Stop ();

		}

		[Bench]
		public void Bench2()
		{
			Console.WriteLine ("Bench2");
			Benchmark b = new Benchmark (); 

			int x = 0;

			b.Start ();
			for (int i = 0; i < 1000000; i++) {
				x += 1;
			}
			b.Stop ();
		}
	}
}

