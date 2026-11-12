using System;
using NGame2.NUI.NManager;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class UseCouponPopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/UseCouponPopup";

	public UIInput CodeInput;

	public UIButton UseButton;

	private Action<string> _onYes;

	public static UseCouponPopup Open(Action<string> onYes)
	{
		WindowPrefab<UseCouponPopup> windowPrefab = new WindowPrefab<UseCouponPopup>("Resources/Prefab2/Window/MessagePopup/UseCouponPopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(onYes);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
		return windowPrefab.instance;
	}

	protected void OpenInternal(Action<string> onYes)
	{
		base.Open();
		_onYes = onYes;
		CodeInput.value = "";
		UpdateState();
	}

	public void OnClickYes()
	{
		if (!string.IsNullOrEmpty(CodeInput.value))
		{
			Vespa_System_Action_Extension_Method.fire(_onYes, CodeInput.value);
			_onYes = null;
			Close();
		}
	}

	private void UpdateState()
	{
		UIHelper.EnableButton(UseButton, !string.IsNullOrEmpty(CodeInput.value));
	}

	public void OnClickNo()
	{
		Close();
	}

	public void OnSubmit()
	{
		UpdateState();
	}

	public void OnChange()
	{
		UpdateState();
	}

	public override void OnSystemBackButton()
	{
		OnClickNo();
	}
}
