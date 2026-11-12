using System;
using System.Collections;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGameAsset;
using NGameAsset.NCreature;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NLogin.NState;

public class StateBase_LoadGameAsset : StateBase
{
	public override void OnBegin()
	{
		base.OnBegin();
		string[] array = new string[2]
		{
			typeof(StateBase_PatchDownload).Name,
			typeof(StateBase_LoadResource).Name
		};
		if (StateBase.SetNecessaryState(array))
		{
			_ = (object)null;
			_ = 5;
			_ = 1;
			_ = 1;
			Array.Empty<object>();
			StartCoroutine(coEntry());
		}
		else
		{
			ErrorAndQuit($"[{typeof(StateBase_LoadGameAsset).Name}] must do first {Vespa_String_Extension_Method.ArrayToString(array)}");
		}
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
		StateOwner.SetNextState(new StateBase_BackgroundDownload());
	}

	public override IEnumerator coRun()
	{
		LoginBackground instance = SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginBackground>();
		if (!StateBase_LoadResource.IsReady)
		{
			instance.SetMessage("Wait for table ready.", animate: true);
		}
		yield return new WaitUntil(() => StateBase_LoadResource.IsReady);
		StateBase.SetBgMessage("Loading Asset.", animate: true);
		bool endBattleTest = false;
		bool needStopBattleTest = false;
		_ = (object)null;
		Array.Empty<object>();
		SingletonUpdater<LoginManager>.instance.BattleTest(delegate
		{
			endBattleTest = true;
		}, () => needStopBattleTest);
		bool readyBundleManager = false;
		_ = (object)null;
		Array.Empty<object>();
		StartCoroutine(coReadyBundleManager(delegate
		{
			readyBundleManager = true;
			needStopBattleTest = true;
			StateBase.SetBgMessage("End LoadAssetBundle", animate: true);
		}));
		StateBase.SetBgMessage("Ready AssetBundle.", animate: true);
		while (!readyBundleManager)
		{
			yield return null;
		}
		StateBase.SetBgMessage("Test Battle.", animate: true);
		while (!endBattleTest)
		{
			yield return null;
		}
		StateBase.SetBgMessage("Ready AssetBundle.", animate: true);
	}

	private IEnumerator coReadyBundleManager(Action onReady)
	{
		SingletonAuto<BundleManager>.CreateInstance();
		SingletonAuto<BundleManager>.instance.OnMessage = delegate(string msg)
		{
			StateBase.SetBgMessage(msg, animate: true);
		};
		while (!SingletonAuto<BundleManager>.instance.IsReady)
		{
			yield return null;
		}
		SingletonAuto<BundleManager>.instance.OnMessage = null;
		WaitingPopup.PreloadWaitingAsset();
		SingletonAuto<ReservePool<AnimationSampleData.SampleFrame>>.instance.ReserveByCount(10, forceReserve: true);
		Vespa_System_Action_Extension_Method.fire(onReady);
	}
}
