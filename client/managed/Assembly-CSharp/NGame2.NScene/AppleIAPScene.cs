using System;
using NGame2.NUI.NManager;
using NGame2.NUtil;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NScene;

public class AppleIAPScene : BaseScene, IInspectorGuiDrawer
{
	public static AppleIAPScene instance;

	public override void OnSceneLoaded(Action onComplete)
	{
	}

	private void Awake()
	{
		instance = this;
	}

	private void OnDestroy()
	{
		instance = null;
	}

	private void Start()
	{
	}

	public override void Update()
	{
		base.Update();
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			using (new SystemBackButtonBlock())
			{
				if (GlobalAutoBehaviour<GlobalManagement>.instance.ExecuteBack())
				{
					return;
				}
				ShowQuitPopup();
			}
		}
		ServerChanger.CheckForwardedServer();
	}
}
