using UnityEngine;

namespace Luminosity.Debug
{
	public class GenericPauseHandler : MonoBehaviour, IPauseHandler
	{
		private void Start()
		{
			DebugConsole.PauseHandler = this;
		}

		private void OnDestroy()
		{
			if(DebugConsole.PauseHandler == (IPauseHandler)this)
			{
				DebugConsole.PauseHandler = null;
			}
		}

		public void OnDebugConsolePause()
		{
			Time.timeScale = 0.0f;
		}

		public void OnDebugConsoleUnpause()
		{
			Time.timeScale = 1.0f;
		}
	}
}