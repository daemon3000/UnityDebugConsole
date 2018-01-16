using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Xml.Linq;

namespace Luminosity.Trello
{
	public class ImgurAPI
	{
		public class ImgurTask : AsyncOperation
		{
			public bool Success { get; set; }
			public string Error { get; set; }
			public string Link { get; set; }

			public ImgurTask() :
				base()
			{
				Success = false;
				Error = null;
				Link = null;
			}
		}

		private const string UPLOAD_URL = "https://api.imgur.com/3/image.xml";

		private string m_clientID;
		private ImgurTask m_runningTask;
		private MonoBehaviour m_context;

		public ImgurAPI(MonoBehaviour context, string clientID)
		{
			m_clientID = clientID;
			m_context = context;
			m_runningTask = null;
		}

		public void Dispose()
		{
			m_context = null;
		}

		public ImgurTask UploadImage(byte[] imageData)
		{
			if(m_runningTask == null)
			{
				m_runningTask = CreateNewTask();
				m_context.StartCoroutine(UploadImageAsync(imageData));

				return m_runningTask;
			}

			return null;
		}

		private IEnumerator UploadImageAsync(byte[] imageData)
		{
			WWWForm uploadData = new WWWForm();
			uploadData.AddField("image", Convert.ToBase64String(imageData));
			uploadData.AddField("type", "base64");

			UnityWebRequest uploadRequest = UnityWebRequest.Post(UPLOAD_URL, uploadData);
			uploadRequest.SetRequestHeader("AUTHORIZATION", "Client-ID " + m_clientID);
			yield return uploadRequest.SendWebRequest();

			if(uploadRequest.isNetworkError)
			{
				FinishRunningTask(null, uploadRequest.error);
			}
			else
			{
				FinishRunningTask(ExtractDownloadUrl(uploadRequest.downloadHandler.text), null);
			}
		}

		private string ExtractDownloadUrl(string result)
		{
			XDocument doc = XDocument.Parse(result);
			return doc.Element("data").Element("link").Value;
		}

		private ImgurTask CreateNewTask()
		{
			return new ImgurTask();
		}

		private void FinishRunningTask(string link, string error)
		{
			if(m_runningTask != null)
			{
				m_runningTask.Success = error == null;
				m_runningTask.Error = error;
				m_runningTask.Link = link;
				m_runningTask.Finish();
				m_runningTask = null;
			}
		}
	}
}
