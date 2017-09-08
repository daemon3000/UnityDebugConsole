using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using System;
using System.Xml.Linq;
using System.Collections;
using Luminosity.Debug.UI;
using UnityDebug = UnityEngine.Debug;

namespace Luminosity.Debug.Examples
{
	public class GoogleFeedbackForm : MonoBehaviour, IFeedbackForm
	{
		private const string NOT_ASSIGNED = "N/A";

		[SerializeField]
		private GoogleFormsSettings m_googleFormSettings;
		[SerializeField]
		private FeedbackUI m_feedbackUI;

		private UnityAction<FeedbackFormResult> m_onDone;

		public FeedbackType FeedbackType { get; set; }
		public string Email { get; set; }
		public string Subject { get; set; }
		public string Feedback { get; set; }
		public string SystemInfo { get; set; }
		public byte[] Screenshot { get; set; }

		private void Start()
		{
			m_feedbackUI.FeedbackForm = this;
		}

		private void OnDestroy()
		{
			if(m_feedbackUI != null && m_feedbackUI.FeedbackForm == (IFeedbackForm)this)
			{
				m_feedbackUI.FeedbackForm = null;
			}
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
			WWWForm form = new WWWForm();
			form.AddField(m_googleFormSettings.FeedbackTypeFieldID, FeedbackType.ToString());
			form.AddField(m_googleFormSettings.EmailFieldID, SanitiseString(Email));
			form.AddField(m_googleFormSettings.SubjectFieldID, SanitiseString(Subject));
			form.AddField(m_googleFormSettings.FeedbackFieldID, SanitiseString(Feedback));
			form.AddField(m_googleFormSettings.SystemInfoFieldID, SanitiseString(SystemInfo));

			if(Screenshot != null)
			{
				yield return StartCoroutine(UploadScreenshotToImgur(result =>
				{
					form.AddField(m_googleFormSettings.ScreenshotFieldID, SanitiseString(result));
				}));
			}
			else
			{
				form.AddField(m_googleFormSettings.ScreenshotFieldID, NOT_ASSIGNED);
			}

			WWW www = new WWW(m_googleFormSettings.FormURL, form.data);
			yield return www;
			
			if(m_onDone != null)
			{
				m_onDone(new FeedbackFormResult(www.error));
				m_onDone = null;
			}
		}

		private IEnumerator SendEnhancementAsync()
		{
			WWWForm form = new WWWForm();
			form.AddField(m_googleFormSettings.FeedbackTypeFieldID, FeedbackType.ToString());
			form.AddField(m_googleFormSettings.EmailFieldID, SanitiseString(Email));
			form.AddField(m_googleFormSettings.SubjectFieldID, SanitiseString(Subject));
			form.AddField(m_googleFormSettings.FeedbackFieldID, SanitiseString(Feedback));
			form.AddField(m_googleFormSettings.ScreenshotFieldID, NOT_ASSIGNED);
			form.AddField(m_googleFormSettings.SystemInfoFieldID, NOT_ASSIGNED);

			WWW www = new WWW(m_googleFormSettings.FormURL, form.data);
			yield return www;

			if(m_onDone != null)
			{
				m_onDone(new FeedbackFormResult(www.error));
				m_onDone = null;
			}
		}

		private IEnumerator UploadScreenshotToImgur(UnityAction<string> onSetResult)
		{
			WWWForm uploadData = new WWWForm();
			uploadData.AddField("image", Convert.ToBase64String(Screenshot));
			uploadData.AddField("type", "base64");

			UnityWebRequest uploadRequest = UnityWebRequest.Post(m_googleFormSettings.ImgurUploadURL, uploadData);
			uploadRequest.SetRequestHeader("AUTHORIZATION", "Client-ID " + m_googleFormSettings.ImgurClientID);
			yield return uploadRequest.Send();

			if(uploadRequest.isError)
			{
				UnityDebug.LogError(uploadRequest.error);
				onSetResult("N/A");
			}
			else
			{
				onSetResult(PrintImgurUploadResult(uploadRequest.downloadHandler.text));
			}
		}

		private string PrintImgurUploadResult(string result)
		{
			XDocument doc = XDocument.Parse(result);
			string link = doc.Element("data").Element("link").Value;
			string deleteHash = doc.Element("data").Element("deletehash").Value;

			return string.Format("Link: {0}\nDelete: imgur.com/delete/{1}", link, deleteHash);
		}

		private string SanitiseString(string str)
		{
			return string.IsNullOrEmpty(str) ? NOT_ASSIGNED : str;
		}
	}
}