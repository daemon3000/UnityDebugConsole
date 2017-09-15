using UnityEngine;
using System;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace Luminosity.Console
{
	public class LuaCommandParser : MonoBehaviour, ICommandParser
	{
		private static Dictionary<string, string> TypeNameLookup = new Dictionary<string, string>()
		{
			{ "String", "string" },
			{ "Single", "float" },
			{ "Int32", "int" },
			{ "Boolean", "bool" }
		};

		private StringBuilder m_helpString;
		private Script m_luaScript;

		private void Awake()
		{
			m_helpString = new StringBuilder();
			m_helpString.AppendLine("\u2022 <color=#3255FFFF>help</color> - Prints a list of available commands.");
			m_helpString.AppendLine("\u2022 <color=#3255FFFF>clear</color> - Removes all log messages.");
			m_helpString.AppendLine("\u2022 <color=#3255FFFF>feedback</color> - Opens the feedback form.");
			m_helpString.AppendLine("\u2022 <color=#3255FFFF>reset</color> - Resets debug console layout.");
			m_helpString.AppendLine();

			m_luaScript = new Script(CoreModules.Preset_HardSandbox);
			m_luaScript.Options.DebugPrint = message => Debug.Log(message);

			Assembly assembly = GetType().Assembly;
			Type cpType = typeof(LuaCommandModule);

			var types = from tp in assembly.GetTypes()
						where !tp.IsAbstract && cpType.IsAssignableFrom(tp)
						select tp;

			foreach(var type in types)
			{
				LuaCommandModule instance = Activator.CreateInstance(type) as LuaCommandModule;
				if(instance != null)
				{
					AddCommands(instance);
				}
			}

			DebugConsole.CommandParser = this;
		}

		private void OnDestroy()
		{
			if(DebugConsole.CommandParser == (ICommandParser)this)
			{
				DebugConsole.CommandParser = null;
			}
		}

		public void Run(string command)
		{
			if(string.IsNullOrEmpty(command))
				return;

			try
			{
				m_luaScript.DoString(command);
			}
			catch
			{
				DebugConsole.Log(LogLevel.Error, string.Format("Unknown command '{0}'.\nType 'help' to get a list of available commands.", command), null);
			}
		}

		public string PrintHelp()
		{
			return m_helpString.ToString();
		}

		public void AddCommands<T>() where T : LuaCommandModule, new()
		{
			var commandModule = new T();
			AddCommands(commandModule);
		}

		private void AddCommands(LuaCommandModule commandModule)
		{
			commandModule.Register();
			m_luaScript.Globals[commandModule.Name] = commandModule;
			DocumentCommands(commandModule);
		}

		private void DocumentCommands(LuaCommandModule commandModule)
		{
			m_helpString.AppendFormat("<size=20><b>{0}</b></size>\n", commandModule.Name.ToUpper());
			DocumentProperties(commandModule);
			DocumentMethods(commandModule);
			m_helpString.AppendLine();
		}

		private void DocumentMethods(LuaCommandModule commandModule)
		{
			Type type = commandModule.GetType();
			var methods = from mi in type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
						  select mi;

			foreach(var method in methods)
			{
				var luaCommandAttribute = Attribute.GetCustomAttribute(method, typeof(LuaCommandAttribute)) as LuaCommandAttribute;
				if(luaCommandAttribute != null)
				{
					m_helpString.Append("\u2022 <color=#3255FFFF>");
					m_helpString.Append(commandModule.Name);
					m_helpString.Append('.');

					m_helpString.Append(method.Name);
					m_helpString.Append('(');

					ParameterInfo[] parameters = method.GetParameters();
					for(int i = 0; i < parameters.Length; i++)
					{
						string typeName = parameters[i].ParameterType.Name;
						if(TypeNameLookup.ContainsKey(typeName))
						{
							typeName = TypeNameLookup[typeName];
						}

						m_helpString.Append(typeName);
						m_helpString.Append(' ');
						m_helpString.Append(parameters[i].Name);
						if(i < parameters.Length - 1)
						{
							m_helpString.Append(", ");
						}
					}

					m_helpString.Append(")</color>");

					m_helpString.AppendFormat(" - {0}", luaCommandAttribute.Help);
					m_helpString.Append('\n');
				}
			}
		}

		private void DocumentProperties(LuaCommandModule commandModule)
		{
			Type type = commandModule.GetType();
			var properties = from pi in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
						  select pi;

			foreach(var property in properties)
			{
				var luaCommandAttribute = Attribute.GetCustomAttribute(property, typeof(LuaCommandAttribute)) as LuaCommandAttribute;
				if(luaCommandAttribute != null)
				{
					m_helpString.Append("\u2022 <color=#3255FFFF>");
					m_helpString.Append(commandModule.Name);
					m_helpString.Append('.');

					m_helpString.Append(property.Name);
					m_helpString.Append(" : ");

					string typeName = property.PropertyType.Name;
					if(TypeNameLookup.ContainsKey(typeName))
					{
						typeName = TypeNameLookup[typeName];
					}

					m_helpString.Append(typeName);
					m_helpString.Append("</color>");

					m_helpString.AppendFormat(" - {0}", luaCommandAttribute.Help);
					m_helpString.Append('\n');
				}
			}
		}
	}
}