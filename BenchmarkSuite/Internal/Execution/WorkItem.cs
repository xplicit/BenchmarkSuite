﻿// ***********************************************************************
// Copyright (c) 2012 Charlie Poole
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
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework.Interfaces;
using log4net;
using BenchmarkSuite.Common;
using System.Diagnostics;

namespace NUnit.Framework.Internal.Execution
{
    /// <summary>
    /// A WorkItem may be an individual test case, a fixture or
    /// a higher level grouping of tests. All WorkItems inherit
    /// from the abstract WorkItem class, which uses the template
    /// pattern to allow derived classes to perform work in
    /// whatever way is needed.
    /// 
    /// A WorkItem is created with a particular TestExecutionContext
    /// and is responsible for re-establishing that context in the
    /// current thread before it begins or resumes execution.
    /// </summary>
    public abstract class WorkItem
    {
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // The current state of the WorkItem
        private WorkItemState _state;

        // The test this WorkItem represents
        private Test _test;

        // The execution context used by this work item
        private TestExecutionContext _context;

        private List<ITestAction> _actions = new List<ITestAction>();

        #region Static Factory Method

        /// <summary>
        /// Creates a work item.
        /// </summary>
        /// <param name="test">The test for which this WorkItem is being created.</param>
        /// <param name="filter">The filter to be used in selecting any child Tests.</param>
        /// <returns></returns>
        static public WorkItem CreateWorkItem(ITest test, ITestFilter filter)
        {
            TestSuite suite = test as TestSuite;
            if (suite != null)
                return new CompositeWorkItem(suite, filter);
            else
                return new SimpleWorkItem((TestMethod)test);
        }

        #endregion

        #region Construction and Initialization

        /// <summary>
        /// Construct a WorkItem for a particular test.
        /// </summary>
        /// <param name="test">The test that the WorkItem will run</param>
        public WorkItem(Test test)
        {
            _test = test;
            Result = test.MakeTestResult();
            _state = WorkItemState.Ready;
        }

        /// <summary>
        /// Initialize the TestExecutionContext. This must be done
        /// before executing the WorkItem.
        /// </summary>
        /// <remarks>
        /// Originally, the context was provided in the constructor
        /// but delaying initialization of the context until the item
        /// is about to be dispatched allows changes in the parent
        /// context during OneTimeSetUp to be reflected in the child.
        /// </remarks>
        /// <param name="context">The TestExecutionContext to use</param>
        public void InitializeContext(TestExecutionContext context)
        {
            Guard.OperationValid(_context == null, "The context has already been initialized");

            _context = context;

            if (Test is TestAssembly)
                _actions.AddRange(ActionsHelper.GetActionsFromAttributeProvider(((TestAssembly)Test).Assembly));
            else if (Test is ParameterizedMethodSuite)
                _actions.AddRange(ActionsHelper.GetActionsFromAttributeProvider(((ParameterizedMethodSuite)Test).Method));
            else if (Test.FixtureType != null)
                _actions.AddRange(ActionsHelper.GetActionsFromTypesAttributes(Test.FixtureType));
        }

        #endregion

        #region Properties and Events

        /// <summary>
        /// Event triggered when the item is complete
        /// </summary>
        public event EventHandler Completed;

        /// <summary>
        /// Gets the current state of the WorkItem
        /// </summary>
        public WorkItemState State
        {
            get { return _state; }
        }

        /// <summary>
        /// The test being executed by the work item
        /// </summary>
        public Test Test
        {
            get { return _test; }
        }

        /// <summary>
        /// The execution context
        /// </summary>
        public TestExecutionContext Context
        {
            get { return _context; }
        }

        /// <summary>
        /// The test actions to be performed before and after this test
        /// </summary>
        public List<ITestAction> Actions
        {
            get { return _actions;  }
        }

#if PARALLEL
        /// <summary>
        /// Indicates whether this WorkItem may be run in parallel
        /// </summary>
        public bool IsParallelizable
        {
            get 
            {
                ParallelScope scope = ParallelScope.None;

                if (Test.Properties.ContainsKey(PropertyNames.ParallelScope))
                {
                    scope = (ParallelScope)Test.Properties.Get(PropertyNames.ParallelScope);

                    if ((scope & ParallelScope.Self) != 0)
                        return true;
                }
                else
                {
                    scope = Context.ParallelScope;

                    if ((scope & ParallelScope.Children) != 0)
                        return true;
                }

                if (Test is TestFixture && (scope & ParallelScope.Fixtures) != 0)
                    return true;

                // Special handling for the top level TestAssembly.
                // If it has any scope specified other than None,
                // we will use the parallel queue. This heuristic
                // is intended to minimize creation of unneeded
                // queues and workers, since the assembly and
                // namespace level tests can easily run in any queue.
                if (Test is TestAssembly && scope != ParallelScope.None)
                    return true;

                return false;
            }
        }
#endif

        /// <summary>
        /// The test result
        /// </summary>
        public TestResult Result { get; protected set; }

#if !SILVERLIGHT && !NETCF && !PORTABLE
        internal ApartmentState TargetApartment
        {
            get 
            {
                return Test.Properties.ContainsKey(PropertyNames.ApartmentState)
                    ? (ApartmentState)_test.Properties.Get(PropertyNames.ApartmentState)
                    : ApartmentState.Unknown;
            }
        }
#endif

        #endregion

        #region Public Methods

