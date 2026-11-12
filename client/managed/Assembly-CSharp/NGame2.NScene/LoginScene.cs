using System;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUtil;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NScene;

public class LoginScene : BaseScene, IInspectorGuiDrawer
{
	private LoginScenePreloader _preload;

	public override IPreload PreloadAsset()
	{
		_preload = new LoginScenePreloader();
		_preload.Begin();
		return _preload;
	}

	public override void OnSceneLoaded(Action onComplete)
	{
		base.OnSceneLoaded(onComplete);
		GlobalTopmostWindow.CreateInstance();
		WaitingPopup.ReleasePreload();
	}

	public override void OnSceneUnloading(LoadingManager.ESceneType nextScene)
	{
		if (_preload != null)
		{
			_preload.ReleaseAsyncLoadAll();
			_preload = null;
		}
	}

	public override void Update()
	{
		base.Update();
		if (!Input.GetKeyUp(KeyCode.Escape))
		{
			return;
		}
		using (new SystemBackButtonBlock())
		{
			if (!GlobalAutoBehaviour<GlobalManagement>.instance.ExecuteBack())
			{
				ShowQuitPopup();
			}
		}
	}
}
