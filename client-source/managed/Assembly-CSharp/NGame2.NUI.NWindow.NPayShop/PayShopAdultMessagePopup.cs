using System;
using NGame2.NUI.NManager;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NPayShop;

public class PayShopAdultMessagePopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/PayShop/PayShopAdultMessagePopup";

	private Action<bool> _callback;

	public static void Open(Action<bool> callback)
	{
		WindowPrefab<PayShopAdultMessagePopup> windowPrefab = new WindowPrefab<PayShopAdultMessagePopup>("Resources/Prefab2/Window/PayShop/PayShopAdultMessagePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(callback);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	protected void OpenInternal(Action<bool> callback)
	{
		base.Open();
		_callback = callback;
	}

	public override void Reset()
	{
		base.Reset();
		_callback = null;
	}

	public override void OnSystemBackButton()
	{
		OnClickClose();
	}

	public void OnClickClose()
	{
		Vespa_System_Action_Extension_Method.fire(_callback, i_argA: false);
		Close();
	}

	public void OnClickYes()
	{
		Vespa_System_Action_Extension_Method.fire(_callback, i_argA: true);
		Close();
	}
}
