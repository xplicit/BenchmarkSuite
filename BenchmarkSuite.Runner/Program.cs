using System;

namespace BenchmarkSuite.Runner
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Benchmark b = new Benchmark (); 

			int x = 0;

			b.Start ();
			for (int i = 0; i < 1000000; i++) {
				x += 1;
			}
			b.Stop ();

			Console.WriteLine ("Benchmark '{0}' ms='{1}'", b.Name, b.ElapsedTicks);
		}
	}
}
