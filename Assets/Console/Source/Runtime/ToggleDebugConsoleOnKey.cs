using UnityEngine;

namespace Luminosity.Console
{
	public class ToggleDebugConsoleOnKey : MonoBehaviour
	{
		public KeyCode KeyCode;

		private void Update()
		{
			if(Input.GetKeyDown(KeyCode))
			{
				if(DebugConsole.IsOpen)
				{
					DebugConsole.Close();
				}
				else
				{
					DebugConsole.Open();
				}
			}
		}

		private void Reset()
		{
			KeyCode = KeyCode.BackQuote;
		}
	}
}