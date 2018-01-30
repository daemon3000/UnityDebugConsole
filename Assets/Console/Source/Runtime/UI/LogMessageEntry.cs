using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Luminosity.Console.UI
{
	public class LogMessageEntry : MonoBehaviour, IPointerClickHandler
	{
		[SerializeField]
		private Text m_messageText;
		[SerializeField]
		private Text m_messageCountText;
		[SerializeField]
		private Image m_background;
		[SerializeField]
		private Color m_debugMessageColor;
		[SerializeField]
		private Color m_warningMessageColor;
		[SerializeField]
		private Color m_errorMessageColor;
		[SerializeField]
		private Color m_normalBackgroundColor;
		[SerializeField]
		private Color m_altBackgroundColor;
		[SerializeField]
		private Color m_selectedBackgroundColor;
		[SerializeField]
		private int m_maxLogCount;

		private RectTransform m_transform;
		private bool m_isSelected;
		private bool m_userAlternateBackground;

		public event UnityAction<LogMessageEntry> Clicked;

		public LogMessage Message { get; set; }
		public int MessageID { get; set; }

		public bool IsSelected
		{
			get { return m_isSelected; }
		}

		public float Height
		{
			get { return Transform.sizeDelta.y; }
		}

		public bool UserAlternateBackground
		{
			get { return m_userAlternateBackground; }
			set
			{
				m_userAlternateBackground = value;
				UpdateBackgroundColor();
			}
		}
		
		private RectTransform Transform
		{
			get
			{
				if(m_transform == null)
					m_transform = GetComponent<RectTransform>();

				return m_transform;
			}
		}

		private void Awake()
		{
			m_isSelected = false;
			UserAlternateBackground = false;
		}

		public void Show(LogMessage message, int messageID)
		{
			gameObject.SetActive(true);

			m_messageText.text = message.Message;
			Message = message;
			MessageID = messageID;
			
			switch(Message.LogLevel)
			{
			case LogLevel.Debug:
				m_messageText.color = m_debugMessageColor;
				break;
			case LogLevel.Warning:
				m_messageText.color = m_warningMessageColor;
				break;
			case LogLevel.Error:
				m_messageText.color = m_errorMessageColor;
				break;
			}

			Message.LogCountChanged += OnLogCountChanged;
			OnLogCountChanged();
		}

		private void OnLogCountChanged()
		{
			if(Message.LogCount > 1)
			{
				m_messageCountText.transform.parent.gameObject.SetActive(true);

				if(Message.LogCount > m_maxLogCount)
				{
					m_messageCountText.text = string.Concat(m_maxLogCount, "+");
				}
				else
				{
					m_messageCountText.text = Message.LogCount.ToString();
				}
			}
			else
			{
				m_messageCountText.transform.parent.gameObject.SetActive(false);
			}
		}

		public void Hide()
		{
			if(Message != null)
			{
				Message.LogCountChanged -= OnLogCountChanged;
				Message = null;
				MessageID = -1;
			}

			OnDeselected();
			gameObject.SetActive(false);
		}

		public void SetParent(RectTransform container)
		{
			Transform.SetParent(container, false);
		}

		public void SetAsFirstSibling()
		{
			Transform.SetAsFirstSibling();
		}

		public void SetAsLastSibling()
		{
			Transform.SetAsLastSibling();
		}
		
		public void OnSelected()
		{
			m_isSelected = true;
			UpdateBackgroundColor();
		}

		public void OnDeselected()
		{
			m_isSelected = false;
			UpdateBackgroundColor();
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if(Clicked != null)
				Clicked(this);
		}

		private void UpdateBackgroundColor()
		{
			if(m_isSelected)
			{
				m_background.color = m_selectedBackgroundColor;
			}
			else
			{
				if(UserAlternateBackground)
				{
					m_background.color = m_altBackgroundColor;
				}
				else
				{
					m_background.color = m_normalBackgroundColor;
				}
			}
		}

		private void Reset()
		{
			m_messageText = GetComponentInChildren<Text>();
			m_background = GetComponentInChildren<Image>();
			m_debugMessageColor = Color.black;
			m_warningMessageColor = Color.yellow;
			m_errorMessageColor = Color.red;
			m_normalBackgroundColor = Color.white;
			m_altBackgroundColor = Color.white;
			m_selectedBackgroundColor = Color.white;
		}
	}
}