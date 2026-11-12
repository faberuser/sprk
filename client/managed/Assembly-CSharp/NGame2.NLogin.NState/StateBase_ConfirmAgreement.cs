using System.Collections;
using System.Runtime.CompilerServices;
using NGame2.NUI.NWindow;
using NShared;
using NVespa;

namespace NGame2.NLogin.NState;

public class StateBase_ConfirmAgreement : StateBase
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass6_0
	{
		public LoginBackground loginBackgroundWindow;

		internal bool _003CcoRun_003Eb__0()
		{
			return loginBackgroundWindow.IsFadeInComplete();
		}
	}

	private bool _isIDFAComplete;

	public override void OnBegin()
	{
		base.OnBegin();
		LoadAdjustTokenTable();
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
		StateOwner.SetNextState(new StateBase_Login());
	}

	public override IEnumerator coRun()
	{
		new _003C_003Ec__DisplayClass6_0();
		yield break;
	}

	private void LoadAdjustTokenTable()
	{
		if (!GameRoot.Instance.AdjustTokenDataContainer.IsLoaded)
		{
			VespaDebug.ErrorWithDialog("AdjustTokenDataContainer load failed");
		}
	}

	private void ShowIosIDFA()
	{
	}
}
