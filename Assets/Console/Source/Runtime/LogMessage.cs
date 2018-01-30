using UnityEngine.Events;

namespace Luminosity.Console
{
	public class LogMessage
	{
		private LogLevel m_logLevel;
		private string m_message;
		private string m_stackTrace;
		private int m_logCount;

		public event UnityAction LogCountChanged;

		public LogLevel LogLevel
		{
			get { return m_logLevel; }
		}

		public string Message
		{
			get { return m_message; }
		}

		public string StackTrace
		{
			get { return m_stackTrace; }
		}

		public int LogCount
		{
			get { return m_logCount; }
		}

		public LogMessage(LogLevel logLevel, string message) :
			this(logLevel,  message, string.Empty)
		{
		}

		public LogMessage(LogLevel logLevel, string message, string stackTrace)
		{
			m_logLevel = logLevel;
			m_message = message;
			m_stackTrace = stackTrace;
			m_logCount = 1;
		}

		public void IncrementLogCount()
		{
			m_logCount++;
			if(LogCountChanged != null)
			{
				LogCountChanged();
			}
		}

		public bool IsEqual(LogMessage message)
		{
			return m_logLevel == message.LogLevel &&
					m_message == message.Message &&
					m_stackTrace == message.StackTrace;
		}
	}
}