
namespace Luminosity.Debug
{
	public interface IPauseHandler
	{
		void OnDebugConsolePause();
		void OnDebugConsoleUnpause();
	}
}