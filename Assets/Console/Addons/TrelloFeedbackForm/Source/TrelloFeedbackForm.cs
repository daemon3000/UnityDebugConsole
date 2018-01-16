using UnityEngine;
using UnityEngine.Events;
using System;
using System.Text;
using System.Collections;
using Luminosity.Console.UI;
using Luminosity.Trello;

namespace Luminosity.Console
{
	public class TrelloFeedbackForm : MonoBehaviour, IFeedbackForm
	{
		private const string NOT_ASSIGNED = "N/A";
		private const string NULL_URL = null;

		[SerializeField]
		private TrelloFormSettings m_formSettings;
		[SerializeField]
		private FeedbackUI m_feedbackUI;
		
		private UnityAction<FeedbackFormResult> m_onDone;
		private TrelloAPI m_trelloAPI;
		private ImgurAPI m_imgurAPI;
		private StringBuilder m_stringBuilder;

		public FeedbackType FeedbackType { get; set; }
		public string Email { get; set; }
		public string Subject { get; set; }
		public string Feedback { get; set; }
		public string SystemInfo { get; set; }
		public byte[] Screenshot { get; set; }

		private void Start()
		{
			m_trelloAPI = new TrelloAPI(this, m_formSettings.TrelloAppKey, m_formSettings.TrelloAccessToken);
			m_imgurAPI = new ImgurAPI(this, m_formSettings.ImgurClientID);
			m_stringBuilder = new StringBuilder();
			m_feedbackUI.FeedbackForm = this;
		}

		private void OnDestroy()
		{
			if(m_feedbackUI != null && m_feedbackUI.FeedbackForm == (IFeedbackForm)this)
			{
				m_feedbackUI.FeedbackForm = null;
			}

			m_trelloAPI.Dispose();
			m_trelloAPI = null;

			m_imgurAPI.Dispose();
			m_imgurAPI = null;
		}

		public void Send(UnityAction<FeedbackFormResult> onDone)
		{
			m_onDone = onDone;
			if(FeedbackType == FeedbackType.Bug)
			{
				StartCoroutine(SendBugReportAsync());
			}
			else
			{
				StartCoroutine(SendEnhancementAsync());
			}
		}

		private IEnumerator SendBugReportAsync()
		{
			string screenshotUrl = NULL_URL;

			if(Screenshot != null)
			{
				var imgurTask = m_imgurAPI.UploadImage(Screenshot);
				yield return imgurTask;

				if(imgurTask.Success)
					screenshotUrl = SanitiseUrl(imgurTask.Link);
			}

			m_stringBuilder.Length = 0;
			m_stringBuilder.AppendLine("#Author");
			m_stringBuilder.AppendFormat("Author: {0}\n", SanitiseString(Email));
			m_stringBuilder.AppendFormat("Date: {0}\n", DateTime.Now.ToString("dddd, MMMM d, yyyy"));
			m_stringBuilder.AppendLine("#System Info");
			m_stringBuilder.AppendLine(SanitiseString(SystemInfo));

			Card card = new Card
			{
				Name = SanitiseString(Subject),
				ListID = m_formSettings.BugListID,
				Description = SanitiseString(Feedback),
				Comment = m_stringBuilder.ToString(),
				Attachment = screenshotUrl
			};

			var trelloTask = m_trelloAPI.SendCard(card);
			yield return trelloTask;

			if(m_onDone != null)
			{
				m_onDone(new FeedbackFormResult(trelloTask.Success, trelloTask.Error));
				m_onDone = null;
			}
		}

		private IEnumerator SendEnhancementAsync()
		{
			string listID = FeedbackType == FeedbackType.Suggestion ?
							m_formSettings.SuggestionsListID :
							m_formSettings.GeneralFeedbackListID;

			m_stringBuilder.Length = 0;
			m_stringBuilder.AppendLine("#Author");
			m_stringBuilder.AppendFormat("Author: {0}\n", SanitiseString(Email));
			m_stringBuilder.AppendFormat("Date: {0}\n", DateTime.Now.ToString("dddd, MMMM d, yyyy"));
			m_stringBuilder.AppendLine("#System Info");
			m_stringBuilder.AppendLine(SanitiseString(SystemInfo));

			Card card = new Card
			{
				Name = SanitiseString(Subject),
				ListID = listID,
				Description = SanitiseString(Feedback),
				Comment = m_stringBuilder.ToString(),
				Attachment = null
			};

			var trelloTask = m_trelloAPI.SendCard(card);
			yield return trelloTask;

			if(m_onDone != null)
			{
				m_onDone(new FeedbackFormResult(trelloTask.Success, trelloTask.Error));
				m_onDone = null;
			}
		}

		private string SanitiseString(string str)
		{
			return string.IsNullOrEmpty(str) ? NOT_ASSIGNED : str;
		}

		private string SanitiseUrl(string str)
		{
			return string.IsNullOrEmpty(str) ? NULL_URL : str;
		}
	}
}