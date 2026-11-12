using System;
using System.Collections;
using NClientData;
using NGame2.NAccount;
using NGame2.NNotification;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGameAsset;
using NShared;
using NShared.NJit;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NLogin.NState;

public class StateBase_LoadResource : StateBase
{
	public static bool IsReady { get; set; }

	private static bool IsLoaded { get; set; }

	public override void OnBegin()
	{
		base.OnBegin();
		StartCoroutine(coEntry());
	}

	public override void OnEnd()
	{
		base.OnEnd();
	}

	public override void ChainUpdate()
	{
		base.ChainUpdate();
	}

	public override void NextState()
	{
		base.NextState();
	}

	public override IEnumerator coRun()
	{
		LoginBackground loginBackground = SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginBackground>();
		if (Define.TableSingleLoad.Value)
		{
			bool isCompleteWait = false;
			GlobalMessagePopup.OpenGlobal("begin TableLoad", string.Empty, delegate
			{
				isCompleteWait = true;
			}, retryButton: false);
			while (!isCompleteWait)
			{
				yield return null;
			}
		}
		else
		{
			bool preloaded = false;
			yield return new WaitUntil(() => StateBase_PatchDownload.IsReady);
			TableRoot.Instance.SetPatchDownLoadComplete(isComplete: true);
			if (!preloaded || SingletonAuto<TableDataManager>.instance.IsLoadAbandoned())
			{
				yield return SingletonAuto<TableDataManager>.instance.WaitAndReset();
				_ = (object)null;
				Array.Empty<object>();
				SingletonAuto<AssetFileManager>.instance.SetPreloadTables("");
				loginBackground.SetMessage("Loading tables.", animate: true);
				SingletonAuto<TableDataManager>.instance.Load(autoRunPostProcess: false);
			}
			yield return SingletonAuto<TableDataManager>.instance.WaitForFinish();
		}
		if (Define.PreloadBundleValue())
		{
			SingletonAuto<BundleManager>.CreateInstance();
		}
		_ = (object)null;
		Array.Empty<object>();
		loginBackground.SetMessage("Loading localization.", animate: true);
		SingletonAuto<LocalizationLoader>.instance.SetEnableLoadFromPatch(enableLoad: true);
		_ = (object)null;
		Array.Empty<object>();
		GameRoot.Instance.OnDataContainerLoaded();
		_ = (object)null;
		Array.Empty<object>();
		_ = (object)null;
		Array.Empty<object>();
		loginBackground.SetMessage("Checking tables.", animate: true);
		string loadError = SingletonAuto<TableDataManager>.instance.LoadError;
		string[] array = new string[1] { loadError };
		bool flag = false;
		string[] array2 = array;
		for (int num = 0; num < array2.Length; num++)
		{
			if (!string.IsNullOrEmpty(array2[num]))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			yield return new WaitQuitError(Vespa_String_Extension_Method.ArrayToString(array));
		}
		if (!Define.PreloadBundleValue())
		{
			SingletonAuto<BundleManager>.CreateInstance();
		}
		GlobalAutoBehaviour<AccountManager>.instance.TableLoadComplete();
		if (AccountManager.IsLogined() && GlobalAutoBehaviour<AccountManager>.instance.UserInfo != null)
		{
			_ = (object)null;
			GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId.ToString();
			if (!SingletonUpdater<LoginManager>.isValidInstance)
			{
				_ = string.Empty;
			}
			else
			{
				_ = SingletonUpdater<LoginManager>.instance.ServerName;
			}
			_ = (object)null;
		}
		_ = (object)null;
		Array.Empty<object>();
		if (SingletonAuto<NotificationManager>.isValidInstance)
		{
			SingletonAuto<NotificationManager>.instance.StartManager();
		}
		IsReady = true;
		IsLoaded = true;
		loginBackground.HideMessage();
		StateOwner.SetSubState(new StateBase_InAppBilling());
	}
}
