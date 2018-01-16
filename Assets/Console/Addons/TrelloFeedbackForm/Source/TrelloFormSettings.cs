using UnityEngine;

namespace Luminosity.Console
{
	[CreateAssetMenu(menuName = "Luminosity/Trello Form Settings")]
	public class TrelloFormSettings : ScriptableObject
	{
		public string TrelloAppKey;
		public string TrelloAccessToken;
		public string ImgurClientID;
		public string BugListID;
		public string SuggestionsListID;
		public string GeneralFeedbackListID;
	}
}