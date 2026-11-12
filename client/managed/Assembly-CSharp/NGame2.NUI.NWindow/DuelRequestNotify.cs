using System;
using NGame2.NMatch;
using NGame2.NMatch.NDuelState;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow.NPayShop;
using NGame2.NUI.NWindow.NSoulWeapon;
using NShared.NMessage.NProtocol;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class DuelRequestNotify : InviteNotifyWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Match/DuelRequestNotify";

	private static GlobalWindowPrefab<DuelRequestNotify> _windowPrefab;

	public UIBundleLoader AvatarBundleTex;

	public UILabel NickLabel;

	public static DuelRequestNotify Open(NShared.NMessage.NProtocol.RequestDuel data, Action<bool> onAction, Action onFailed)
	{
		if (_windowPrefab == null)
		{
			_windowPrefab = new GlobalWindowPrefab<DuelRequestNotify>("Resources/Prefab2/Window/Match/DuelRequestNotify", GlobalUIRoot.ELayerType.GlobalUI);
		}
		_windowPrefab.instance.OpenInternal(data);
		_windowPrefab.instance.SetCallBack(onAction, onFailed);
		return _windowPrefab.instance;
	}

	private void OpenInternal(NShared.NMessage.NProtocol.RequestDuel data)
	{
		OpenInternal();
		string battlePortraitPath = UIAvatarHelper.GetBattlePortraitPath(data.AvatarIndex);
		UIHelper.SetBundleTexture(AvatarBundleTex, battlePortraitPath);
		UIHelper.SetLabelText(NickLabel, $"Lv {data.TeamLevel} {data.Nick}");
		SprkHonorDuel.SetInviteMessage(this, data);
	}

	public override void OnClickAccept()
	{
		if (!SceneAutoBehaviour<MultiPartyManagement>.instance.CheckEnterBattle())
		{
			return;
		}
		if (SceneAutoBehaviour<SoulWeaponManagement>.isValidInstance)
		{
			SoulStoneRestoreWindow instance = SceneAutoBehaviour<SoulWeaponManagement>.instance.GetInstance<SoulStoneRestoreWindow>();
			if (instance != null && instance.IsSoulStoneSelect())
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("SOULWEAPON_RESTORE_BUFF_POPUP"));
				return;
			}
		}
		base.OnClickAccept();
		PayShopMessage3ButtonPopup.ClosePortalMovePopup();
		UIHelper.CloseWindowsBeforeRaidPartySetting();
		OnFailed = null;
		Action<bool> onAction = OnAction;
		_windowPrefab.ReleaseWindow();
		Vespa_System_Action_Extension_Method.fire(onAction, i_argA: true);
	}

	public override void OnClickDeny()
	{
		OnFailed = null;
		Action<bool> onAction = OnAction;
		_windowPrefab.ReleaseWindow();
		Vespa_System_Action_Extension_Method.fire(onAction, i_argA: false);
	}

	protected override void OnDestroy()
	{
		if (OnFailed != null)
		{
			OnFailed();
		}
		OnFailed = null;
		base.OnDestroy();
	}

	protected override void UpdateTime()
	{
		if (Time.unscaledTime - base.OpenedTime > ExpireTime || !(SingletonUpdater<DuelManager>.instance.DuelState is None))
		{
			if (OnFailed != null)
			{
				OnFailed();
			}
			OnFailed = null;
			_windowPrefab.ReleaseWindow();
		}
	}
}
