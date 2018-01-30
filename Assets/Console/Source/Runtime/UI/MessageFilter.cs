using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Luminosity.Console.UI
{
	public class MessageFilter : MonoBehaviour
	{
		[SerializeField]
		private Toggle m_toggle;
		[SerializeField]
		private Text m_messageCountText;
		[SerializeField]
		private LogLevel m_logLevel;
		[SerializeField]
		private int m_maxMessageCount;

		private int m_messageCount;

		public event UnityAction Changed;
		public LogLevel LogLevel { get { return m_logLevel; } }

		public bool IsOn
		{
			get { return m_toggle.isOn; }
			set { m_toggle.isOn = value; }
		}

		public int MessageCount
		{
			get { return m_messageCount; }
			set
			{
				value = Mathf.Max(value, 0);
				if(value != m_messageCount)
				{
					m_messageCount = value;
					if(m_messageCount > m_maxMessageCount)
					{
						m_messageCountText.text = string.Format("{0}+", m_maxMessageCount);
					}
					else
					{
						m_messageCountText.text = m_messageCount.ToString();
					}
				}
			}
		}

		private void Awake()
		{
			m_messageCount = 0;
			m_toggle.onValueChanged.AddListener(isOn =>
			{
				if(Changed != null)
				{
					Changed();
				}
			});
		}

		private void Reset()
		{
			m_toggle = GetComponentInChildren<Toggle>();
			m_logLevel = LogLevel.Debug;
		}
	}
}