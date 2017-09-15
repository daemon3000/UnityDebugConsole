using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using Luminosity.Console.Internal;
using _InputField = UnityEngine.UI.InputField;

namespace Luminosity.Console.UI
{
	public class FeedbackUI : MonoBehaviour
	{
		[SerializeField]
		private RectTransform m_panel;
		[SerializeField]
		private DialogBox m_dialogBox;
		[SerializeField]
		private LoadingDialogBox m_loadingBox;
		[SerializeField]
		private Dropdown m_feedbackTypeField;
		[SerializeField]
		private _InputField m_emailField;
		[SerializeField]
		private _InputField m_subjectField;
		[SerializeField]
		private Text m_subjectFieldPlaceholder;
		[SerializeField]
		private _InputField m_contentField;
		[SerializeField]
		private Text m_contentFieldLabel;
		[SerializeField]
		private Text m_contentFieldPlaceholder;
		[SerializeField]
		private Toggle m_screenshotToggle;
		[SerializeField]
		private Toggle m_systemInfoToggle;
		[SerializeField]
		private Vector2 m_padding;

		private CanvasGroup m_canvasGroup;

		public IFeedbackForm FeedbackForm
		{
			get;
			set;
		}

		private static string AbsoluteScreenshotPath
		{
			get
			{
#if UNITY_ANDROID
				return Application.persistentDataPath + "/bug_report.png";
#else
				return Application.temporaryCachePath + "/bug_report.png";
#endif
			}
		}

		private static string ScreenshotPath
		{
			get
			{
#if UNITY_ANDROID
				return "bug_report.png";
#else
				return Application.temporaryCachePath + "/bug_report.png";
#endif
			}
		}

		private string LastUsedEmail
		{
			get
			{
				return PlayerPrefs.GetString("FeedbackUI.LastUsedEmail", null);
			}
			set
			{
				PlayerPrefs.SetString("FeedbackUI.LastUsedEmail", value);
			}
		}

		private FeedbackType FeedbackTypeValue
		{
			get
			{
				return (FeedbackType)m_feedbackTypeField.value;
			}
			set
			{
				m_feedbackTypeField.value = (int)value;
			}
		}

		private void Start()
		{
			m_canvasGroup = m_panel.GetComponent<CanvasGroup>();
			m_feedbackTypeField.onValueChanged.AddListener(HandleFeedbackTypeChanged);
			m_feedbackTypeField.ClearOptions();
			m_feedbackTypeField.AddOptions(new List<string>()
			{
				FeedbackType.Bug.ToString(),
				FeedbackType.Suggestion.ToString(),
				FeedbackType.Other.ToString(),
			});

			LoadLayoutChanges();
		}

		public void Open()
		{
			FeedbackTypeValue = FeedbackType.Bug;
			m_contentFieldLabel.text = "Tell us, in detail, what happened:";
			m_contentFieldPlaceholder.text = "Include a detailed description of the issue as well as steps to reproduce it...";
			m_emailField.text = LastUsedEmail;
			m_subjectField.text = string.Empty;
			m_contentField.text = string.Empty;
			m_screenshotToggle.interactable = true;
			m_systemInfoToggle.interactable = true;
			m_screenshotToggle.isOn = true;
			m_systemInfoToggle.isOn = true;
			m_panel.gameObject.SetActive(true);
			DebugConsole.Lock(true);
		}

		public void Close()
		{
			m_panel.gameObject.SetActive(false);
			DebugConsole.Unlock();
		}

		public void SendErrorReport(LogMessage errorMessage)
		{
			Open();
			FeedbackTypeValue = FeedbackType.Bug;
			m_subjectField.text = errorMessage.Message;
			m_contentField.text = string.Format("{0}\n\n{1}", errorMessage.Message, errorMessage.StackTrace);
		}

		public void SendFeedback()
		{
			if(!DebugConsoleUtils.IsEmailAddressValid(m_emailField.text))
			{
				ShowDialogBox("The email address you have entered is not valid!",
							  DialogBox.ButtonLayout.OK);
				return;
			}

			if(string.IsNullOrEmpty(m_subjectField.text))
			{
				string message = FeedbackTypeValue == FeedbackType.Bug ?
								 "Your bug report must contain a subject!" :
								 "Your feedback must contain a subject!";

				ShowDialogBox(message, DialogBox.ButtonLayout.OK);
				return;
			}

			if(string.IsNullOrEmpty(m_contentField.text))
			{
				string message = FeedbackTypeValue == FeedbackType.Bug ?
								 "Your bug report cannot be empty!" :
								 "Your feedback cannot be empty!";

				ShowDialogBox(message, DialogBox.ButtonLayout.OK);
				return;
			}

			if(FeedbackForm == null)
			{
				ShowDialogBox("No feedback form was assigned!",
							  DialogBox.ButtonLayout.OK);
				return;
			}

			if(FeedbackTypeValue == FeedbackType.Bug)
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
			FeedbackFormResult result = new FeedbackFormResult();
			byte[] screenshotData = null;
			bool isWaiting = true;

			if(m_screenshotToggle.isOn)
				yield return StartCoroutine(CaptureScreenshot(data => screenshotData = data));

			m_loadingBox.Open("Sending bug report...");

			FeedbackForm.FeedbackType = FeedbackTypeValue;
			FeedbackForm.Email = m_emailField.text;
			FeedbackForm.Subject = m_subjectField.text;
			FeedbackForm.Feedback = m_contentField.text;
			FeedbackForm.Screenshot = screenshotData;
			FeedbackForm.SystemInfo = PrintSystemInfo();

			FeedbackForm.Send(obj =>
			{
				result = obj;
				isWaiting = false;
			});
			
			while(isWaiting)
				yield return null;

			m_loadingBox.Close();

			if(result.Success)
			{
				ShowDialogBox("Your bug report has been sent! Thank you.",
							  DialogBox.ButtonLayout.OK);

				LastUsedEmail = m_emailField.text;
				m_subjectField.text = string.Empty;
				m_contentField.text = string.Empty;
				m_screenshotToggle.isOn = true;
				m_systemInfoToggle.isOn = true;
			}
			else
			{
				Debug.LogError(result.Error);
				ShowDialogBox("Your bug report could not be sent! Please try again.",
							  DialogBox.ButtonLayout.OK);
			}
		}

