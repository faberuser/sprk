using System;
using System.Collections;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGameAsset;
using NGameAsset.NPatch;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NLogin.NState;

public class StateBase_PatchDownload : StateBase
{
	private bool _checkDone;

	private float _startedTime;

	private const float CheckElapsedTime = 5f;

	private const float RequireDownloadExperctTime = 0f;

	public static bool IsReady { get; set; }

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
		_ = (object)null;
		_ = 54;
		_ = 1;
		_ = 0;
		Array.Empty<object>();
		LoginAccountInfo instance = SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginAccountInfo>();
		if (instance != null)
		{
			instance.Close();
		}
		StateOwner.SetNextState(new StateBase_LoadGameAsset());
	}

	public override IEnumerator coRun()
	{
		BackgroundDownloader.IsBackgroundDownload.Value = false;
		BackgroundDownloader.DownloadFileWriteSleepTime.Value = SingletonUpdater<LoginManager>.instance.VersionInfo.DownloadFileWriteSleepTimeMS;
		LoginPatchDownloader patchDownloadWindow = SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginPatchDownloader>();
		StartCoroutine(coOpenAfterFadeIn(patchDownloadWindow));
		AppVersionInfo versionInfo = SingletonUpdater<LoginManager>.instance.VersionInfo;
		PatchDownloader.SetAppVersionInfo(versionInfo);
		bool isCompletePatch = false;
		LoginPatchDownloader.DownloadResultType downloadResult = LoginPatchDownloader.DownloadResultType.DownloadFail;
		if (Define.TestMiniGame.Value)
		{
			LoginMiniGame.OpenPopup(openAtPatchDownload: false);
		}
		patchDownloadWindow.OnProgressChanged = OnProgressChanged;
		patchDownloadWindow.OnMessage = delegate(string msg)
		{
			StateBase.SetBgMessage(msg, animate: true);
		};
		patchDownloadWindow.Init(versionInfo.CdnProxyUrl, delegate(int remoteVersion)
		{
			SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginBackground>().SetPatchVersion(remoteVersion);
			SingletonUpdater<WebServiceManager>.instance.Requester.SetRequestHeader("X-Vespa-KR-DataVersion", remoteVersion.ToString());
		}, delegate(LoginPatchDownloader.DownloadResultType result)
		{
			isCompletePatch = true;
			downloadResult = result;
		});
		SingletonAuto<BundleManager>.CreateInstance();
		while (!isCompletePatch)
		{
			if (!SingletonAuto<BundleManager>.instance.IsLoadShader)
			{
				SingletonAuto<BundleManager>.instance.Load();
			}
			yield return null;
		}
		patchDownloadWindow.Close();
		if (downloadResult != LoginPatchDownloader.DownloadResultType.Success)
		{
			yield return new WaitQuitError(downloadResult);
		}
		SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginBackground>().SetMessage("Completing Downloading.", animate: true);
		IsReady = true;
		SystemOptionData.ShowTitleGameIntroduce = true;
		DownloadComplete();
	}

	private IEnumerator coOpenAfterFadeIn(LoginPatchDownloader patchDownloadWindow)
	{
		LoginBackground loginBackgroundWindow = SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginBackground>();
		yield return new WaitUntil(() => loginBackgroundWindow.IsFadeInComplete());
		patchDownloadWindow.Open();
		_ = (object)null;
		Array.Empty<object>();
	}

	private void OnProgressChanged(float progress)
	{
		if (_startedTime == 0f)
		{
			_startedTime = Time.time;
		}
		CheckOpenKillingTime(progress);
	}

	private void CheckOpenKillingTime(float progress)
	{
		if (_checkDone || LoginMiniGame.MiniGameOpend || _startedTime == 0f)
		{
			return;
		}
		float num = Time.time - _startedTime;
		if (num < 5f || progress < float.Epsilon || !CanOpenKillingTime(progress, num))
		{
			return;
		}
		LoginAccountInfo instance = SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginAccountInfo>();
		if (instance != null)
		{
			instance.Open(null, null, null, null, delegate
			{
				OpenGameIntroduce();
			});
			instance.EnableGameIntroduce(enable: true);
			instance.SetMode(LoginAccountInfo.eMenuMode.PatchDownloading);
		}
		OpenGameIntroduce();
		_checkDone = true;
	}

	private void OpenGameIntroduce()
	{
		LoginIntroduceHero instance = SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginIntroduceHero>();
		if (!(instance != null) || instance.IsOpen())
		{
			return;
		}
		instance.Open(delegate
		{
			LoginAccountInfo instance2 = SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginAccountInfo>();
			if (instance2 != null)
			{
				instance2.EnableGameIntroduce(enable: false);
				instance2.Refresh();
			}
		}, delegate
		{
			LoginAccountInfo instance2 = SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginAccountInfo>();
			if (instance2 != null)
			{
				instance2.EnableGameIntroduce(enable: true);
				instance2.Refresh();
			}
		});
	}

	private bool CanOpenKillingTime(float progress, float elapsedTime)
	{
		float num = progress / elapsedTime;
		return 1f / num > 15f;
	}

	private void DownloadComplete()
	{
		bool flag = false;
		LoginMiniGame loginMiniGame = GlobalAutoBehaviour<PopupManegement>.instance.PopUpExist<LoginMiniGame>();
		if (loginMiniGame != null)
		{
			loginMiniGame.OnDownloadComplete();
			if (loginMiniGame.IsOpen())
			{
				flag = (byte)((flag ? 1u : 0u) | 1u) != 0;
			}
		}
		LoginIntroduceHero instance = SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginIntroduceHero>();
		if (instance != null && instance.IsOpen())
		{
			flag = (byte)((flag ? 1u : 0u) | 1u) != 0;
		}
		LoginIntroduceHeroPopup instance2 = SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginIntroduceHeroPopup>();
		if (instance2 != null && instance2.IsOpen())
		{
			flag = (byte)((flag ? 1u : 0u) | 1u) != 0;
		}
		LoginIntroduceWorldPopup instance3 = SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginIntroduceWorldPopup>();
		if (instance3 != null && instance3.IsOpen())
		{
			flag = (byte)((flag ? 1u : 0u) | 1u) != 0;
		}
		if (flag)
		{
			string stringDirect = LocalizationManager.GetStringDirect("COMMON_NOTIFICATION");
			string stringDirect2 = LocalizationManager.GetStringDirect("KINGSRAID_INTRODUCE_NOTIFICATION");
			MessagePopup.Open(stringDirect, stringDirect2);
		}
	}
}
