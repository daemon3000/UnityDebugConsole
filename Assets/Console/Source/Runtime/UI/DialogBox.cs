using UnityEngine;
using UnityEngine.Events;
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

		private UnityAction<bool> m_doneHandler;

		public void Open(string message, ButtonLayout buttonLayout, UnityAction<bool> doneHandler)
		{
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
	}
}