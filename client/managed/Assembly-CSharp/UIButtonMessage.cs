using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Button Message (Legacy)")]
public class UIButtonMessage : MonoBehaviour, UICamera.IOnPressListener, UICamera.IOnClickListener, UICamera.IOnDoubleClickListener
{
	public enum Trigger
	{
		OnClick,
		OnMouseOver,
		OnMouseOut,
		OnPress,
		OnRelease,
		OnDoubleClick
	}

	public GameObject target;

	public string functionName;

	public Trigger trigger;

	public bool includeChildren;

	private bool mStarted;

	private void Start()
	{
		mStarted = true;
	}

	private void OnEnable()
	{
		if (mStarted)
		{
			OnHover(UICamera.IsHighlighted(base.gameObject));
		}
	}

	private void OnHover(bool isOver)
	{
		if (base.enabled && ((isOver && trigger == Trigger.OnMouseOver) || (!isOver && trigger == Trigger.OnMouseOut)))
		{
			Send();
		}
	}

	public void OnPress(bool isPressed)
	{
		if (base.enabled && ((isPressed && trigger == Trigger.OnPress) || (!isPressed && trigger == Trigger.OnRelease)))
		{
			Send();
		}
	}

	private void OnSelect(bool isSelected)
	{
		if (base.enabled && (!isSelected || UICamera.currentScheme == UICamera.ControlScheme.Controller))
		{
			OnHover(isSelected);
		}
	}

	public void OnClick()
	{
		if (base.enabled && trigger == Trigger.OnClick)
		{
			Send();
		}
	}

	public void OnDoubleClick()
	{
		if (base.enabled && trigger == Trigger.OnDoubleClick)
		{
			Send();
		}
	}

	private void Send()
	{
		if (string.IsNullOrEmpty(functionName))
		{
			return;
		}
		if (target == null)
		{
			target = base.gameObject;
		}
		if (includeChildren)
		{
			Transform[] componentsInChildren = target.GetComponentsInChildren<Transform>();
			int i = 0;
			for (int num = componentsInChildren.Length; i < num; i++)
			{
				Vespa_Unity_GameObject_Extension_Method.SENDMESSAGE(componentsInChildren[i].gameObject, functionName, base.gameObject, SendMessageOptions.DontRequireReceiver);
			}
		}
		else
		{
			Vespa_Unity_GameObject_Extension_Method.SENDMESSAGE(target, functionName, base.gameObject, SendMessageOptions.DontRequireReceiver);
		}
	}
}
