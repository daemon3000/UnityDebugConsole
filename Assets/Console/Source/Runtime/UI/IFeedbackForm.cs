using UnityEngine.Events;

namespace Luminosity.Console.UI
{
	public struct FeedbackFormResult
	{
		public bool Success;
		public string Error;

		public FeedbackFormResult(bool success, string error)
		{
			Success = success;
			Error = error;
		}

		public FeedbackFormResult(string error)
		{
			Success = error == null;
			Error = error;
		}
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
