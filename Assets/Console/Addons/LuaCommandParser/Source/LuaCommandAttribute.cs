using System;

namespace Luminosity.Debug
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
