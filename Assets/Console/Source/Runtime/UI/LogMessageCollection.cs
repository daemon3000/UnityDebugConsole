using UnityEngine.Events;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Luminosity.Console.UI
{
	public class LogMessageCollection
	{
		private const int NUMBER_OF_MESSAGES_TO_RECYCLE = 100;

		private List<LogMessage> m_messages;
		private List<LogMessage> m_filteredMessages;
		private LogLevel? m_filter;
		private int m_numberOfDebugMessages;
		private int m_numberOfWarningMessages;
		private int m_numberOfErrorMessages;

		public event UnityAction FilterChanged;
		public event UnityAction ItemAdded;
		public event UnityAction AllItemsRemoved;

		public bool StackLogMessages { get; set; }

		public LogMessage this[int index]
		{
			get { return m_filteredMessages[index]; }
		}
		
		public int Count
		{
			get { return m_filteredMessages.Count; }
		}

		public int NumberOfDebugMessages
		{
			get { return m_numberOfDebugMessages; }
		}

		public int NumberOfWarningMessages
		{
			get { return m_numberOfWarningMessages; }
		}

		public int NumberOfErrorMessages
		{
			get { return m_numberOfErrorMessages; }
		}

		public LogMessageCollection()
		{
			m_messages = new List<LogMessage>();
			m_filteredMessages = new List<LogMessage>();
			m_numberOfDebugMessages = 0;
			m_numberOfWarningMessages = 0;
			m_numberOfErrorMessages = 0;
			m_filter = null;
		}

		public void Add(LogMessage message)
		{
			if(StackLogMessages)
			{
				for(int i = Mathf.Max(m_messages.Count - NUMBER_OF_MESSAGES_TO_RECYCLE, 0); i < m_messages.Count; i++)
				{
					if(m_messages[i].IsEqual(message))
					{
						m_messages[i].IncrementLogCount();
						return;
					}
				}
			}

			if(message.LogLevel == LogLevel.Debug)
				m_numberOfDebugMessages++;

			if(message.LogLevel == LogLevel.Warning)
				m_numberOfWarningMessages++;

			if(message.LogLevel == LogLevel.Error)
				m_numberOfErrorMessages++;

			m_messages.Add(message);
			if(IsLogMessageCoveredByFilter(message))
			{
				m_filteredMessages.Add(message);
			}

			if(ItemAdded != null)
			{
				ItemAdded();
			}
		}

		public void RemoveAll()
		{
			m_messages.Clear();
			m_filteredMessages.Clear();
			if(AllItemsRemoved != null)
			{
				AllItemsRemoved();
			}
		}

		public void SetFilter(LogLevel? filter)
		{
			if(filter != m_filter)
			{
				m_filter = filter;
				m_filteredMessages.Clear();

				for(int i = 0; i < m_messages.Count; i++)
				{
					if(IsLogMessageCoveredByFilter(m_messages[i]))
					{
						m_filteredMessages.Add(m_messages[i]);
					}
				}

				if(FilterChanged != null)
				{
					FilterChanged();
				}
			}
		}

		private bool IsLogMessageCoveredByFilter(LogMessage message)
		{
			if(m_filter.HasValue)
			{
				switch(message.LogLevel)
				{
				case LogLevel.Debug:
					return (((int)m_filter.Value & (int)LogLevel.Debug) != 0);
				case LogLevel.Warning:
					return (((int)m_filter.Value & (int)LogLevel.Warning) != 0);
				case LogLevel.Error:
					return (((int)m_filter.Value & (int)LogLevel.Error) != 0);
				}
			}

			return false;
		}
	}
}