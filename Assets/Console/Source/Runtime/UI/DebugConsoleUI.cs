using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Luminosity.Debug.Internal;

namespace Luminosity.Debug.UI
{
	public class DebugConsoleUI : MonoBehaviour
	{
		[SerializeField]
		private FeedbackUI m_feedbackUI;

		[SerializeField]
		private Canvas m_canvas;

		[SerializeField]
		private RectTransform m_panel;

		[SerializeField]
		private GameObject m_logMessageTemplate;

		[SerializeField]
		private RectTransform m_logMessageRoot;

		[SerializeField]
		private Text m_stackTraceField;

		[SerializeField]
		private RectTransform m_stackTracePanel;
		
		[SerializeField]
		private InputField m_commandField;

		[SerializeField]
		private Button m_sendErrorButton;

		[SerializeField]
		private Mask m_logMessageMask;

		[SerializeField]
		private ScrollRect m_logMessageScrollView;

		[SerializeField]
		private Vector2 m_minWindowSize;

		[SerializeField]
		private Vector2 m_padding;

		[SerializeField]
		private int m_minStackTraceFieldHeight;

		[SerializeField]
		private int m_minStackTraceFieldTopPadding;

		[SerializeField]
		[Range(1, 10000)]
		private int m_maxLogMessages;

		[SerializeField]
		private MessageFilter[] m_filters;

		private LinkedList<LogMessageEntry> m_messages;
		private LogMessageEntry m_selectedMessage;
		private CanvasGroup m_canvasGroup;
		private LayoutElement m_stackTraceLayout;
		private RectTransform m_stackTranceParent;
		private float m_defaultStackTraceHeight;

		public event UnityAction<string> CommandReceived;

		private void Awake()
		{
			m_stackTraceLayout = m_stackTracePanel.GetComponent<LayoutElement>();
			m_stackTranceParent = m_stackTracePanel.parent as RectTransform;
			m_canvasGroup = m_panel.GetComponent<CanvasGroup>();
			m_messages = new LinkedList<LogMessageEntry>();
			m_selectedMessage = null;
			foreach(var filter in m_filters)
			{
				filter.Changed += HandleMessageFilterChanged;
			}

			m_sendErrorButton.gameObject.SetActive(false);
			m_defaultStackTraceHeight = m_stackTraceLayout.preferredHeight;
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
		}

		public void Close()
		{
			m_panel.gameObject.SetActive(false);
			m_commandField.text = string.Empty;
		}

		public void AddMessage(LogMessage logMessage)
		{
			if(logMessage != null)
			{
				if(m_messages.Count < m_maxLogMessages)
				{
					CreateMessageEntry(logMessage);
					m_logMessageMask.enabled = false;
					m_logMessageMask.enabled = true;    //	Force the viewport mask to refresh.
				}
				else
				{
					RecycleMessageEntry(logMessage);
				}

				Canvas.ForceUpdateCanvases();
				m_logMessageScrollView.verticalNormalizedPosition = 0.0f;
			}
		}

		public void ClearMessageLog()
		{
			m_messages.Clear();
			for(int i = 0; i < m_logMessageRoot.childCount; i++)
			{
				Transform child = m_logMessageRoot.GetChild(i);
				if(child.gameObject != m_logMessageTemplate)
				{
					GameObject.Destroy(child.gameObject);
				}
			}

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
			m_feedbackUI.SendErrorReport(m_selectedMessage.LogMessage);
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
				if(size.x > Screen.width - m_padding.x * 2)
					size.x = Screen.width - m_padding.x * 2;
				if(size.y > Screen.height - m_padding.y * 2)
					size.y = Screen.height - m_padding.y * 2;

				m_panel.sizeDelta = size;
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
			}
		}

		private void CreateMessageEntry(LogMessage logMessage)
		{
			GameObject entryGO = GameObject.Instantiate<GameObject>(m_logMessageTemplate);
			entryGO.SetActive(true);
			entryGO.transform.SetParent(m_logMessageRoot, false);
			entryGO.transform.SetAsLastSibling();

			LogMessageEntry entry = entryGO.GetComponent<LogMessageEntry>();
			entry.SetMessage(logMessage);
			entry.Clicked += HandleLogMessageClicked;
			entry.UserAlternateBackground = m_messages.Count > 0 ? !m_messages.Last.Value.UserAlternateBackground : false;
			m_messages.AddLast(entry);
			ApplyFilters(entry);
		}

		private void RecycleMessageEntry(LogMessage logMessage)
		{
			var entry = m_messages.First.Value;
			entry.SetMessage(logMessage);
			entry.transform.SetAsLastSibling();
			entry.UserAlternateBackground = m_messages.Count > 0 ? !m_messages.Last.Value.UserAlternateBackground : false;
			m_messages.RemoveFirst();
			m_messages.AddLast(entry);
			ApplyFilters(entry);
		}

		private void ApplyFilters(LogMessageEntry entry)
		{
			bool isOn = false;
			foreach(var filter in m_filters)
			{
				if(filter.LogLevel == entry.LogLevel)
				{
					filter.MessageCount++;
					isOn = filter.IsOn;
					break;
				}
			}

			entry.gameObject.SetActive(isOn);
		}

		private void HandleLogMessageClicked(LogMessageEntry entry)
		{
			if(m_selectedMessage != null)
			{
				m_selectedMessage.OnDeselected();
				m_stackTraceField.text = null;
			}

			m_sendErrorButton.gameObject.SetActive(entry.LogLevel == LogLevel.Error);
			m_selectedMessage = entry;
			m_selectedMessage.OnSelected();
			m_stackTraceField.text = m_selectedMessage.StackTrace;
		}

		private void HandleMessageFilterChanged(MessageFilter filter)
		{
			foreach(var entry in m_messages)
			{
				if(entry.LogLevel == filter.LogLevel)
				{
					entry.gameObject.SetActive(filter.IsOn);
				}
			}

			SaveLayoutChanges();
		}

		private void SaveLayoutChanges()
		{
			DebugConsolePrefs.SetVector2("WindowSize", m_panel.sizeDelta);
			DebugConsolePrefs.SetFloat("StackTraceHeight", m_stackTraceLayout.preferredHeight);
			for(int i = 0; i < m_filters.Length; i++)
			{
				DebugConsolePrefs.SetBool("Filter_" + i, m_filters[i].IsOn);
			}
		}

		private void LoadLayoutChanges()
		{
			m_panel.sizeDelta = DebugConsolePrefs.GetVector2("WindowSize", m_minWindowSize);
			m_stackTraceLayout.preferredHeight = DebugConsolePrefs.GetFloat("StackTraceHeight", m_stackTraceLayout.preferredHeight);
			for(int i = 0; i < m_filters.Length; i++)
			{
				m_filters[i].Changed -= HandleMessageFilterChanged;
				m_filters[i].IsOn = DebugConsolePrefs.GetBool("Filter_" + i, true);
				m_filters[i].Changed += HandleMessageFilterChanged;
			}
		}

		public void ResetLayout()
		{
			m_stackTraceLayout.preferredHeight = m_defaultStackTraceHeight;
			m_panel.sizeDelta = m_minWindowSize;
			for(int i = 0; i < m_filters.Length; i++)
			{
				m_filters[i].IsOn = true;
			}
		}
	}
}