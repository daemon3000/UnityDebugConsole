using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Luminosity.Console.Internal;

namespace Luminosity.Console.UI
{
	public class HelpUI : MonoBehaviour
	{
		[SerializeField]
		private RectTransform m_panel;
		[SerializeField]
		private Text m_contentField;
		[SerializeField]
		private Vector2 m_padding;

		private void Start()
		{
			LoadLayoutChanges();
		}

		public void Open(ICommandParser commandParser)
		{
			if(commandParser != null)
			{
				m_contentField.text = commandParser.PrintHelp();
			}
			else
			{
				m_contentField.text = "No command parser has been assigned.";
			}
			
			m_panel.gameObject.SetActive(true);
			DebugConsole.Lock(false);
		}

		public void Close()
		{
			m_panel.gameObject.SetActive(false);
			DebugConsole.Unlock();
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

		private void SaveLayoutChanges()
		{
			DebugConsolePrefs.SetVector2("HelpUI_Position", m_panel.anchoredPosition);
		}

		private void LoadLayoutChanges()
		{
			Vector2 defPosition;
			defPosition.x = Screen.width / 2 - m_panel.sizeDelta.x / 2;
			defPosition.y = -(Screen.height / 2 - m_panel.sizeDelta.y / 2);

			m_panel.anchoredPosition = DebugConsolePrefs.GetVector2("HelpUI_Position", defPosition);
		}

		public void ResetLayout()
		{
			Vector2 defPosition;
			defPosition.x = Screen.width / 2 - m_panel.sizeDelta.x / 2;
			defPosition.y = -(Screen.height / 2 - m_panel.sizeDelta.y / 2);

			m_panel.anchoredPosition = defPosition;
		}
	}
}