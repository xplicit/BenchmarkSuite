﻿// ***********************************************************************
// Copyright (c) 2012-2014 Charlie Poole
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
using System.IO;
using System.Reflection;
using System.Threading;
using BenchmarkSuite.Framework.Interfaces;
using BenchmarkSuite.Framework.Internal;
using BenchmarkSuite.Framework.Internal.Execution;
using BenchmarkSuite.Framework.Common;
using BenchmarkSuite.Framework.Logging;

namespace BenchmarkSuite.Framework.Api
{
    /// <summary>
    /// Implementation of ITestAssemblyRunner
    /// </summary>
    public class NUnitTestAssemblyRunner : ITestAssemblyRunner
    {
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ITestAssemblyBuilder _builder;
        private ManualResetEvent _runComplete = new ManualResetEvent(false);

#if !SILVERLIGHT && !NETCF && !PORTABLE
        // Saved Console.Out and Console.Error
        private TextWriter _savedOut;
        private TextWriter _savedErr;
#endif

#if PARALLEL
        // Event Pump
        private EventPump _pump;
#endif

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NUnitTestAssemblyRunner"/> class.
        /// </summary>
        /// <param name="builder">The builder.</param>
        public NUnitTestAssemblyRunner(ITestAssemblyBuilder builder)
        {
            _builder = builder;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The tree of tests that was loaded by the builder
        /// </summary>
        public ITest LoadedTest { get; private set; }

        /// <summary>
        /// The test result, if a run has completed
        /// </summary>
        public ITestResult Result
        {
            get { return TopLevelWorkItem == null ? null : TopLevelWorkItem.Result; }
        }

        /// <summary>
        /// Indicates whether a test is loaded
        /// </summary>
        public bool IsTestLoaded
        {
            get { return LoadedTest != null; }
        }

        /// <summary>
        /// Indicates whether a test is running
        /// </summary>
        public bool IsTestRunning
        {
            get { return TopLevelWorkItem != null && TopLevelWorkItem.State == WorkItemState.Running; }
        }

        /// <summary>
        /// Indicates whether a test run is complete
        /// </summary>
        public bool IsTestComplete
        {
            get { return TopLevelWorkItem != null && TopLevelWorkItem.State == WorkItemState.Complete; }
        }

        /// <summary>
        /// Our settings, specified when loading the assembly
        /// </summary>
        private IDictionary Settings { get; set; }

        /// <summary>
        /// The top level WorkItem created for the assembly as a whole
        /// </summary>
        private WorkItem TopLevelWorkItem { get; set; }

        /// <summary>
        /// The TestExecutionContext for the top level WorkItem
        /// </summary>
        private TestExecutionContext Context { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads the tests found in an Assembly
        /// </summary>
        /// <param name="assemblyName">File name of the assembly to load</param>
        /// <param name="settings">Dictionary of option settings for loading the assembly</param>
        /// <returns>True if the load was successful</returns>
        public ITest Load(string assemblyName, IDictionary settings)
        {
            Settings = settings;

            Randomizer.InitialSeed = GetInitialSeed(settings);

            return LoadedTest = _builder.Build(assemblyName, settings);
        }

        /// <summary>
        /// Loads the tests found in an Assembly
        /// </summary>
        /// <param name="assembly">The assembly to load</param>
        /// <param name="settings">Dictionary of option settings for loading the assembly</param>
        /// <returns>True if the load was successful</returns>
        public ITest Load(Assembly assembly, IDictionary settings)
        {
            Settings = settings;

            Randomizer.InitialSeed = GetInitialSeed(settings);

            return LoadedTest = _builder.Build(assembly, settings);
        }

        /// <summary>
        /// Count Test Cases using a filter
        /// </summary>
        /// <param name="filter">The filter to apply</param>
        /// <returns>The number of test cases found</returns>
        public int CountTestCases(ITestFilter filter)
        {
            if (LoadedTest == null)
                throw new InvalidOperationException("The CountTestCases method was called but no test has been loaded");

            return CountTestCases(LoadedTest, filter);
        }

        /// <summary>
        /// Run selected tests and return a test result. The test is run synchronously,
        /// and the listener interface is notified as it progresses.
        /// </summary>
        /// <param name="listener">Interface to receive EventListener notifications.</param>
        /// <param name="filter">A test filter used to select tests to be run</param>
        /// <returns></returns>
        public ITestResult Run(ITestListener listener, ITestFilter filter)
        {
            RunAsync(listener, filter);
            WaitForCompletion(Timeout.Infinite);
            return Result;
        }

        /// <summary>
        /// Run selected tests asynchronously, notifying the listener interface as it progresses.
        /// </summary>
        /// <param name="listener">Interface to receive EventListener notifications.</param>
        /// <param name="filter">A test filter used to select tests to be run</param>
        /// <remarks>
        /// RunAsync is a template method, calling various abstract and
        /// virtual methods to be overriden by derived classes.
        /// </remarks>
        public void RunAsync(ITestListener listener, ITestFilter filter)
        {
            log.Info("Running tests");
            if (LoadedTest == null)
                throw new InvalidOperationException("The Run method was called but no test has been loaded");

            CreateTestExecutionContext(listener);

            TopLevelWorkItem = WorkItem.CreateWorkItem(LoadedTest, filter);
            TopLevelWorkItem.InitializeContext(Context);
            TopLevelWorkItem.Completed += OnRunCompleted;

            StartRun(listener);
        }

        /// <summary>
        /// Wait for the ongoing run to complete.
        /// </summary>
        /// <param name="timeout">Time to wait in milliseconds</param>
        /// <returns>True if the run completed, otherwise false</returns>
        public bool WaitForCompletion(int timeout)
        {
#if !SILVERLIGHT && !PORTABLE
            return _runComplete.WaitOne(timeout, false);
#else
            return _runComplete.WaitOne(timeout);
#endif
        }

        /// <summary>
        /// Initiate the test run.
        /// </summary>
        public void StartRun(ITestListener listener)
        {
#if !SILVERLIGHT && !NETCF && !PORTABLE
            // Save Console.Out and Error for later restoration
            _savedOut = Console.Out;
            _savedErr = Console.Error;

//            Console.SetOut(new TextCapture(Console.Out));
//            Console.SetError(new TextCapture(Console.Error));
#endif

#if PARALLEL
            QueuingEventListener queue = new QueuingEventListener();
            Context.Listener = queue;

            _pump = new EventPump(listener, queue.Events);
            _pump.Start();
#else
            Context.Dispatcher = new SimpleWorkItemDispatcher();
#endif

            Context.Dispatcher.Dispatch(TopLevelWorkItem);
        }

        /// <summary>
        /// Signal any test run that is in process to stop. Return without error if no test is running.
        /// </summary>
        /// <param name="force">If true, kill any test-running threads</param>
        public void StopRun(bool force)
        {
            if (IsTestRunning)
            {
                Context.ExecutionStatus = force
                    ? TestExecutionStatus.AbortRequested
                    : TestExecutionStatus.StopRequested;

                if (force)
                    Context.Dispatcher.CancelRun();
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Create the initial TestExecutionContext used to run tests
        /// </summary>
        /// <param name="listener">The ITestListener specified in the RunAsync call</param>
        private void CreateTestExecutionContext(ITestListener listener)
        {
            Context = new TestExecutionContext();

            if (Settings.Contains(DriverSettings.DefaultTimeout))
                Context.TestCaseTimeout = (int)Settings[DriverSettings.DefaultTimeout];
            if (Settings.Contains(DriverSettings.StopOnError))
                Context.StopOnError = (bool)Settings[DriverSettings.StopOnError];

            if (Settings.Contains(DriverSettings.WorkDirectory))
                Context.WorkDirectory = (string)Settings[DriverSettings.WorkDirectory];
            else
                Context.WorkDirectory = Env.DefaultWorkDirectory;

            // Overriding runners may replace this
            Context.Listener = listener;

#if PARALLEL
            int levelOfParallelization = GetLevelOfParallelization();

            if (levelOfParallelization > 0)
            {
                Context.Dispatcher = new ParallelWorkItemDispatcher(levelOfParallelization);
                // Assembly does not have IApplyToContext attributes applied
                // when the test is built, so  we do it here.
                // TODO: Generalize this
                if (LoadedTest.Properties.ContainsKey(PropertyNames.ParallelScope))
                    Context.ParallelScope =
                        (ParallelScope)LoadedTest.Properties.Get(PropertyNames.ParallelScope) & ~ParallelScope.Self;
            }
            else
                Context.Dispatcher = new SimpleWorkItemDispatcher();
#endif
        }


        /// <summary>
        /// Handle the the Completed event for the top level work item
        /// </summary>
        private void OnRunCompleted(object sender, EventArgs e)
        {
#if PARALLEL
            _pump.Dispose();
#endif

#if !SILVERLIGHT && !NETCF && !PORTABLE
            Console.SetOut(_savedOut);
            Console.SetError(_savedErr);
#endif

            _runComplete.Set();
        }

        private int CountTestCases(ITest test, ITestFilter filter)
        {
            if (!test.IsSuite)
                return 1;

            int count = 0;
            foreach (ITest child in test.Tests)
                if (filter.Pass(child))
                    count += CountTestCases(child, filter);

            return count;
        }

        private static int GetInitialSeed(IDictionary settings)
        {
            return settings.Contains(DriverSettings.RandomSeed)
                ? (int)settings[DriverSettings.RandomSeed]
                : new Random().Next();
        }

#if PARALLEL
        private int GetLevelOfParallelization()
        {
            return Settings.Contains(DriverSettings.NumberOfTestWorkers)
                ? (int)Settings[DriverSettings.NumberOfTestWorkers]
                : LoadedTest.Properties.ContainsKey(PropertyNames.LevelOfParallelization)
                    ? (int)LoadedTest.Properties.Get(PropertyNames.LevelOfParallelization)
                    : Math.Max(Environment.ProcessorCount, 2);
        }
#endif

        #endregion
    }
}