		private IEnumerator SendEnhancementAsync()
		{
			FeedbackFormResult result = new FeedbackFormResult();
			bool isWaiting = true;

			m_loadingBox.Open("Sending your feedback...");

			FeedbackForm.FeedbackType = FeedbackTypeValue;
			FeedbackForm.Email = m_emailField.text;
			FeedbackForm.Subject = m_subjectField.text;
			FeedbackForm.Feedback = m_contentField.text;
			FeedbackForm.Screenshot = null;
			FeedbackForm.SystemInfo = PrintSystemInfo();

			FeedbackForm.Send(obj =>
			{
				result = obj;
				isWaiting = false;
			});

			while(isWaiting)
				yield return null;

			m_loadingBox.Close();

			if(result.Success)
			{
				ShowDialogBox("Your feedback has been sent! Thank you.",
							  DialogBox.ButtonLayout.OK);

				LastUsedEmail = m_emailField.text;
				m_subjectField.text = string.Empty;
				m_contentField.text = string.Empty;
			}
			else
			{
				Debug.LogError(result.Error);
				ShowDialogBox("Your feedback could not be sent! Please try again.",
							  DialogBox.ButtonLayout.OK);
			}
		}

		private void HandleFeedbackTypeChanged(int valueIndex)
		{
			if(FeedbackTypeValue == FeedbackType.Bug)
			{
				m_screenshotToggle.interactable = true;
				m_screenshotToggle.isOn = true;
				m_systemInfoToggle.interactable = true;
				m_systemInfoToggle.isOn = true;
				m_subjectFieldPlaceholder.text = "Enter a short summary of the issue...";
				m_contentFieldLabel.text = "Tell us, in detail, what happened:";
				m_contentFieldPlaceholder.text = "Include a detailed description of the issue as well as steps to reproduce it...";
			}
			else
			{
				m_screenshotToggle.interactable = false;
				m_screenshotToggle.isOn = false;
				m_systemInfoToggle.interactable = false;
				m_systemInfoToggle.isOn = false;
				m_subjectFieldPlaceholder.text = "Enter a short summary of your feedback...";
				m_contentFieldLabel.text = "How can we improve the game?";
				m_contentFieldPlaceholder.text = "Tell us how we can make the game better...";
			}
		}

		private string PrintSystemInfo()
		{
			return m_systemInfoToggle.isOn ? DebugConsoleUtils.PrintSystemInfo() : "N/A";
		}

		private IEnumerator CaptureScreenshot(UnityAction<byte[]> onDone)
		{
			m_panel.gameObject.SetActive(false);

			Application.CaptureScreenshot(ScreenshotPath);
			yield return null;

			Uri uri = new Uri(AbsoluteScreenshotPath, UriKind.Absolute);
			WWW loadFileWWW = new WWW(uri.AbsoluteUri);
			yield return loadFileWWW;

			if(loadFileWWW.error == null)
			{
				onDone(loadFileWWW.bytes);
			}
			else
			{
				Debug.LogError(loadFileWWW.error);
				onDone(null);
			}

			m_panel.gameObject.SetActive(true);
		}

		private void ShowDialogBox(string message, DialogBox.ButtonLayout buttonLayout)
		{
			m_canvasGroup.interactable = false;
			m_dialogBox.Open(message, buttonLayout, res => m_canvasGroup.interactable = true);
		}

		public void OnBeginWindowDrag(BaseEventData eventData)
		{
		}

		public void OnEndWindowDrag(BaseEventData eventData)
		{
			SaveLayoutChanges();
		}

		public void OnWindowDrag(BaseEventData eventData)
		{
			PointerEventData pointerData = eventData as PointerEventData;
			if(pointerData != null)
			{
				Vector2 position = m_panel.anchoredPosition + pointerData.delta;
				position.x = Mathf.Clamp(position.x, m_padding.x, Screen.width - m_panel.sizeDelta.x - m_padding.x);
				position.y = Mathf.Clamp(position.y, -Screen.height + m_panel.sizeDelta.y + m_padding.y, -m_padding.y);

				m_panel.anchoredPosition = position;
			}
		}

		private void SaveLayoutChanges()
		{
			DebugConsolePrefs.SetVector2("FeedbackUI_Position", m_panel.anchoredPosition);
		}

		private void LoadLayoutChanges()
		{
			Vector2 defPosition;
			defPosition.x = Screen.width / 2 - m_panel.sizeDelta.x / 2;
			defPosition.y = -(Screen.height / 2 - m_panel.sizeDelta.y / 2);

			m_panel.anchoredPosition = DebugConsolePrefs.GetVector2("FeedbackUI_Position", defPosition);
		}

		public void ResetLayout()
		{
			Vector2 defPosition;
			defPosition.x = Screen.width / 2 - m_panel.sizeDelta.x / 2;
			defPosition.y = -(Screen.height / 2 - m_panel.sizeDelta.y / 2);

			m_panel.anchoredPosition = defPosition;
		}
	}
}