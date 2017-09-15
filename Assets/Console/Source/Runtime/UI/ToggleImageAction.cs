using UnityEngine;
using UnityEngine.UI;

namespace Luminosity.Console.UI
{
	public class ToggleImageAction : MonoBehaviour
	{
		[SerializeField]
		private Image m_targetImage;
		[SerializeField]
		private bool m_invert;

		public void ToggleImage()
		{
			m_targetImage.enabled = !m_targetImage.enabled;
		}

		public void SetImageEnabled(bool enabled)
		{
			m_targetImage.enabled = m_invert ? !enabled : enabled;
		}
	}
}