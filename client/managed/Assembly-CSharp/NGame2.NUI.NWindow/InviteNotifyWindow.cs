using System;
using System.Collections;
using NGame2.NScene;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.Mission;
using NGame2.NUI.NManager.NPet;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class InviteNotifyWindow : BaseWindow, IGlobalWindow
{
	public float ExpireTime = 10f;

	protected Action<bool> OnAction;

	protected Action OnFailed;

	private const int LoadingWaitTime = 20;

	public float OpenedTime { get; private set; }

	public void OpenInternal()
	{
		base.Open();
		OpenedTime = Time.unscaledTime;
	}

	public void SetOpenedTime(float openedTime)
	{
		OpenedTime = openedTime;
	}

	public void SetCallBack(Action<bool> onAction, Action onFailed = null)
	{
		OnAction = onAction;
		OnFailed = onFailed;
	}

	public void CheckSceneLoading()
	{
		if (GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading)
		{
			UIHelper.EnableGameObject(WindowRoot, enable: false);
			VespaCoroutine.Start(this, coEnable());
		}
	}

	public void CloseCoveredPopup()
	{
		if (GlobalAutoBehaviour<GlobalManagement>.isValidInstance && GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpened<RaidLevelSelect>())
		{
			GlobalAutoBehaviour<GlobalManagement>.instance.GetWindow<RaidLevelSelect>().Close();
		}
		if (SceneAutoBehaviour<NGame2.NUI.NManager.ShakmehDungeonManagement>.isValidInstance && SceneAutoBehaviour<NGame2.NUI.NManager.ShakmehDungeonManagement>.instance.IsOpened<ShakmehBossInfoPopup>())
		{
			SceneAutoBehaviour<NGame2.NUI.NManager.ShakmehDungeonManagement>.instance.GetInstance<ShakmehBossInfoPopup>().Close();
		}
		if (SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager != null)
		{
			SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.CloseMissionWnds();
		}
		if (GlobalAutoBehaviour<PopupManegement>.isValidInstance)
		{
			GlobalAutoBehaviour<PopupManegement>.instance.CloseAll();
		}
		if (SceneAutoBehaviour<PayShopManagement>.isValidInstance)
		{
			SceneAutoBehaviour<PayShopManagement>.instance.CloseAllActiveWindow();
		}
		if (SceneAutoBehaviour<CostumeManagement>.isValidInstance)
		{
			SceneAutoBehaviour<CostumeManagement>.instance.CloseAllActiveWindow();
		}
		if (SceneAutoBehaviour<PetManagement>.isValidInstance)
		{
			SceneAutoBehaviour<PetManagement>.instance.CloseAllActiveWindow();
		}
		if (SceneAutoBehaviour<PunishManagment>.isValidInstance)
		{
			SceneAutoBehaviour<PunishManagment>.instance.CloseAllActiveWindow();
		}
	}

	private IEnumerator coEnable()
	{
		float startTime = Time.unscaledTime;
		while (GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading && !(Time.unscaledTime - startTime > 20f))
		{
			yield return null;
		}
		UIHelper.EnableGameObject(WindowRoot, enable: true);
	}

	public virtual void OnClickAccept()
	{
		CloseCoveredPopup();
	}

	public virtual void OnClickDeny()
	{
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateTime();
	}

	protected virtual void UpdateTime()
	{
		if (Time.unscaledTime - OpenedTime > ExpireTime)
		{
			OnClickDeny();
		}
	}
}
