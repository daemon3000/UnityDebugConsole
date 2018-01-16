using UnityEngine;
using UnityEngine.SceneManagement;
using Luminosity.Console.Internal;

namespace Luminosity.Console
{
	public class StandardCommandModule : LuaCommandModule
	{
		public override string Name { get { return "game"; } }

		[LuaCommand("Toggles fullscreen on/off.")]
		public bool fullscreen
		{
			set
			{
				Screen.fullScreen = value;
			}
		}

		[LuaCommand("Closes the game application.")]
		public void exit()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}

		[LuaCommand("Prints information about system hardware.")]
		public void systemInfo()
		{
			DebugConsole.Log(LogLevel.Debug, DebugConsoleUtils.PrintSystemInfo());
		}

		[LuaCommand("Loads a new scene.")]
		public void loadScene(string sceneName, bool additive)
		{
			SceneManager.LoadScene(sceneName, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
		}

		[LuaCommand("Loads a new scene asynchronously.")]
		public void loadSceneAsync(string sceneName, bool additive)
		{
			SceneManager.LoadSceneAsync(sceneName, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
		}

		[LuaCommand("Changes screen resolution.")]
		public void setResolution(int width, int height, bool fullscreen)
		{
			Screen.SetResolution(width, height, fullscreen);
		}

		[LuaCommand("Changes screen resolution.")]
		public void setResolution(int width, int height)
		{
			Screen.SetResolution(width, height, true);
		}

		[LuaCommand("Takes a screenshot and saves it to the specified file.")]
		public void captureScreenshot(string filename)
		{
			ScreenCapture.CaptureScreenshot(filename);
		}
	}
}