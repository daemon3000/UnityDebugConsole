
namespace Luminosity.Console
{
	public class LogMessage
	{
		private LogLevel m_logLevel;
		private string m_time;
		private string m_message;
		private string m_stackTrace;

		public LogLevel LogLevel
		{
			get { return m_logLevel; }
		}

		public string Time
		{
			get { return m_time; }
		}

		public string Message
		{
			get { return m_message; }
		}

		public string StackTrace
		{
			get { return m_stackTrace; }
		}

		public LogMessage(LogLevel logLevel, string message) :
			this(logLevel, string.Empty, message, string.Empty)
		{
		}

		public LogMessage(LogLevel logLevel, string time, string message) :
			this(logLevel, time, message, string.Empty)
		{
		}

		public LogMessage(LogLevel logLevel, string time, string message, string stackTrace)
		{
			m_logLevel = logLevel;
			m_time = time;
			m_message = message;
			m_stackTrace = stackTrace;
		}
	}
}