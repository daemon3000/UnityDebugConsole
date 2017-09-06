using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Luminosity.Debug.UI
{
	public class LogMessageEntry : MonoBehaviour, IPointerClickHandler
	{
		[SerializeField]
		private Text m_messageText;
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

		private LogMessage m_message;
		private bool m_isSelected;
		private bool m_userAlternateBackground;

		public event UnityAction<LogMessageEntry> Clicked;

		public LogMessage LogMessage
		{
			get { return m_message; }
		}

		public string Message
		{
			get { return m_message.Message; }
		}

		public string StackTrace
		{
			get { return m_message.StackTrace; }
		}

		public LogLevel LogLevel
		{
			get { return m_message.LogLevel; }
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

		private void Awake()
		{
			m_isSelected = false;
			UserAlternateBackground = false;
		}

		public void SetMessage(LogMessage logMessage)
		{
			m_messageText.text = logMessage.Message;
			m_message = logMessage;

			switch(LogLevel)
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