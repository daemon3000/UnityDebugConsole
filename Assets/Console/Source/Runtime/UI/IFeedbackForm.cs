using UnityEngine.Events;

namespace Luminosity.Debug.UI
{
	public struct FeedbackFormResult
	{
		public bool Success;
		public string Error;
	}

	public interface IFeedbackForm
	{
		FeedbackType FeedbackType { get; set; }
		string Email { get; set; }
		string Subject { get; set; }
		string Feedback { get; set; }
		string SystemInfo { get; set; }
		byte[] Screenshot { get; set; }

		void Send(UnityAction<FeedbackFormResult> onDone);
	}
}
