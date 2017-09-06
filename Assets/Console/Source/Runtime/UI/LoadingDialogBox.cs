using UnityEngine;
using UnityEngine.UI;

namespace Luminosity.Debug.UI
{
	public class LoadingDialogBox : MonoBehaviour
	{
		[SerializeField]
		private RectTransform m_panel;
		[SerializeField]
		private Text m_content;

		public void Open(string message)
		{
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
	}
}