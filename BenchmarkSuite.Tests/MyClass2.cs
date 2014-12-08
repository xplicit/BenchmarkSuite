using System;
using NUnit.Framework;

namespace BenchmarkSuite.Tests
{
	[TestFixture]
	public class MyClass2
	{
		public MyClass2 ()
		{
		}

		[Test]
		public void Bench1()
		{
			Console.WriteLine ("Bench1");
		}

	}
}

