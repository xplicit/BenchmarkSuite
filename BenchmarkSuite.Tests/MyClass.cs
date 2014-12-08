using System;
using NUnit.Framework;


namespace BenchmarkSuite.Tests
{
	[TestFixture]
	public class MyClass
	{
		public MyClass ()
		{
		}

		[Test]
		public void Bench1()
		{
			Console.WriteLine ("Bench1");
		}

		[Test]
		public void Bench2()
		{
			Console.WriteLine ("Bench2");
		}
	}
}

