using System;
using NGame2.NUI.NManager;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class PreserveMessagePopup : BaseWindow
{
	[Flags]
	public enum Flags
	{
		None = 0,
		EnableRuneRemoveButton = 1,
		EnableRunePreserveButton = 2,
		SkipPopup = 4,
		AttachEventSprite = 8
	}

	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/PreserveMessagePopup";

	public UILabel Label_Content;

	public UILabel Label_Yes;

	public UIButton Button_RemoveRune;

	public UIButton Button_PreserveRune;

	public UISprite Sprite_Event;

	private Action<bool> _onYes;

	private Action _onCancel;

	public static void Open(string content, string yes, Action<bool> onYes, Action onCancel = null, Flags flags = Flags.None)
	{
		if ((flags & Flags.SkipPopup) != Flags.None)
		{
			Vespa_System_Action_Extension_Method.fire(onYes, i_argA: false);
			return;
		}
		WindowPrefab<PreserveMessagePopup> windowPrefab = new WindowPrefab<PreserveMessagePopup>("Resources/Prefab2/Window/MessagePopup/PreserveMessagePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(content, yes, onYes, onCancel, flags);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	protected void OpenInternal(string content, string yes, Action<bool> onYes, Action onCancel, Flags flags)
	{
		base.Open();
		_onYes = onYes;
		_onCancel = onCancel;
		UIHelper.SetLabelText(Label_Content, content);
		UIHelper.SetLabelText(Label_Yes, yes);
		UIHelper.EnableButton(Button_RemoveRune, (flags & Flags.EnableRuneRemoveButton) != 0);
		UIHelper.EnableButton(Button_PreserveRune, (flags & Flags.EnableRunePreserveButton) != 0);
		UIHelper.EnableGameObject(Sprite_Event, (flags & Flags.AttachEventSprite) != 0);
	}

	public override void Reset()
	{
		base.Reset();
	}

	public override void OnSystemBackButton()
	{
		OnClickCancel();
	}

	public void OnClickYes()
	{
		Action<bool> onYes = _onYes;
		Close();
		Vespa_System_Action_Extension_Method.fire(onYes, i_argA: false);
	}

	public void OnClickPreserve()
	{
		Action<bool> onYes = _onYes;
		Close();
		Vespa_System_Action_Extension_Method.fire(onYes, i_argA: true);
	}

	public void OnClickCancel()
	{
		Action onCancel = _onCancel;
		Close();
		Vespa_System_Action_Extension_Method.fire(onCancel);
	}
}
