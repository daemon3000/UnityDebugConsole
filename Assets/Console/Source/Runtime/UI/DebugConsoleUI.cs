using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Luminosity.Console.Internal;

namespace Luminosity.Console.UI
{
	public class DebugConsoleUI : MonoBehaviour
	{
		[SerializeField]
		private FeedbackUI m_feedbackUI;

		[SerializeField]
		private LogScrollView m_logScrollView;

		[SerializeField]
		private Canvas m_canvas;

		[SerializeField]
		private RectTransform m_panel;

		[SerializeField]
		private Text m_stackTraceField;

		[SerializeField]
		private RectTransform m_stackTracePanel;
		
		[SerializeField]
		private InputField m_commandField;

		[SerializeField]
		private Button m_sendErrorButton;

		[SerializeField]
		private Vector2 m_minWindowSize;

		[SerializeField]
		private Vector2 m_padding;

		[SerializeField]
		private int m_minStackTraceFieldHeight;

		[SerializeField]
		private int m_minStackTraceFieldTopPadding;

		[SerializeField]
		private bool m_stackLogMessages;

		[SerializeField]
		private MessageFilter[] m_filters;

		private LogMessageCollection m_messages;
		private CanvasGroup m_canvasGroup;
		private LayoutElement m_stackTraceLayout;
		private RectTransform m_stackTranceParent;
		private int? m_selectedMessage;
		private float m_defaultStackTraceHeight;
		private bool m_isOpen;

		public event UnityAction<string> CommandReceived;

		private void Awake()
		{
			m_stackTraceLayout = m_stackTracePanel.GetComponent<LayoutElement>();
			m_stackTranceParent = m_stackTracePanel.parent as RectTransform;
			m_canvasGroup = m_panel.GetComponent<CanvasGroup>();
			m_messages = new LogMessageCollection
			{
				StackLogMessages = m_stackLogMessages
			};

			foreach(var filter in m_filters)
			{
				filter.Changed += OnMessageFilterChanged;
			}

			m_sendErrorButton.gameObject.SetActive(false);
			m_defaultStackTraceHeight = m_stackTraceLayout.preferredHeight;

			m_logScrollView.Messages = m_messages;
			m_logScrollView.EntrySelected += OnLogMessageSelected;

			m_isOpen = false;
			LoadLayoutChanges();
		}

		public void Lock(bool hide)
		{
			m_canvasGroup.interactable = false;
			if(hide)
			{
				m_panel.gameObject.SetActive(false);
			}
		}

		public void Unlock()
		{
			m_canvasGroup.interactable = true;
			if(!m_panel.gameObject.activeSelf)
			{
				m_panel.gameObject.SetActive(true);
			}
		}

		public void Open()
		{
			m_panel.gameObject.SetActive(true);
			m_logScrollView.OnShown();
			m_isOpen = true;
		}

		public void Close()
		{
			m_commandField.text = string.Empty;
			m_isOpen = false;
			m_panel.gameObject.SetActive(false);
			m_logScrollView.OnHidden();
		}

		public void AddMessage(LogMessage logMessage)
		{
			m_messages.Add(logMessage);
			foreach(var filter in m_filters)
			{
				switch(filter.LogLevel)
				{
				case LogLevel.Debug:
					filter.MessageCount = m_messages.NumberOfDebugMessages;
					break;
				case LogLevel.Warning:
					filter.MessageCount = m_messages.NumberOfWarningMessages;
					break;
				case LogLevel.Error:
					filter.MessageCount = m_messages.NumberOfErrorMessages;
					break;
				}
			}
		}

		public void ClearMessageLog()
		{
			m_messages.RemoveAll();
			foreach(var filter in m_filters)
			{
				filter.MessageCount = 0;
			}

			m_stackTraceField.text = string.Empty;
		}

		public void RunCommand()
		{
			if(!m_commandField.IsDeselecting && !m_commandField.IsBeingDisabled)
			{
				string command = m_commandField.text;
				if(!string.IsNullOrEmpty(command))
				{
					if(CommandReceived != null)
						CommandReceived(command);
				}
				m_commandField.text = string.Empty;
			}
		}

		public void SendErrorReport()
		{
			if(m_selectedMessage.HasValue)
			{
				m_feedbackUI.SendErrorReport(m_messages[m_selectedMessage.Value]);
			}
		}

		public void OnBeginResizeWindowDrag(BaseEventData eventData)
		{
		}

		public void OnEndResizeWindowDrag(BaseEventData eventData)
		{
			SaveLayoutChanges();
		}

		public void OnResizeWindowDrag(BaseEventData eventData)
		{
			PointerEventData pointerData = eventData as PointerEventData;
			if(pointerData != null)
			{
				Vector2 size = (pointerData.position - (Vector2)m_panel.position) / m_canvas.transform.localScale.x;
				size.y = -size.y;

				if(size.x < m_minWindowSize.x)
					size.x = m_minWindowSize.x;
				if(size.y < m_minWindowSize.y)
					size.y = m_minWindowSize.y;
				if(size.x > Screen.width - m_panel.anchoredPosition.x - m_padding.x)
					size.x = Screen.width - m_panel.anchoredPosition.x - m_padding.x;
				if(size.y > Screen.height + m_panel.anchoredPosition.y - m_padding.y)
					size.y = Screen.height + m_panel.anchoredPosition.y - m_padding.y;

				m_panel.sizeDelta = size;
				m_stackTraceLayout.preferredHeight = Mathf.Clamp(m_stackTraceLayout.preferredHeight,
																  m_minStackTraceFieldHeight,
																  m_stackTranceParent.rect.size.y - m_minStackTraceFieldTopPadding);

				m_logScrollView.OnViewSizeChanged();
			}
		}

