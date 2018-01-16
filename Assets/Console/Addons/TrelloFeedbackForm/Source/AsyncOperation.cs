using UnityEngine;
using UnityEngine.Events;

namespace Luminosity.Trello
{
	public interface IAsyncOperation
	{
		event UnityAction<IAsyncOperation> Finished;
		bool IsRunning { get; }
		void Finish();
	}

	public class AsyncOperation : CustomYieldInstruction, IAsyncOperation
	{
		public event UnityAction<IAsyncOperation> Finished;
		public bool IsRunning { get; protected set; }
	
		public override bool keepWaiting
		{
			get { return IsRunning; }
		}

		public AsyncOperation()
		{
			IsRunning = true;
		}

		public void Finish()
		{
			if(IsRunning)
			{
				IsRunning = false;
				RaiseFinishedEvent();
			}
		}

		protected void RaiseFinishedEvent()
		{
			if(Finished != null)
				Finished(this);
		}
	}

	public class AsyncOperation<V> : AsyncOperation
	{
		public V Value { get; set; }

		public void Finish(V value)
		{
			if(IsRunning)
			{
				IsRunning = false;
				Value = value;
				RaiseFinishedEvent();
			}
		}
	}
}