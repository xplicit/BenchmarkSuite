// ***********************************************************************
// Copyright (c) 2009 Charlie Poole
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
using BenchmarkSuite.Framework.Interfaces;

namespace BenchmarkSuite.Framework.Internal
{
    /// <summary>
    /// TestListener provides an implementation of ITestListener that
    /// does nothing. It is used only throught its NULL property.
    /// </summary>
    public class TestListener : ITestListener
    {
        /// <summary>
        /// Called when a test has just started
        /// </summary>
        /// <param name="test">The test that is starting</param>
        public void TestStarted(ITest test){}

        /// <summary>
        /// Called when a test case has finished
        /// </summary>
        /// <param name="result">The result of the test</param>
        public void TestFinished(ITestResult result){}

        /// <summary>
        /// Called when a new benchmarking iteration started
        /// </summary>
        /// <param name="test">The test that has finished</param>
        /// <param name="iter">Current iteration number</param>
        /// <param name="iterCount">Total number of iterations</param>
        public void BenchmarkIterationStarted(ITest test, int iter, int iterCount){}

        /// <summary>
        /// Called when a new benchmarking iteration finished
        /// </summary>
        /// <param name="test">The test that is starting</param>
        /// <param name="iter">Current iteration number</param>
        /// <param name="iterCount">Total number of iterations</param>
        public void BenchmarkIterationFinished(ITest test, int iter, int iterCount){}

        /// <summary>
        /// Construct a new TestListener - private so it may not be used.
        /// </summary>
        private TestListener() { }

        /// <summary>
        /// Get a listener that does nothing
        /// </summary>
        public static ITestListener NULL
        {
            get { return new TestListener();}
        }
    }
}
