﻿// ***********************************************************************
// Copyright (c) 2008 Charlie Poole
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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BenchmarkSuite.Framework.Interfaces;

namespace BenchmarkSuite.Framework.Internal.Builders
{
    /// <summary>
    /// SequentialStrategy creates test cases by using all of the
    /// parameter data sources in parallel, substituting <c>null</c>
    /// when any of them run out of data.
    /// </summary>
    public class SequentialStrategy : ICombiningStrategy
    {
        /// <summary>
        /// Gets the test cases generated by the CombiningStrategy.
        /// </summary>
        /// <returns>The test cases.</returns>
        public IEnumerable<ITestCaseData> GetTestCases(IEnumerable[] sources)
        {
            List<ITestCaseData> testCases = new List<ITestCaseData>();

            IEnumerator[] enumerators = new IEnumerator[sources.Length];
            for (int i = 0; i < sources.Length; i++)
                enumerators[i] = sources[i].GetEnumerator();

            for (; ; )
            {
                bool gotData = false;
                object[] testdata = new object[sources.Length];

                for (int i = 0; i < sources.Length; i++)
                    if (enumerators[i].MoveNext())
                    {
                        testdata[i] = enumerators[i].Current;
                        gotData = true;
                    }
                    else
                        testdata[i] = null;

                if (!gotData)
                    break;

                ParameterSet parms = new ParameterSet(testdata);
                testCases.Add(parms);
            }

            return testCases;
        }
    }
}
