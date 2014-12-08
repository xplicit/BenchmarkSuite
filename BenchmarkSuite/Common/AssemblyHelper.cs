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
using System.Reflection;


namespace BenchmarkSuite.Common
{
	/// <summary>
	/// AssemblyHelper provides static methods for working 
	/// with assemblies.
	/// </summary>
	public class AssemblyHelper
	{
		#region GetAssemblyPath

		#if !SILVERLIGHT && !PORTABLE
		/// <summary>
		/// Gets the path from which the assembly defining a type was loaded.
		/// </summary>
		/// <param name="type">The Type.</param>
		/// <returns>The path.</returns>
		public static string GetAssemblyPath(Type type)
		{
			return GetAssemblyPath(type.Assembly);
		}

		/// <summary>
		/// Gets the path from which an assembly was loaded.
		/// </summary>
		/// <param name="assembly">The assembly.</param>
		/// <returns>The path.</returns>
		public static string GetAssemblyPath(Assembly assembly)
		{
		#if NETCF
		return assembly.ManifestModule.FullyQualifiedName;
		#else
			string codeBase = assembly.CodeBase;

			if (IsFileUri(codeBase))
				return GetAssemblyPathFromCodeBase(codeBase);

			return assembly.Location;
		#endif
		}
		#endif

		#endregion

		#region GetDirectoryName

		#if !SILVERLIGHT && !PORTABLE
		/// <summary>
		/// Gets the path to the directory from which an assembly was loaded.
		/// </summary>
		/// <param name="assembly">The assembly.</param>
		/// <returns>The path.</returns>
		public static string GetDirectoryName( Assembly assembly )
		{
			return System.IO.Path.GetDirectoryName(GetAssemblyPath(assembly));
		}
		#endif

		#endregion

		#region GetAssemblyName

		/// <summary>
		/// Gets the AssemblyName of an assembly.
		/// </summary>
		/// <param name="assembly">The assembly</param>
		/// <returns>An AssemblyName</returns>
		public static AssemblyName GetAssemblyName(Assembly assembly)
		{
			#if SILVERLIGHT || PORTABLE
			return new AssemblyName(assembly.FullName);
			#else
			return assembly.GetName();
			#endif
		}

		#endregion

		#region Helper Methods

		#if !NETCF && !PORTABLE
		private static bool IsFileUri(string uri)
		{
			return uri.ToLower().StartsWith(Uri.UriSchemeFile);
		}

		/// <summary>
		/// Gets the assembly path from code base.
		/// </summary>
		/// <remarks>Public for testing purposes</remarks>
		/// <param name="codeBase">The code base.</param>
		/// <returns></returns>
		public static string GetAssemblyPathFromCodeBase(string codeBase)
		{
			// Skip over the file:// part
			int start = Uri.UriSchemeFile.Length + Uri.SchemeDelimiter.Length;

			if (codeBase[start] == '/') // third slash means a local path
			{
				// Handle Windows Drive specifications
				if (codeBase[start + 2] == ':')
					++start;
				// else leave the last slash so path is absolute  
			}
			else // It's either a Windows Drive spec or a share
			{
				if (codeBase[start + 1] != ':')
					start -= 2; // Back up to include two slashes
			}

			return codeBase.Substring(start);
		}
		#endif

		#endregion
	}
}

