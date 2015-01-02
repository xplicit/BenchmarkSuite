using System;
using BenchmarkSuite.Framework.Api;
using BenchmarkSuite.Framework.Interfaces;
using BenchmarkSuite.ConsoleRunner.Options;
using System.Collections.Generic;
using BenchmarkSuite.Framework.Internal;
using System.IO;
using System.Text;
using System.Xml;
using BenchmarkSuite.Framework;
using Mono.Options;
using System.Reflection;
using BenchmarkSuite.Common;

namespace BenchmarkSuite.ConsoleRunner
{
	class MainClass
	{
        private const int OK = 0;
        private const int InvalidArguments = -1;
        private const int FileNotFound = -2;


		public static int Main (string[] args)
		{
            ConsoleOptions options = new ConsoleOptions();

            try
            {
                options.Parse(args);
            }
            catch (OptionException ex)
            {
                WriteHeader();
                Console.WriteLine(string.Format(ex.Message, ex.OptionName));
                return InvalidArguments;
            }

            if (options.PauseBeforeRun)
            {
                Console.WriteLine("Press any key to continue . . .");
                Console.ReadKey(true);
            }

            if (!options.NoHeader)
                WriteHeader();

            if (options.ShowHelp)
            {
                WriteHelpText(options);
                return OK;
            }

            if (!options.Validate())
            {
                foreach (string message in options.ErrorMessages)
                    Console.Error.WriteLine(message);

                return InvalidArguments;
            }

            if (options.InputFiles.Count == 0)
            {
                Console.Error.WriteLine("Error: no inputs specified");
                return OK;
            }

            foreach (string file in options.InputFiles)
            {
                string ext = Path.GetExtension(file);
                if (ext != ".dll" && ext != ".exe" && ext != ".nbench")
                {
                    Console.WriteLine("File type not known: {0}", file);
                    return InvalidArguments;
                }
            }

			var settings = new Dictionary<string, object>();

            if (options.ProcessModel != null)//ProcessModel.Default)
                settings[PackageSettings.ProcessModel] = options.ProcessModel;

            if (options.DomainUsage != null)
                settings[PackageSettings.DomainUsage] = options.DomainUsage;

            if (options.Framework != null)
                settings[PackageSettings.RuntimeFramework] = options.Framework;

            if (options.RunAsX86)
                settings[PackageSettings.RunAsX86] = true;

            if (options.DisposeRunners)
                settings[PackageSettings.DisposeRunners] = true;

            if (options.DefaultTimeout >= 0)
                settings[PackageSettings.DefaultTimeout] = options.DefaultTimeout;

            if (options.InternalTraceLevel != null)
                settings[PackageSettings.InternalTraceLevel] = options.InternalTraceLevel;

            if (options.ActiveConfig != null)
                settings[PackageSettings.ActiveConfig] = options.ActiveConfig;

            if (options.WorkDirectory != null)
                settings[PackageSettings.WorkDirectory] = options.WorkDirectory;

            if (options.StopOnError)
                settings[PackageSettings.StopOnError] = true;

            if (options.NumWorkers > 0)
                settings[PackageSettings.NumberOfTestWorkers] = options.NumWorkers;

            if (options.RandomSeed > 0)
                settings[PackageSettings.RandomSeed] = options.RandomSeed;

            if (options.Verbose)
                settings["Verbose"] = true;

            TestFilter filter = CreateTestFilter(options);

            List<XmlNode> nodes = new List<XmlNode>();

            foreach (string assembly in options.InputFiles)
            {
                DefaultTestAssemblyBuilder builder = new DefaultTestAssemblyBuilder ();
                NUnitTestAssemblyRunner runner = new NUnitTestAssemblyRunner (builder);

                ITest test = runner.Load(assembly, settings);
                ITestResult result = runner.Run(TestListener.NULL, filter);
                nodes.Add(result.ToXml(true));
            }

            XmlNode resultNode = ResultHelper.Aggregate("bench-suite","","",nodes);

//            TestEngineResult result = _realRunner.Run(listener, filter).Aggregate("test-run", TestPackage.Name, TestPackage.FullName);

            resultNode.InsertEnvironmentElement();

//			ITest test = builder.Build ("BenchmarkSuite.Tests.dll", settings);

            string outputPath = "BenchmarkResult.xml";

            using (StreamWriter writer = new StreamWriter(outputPath, false, Encoding.UTF8))
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings(){Indent=true}))
                {
                    xmlWriter.WriteStartDocument(false);
                    resultNode.WriteTo(xmlWriter);
                }
            }

            return 0;
		}

        public static TestFilter CreateTestFilter(ConsoleOptions options)
        {
            TestFilterBuilder builder = new TestFilterBuilder();
            foreach (string testName in options.TestList)
                builder.Tests.Add(testName);

            // TODO: Support multiple include / exclude options

            if (options.Include != null)
                builder.Include.Add(options.Include);

            if (options.Exclude != null)
                builder.Exclude.Add(options.Exclude);

            return builder.GetFilter();
        }


        private static void WriteHeader()
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            string versionText = executingAssembly.GetName().Version.ToString(3);

            string programName = "BenchmarkSuite Console Runner";
            string copyrightText = "Copyright (C) 2014 Sergey Zhukov.\r\nAll Rights Reserved.";
            string configText = String.Empty;

            object[] attrs = executingAssembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            if (attrs.Length > 0)
                programName = ((AssemblyTitleAttribute)attrs[0]).Title;

            attrs = executingAssembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            if ( attrs.Length > 0 )
                copyrightText = ((AssemblyCopyrightAttribute)attrs[0]).Copyright;

            attrs = executingAssembly.GetCustomAttributes(typeof(AssemblyConfigurationAttribute), false);
            if ( attrs.Length > 0 )
            {
                string configuration = ( ( AssemblyConfigurationAttribute )attrs[0] ).Configuration;
                if ( !String.IsNullOrEmpty( configuration ) )
                {
                    configText = string.Format( "({0})", ( ( AssemblyConfigurationAttribute )attrs[0] ).Configuration );
                }
            }

            Console.WriteLine("{0} {1} {2}", programName, versionText, configText);
            Console.WriteLine(copyrightText);
            Console.WriteLine();
            Console.WriteLine("Runtime Environment");
            Console.WriteLine("   OS Version: {0}", Environment.OSVersion.ToString());
            Console.WriteLine("  CLR Version: {0}", Environment.Version.ToString());
            Console.WriteLine();
        }

        private static void WriteHelpText(ConsoleOptions options)
        {
            Console.WriteLine();
            Console.WriteLine("bench-console [inputfiles] [options]");
            Console.WriteLine();
            Console.WriteLine("Runs a set of benchmarks from the console.");
            Console.WriteLine();
            Console.WriteLine("InputFiles:");
            Console.WriteLine("      One or more assemblies or benchmark projects of a recognized type.");
            Console.WriteLine();
            Console.WriteLine("Options:");

            options.WriteOptionDescriptions(Console.Out);
            Console.WriteLine();
            Console.WriteLine("Description:");

            Console.WriteLine("      By default, this command runs the benchmarks contained in the");
            Console.WriteLine("      assemblies and projects specified. If the --explore option");
            Console.WriteLine("      is used, no benchmarks are executed but a description of the benchmarks");
            Console.WriteLine("      is saved in the specified or default format.");
            Console.WriteLine();
            Console.WriteLine("      Several options that specify processing of XML output take");
            Console.WriteLine("      an output specification as a value. A SPEC may take one of");
            Console.WriteLine("      the following forms:");
            Console.WriteLine("          --OPTION:filename");
            Console.WriteLine("          --OPTION:filename;format=formatname");
            Console.WriteLine("          --OPTION:filename;transform=xsltfile");
            Console.WriteLine();
            Console.WriteLine("      The --explore option may use any of the following formats:");
            Console.WriteLine("          bench - the native XML format of Benchmark Suite");
            Console.WriteLine("          cases  - a text file listing the full names of all benchmarks.");
            Console.WriteLine("      If --explore is used without any specification following, a list of");
            Console.WriteLine("      benchmarks is output to the console.");
            Console.WriteLine();
            Console.WriteLine("      If none of the options {--explore, --noxml} is used,");
            Console.WriteLine("      Benchmark Suite saves the results to BenchmarkResult.xml in Benchmark Suite format");
            Console.WriteLine();
            Console.WriteLine("      Any transforms provided must handle input in the native Benchmark Suite format.");
            Console.WriteLine();
            //Console.WriteLine("Options that take values may use an equal sign, a colon");
            //Console.WriteLine("or a space to separate the option from its value.");
            //Console.WriteLine();
        }

        public static void PrintBenchmarks(ITestResult result)
        {
            foreach (BenchmarkResult br in result.BenchmarkResults)
            {
                Console.WriteLine("Name={0}, Mean={1}, Min={2}, Max={3}, StdDev={4}, StdErr%={5}", br.Name, br.Mean, br.Min, br.Max, br.StdDev, br.StdErrPercents);
                foreach (Benchmark b in br.Benchmarks)
                {
                    Console.WriteLine("Name={0}, Elapsed={1}", b.Name, b.ElapsedTicks);
                }
            }

            if (result.HasChildren)
            {
                foreach (ITestResult child in result.Children)
                {
                    PrintBenchmarks(child);
                }
            }
        }
	}
}
