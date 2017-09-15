
namespace Luminosity.Console
{
	public interface ICommandParser
	{
		void Run(string command);
		string PrintHelp();
	}
}