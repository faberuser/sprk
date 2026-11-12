using System.Collections.Generic;
using NGame2.NUI.NManager;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class LabelPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/LabelPopup";

	private static List<WindowPrefab<LabelPopup>> _windowPrefabList = new List<WindowPrefab<LabelPopup>>();

	public UILabel MessageLabel;

	public UIWidget BackgroundWidget;

	public bool CloseOnTouch { get; private set; }

	public static LabelPopup Open(string message = null, bool closeOnTouch = false, float alpha = 0f)
	{
		WindowPrefab<LabelPopup> windowPrefab = new WindowPrefab<LabelPopup>("Resources/Prefab2/Window/MessagePopup/LabelPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(message, closeOnTouch, alpha);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		return windowPrefab.instance;
	}

	protected void OpenInternal(string message, bool closeOnTouch)
	{
		base.Open();
		SetMessage(message);
		CloseOnTouch = closeOnTouch;
	}

	protected void OpenInternal(string message, bool closeOnTouch, float alpha)
	{
		base.Open();
		SetMessage(message);
		UIHelper.SetWidgetAlpha(BackgroundWidget, alpha);
		CloseOnTouch = closeOnTouch;
	}

	public void OnClickArea()
	{
		if (CloseOnTouch)
		{
			Close();
		}
	}

	public void SetMessage(string message)
	{
		UIHelper.SetLabelText(MessageLabel, message);
	}
}
