using UnityEngine;

namespace Luminosity.Debug.UI
{
	public class SpinningIcon : MonoBehaviour
	{
		[SerializeField]
		private float m_iconSpinSpeed;

		private void OnEnable()
		{
			transform.localEulerAngles = Vector3.zero;
		}

		private void Update()
		{
			Vector3 eulerAngles = transform.localEulerAngles;
			eulerAngles += Vector3.forward * m_iconSpinSpeed * Time.unscaledDeltaTime;
			eulerAngles.z %= 360;

			transform.localEulerAngles = eulerAngles;
		}
	}
}
