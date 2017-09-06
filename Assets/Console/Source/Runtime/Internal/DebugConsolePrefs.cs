using UnityEngine;
using System;

namespace Luminosity.Debug.Internal
{
	public static class DebugConsolePrefs
	{
		private const int BYTES_PER_FLOAT = 4;
		
		public static void SetBool(string key, bool value)
		{
			key = GenerateSaveKey(key);
			int valueAsInt = value ? 1 : 0;
			PlayerPrefs.SetInt(key, valueAsInt);
		}

		public static bool GetBool(string key, bool defValue = false)
		{
			key = GenerateSaveKey(key);
			int valueAsInt = defValue ? 1 : 0;
			valueAsInt = PlayerPrefs.GetInt(key, valueAsInt);
			return valueAsInt != 0;
		}

		public static void SetInt(string key, int value)
		{
			key = GenerateSaveKey(key);
			PlayerPrefs.SetInt(key, value);
		}

		public static int GetInt(string key, int defValue = 0)
		{
			key = GenerateSaveKey(key);
			return PlayerPrefs.GetInt(key, defValue);
		}

		public static void SetFloat(string key, float value)
		{
			key = GenerateSaveKey(key);
			PlayerPrefs.SetFloat(key, value);
		}

		public static float GetFloat(string key, float defValue = 0)
		{
			key = GenerateSaveKey(key);
			return PlayerPrefs.GetFloat(key, defValue);
		}

		public static void SetString(string key, string value)
		{
			key = GenerateSaveKey(key);
			PlayerPrefs.SetString(key, value);
		}

		public static string GetString(string key, string defValue = null)
		{
			key = GenerateSaveKey(key);
			return PlayerPrefs.GetString(key, defValue);
		}

		public static void SetVector2(string key, Vector2 vector)
		{
			key = GenerateSaveKey(key);
			SetFloatArray(key, new float[] { vector.x, vector.y });
		}

		public static Vector2 GetVector2(string key)
		{
			return GetVector2(key, Vector2.zero);
		}

		public static Vector2 GetVector2(string key, Vector2 defValue)
		{
			key = GenerateSaveKey(key);
			if(PlayerPrefs.HasKey(key))
			{
				float[] components = GetFloatArray(key);
				if(components.Length != 2)
				{
					return defValue;
				}
				return new Vector2(components[0], components[1]);
			}

			return defValue;
		}

		public static void SetVector3(string key, Vector3 vector)
		{
			key = GenerateSaveKey(key);
			SetFloatArray(key, new float[] { vector.x, vector.y, vector.z });
		}

		public static Vector3 GetVector3(string key)
		{
			return GetVector3(key, Vector3.zero);
		}

		public static Vector3 GetVector3(string key, Vector3 defValue)
		{
			key = GenerateSaveKey(key);
			if(PlayerPrefs.HasKey(key))
			{
				float[] components = GetFloatArray(key);
				if(components.Length != 3)
				{
					return defValue;
				}

				return new Vector3(components[0], components[1], components[2]);
			}

			return defValue;
		}

		public static void SetQuaternion(string key, Quaternion quaternion)
		{
			key = GenerateSaveKey(key);
			SetFloatArray(key, new float[] { quaternion.x, quaternion.y, quaternion.z, quaternion.w });
		}

		public static Quaternion GetQuaternion(string key)
		{
			return GetQuaternion(key, Quaternion.identity);
		}

		public static Quaternion GetQuaternion(string key, Quaternion defValue)
		{
			key = GenerateSaveKey(key);
			if(PlayerPrefs.HasKey(key))
			{
				float[] components = GetFloatArray(key);
				if(components.Length != 4)
				{
					return defValue;
				}

				return new Quaternion(components[0], components[1], components[2], components[3]);
			}

			return defValue;
		}

		public static void SetColor(string key, Color color)
		{
			key = GenerateSaveKey(key);
			SetFloatArray(key, new float[] { color.r, color.g, color.b, color.a });
		}

		public static Color GetColor(string key)
		{
			return GetColor(key, Color.white);
		}

		public static Color GetColor(string key, Color defValue)
		{
			key = GenerateSaveKey(key);
			if(PlayerPrefs.HasKey(key))
			{
				float[] components = GetFloatArray(key);
				if(components.Length != 4)
				{
					return defValue;
				}

				return new Color(components[0], components[1], components[2], components[3]);
			}

			return defValue;
		}

		private static void SetFloatArray(string key, float[] array)
		{
			byte[] bytes = new byte[BYTES_PER_FLOAT * array.Length];
			for(int i = 0; i < array.Length; i++)
			{
				Array.Copy(BitConverter.GetBytes(array[i]), 0, bytes, i * BYTES_PER_FLOAT, BYTES_PER_FLOAT);
			}

			SetBytes(key, bytes);
		}

		private static float[] GetFloatArray(string key)
		{
			byte[] bytes = GetBytes(key);
			if(bytes != null && bytes.Length % BYTES_PER_FLOAT == 0)
			{
				try
				{
					float[] values = new float[bytes.Length / BYTES_PER_FLOAT];
					for(int i = 0; i < values.Length; i++)
					{
						values[i] = BitConverter.ToSingle(bytes, i * BYTES_PER_FLOAT);
					}

					return values;
				}
				catch
				{
					return new float[0];
				}
			}

			return new float[0];
		}

		private static void SetBytes(string key, byte[] bytes)
		{
			PlayerPrefs.SetString(key, Convert.ToBase64String(bytes));
		}

		private static byte[] GetBytes(string key)
		{
			string str = PlayerPrefs.GetString(key, null);
			if(str != null)
			{
				try
				{
					return Convert.FromBase64String(str);
				}
				catch
				{
					return null;
				}
			}

			return null;
		}

		private static string GenerateSaveKey(string key)
		{
			return string.Format("DebugConsole_{0}_{1}", DebugConsole.VERSION, key);
		}
	}
}