        /// <summary>
        /// Execute the current work item, including any
        /// child work items.
        /// </summary>
        public virtual void Execute()
        {
            // Timeout set at a higher level
            int timeout = _context.TestCaseTimeout;

            // Timeout set on this test
            if (Test.Properties.ContainsKey(PropertyNames.Timeout))
                timeout = (int)Test.Properties.Get(PropertyNames.Timeout);
            
#if SILVERLIGHT || NETCF
            if (Test.RequiresThread || Test is TestMethod && timeout > 0)
                RunTestOnOwnThread(timeout);
            else
                RunTest();
#elif PORTABLE
            RunTest();
#else
            ApartmentState currentApartment = Thread.CurrentThread.GetApartmentState();

            if (Test.RequiresThread || Test is TestMethod && timeout > 0 || currentApartment != TargetApartment && TargetApartment != ApartmentState.Unknown)
                RunTestOnOwnThread(timeout, TargetApartment);
            else
                RunTest();
#endif
        }

#if SILVERLIGHT || NETCF
        private void RunTestOnOwnThread(int timeout)
        {
            string reason = Test.RequiresThread ? "has RequiresThreadAttribute." : "has Timeout value set.";
            log.Debug("Running test on own thread because it " + reason);

            Thread thread = new Thread(RunTest);

#if !NETCF
            thread.CurrentCulture = Context.CurrentCulture;
            thread.CurrentUICulture = Context.CurrentUICulture;
#endif

            thread.Start();

            if (!Test.IsAsynchronous || timeout > 0)
            {
                if (timeout <= 0)
                    timeout = Timeout.Infinite;

                // Previous code:
                // thread.Join(timeout);
                //
                // if (thread.IsAlive)
                // Was this here for a reason?
                // Is there some platform that needs it?

                if (!thread.Join(timeout))
                {
                    ThreadUtility.Kill(thread);

                    // NOTE: Without the use of Join, there is a race condition here.
                    // The thread sets the result to Cancelled and our code below sets
                    // it to Failure. In order for the result to be shown as a failure,
                    // we need to ensure that the following code executes after the
                    // thread has terminated. There is a risk here: the test code might
                    // refuse to terminate. However, it's more important to deal with
                    // the normal rather than a pathological case.
                    thread.Join();

                    Result.SetResult(ResultState.Failure,
                        string.Format("Test exceeded Timeout value of {0}ms", timeout));

                    WorkItemComplete();
                }
            }
        }
#endif


#if !SILVERLIGHT && !NETCF && !PORTABLE
        private void RunTestOnOwnThread(int timeout, ApartmentState apartment)
        {
            string reason = Test.RequiresThread
                ? "has RequiresThreadAttribute."
                : timeout > 0
                    ? "has Timeout value set."
                    : "requires a different apartment.";
            log.Debug("Running test on own thread because it " + reason);

            Thread thread = new Thread(new ThreadStart(RunTest));

            thread.SetApartmentState(apartment);
            thread.CurrentCulture = Context.CurrentCulture;
            thread.CurrentUICulture = Context.CurrentUICulture;

            thread.Start();

            if (!Test.IsAsynchronous || timeout > 0)
            {
                if (timeout <= 0)
                    timeout = Timeout.Infinite;

                thread.Join(timeout);

                if (thread.IsAlive)
                {
                    log.DebugFormat("Killing thread {0}, which exceeded timeout", thread.ManagedThreadId);
                    ThreadUtility.Kill(thread);

                    // NOTE: Without the use of Join, there is a race condition here.
                    // The thread sets the result to Cancelled and our code below sets
                    // it to Failure. In order for the result to be shown as a failure,
                    // we need to ensure that the following code executes after the
                    // thread has terminated. There is a risk here: the test code might
                    // refuse to terminate. However, it's more important to deal with
                    // the normal rather than a pathological case.
                    thread.Join();

                    log.DebugFormat("Changing result from {0} to Timeout Failure", Result.ResultState);

                    Result.SetResult(ResultState.Failure,
                        string.Format("Test exceeded Timeout value of {0}ms", timeout));

                    WorkItemComplete();
                }
            }
        }
#endif

        private void RunTest()
        {
            _context.CurrentTest = this.Test;
            _context.CurrentResult = this.Result;
            _context.Listener.TestStarted(this.Test);
            _context.StartTime = DateTime.UtcNow;
            _context.StartTicks = Stopwatch.GetTimestamp();
            _context.EstablishExecutionEnvironment();

            _state = WorkItemState.Running;
#if PORTABLE
            PerformWork();
#else
            try
            {
                PerformWork();
            }
            catch (ThreadAbortException)
            {
                //Result.SetResult(ResultState.Cancelled);
            }
#endif
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Method that performs actually performs the work. It should
        /// set the State to WorkItemState.Complete when done.
        /// </summary>
        protected abstract void PerformWork();

        /// <summary>
        /// Method called by the derived class when all work is complete
        /// </summary>
        protected void WorkItemComplete()
        {
            _state = WorkItemState.Complete;

            Result.StartTime = Context.StartTime;
            Result.EndTime = DateTime.UtcNow;
            
            long tickCount = Stopwatch.GetTimestamp() - Context.StartTicks;
            double seconds = (double)tickCount / Stopwatch.Frequency;
            Result.Duration = TimeSpan.FromSeconds(seconds);

            // We add in the assert count from the context. If
            // this item is for a test case, we are adding the
            // test assert count to zero. If it's a fixture, we
            // are adding in any asserts that were run in the
            // fixture setup or teardown. Each context only
            // counts the asserts taking place in that context.
            // Each result accumulates the count from child
            // results along with it's own asserts.
            Result.AssertCount += Context.AssertCount;

            _context.Listener.TestFinished(Result);

            if (Completed != null)
                Completed(this, EventArgs.Empty);
        }

        #endregion
    }
}
