using System;
using UnityEngine;
using UnityEngine.Events;
using Luminosity.Console.UI;

namespace Luminosity.Console
{
	public class DebugConsole : MonoBehaviour
	{
		public const string VERSION = "0.1";

		[SerializeField]
		private DebugConsoleUI m_consoleWindow;
		[SerializeField]
		private FeedbackUI m_feedbackUI;
		[SerializeField]
		private HelpUI m_helpUI;
		[SerializeField]
		private DebugLevel m_debugLevel;
		[SerializeField]
		private bool m_createEventSystem;
		[SerializeField]
		private bool m_dontDestroyOnLoad;

		private bool m_isLocked;
		private bool m_isOpen;
		private IPauseHandler m_pauseHandler;
		private ICommandParser m_commandParser;
		private static DebugConsole m_instance;

		private void Awake()
		{
			if(m_instance == null)
			{
				if(m_createEventSystem)
				{
					GameObject prefab = Resources.Load<GameObject>("DebugConsole/event_system");
					GameObject clone = GameObject.Instantiate<GameObject>(prefab);
					clone.name = prefab.name;
				}

				if(m_dontDestroyOnLoad)
				{
					GameObject.DontDestroyOnLoad(gameObject);
				}

				m_isLocked = false;
				m_isOpen = false;
				m_instance = this;
				m_consoleWindow.CommandReceived += OnCommandReceived;

				Application.logMessageReceived += OnLogMessageReceived;
			}
			else
			{
				Debug.LogError("You can't have more than one Debug Console in the game!");
				GameObject.Destroy(gameObject);
			}
		}

		private void OnDestroy()
		{
			if(m_instance == this)
			{
				Opened = null;
				Closed = null;
				m_instance = null;
				Application.logMessageReceived -= OnLogMessageReceived;
			}
		}

		private void OnCommandReceived(string command)
		{
			if(m_commandParser != null)
			{
				if(command == "help")
				{
					m_helpUI.Open(m_commandParser);
				}
				else if(command == "clear")
				{
					m_consoleWindow.ClearMessageLog();
				}
				else if(command == "feedback")
				{
					m_feedbackUI.Open();
				}
				else if(command == "reset")
				{
					m_consoleWindow.ResetLayout();
					m_feedbackUI.ResetLayout();
					m_helpUI.ResetLayout();
				}
				else if(!string.IsNullOrEmpty(command))
				{
					m_commandParser.Run(command);
				}
			}
			else
			{
				OnLogMessageReceived("Command processor is not set!", null, LogType.Error);
			}
		}

		private void OnLogMessageReceived(string message, string stackTrace, LogType logType)
		{
			if(IsCoveredByDebugLevel(logType))
			{
				OnAddLogMessage(message, stackTrace, logType);
			}
		}

		private bool IsCoveredByDebugLevel(LogType logType)
		{
			if(m_debugLevel == DebugLevel.Low)
			{
				return (logType != LogType.Log && logType != LogType.Warning);
			}
			else if(m_debugLevel == DebugLevel.Normal)
			{
				return (logType != LogType.Log);
			}

			return true;
		}

		private bool IsCoveredByDebugLevel(LogLevel logLevel)
		{
			if(m_debugLevel == DebugLevel.Low)
			{
				return logLevel == LogLevel.Error;
			}
			else if(m_debugLevel == DebugLevel.Normal)
			{
				return (logLevel != LogLevel.Debug);
			}

			return true;
		}

		private void OnAddLogMessage(LogMessage message)
		{
			m_consoleWindow.AddMessage(message);
		}

		private void OnAddLogMessage(string message, string stackTrace, LogType logType)
		{
			OnAddLogMessage(new LogMessage(LogTypeToLogLevel(logType), message, stackTrace));
		}

		#region [Public]
		public static event UnityAction Opened;
		public static event UnityAction Closed;

		public static bool Exists
		{
			get { return m_instance != null; }
		}

		public static bool IsLocked
		{
			get
			{
				if(m_instance != null)
					return m_instance.m_isLocked;

				return false;
			}
			private set
			{
				if(m_instance != null)
					m_instance.m_isLocked = value;
			}
		}

		public static bool IsOpen
		{
			get
			{
				if(m_instance != null)
					return m_instance.m_isOpen;

				return false;
			}
			private set
			{
				if(m_instance != null)
					m_instance.m_isOpen = value;
			}
		}

		public static ICommandParser CommandParser
		{
			get
			{
				if(m_instance != null)
					return m_instance.m_commandParser;

				return null;
			}
			set
			{
				if(m_instance != null)
					m_instance.m_commandParser = value;
			}
		}

		public static IPauseHandler PauseHandler
		{
			get
			{
				if(m_instance != null)
					return m_instance.m_pauseHandler;

				return null;
			}
			set
			{
				if(m_instance != null)
					m_instance.m_pauseHandler = value;
			}
		}

		public static void Lock(bool hide = false)
		{
			if(m_instance != null && IsOpen && !IsLocked)
			{
				m_instance.m_consoleWindow.Lock(hide);
				IsLocked = true;
			}
		}

		public static void Unlock()
		{
			if(m_instance != null && IsOpen && IsLocked)
			{
				m_instance.m_consoleWindow.Unlock();
				IsLocked = false;
			}
		}

		public static void Open()
		{
			if(m_instance != null && !IsLocked && !IsOpen)
			{
				m_instance.m_consoleWindow.Open();
				IsOpen = true;

				if(Opened != null)
					Opened();

				if(m_instance.m_pauseHandler != null)
					m_instance.m_pauseHandler.OnDebugConsolePause();
			}
		}

		public static void Close()
		{
			if(m_instance != null && !IsLocked && IsOpen)
			{
				m_instance.m_consoleWindow.Close();
				IsOpen = false;

				if(Closed != null)
					Closed();

				if(m_instance.m_pauseHandler != null)
					m_instance.m_pauseHandler.OnDebugConsoleUnpause();
			}
		}

		public static void Log(LogLevel logLevel, string message, string stackTrace = null)
		{
			if(m_instance != null && m_instance.IsCoveredByDebugLevel(logLevel))
			{
				m_instance.OnAddLogMessage(new LogMessage(logLevel, message, stackTrace));
			}
		}

		public static LogLevel LogTypeToLogLevel(LogType logType)
		{
			switch(logType)
			{
			case LogType.Log:
				return LogLevel.Debug;
			case LogType.Warning:
				return LogLevel.Warning;
			case LogType.Error:
			case LogType.Assert:
			case LogType.Exception:
				return LogLevel.Error;
			default:
				return LogLevel.Debug;
			}
		}

		public static string GetDateString()
		{
			return DateTime.Now.ToString("dd'-'MM'-'yyyy");
		}

		public static string GetTimeString()
		{
			return DateTime.Now.ToString("HH':'mm':'ss");
		}
		#endregion
	}
}