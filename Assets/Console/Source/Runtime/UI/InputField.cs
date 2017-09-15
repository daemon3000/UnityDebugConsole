using UnityEngine.EventSystems;

namespace Luminosity.Console.UI
{
	public class InputField : UnityEngine.UI.InputField
	{
		public bool IsDeselecting { get; private set; }
		public bool IsBeingDisabled { get; private set; }

		public override void OnDeselect(BaseEventData eventData)
		{
			IsDeselecting = true;
			base.OnDeselect(eventData);
			IsDeselecting = false;
		}

		protected override void OnDisable()
		{
			IsBeingDisabled = true;
			base.OnDisable();
			IsBeingDisabled = false;
		}
	}
}
