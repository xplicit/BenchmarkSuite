using System;
using BenchmarkSuite.Framework.Internal;

namespace BenchmarkSuite.Framework
{
    public class IterationsAttribute : BenchBaseAttribute, IApplyToTest
    {
        /// <summary>
        /// Number of test iterations
        /// </summary>
        public int Count { get; set; } 

        public IterationsAttribute(int count)
        {
            Count = count;
        }

        #region IApplyToTest implementation

        public void ApplyToTest(Test test)
        {
            test.Properties.Add(PropertyNames.Iterations, Count);
        }

        #endregion
    }
}

