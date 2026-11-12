using System.Collections;
using System.Runtime.CompilerServices;

namespace NGame2.NLogin.NState;

public class StateBase_InAppBilling : StateBase
{
	private enum InAppBillingState
	{
		Success,
		Failed,
		DetailScene
	}

	private InAppBillingState _checkState;

	public static bool IsReady { get; set; }

	public override void OnBegin()
	{
		base.OnBegin();
		string[] array = new string[1] { typeof(StateBase_LoadResource).Name };
		if (StateBase.SetNecessaryState(array))
		{
			StartCoroutine(coEntry());
		}
		else
		{
			ErrorAndQuit($"[{typeof(StateBase_InAppBilling).Name}] must do first {Vespa_String_Extension_Method.ArrayToString(array)}");
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
	}

	public override void EndSubState()
	{
		base.EndSubState();
		if (_checkState != InAppBillingState.DetailScene)
		{
			IsReady = true;
		}
	}

	public override IEnumerator coRun()
	{
		_checkState = InAppBillingState.Success;
		IsReady = true;
		return ((IEnumerable)new object[0]).GetEnumerator();
	}

	public static void MoveIAPScene(string callLog = "")
	{
	}
}
