using System;
using BenchmarkSuite.Framework.Interfaces;
using BenchmarkSuite.Framework.Internal;

namespace BenchmarkSuite.ConsoleRunner
{
    public class ConsoleTestListener : ITestListener
    {
        const int indent = 2;
        int curIndent = 0;

        #region ITestListener implementation

        public void TestStarted(ITest test)
        {
            if (test is TestMethod) 
            {
                Console.Write("{0,"+curIndent+"}{1} => ", String.Empty, test.Name);
            }
            else
            {
                Console.WriteLine("{0,"+curIndent+"}{1}", String.Empty, test.FullName);
            }
            curIndent += indent;

        }

        public void TestFinished(ITestResult result)
        {
            curIndent -= indent;

            if (result.Test is TestMethod)
            {
                if (result.ResultState == ResultState.Success)
                {
                    if (result.BenchmarkResults.Count > 0)
                    {
                        var b = result.BenchmarkResults[0];
                        string format = " Mean={0:F2}, Min={1:F2}, Max={2:F2}, StdDev={3:F2}, StdErr={4:F2}%";
                        if (!double.IsNaN(b.OpsPerSecond))
                            format += ", Ops={5:F2}";
                        Console.WriteLine(format, b.Mean, b.Min, b.Max, b.StdDev, b.StdErrPercents, b.OpsPerSecond);
                    }
                    else
                    {
                        Console.WriteLine();
                    }
                }
                else
                {
                    Console.WriteLine(" {0}", result.ResultState);
                }
            }
        }


        public void BenchmarkIterationStarted(ITest test, int iter, int iterCount)
        {
        }

        public void BenchmarkIterationFinished(ITest test, int iter, int iterCount)
        {
            Console.Write("*");
        }
        #endregion
    }
}