		public void OnBeginResizeStackTraceDrag(BaseEventData eventData)
		{
		}

		public void OnEndResizeStackTraceDrag(BaseEventData eventData)
		{
			SaveLayoutChanges();
		}

		public void OnResizeStackTraceDrag(BaseEventData eventData)
		{
			PointerEventData pointerData = eventData as PointerEventData;
			if(pointerData != null)
			{
				Vector2 parentSize = m_stackTranceParent.rect.size;
				m_stackTraceLayout.preferredHeight += pointerData.delta.y;
				m_stackTraceLayout.preferredHeight = Mathf.Clamp(m_stackTraceLayout.preferredHeight,
																  m_minStackTraceFieldHeight,
																  parentSize.y - m_minStackTraceFieldTopPadding);
				m_logScrollView.OnViewSizeChanged();
			}
		}

		public void OnBeginWindowDrag(BaseEventData eventData)
		{
		}

		public void OnEndWindowDrag(BaseEventData eventData)
		{
			SaveLayoutChanges();
		}

		public void OnWindowDrag(BaseEventData eventData)
		{
			PointerEventData pointerData = eventData as PointerEventData;
			if(pointerData != null)
			{
				Vector2 position = m_panel.anchoredPosition + pointerData.delta;
				position.x = Mathf.Clamp(position.x, m_padding.x, Screen.width - m_panel.sizeDelta.x - m_padding.x);
				position.y = Mathf.Clamp(position.y, -Screen.height + m_panel.sizeDelta.y + m_padding.y, -m_padding.y);

				m_panel.anchoredPosition = position;
			}
		}

		private void OnLogMessageSelected(LogMessageEntry entry)
		{
			m_sendErrorButton.gameObject.SetActive(entry.Message.LogLevel == LogLevel.Error);
			m_selectedMessage = entry.MessageID;
			m_stackTraceField.text = entry.Message.StackTrace;
		}

		private void OnLogMessageDeselected()
		{
			m_sendErrorButton.gameObject.SetActive(false);
			m_selectedMessage = null;
			m_stackTraceField.text = null;
		}

		private void OnMessageFilterChanged()
		{
			LogLevel? filter = null;
			foreach(var item in m_filters)
			{
				if(item.IsOn)
				{
					filter = filter.HasValue ? filter | item.LogLevel : item.LogLevel;
				}
			}
			
			m_messages.SetFilter(filter);
			OnLogMessageDeselected();
			SaveLayoutChanges();
		}

		private void SaveLayoutChanges()
		{
			DebugConsolePrefs.SetVector2("Console_Size", m_panel.sizeDelta);
			DebugConsolePrefs.SetVector2("Console_Position", m_panel.anchoredPosition);
			DebugConsolePrefs.SetFloat("Console_StackTraceHeight", m_stackTraceLayout.preferredHeight);
			for(int i = 0; i < m_filters.Length; i++)
			{
				DebugConsolePrefs.SetBool("Console_Filter_" + i, m_filters[i].IsOn);
			}
		}

		private void LoadLayoutChanges()
		{
			m_panel.sizeDelta = DebugConsolePrefs.GetVector2("Console_Size", m_minWindowSize);
			m_panel.anchoredPosition = DebugConsolePrefs.GetVector2("Console_Position", new Vector2(m_padding.x, -m_padding.y));
			m_stackTraceLayout.preferredHeight = DebugConsolePrefs.GetFloat("Console_StackTraceHeight", m_stackTraceLayout.preferredHeight);
			for(int i = 0; i < m_filters.Length; i++)
			{
				m_filters[i].Changed -= OnMessageFilterChanged;
				m_filters[i].IsOn = DebugConsolePrefs.GetBool("Console_Filter_" + i, true);
				m_filters[i].Changed += OnMessageFilterChanged;
			}

			if(m_isOpen)
				m_logScrollView.OnViewSizeChanged();
			
			OnMessageFilterChanged();
		}

		public void ResetLayout()
		{
			m_stackTraceLayout.preferredHeight = m_defaultStackTraceHeight;
			m_panel.anchoredPosition = new Vector2(m_padding.x, -m_padding.y);
			m_panel.sizeDelta = m_minWindowSize;
			for(int i = 0; i < m_filters.Length; i++)
			{
				m_filters[i].Changed -= OnMessageFilterChanged;
				m_filters[i].IsOn = true;
				m_filters[i].Changed += OnMessageFilterChanged;
			}

			if(m_isOpen)
				m_logScrollView.OnViewSizeChanged();

			OnMessageFilterChanged();
		}
	}
}