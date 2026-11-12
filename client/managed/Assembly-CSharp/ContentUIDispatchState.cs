using System;
using System.Collections;
using NGame2.NDispatchBattle;
using NGame2.NUI;
using NGame2.NUtil.NWrappedInfo;
using NShared;
using NShared.NLocalization;
using UnityEngine;

public class ContentUIDispatchState : MonoBehaviour
{
	[Serializable]
	public class DispatchStateColor
	{
		public Color ColorOnStatePrepare;

		public Color ColorOnStateBattle;

		public Color ColorOnStateReadyToComplete;
	}

	public UILabel LabelState;

	public DispatchStateColor DispatchStateColorSet;

	private WCL_DispatchBattleInfo _info;

	private bool _useStateColor;

	private Enum _formattedStr_HH_MM_SS = DispatchBattle.DISPATCH_REMAIN_TIME_HH_MM;

	private Enum _formattedStr_MM_SS = DispatchBattle.DISPATCH_REMAIN_TIME_MM_SS;

	private bool _showWithState = true;

	public void SetData(WCL_DispatchBattleInfo info, bool useStateColor = false)
	{
		_info = info;
		_useStateColor = useStateColor;
	}

	public void SetRemainTimeFormatString(Enum formattedStr_HH_MM_SS, Enum formattedStr_MM_SS, bool showWithState)
	{
		_formattedStr_HH_MM_SS = formattedStr_HH_MM_SS;
		_formattedStr_MM_SS = formattedStr_MM_SS;
		_showWithState = showWithState;
	}

	private void OnEnable()
	{
		VespaCoroutine.Start(this, CoProcessingUpdateState());
	}

	private void UpdateUIStateColor()
	{
		if (_info != null)
		{
			switch (_info.State)
			{
			case DispatchState.Prepare:
			case DispatchState.Simulation:
				UIHelper.SetLabelColor(LabelState, DispatchStateColorSet.ColorOnStatePrepare);
				break;
			case DispatchState.Battle:
				UIHelper.SetLabelColor(LabelState, DispatchStateColorSet.ColorOnStateBattle);
				break;
			case DispatchState.ReadyToComplete:
				UIHelper.SetLabelColor(LabelState, DispatchStateColorSet.ColorOnStateReadyToComplete);
				break;
			}
		}
	}

	private IEnumerator CoProcessingUpdateState()
	{
		WaitForSeconds waitSec = new WaitForSeconds(0.5f);
		WaitUntil waituntil = new WaitUntil(() => _info != null);
		while (true)
		{
			yield return waituntil;
			WCL_DispatchBattleInfo info = _info;
			if (info == null)
			{
				UIHelper.SetLabelText(LabelState, "");
				continue;
			}
			if (_useStateColor)
			{
				UpdateUIStateColor();
			}
			if (info.State == DispatchState.Prepare || info.State == DispatchState.Simulation || info.State == DispatchState.ReadyToComplete)
			{
				UIHelper.SetLabelText(LabelState, DispatchHelper.GetDispatchStateString(info.State));
				yield return waitSec;
			}
			else
			{
				string dispatchStateString = DispatchHelper.GetDispatchStateString(info.State);
				string remainTimeFormattedStr = DispatchHelper.GetRemainTimeFormattedStr(info, _formattedStr_HH_MM_SS, _formattedStr_MM_SS);
				UIHelper.SetLabelText(text: _showWithState ? $"{dispatchStateString}: {remainTimeFormattedStr}" : remainTimeFormattedStr, label: LabelState);
				yield return waitSec;
			}
		}
	}
}
