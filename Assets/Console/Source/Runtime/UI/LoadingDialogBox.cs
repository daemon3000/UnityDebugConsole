using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Luminosity.Console.UI
{
	public class LoadingDialogBox : MonoBehaviour
	{
		[SerializeField]
		private RectTransform m_panel;
		[SerializeField]
		private Text m_content;
		[SerializeField]
		private Vector2 m_padding;

		public void Open(string message)
		{
			Vector2 position;
			position.x = Screen.width / 2 - m_panel.sizeDelta.x / 2;
			position.y = -(Screen.height / 2 - m_panel.sizeDelta.y / 2);

			Open(position, message);
		}

		public void Open(Vector2 position, string message)
		{
			m_panel.anchoredPosition = position;
			m_content.text = message;
			m_panel.gameObject.SetActive(true);
		}

		public void Close()
		{
			m_panel.gameObject.SetActive(false);
		}

		private void Start()
		{
			Close();
		}

		public void OnBeginWindowDrag(BaseEventData eventData)
		{
		}

		public void OnEndWindowDrag(BaseEventData eventData)
		{
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
	}
}