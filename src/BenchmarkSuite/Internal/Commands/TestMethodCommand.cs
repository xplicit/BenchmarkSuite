// ***********************************************************************
// Copyright (c) 2010 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.Reflection;
using BenchmarkSuite.Framework.Interfaces;
using BenchmarkSuite;

namespace BenchmarkSuite.Framework.Internal.Commands
{
    /// <summary>
    /// TestMethodCommand is the lowest level concrete command
    /// used to run actual test cases.
    /// </summary>
    public class TestMethodCommand : TestCommand
    {
        private readonly TestMethod testMethod;
        private readonly object[] arguments;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestMethodCommand"/> class.
        /// </summary>
        /// <param name="testMethod">The test.</param>
        public TestMethodCommand(TestMethod testMethod) : base(testMethod)
        {
            this.testMethod = testMethod;
            this.arguments = testMethod.Arguments;
        }

        /// <summary>
        /// Runs the test, saving a TestResult in the execution context, as
        /// well as returning it. If the test has an expected result, it
        /// is asserts on that value. Since failed tests and errors throw
        /// an exception, this command must be wrapped in an outer command,
        /// will handle that exception and records the failure. This role
        /// is usually played by the SetUpTearDown command.
        /// </summary>
        /// <param name="context">The execution context</param>
        public override TestResult Execute(TestExecutionContext context)
        {
            var thisThread = System.Threading.Thread.CurrentThread.ManagedThreadId;
            // TODO: Decide if we should handle exceptions here
            Benchmarks.Clear();

            //number of iterations inside  the benchmark
            int iter = -1;
            if (testMethod.Properties.ContainsKey(PropertyNames.Iterations))
            {
                iter = (int)testMethod.Properties.Get(PropertyNames.Iterations);
            }
            Benchmark.Iter = iter;

            // we skip the first iteration, so add +1 to number of benchmarks
            int benchCount = 5 + 1;

            ITest curTest = testMethod;

            //search first parent, where BenchCount is set
            while (curTest.Properties.Get(PropertyNames.BenchCount) == null && curTest.Parent != null)
                curTest = curTest.Parent;

            if (curTest.Properties.Get(PropertyNames.BenchCount) != null)
            {
                benchCount = (int)curTest.Properties.Get(PropertyNames.BenchCount) + 1;
            }

            for (int i = 0; i < benchCount; i++)
            {
                context.Listener.BenchmarkIterationStarted(context.CurrentTest,i,benchCount);
                object result = RunTestMethod(context);
                context.Listener.BenchmarkIterationFinished(context.CurrentTest,i,benchCount);
            }
			//TODO: write bench
//            if (testMethod.HasExpectedResult)
//                BenchmarkSuite.Framework.Assert.AreEqual(testMethod.ExpectedResult, result);

            context.CurrentResult.SetResult(ResultState.Success);

            var results = BenchmarkResult.CalculateResults(Benchmarks);

            foreach (BenchmarkResult br in results)
            {
                context.CurrentResult.BenchmarkResults.Add(br);
            }

            // TODO: Set assert count here?
            //context.CurrentResult.AssertCount = context.AssertCount;
            return context.CurrentResult;
        }

        private object RunTestMethod(TestExecutionContext context)
        {
#if NET_4_0 || NET_4_5
            if (AsyncInvocationRegion.IsAsyncOperation(testMethod.Method))
                return RunAsyncTestMethod(context);
            else
#endif
                return RunNonAsyncTestMethod(context);
        }

#if NET_4_0 || NET_4_5
        private object RunAsyncTestMethod(TestExecutionContext context)
        {
            using (AsyncInvocationRegion region = AsyncInvocationRegion.Create(testMethod.Method))
            {
                object result = Reflect.InvokeMethod(testMethod.Method, context.TestObject, arguments);

                try
                {
                    return region.WaitForPendingOperationsToComplete(result);
                }
                catch (Exception e)
                {
                    throw new NUnitException("Rethrown", e);
                }
            }
        }
#endif

        private object RunNonAsyncTestMethod(TestExecutionContext context)
        {
            return Reflect.InvokeMethod(testMethod.Method, context.TestObject, arguments);
        }
    }
}