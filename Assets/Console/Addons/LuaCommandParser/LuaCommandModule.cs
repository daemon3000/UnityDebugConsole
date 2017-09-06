using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using System;
using System.Collections.Generic;

namespace Luminosity.Debug
{
	public abstract class LuaCommandModule
	{
		private static HashSet<Type> m_registeredTypes = new HashSet<Type>();

		[MoonSharpHidden]
		public abstract string Name { get; }

		[MoonSharpHidden]
		public void Register()
		{
			Type type = GetType();
			if(!m_registeredTypes.Contains(type))
			{
				var descriptor = (StandardUserDataDescriptor)UserData.RegisterType(type);
				descriptor.RemoveMember("get_Name");
				descriptor.RemoveMember("Equals");
				descriptor.RemoveMember("ToString");
				descriptor.RemoveMember("GetType");
				descriptor.RemoveMember("GetHashCode");
				m_registeredTypes.Add(type);
			}
		}
	}
}