
namespace Luminosity.Debug
{
	public interface ICommandParser
	{
		void Run(string command);
		string PrintHelp();
	}
}