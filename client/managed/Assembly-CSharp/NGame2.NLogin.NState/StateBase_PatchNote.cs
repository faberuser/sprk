using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NLogin.NState;

public class StateBase_PatchNote : StateBase
{
	private bool _isClickPopupOpen;

	public StateBase_PatchNote(bool isClickPopupOpen = false)
	{
		_isClickPopupOpen = isClickPopupOpen;
	}

	public override void OnBegin()
	{
		base.OnBegin();
		string[] array = new string[1] { typeof(StateBase_LoginGameServer).Name };
		if (StateBase.SetNecessaryState(array))
		{
			StartCoroutine(coEntry());
		}
		else
		{
			ErrorAndQuit($"[{typeof(StateBase_PatchNote).Name}] must do first {Vespa_String_Extension_Method.ArrayToString(array)}");
		}
	}

	public override void OnEnd()
	{
		base.OnEnd();
		_isClickPopupOpen = false;
	}

	public override void ChainUpdate()
	{
		base.ChainUpdate();
	}

	public override void NextState()
	{
		base.NextState();
		StateOwner.SetNextState(new StateBase_WaitTouch());
	}

	public override IEnumerator coRun()
	{
		if (!AccountManager.IsLogin())
		{
			yield break;
		}
		if (!_isClickPopupOpen)
		{
			ConstantDataContainer constantDataContainer = SingletonAuto<TableDataManager>.instance.LoadConstantTableInResource();
			if (constantDataContainer == null)
			{
				yield break;
			}
			if (AccountManager.IsLogined())
			{
				int valueInt = constantDataContainer.GetValueInt("LoginPatchNoteCondition");
				if (!GlobalAutoBehaviour<AccountManager>.instance.TutorialManager.IsComplete(valueInt))
				{
					yield break;
				}
			}
		}
		if (!(SceneAutoBehaviour<LoginManagement>.instance.GetPrefab<LoginPatchNotePopup>() == null))
		{
			bool confirmPatchNote = false;
			SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginPatchNotePopup>().Open(accepted: false, delegate
			{
				confirmPatchNote = true;
			});
			while (!confirmPatchNote)
			{
				yield return null;
			}
			SceneAutoBehaviour<LoginManagement>.instance.GetInstance<LoginPatchNotePopup>().Close();
		}
	}
}
