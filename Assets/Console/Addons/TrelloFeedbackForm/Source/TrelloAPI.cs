using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Luminosity.Trello
{
	public class TrelloAPI
	{
		public class TrelloTask : AsyncOperation
		{
			public bool Success { get; set; }
			public string Error { get; set; }

			public TrelloTask() :
				base()
			{
				Success = false;
				Error = null;
			}
		}

		private const string CARD_NEW_URL = "https://api.trello.com/1/cards/?key={0}&token={1}";
		private const string CARD_COMMENT_URL = "https://api.trello.com/1/cards/{0}/actions/comments/?key={1}&token={2}";

		private string m_accessToken;
		private string m_applicationKey;
		private TrelloTask m_runningTask;
		private MonoBehaviour m_context;
				
		public TrelloAPI(MonoBehaviour context, string key, string token)
		{
			m_applicationKey = key;
			m_accessToken = token;
			m_runningTask = null;
			m_context = context;
		}

		public void Dispose()
		{
			m_context = null;
		}

		public TrelloTask SendCard(Card card)
		{
			if(m_runningTask == null)
			{
				m_runningTask = CreateNewTask();
				m_context.StartCoroutine(SendCardAsync(card));

				return m_runningTask;
			}

			return null;
		}
		
		private IEnumerator SendCardAsync(Card card)
		{
			WWWForm post = new WWWForm();
			post.AddField("name", card.Name ?? "");
			post.AddField("desc", card.Description ?? "");
			post.AddField("due", "null");
			post.AddField("idList", card.ListID);
			post.AddField("urlSource", card.Attachment ?? "null");

			WWW www = new WWW(string.Format(CARD_NEW_URL, m_applicationKey, m_accessToken), post);
			yield return www;

			if(www.error != null)
			{
				FinishRunningTask(www.error);
				yield break;
			}

			if(!string.IsNullOrEmpty(card.Comment))
			{
				Dictionary<string, object> response = Json.Deserialize(www.text) as Dictionary<string, object>;
				string cardID = response["id"].ToString();

				post = new WWWForm();
				post.AddField("text", card.Comment);

				www = new WWW(string.Format(CARD_COMMENT_URL, cardID, m_applicationKey, m_accessToken), post);
				yield return www;

				if(www.error != null)
				{
					FinishRunningTask(www.error);
					yield break;
				}
			}

			FinishRunningTask(null);
		}

		private TrelloTask CreateNewTask()
		{
			return new TrelloTask();
		}

		private void FinishRunningTask(string error)
		{
			if(m_runningTask != null)
			{
				m_runningTask.Success = error == null;
				m_runningTask.Error = error;
				m_runningTask.Finish();
				m_runningTask = null;
			}
		}
	}
}