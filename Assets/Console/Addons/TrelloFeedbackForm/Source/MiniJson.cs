using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Luminosity.Trello
{
	/// <summary>
	/// This class encodes and decodes JSON strings.
	/// Spec. details, see http://www.json.org/
	///
	/// JSON uses Arrays and Objects. These correspond here to the datatypes IList and IDictionary.
	/// All numbers are parsed to doubles.
	/// </summary>
	public static class Json
	{
		/// <summary>
		/// Parses the string json into a value
		/// </summary>
		/// <param name="json">A JSON string.</param>
		/// <returns>An List&lt;object&gt;, a Dictionary&lt;string, object&gt;, a double, an integer,a string, null, true, or false</returns>
		public static object Deserialize(string json)
		{
			// save the string for debug information
			if(json == null)
			{
				return null;
			}

			return Parser.Parse(json);
		}

		internal sealed class Parser : IDisposable
		{
			private const string WORD_BREAK = "{}[],:\"";

			public static bool IsWordBreak(char c)
			{
				return Char.IsWhiteSpace(c) || WORD_BREAK.IndexOf(c) != -1;
			}

			private enum TOKEN
			{
				NONE,
				CURLY_OPEN,
				CURLY_CLOSE,
				SQUARED_OPEN,
				SQUARED_CLOSE,
				COLON,
				COMMA,
				STRING,
				NUMBER,
				TRUE,
				FALSE,
				NULL
			};

			private StringReader m_json;

			private Parser(string jsonString)
			{
				m_json = new StringReader(jsonString);
			}

			public static object Parse(string jsonString)
			{
				using(var instance = new Parser(jsonString))
				{
					return instance.ParseValue();
				}
			}

			public void Dispose()
			{
				m_json.Dispose();
				m_json = null;
			}

			private Dictionary<string, object> ParseObject()
			{
				Dictionary<string, object> table = new Dictionary<string, object>();

				// ditch opening brace
				m_json.Read();

				// {
				while(true)
				{
					switch(NextToken)
					{
					case TOKEN.NONE:
						return null;
					case TOKEN.COMMA:
						continue;
					case TOKEN.CURLY_CLOSE:
						return table;
					default:
						// name
						string name = ParseString();
						if(name == null)
						{
							return null;
						}

						// :
						if(NextToken != TOKEN.COLON)
						{
							return null;
						}
						// ditch the colon
						m_json.Read();

						// value
						table[name] = ParseValue();
						break;
					}
				}
			}

			private List<object> ParseArray()
			{
				List<object> array = new List<object>();

				// ditch opening bracket
				m_json.Read();

				// [
				var parsing = true;
				while(parsing)
				{
					TOKEN nextToken = NextToken;

					switch(nextToken)
					{
					case TOKEN.NONE:
						return null;
					case TOKEN.COMMA:
						continue;
					case TOKEN.SQUARED_CLOSE:
						parsing = false;
						break;
					default:
						object value = ParseByToken(nextToken);

						array.Add(value);
						break;
					}
				}

				return array;
			}

			private object ParseValue()
			{
				TOKEN nextToken = NextToken;
				return ParseByToken(nextToken);
			}

			private object ParseByToken(TOKEN token)
			{
				switch(token)
				{
				case TOKEN.STRING:
					return ParseString();
				case TOKEN.NUMBER:
					return ParseNumber();
				case TOKEN.CURLY_OPEN:
					return ParseObject();
				case TOKEN.SQUARED_OPEN:
					return ParseArray();
				case TOKEN.TRUE:
					return true;
				case TOKEN.FALSE:
					return false;
				case TOKEN.NULL:
					return null;
				default:
					return null;
				}
			}

			private string ParseString()
			{
				StringBuilder s = new StringBuilder();
				char c;

				// ditch opening quote
				m_json.Read();

				bool parsing = true;
				while(parsing)
				{

					if(m_json.Peek() == -1)
					{
						parsing = false;
						break;
					}

					c = NextChar;
					switch(c)
					{
					case '"':
						parsing = false;
						break;
					case '\\':
						if(m_json.Peek() == -1)
						{
							parsing = false;
							break;
						}

						c = NextChar;
						switch(c)
						{
						case '"':
						case '\\':
						case '/':
							s.Append(c);
							break;
						case 'b':
							s.Append('\b');
							break;
						case 'f':
							s.Append('\f');
							break;
						case 'n':
							s.Append('\n');
							break;
						case 'r':
							s.Append('\r');
							break;
						case 't':
							s.Append('\t');
							break;
						case 'u':
							var hex = new char[4];

							for(int i = 0; i < 4; i++)
							{
								hex[i] = NextChar;
							}

							s.Append((char)Convert.ToInt32(new string(hex), 16));
							break;
						}
						break;
					default:
						s.Append(c);
						break;
					}
				}

				return s.ToString();
			}

			private object ParseNumber()
			{
				string number = NextWord;

				if(number.IndexOf('.') == -1)
				{
					long parsedInt;
					long.TryParse(number, out parsedInt);
					return parsedInt;
				}

				double parsedDouble;
				double.TryParse(number, out parsedDouble);
				return parsedDouble;
			}

			private void EatWhitespace()
			{
				while(Char.IsWhiteSpace(PeekChar))
				{
					m_json.Read();

					if(m_json.Peek() == -1)
					{
						break;
					}
				}
			}

			private char PeekChar
			{
				get
				{
					return Convert.ToChar(m_json.Peek());
				}
			}

			private char NextChar
			{
				get
				{
					return Convert.ToChar(m_json.Read());
				}
			}

			private string NextWord
			{
				get
				{
					StringBuilder word = new StringBuilder();

					while(!IsWordBreak(PeekChar))
					{
						word.Append(NextChar);

						if(m_json.Peek() == -1)
						{
							break;
						}
					}

					return word.ToString();
				}
			}

			private TOKEN NextToken
			{
				get
				{
					EatWhitespace();

					if(m_json.Peek() == -1)
					{
						return TOKEN.NONE;
					}

					switch(PeekChar)
					{
					case '{':
						return TOKEN.CURLY_OPEN;
					case '}':
						m_json.Read();
						return TOKEN.CURLY_CLOSE;
					case '[':
						return TOKEN.SQUARED_OPEN;
					case ']':
						m_json.Read();
						return TOKEN.SQUARED_CLOSE;
					case ',':
						m_json.Read();
						return TOKEN.COMMA;
					case '"':
						return TOKEN.STRING;
					case ':':
						return TOKEN.COLON;
					case '0':
					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
					case '8':
					case '9':
					case '-':
						return TOKEN.NUMBER;
					}

					switch(NextWord)
					{
					case "false":
						return TOKEN.FALSE;
					case "true":
						return TOKEN.TRUE;
					case "null":
						return TOKEN.NULL;
					}

					return TOKEN.NONE;
				}
			}
		}

		/// <summary>
		/// Converts a IDictionary / IList object or a simple type (string, int, etc.) into a JSON string
		/// </summary>
		/// <param name="json">A Dictionary&lt;string, object&gt; / List&lt;object&gt;</param>
		/// <returns>A JSON encoded string, or null if object 'json' is not serializable</returns>
		public static string Serialize(object obj)
		{
			return Serializer.Serialize(obj);
		}

		internal sealed class Serializer
		{
			private StringBuilder m_builder;

			private Serializer()
			{
				m_builder = new StringBuilder();
			}

			public static string Serialize(object obj)
			{
				var instance = new Serializer();
				instance.SerializeValue(obj);

				return instance.m_builder.ToString();
			}

			private void SerializeValue(object value)
			{
				IList asList;
				IDictionary asDict;
				string asStr;

				if(value == null)
				{
					m_builder.Append("null");
				}
				else if((asStr = value as string) != null)
				{
					SerializeString(asStr);
				}
				else if(value is bool)
				{
					m_builder.Append((bool)value ? "true" : "false");
				}
				else if((asList = value as IList) != null)
				{
					SerializeArray(asList);
				}
				else if((asDict = value as IDictionary) != null)
				{
					SerializeObject(asDict);
				}
				else if(value is char)
				{
					SerializeString(new string((char)value, 1));
				}
				else
				{
					SerializeOther(value);
				}
			}

			private void SerializeObject(IDictionary obj)
			{
				bool first = true;

				m_builder.Append('{');

				foreach(object e in obj.Keys)
				{
					if(!first)
					{
						m_builder.Append(',');
					}

					SerializeString(e.ToString());
					m_builder.Append(':');

					SerializeValue(obj[e]);

					first = false;
				}

				m_builder.Append('}');
			}

			private void SerializeArray(IList anArray)
			{
				m_builder.Append('[');

				bool first = true;

				foreach(object obj in anArray)
				{
					if(!first)
					{
						m_builder.Append(',');
					}

					SerializeValue(obj);

					first = false;
				}

				m_builder.Append(']');
			}

			private void SerializeString(string str)
			{
				m_builder.Append('\"');

				char[] charArray = str.ToCharArray();
				foreach(var c in charArray)
				{
					switch(c)
					{
					case '"':
						m_builder.Append("\\\"");
						break;
					case '\\':
						m_builder.Append("\\\\");
						break;
					case '\b':
						m_builder.Append("\\b");
						break;
					case '\f':
						m_builder.Append("\\f");
						break;
					case '\n':
						m_builder.Append("\\n");
						break;
					case '\r':
						m_builder.Append("\\r");
						break;
					case '\t':
						m_builder.Append("\\t");
						break;
					default:
						int codepoint = Convert.ToInt32(c);
						if((codepoint >= 32) && (codepoint <= 126))
						{
							m_builder.Append(c);
						}
						else
						{
							m_builder.Append("\\u");
							m_builder.Append(codepoint.ToString("x4"));
						}
						break;
					}
				}

				m_builder.Append('\"');
			}

			private void SerializeOther(object value)
			{
				// NOTE: decimals lose precision during serialization.
				// They always have, I'm just letting you know.
				// Previously floats and doubles lost precision too.
				if(value is float)
				{
					m_builder.Append(((float)value).ToString("R"));
				}
				else if(value is int
						 || value is uint
						 || value is long
						 || value is sbyte
						 || value is byte
						 || value is short
						 || value is ushort
						 || value is ulong)
				{
					m_builder.Append(value);
				}
				else if(value is double
						 || value is decimal)
				{
					m_builder.Append(Convert.ToDouble(value).ToString("R"));
				}
				else
				{
					SerializeString(value.ToString());
				}
			}
		}
	}
}