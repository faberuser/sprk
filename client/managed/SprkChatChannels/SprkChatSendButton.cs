using NGame2.NUI;
using NVespa.NNGUIExtension;
using UnityEngine;

public sealed class SprkChatSendButton : MonoBehaviour
{
	private UIInput input;

	private UIButton send;

	public void Initialize(UIButton share, UIInput chatInput)
	{
		input = chatInput;
		if (send != null)
		{
			Refresh();
			return;
		}
		UIWidget uIWidget = share.GetComponent<UIWidget>();
		if (uIWidget == null)
		{
			uIWidget = share.GetComponentInChildren<UISprite>();
		}
		if (uIWidget == null)
		{
			return;
		}
		int width = uIWidget.width;
		int num = Mathf.Max(48, (width - 8) / 2);
		Vector3 localPosition = share.transform.localPosition;
		GameObject gameObject = Object.Instantiate(share.gameObject, share.transform.parent, worldPositionStays: false);
		gameObject.name = "Button_Send";
		send = gameObject.GetComponent<UIButton>();
		UIButton[] componentsInChildren = gameObject.GetComponentsInChildren<UIButton>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].onClick.Clear();
		}
		UIEventTrigger[] componentsInChildren2 = gameObject.GetComponentsInChildren<UIEventTrigger>(includeInactive: true);
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].onClick.Clear();
		}
		UIEventListener[] componentsInChildren3 = gameObject.GetComponentsInChildren<UIEventListener>(includeInactive: true);
		for (int i = 0; i < componentsInChildren3.Length; i++)
		{
			componentsInChildren3[i].onClick = null;
		}
		Resize(share.gameObject, width, num);
		Resize(gameObject, width, num);
		share.transform.localPosition = localPosition + Vector3.right * ((float)(num + 8) / 2f);
		gameObject.transform.localPosition = localPosition + Vector3.left * ((float)(num + 8) / 2f);
		UILabel[] componentsInChildren4 = gameObject.GetComponentsInChildren<UILabel>(includeInactive: true);
		foreach (UILabel uILabel in componentsInChildren4)
		{
			UILocalize[] components = uILabel.GetComponents<UILocalize>();
			for (int j = 0; j < components.Length; j++)
			{
				components[j].enabled = false;
			}
			UILocalizeEx[] components2 = uILabel.GetComponents<UILocalizeEx>();
			for (int j = 0; j < components2.Length; j++)
			{
				components2[j].enabled = false;
			}
			uILabel.text = SprkText.Get("Send");
		}
		EventDelegate.Add(send.onClick, Send);
		gameObject.SetActive(value: true);
		Refresh();
	}

	private static void Resize(GameObject button, int originalWidth, int width)
	{
		UIWidget[] componentsInChildren = button.GetComponentsInChildren<UIWidget>(includeInactive: true);
		foreach (UIWidget uIWidget in componentsInChildren)
		{
			uIWidget.leftAnchor.target = null;
			uIWidget.rightAnchor.target = null;
			if (uIWidget is UILabel)
			{
				uIWidget.width = Mathf.Max(24, width - 16);
			}
			else if (uIWidget.width >= originalWidth - 20)
			{
				uIWidget.width = width;
			}
		}
		NGUITools.UpdateWidgetCollider(button);
	}

	private bool CanSend()
	{
		if (input != null && input.isActiveAndEnabled)
		{
			return !string.IsNullOrWhiteSpace(input.value);
		}
		return false;
	}

	private void Send()
	{
		if (CanSend())
		{
			input.Submit();
		}
		Refresh();
	}

	private void Refresh()
	{
		if (send != null)
		{
			UIHelper.EnableButton(send, CanSend(), grayscaleIfDisable: true);
		}
	}

	private void LateUpdate()
	{
		Refresh();
	}
}
