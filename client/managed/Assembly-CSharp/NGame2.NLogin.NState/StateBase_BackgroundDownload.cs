using System.Collections;
using NGame2.NEventTrigger;
using NGame2.NScene;
using NShared.NEvent;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NLogin.NState;

public class StateBase_BackgroundDownload : StateBase
{
	public override void OnBegin()
	{
		base.OnBegin();
		string[] array = new string[1] { typeof(StateBase_PatchDownload).Name };
		if (StateBase.SetNecessaryState(array))
		{
			StartCoroutine(coEntry());
		}
		else
		{
			ErrorAndQuit($"[{typeof(StateBase_BackgroundDownload).Name}] must do first {Vespa_String_Extension_Method.ArrayToString(array)}");
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
		if (!SingletonAuto<EventTriggerManager>.instance.RaiseEvent(new Login()))
		{
			GlobalAutoBehaviour<LoadingManager>.instance.MoveScene(LoadingManager.ESceneType.Lobby);
		}
		StateOwner.SetNextState(new StateBase_Logined());
	}

	public override IEnumerator coRun()
	{
		StateBase.SetBgMessage("Waiting InApp Check", animate: true);
		while (!StateBase_InAppBilling.IsReady)
		{
			yield return null;
		}
	}
}
