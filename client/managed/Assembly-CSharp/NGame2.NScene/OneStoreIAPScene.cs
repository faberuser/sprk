using System;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NGame2.NUtil;
using NVespa;
using NVespa.NGui;
using NVespa.NScene;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NScene;

public class OneStoreIAPScene : BaseScene, IInspectorGuiDrawer
{
	public static OneStoreIAPScene instance;

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
		SceneAutoBehaviour<GuiManager>.instance.Add("OneStoreIAPScene", this, DrawGui);
	}

	private void DrawGui()
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return;
		}
		using (new BlockManager().Vertical())
		{
		}
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
