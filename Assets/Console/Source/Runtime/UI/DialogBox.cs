using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Luminosity.Debug.UI
{
	public class DialogBox : MonoBehaviour
	{
		public enum ButtonLayout
		{
			AcceptAndDecline, OK
		}

		[SerializeField]
		private RectTransform m_panel;
		[SerializeField]
		private Button m_acceptButton;
		[SerializeField]
		private Button m_declineButton;
		[SerializeField]
		private Button m_okButton;
		[SerializeField]
		private Text m_content;
		[SerializeField]
		private Vector2 m_padding;

		private UnityAction<bool> m_doneHandler;

		public void Open(string message, ButtonLayout buttonLayout, UnityAction<bool> doneHandler)
		{
			Vector2 position;
			position.x = Screen.width / 2 - m_panel.sizeDelta.x / 2;
			position.y = -(Screen.height / 2 - m_panel.sizeDelta.y / 2);

			Open(position, message, buttonLayout, doneHandler);
		}

		public void Open(Vector2 position, string message, ButtonLayout buttonLayout, UnityAction<bool> doneHandler)
		{
			m_panel.anchoredPosition = position;
			m_doneHandler = doneHandler;
			m_content.text = message;
			ApplyLayout(buttonLayout);

			m_panel.gameObject.SetActive(true);
		}

		private void Close()
		{
			m_panel.gameObject.SetActive(false);
		}

		private void Start()
		{
			m_acceptButton.onClick.AddListener(OnAccepted);
			m_declineButton.onClick.AddListener(OnDeclined);
			m_okButton.onClick.AddListener(OnAccepted);
			Close();
		}

		private void OnAccepted()
		{
			m_doneHandler(true);
			m_doneHandler = null;
			Close();
		}

		private void OnDeclined()
		{
			m_doneHandler(false);
			m_doneHandler = null;
			Close();
		}

		private void ApplyLayout(ButtonLayout layout)
		{
			if(layout == ButtonLayout.AcceptAndDecline)
			{
				m_acceptButton.gameObject.SetActive(true);
				m_declineButton.gameObject.SetActive(true);
				m_okButton.gameObject.SetActive(false);
			}
			else
			{
				m_acceptButton.gameObject.SetActive(false);
				m_declineButton.gameObject.SetActive(false);
				m_okButton.gameObject.SetActive(true);
			}
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