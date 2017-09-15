using System;

namespace Luminosity.Console
{
	public class LuaCommandAttribute : Attribute
	{
		public readonly string Help;

		public LuaCommandAttribute(string help)
		{
			Help = help;
		}
	}
}
