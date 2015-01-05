using System;
using BenchmarkSuite.Framework.Interfaces;

namespace BenchmarkSuite.Logging
{
    public class EmptyLogFactory : ILogFactory
    {
        #region ILogFactory implementation

        public ILog GetLogger(string name)
        {
            return new EmptyLogger();
        }

        public ILog GetLogger(Type type)
        {
            return new EmptyLogger();
        }

        #endregion

        public EmptyLogFactory()
        {
        }
    }
}

