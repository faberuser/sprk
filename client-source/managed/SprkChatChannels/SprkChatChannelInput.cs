using UnityEngine;

public sealed class SprkChatChannelInput : MonoBehaviour
{
	private static readonly KeyCode[] EditingKeys = new KeyCode[12]
	{
		KeyCode.Backspace,
		KeyCode.Delete,
		KeyCode.LeftArrow,
		KeyCode.RightArrow,
		KeyCode.UpArrow,
		KeyCode.DownArrow,
		KeyCode.Home,
		KeyCode.End,
		KeyCode.A,
		KeyCode.C,
		KeyCode.V,
		KeyCode.X
	};

	private UIInput input;

	private void Awake()
	{
		input = GetComponent<UIInput>();
	}

	private void Update()
	{
		if (input == null || !input.isActiveAndEnabled || !input.isSelected)
		{
			return;
		}
		EventModifiers eventModifiers = EventModifiers.None;
		if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
		{
			eventModifiers |= EventModifiers.Control;
		}
		if (Input.GetKey(KeyCode.LeftMeta) || Input.GetKey(KeyCode.RightMeta))
		{
			eventModifiers |= EventModifiers.Command;
		}
		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		{
			eventModifiers |= EventModifiers.Shift;
		}
		if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
		{
			eventModifiers |= EventModifiers.Alt;
		}
		KeyCode[] editingKeys = EditingKeys;
		foreach (KeyCode keyCode in editingKeys)
		{
			if (Input.GetKeyDown(keyCode))
			{
				input.ProcessEvent(new Event
				{
					type = EventType.KeyDown,
					keyCode = keyCode,
					modifiers = eventModifiers
				});
			}
		}
	}
}
