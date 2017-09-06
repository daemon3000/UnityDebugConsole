using UnityEngine;
using System.Text;
using System.Text.RegularExpressions;

namespace Luminosity.Debug.Internal
{
	public static class DebugConsoleUtils
	{
		private const string EMAIL_PATTERN =
			@"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
			+ @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
			  + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
			+ @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})$";

		private static StringBuilder m_stringBuilder = new StringBuilder();

		public static string PrintSystemInfo()
		{
			m_stringBuilder.Length = 0;
			m_stringBuilder.AppendFormat("Platform: {0}\n", Application.platform);
			m_stringBuilder.AppendFormat("Operating System: {0}\n", SystemInfo.operatingSystem);
			m_stringBuilder.AppendFormat("Language: {0}\n", Application.systemLanguage);
			m_stringBuilder.AppendFormat("Screen: {0}x{1}; DPI: {2}; Target: {3}FPS\n", Screen.width, Screen.height, Screen.dpi, Application.targetFrameRate);
			m_stringBuilder.AppendLine();
			m_stringBuilder.AppendFormat("Device Type: {0}\n", SystemInfo.deviceType);
			m_stringBuilder.AppendFormat("Device Model: {0}\n", SystemInfo.deviceModel);
			m_stringBuilder.AppendLine();
			m_stringBuilder.AppendFormat("Graphics Device Name: {0}\n", SystemInfo.graphicsDeviceName);
			m_stringBuilder.AppendFormat("Graphics Device Vendor: {0}\n", SystemInfo.graphicsDeviceVendor);
			m_stringBuilder.AppendFormat("Graphics API: {0}\n", SystemInfo.graphicsDeviceVersion);
			m_stringBuilder.AppendFormat("Graphich Memory: {0}\n", SystemInfo.graphicsMemorySize);
			m_stringBuilder.AppendFormat("Shader Level: {0}\n", SystemInfo.graphicsShaderLevel);
			m_stringBuilder.AppendFormat("Max Texture Size: {0}\n", SystemInfo.maxTextureSize);
			m_stringBuilder.AppendLine();
			m_stringBuilder.AppendFormat("Processor Type: {0}\n", SystemInfo.processorType);
			m_stringBuilder.AppendFormat("Processor Count: {0}\n", SystemInfo.processorCount);
			m_stringBuilder.AppendFormat("Processor Frequency: {0}\n", SystemInfo.processorFrequency);
			m_stringBuilder.AppendLine();
			m_stringBuilder.AppendFormat("System Memory: {0}\n", SystemInfo.systemMemorySize);
			m_stringBuilder.AppendLine();

			return m_stringBuilder.ToString();
		}

		public static bool IsEmailAddressValid(string email)
		{
			if(!string.IsNullOrEmpty(email))
				return Regex.IsMatch(email, EMAIL_PATTERN);

			return false;
		}
	}
}
