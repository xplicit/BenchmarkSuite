#region License
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
using BenchmarkSuite.Framework.Interfaces;
using BenchmarkSuite.Logging;


#endregion

using System;
using System.Reflection;

namespace BenchmarkSuite.Framework.Logging
{
	/// <summary>
	/// This class is used by client applications to request logger instances.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This class has static methods that are used by a client to request
	/// a logger instance. The <see cref="M:GetLogger(string)"/> method is 
	/// used to retrieve a logger.
	/// </para>
	/// <para>
	/// See the <see cref="ILog"/> interface for more details.
	/// </para>
	/// </remarks>
	/// <example>Simple example of logging messages
	/// <code lang="C#">
	/// ILog log = LogManager.GetLogger("application-log");
	/// 
	/// log.Info("Application Start");
	/// log.Debug("This is a debug message");
	/// 
	/// if (log.IsDebugEnabled)
	/// {
	///		log.Debug("This is another debug message");
	/// }
	/// </code>
	/// </example>
	/// <threadsafety static="true" instance="true" />
	/// <seealso cref="ILog"/>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public sealed class LogManager
	{
        #region Private Static Fields
        private static ILogFactory logFactory = new EmptyLogFactory(); 

        #endregion Private Static Fields


		#region Private Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="LogManager" /> class. 
		/// </summary>
		/// <remarks>
		/// Uses a private access modifier to prevent instantiation of this class.
		/// </remarks>
		private LogManager()
		{
		}

		#endregion Private Instance Constructors

		#region Type Specific Manager Methods

        /// <summary>
        /// Gets or sets the log factory.
        /// </summary>
        /// <value>The log factory.</value>
        public static ILogFactory LogFactory
        {
            get { return logFactory; }
            set { logFactory = value; }
        }

		/// <overloads>Get or create a logger.</overloads>
		/// <summary>
		/// Retrieves or creates a named logger.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Retrieves a logger named as the <paramref name="name"/>
		/// parameter. If the named logger already exists, then the
		/// existing instance will be returned. Otherwise, a new instance is
		/// created.
		/// </para>
		/// <para>By default, loggers do not have a set level but inherit
		/// it from the hierarchy. This is one of the central features of
		/// log4net.
		/// </para>
		/// </remarks>
		/// <param name="name">The name of the logger to retrieve.</param>
		/// <returns>The logger with the name specified.</returns>
		public static ILog GetLogger(string name)
		{
            return logFactory.GetLogger(name);
		}

		/// <summary>
		/// Shorthand for <see cref="M:LogManager.GetLogger(string)"/>.
		/// </summary>
		/// <remarks>
		/// Get the logger for the fully qualified name of the type specified.
		/// </remarks>
		/// <param name="type">The full name of <paramref name="type"/> will be used as the name of the logger to retrieve.</param>
		/// <returns>The logger with the name specified.</returns>
		public static ILog GetLogger(Type type) 
		{
            return logFactory.GetLogger(type);
		}
		#endregion Type Specific Manager Methods

	}
}
