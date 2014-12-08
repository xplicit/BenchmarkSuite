using System;
using NUnit.Framework;

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
			Console.WriteLine ("Bench1");
		}

	}
}

