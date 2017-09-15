using UnityEngine;

namespace Luminosity.Console.Examples
{
	[CreateAssetMenu(menuName = "Luminosity/Google Forms Settings")]
	public class GoogleFormsSettings : ScriptableObject
	{
		public string FormURL;
		public string ImgurUploadURL;
		public string ImgurClientID;
		public string FeedbackTypeFieldID;
		public string EmailFieldID;
		public string SubjectFieldID;
		public string ScreenshotFieldID;
		public string SystemInfoFieldID;
		public string FeedbackFieldID;
	}
}