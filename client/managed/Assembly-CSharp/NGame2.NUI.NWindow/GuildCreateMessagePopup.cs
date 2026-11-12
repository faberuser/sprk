using System;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class GuildCreateMessagePopup : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/GuildCreateMessagePopup";

	public UILabel Label_Content2;

	public UIButton Button_Yes;

	private Action _onYes;

	public new static void Open(Action onYes)
	{
		WindowPrefab<GuildCreateMessagePopup> windowPrefab = new WindowPrefab<GuildCreateMessagePopup>("Resources/Prefab2/Window/MessagePopup/GuildCreateMessagePopup", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(onYes);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	protected void OpenInternal(Action onYes)
	{
		base.Open();
		_onYes = onYes;
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("GuildCreateGold");
		long num = valueInt;
		UIHelper.SetLabelText(Label_Content2, LocalizationManager.GetStringDirect("CREATE_GUILD_MESSAGE2", Vespa_Int_Extension_Method.ToCommaString(num)));
		UIHelper.EnableButton(Button_Yes, GlobalAutoBehaviour<AccountManager>.instance.UserManager.Gold >= num);
	}

	public override void Reset()
	{
		base.Reset();
	}

	public void OnClickYes()
	{
		Action onYes = _onYes;
		Close();
		Vespa_System_Action_Extension_Method.fire(onYes);
	}

	public void OnClickCancel()
	{
		Close();
	}
}
