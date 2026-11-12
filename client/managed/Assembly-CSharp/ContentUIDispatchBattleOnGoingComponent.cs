using System;
using NGame2.NAccount;
using NGame2.NDispatchBattle;
using NGame2.NUI;
using NGame2.NUI.NWindow;
using NGame2.NUtil.NDataHelper;
using NGame2.NUtil.NWrappedInfo;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

public class ContentUIDispatchBattleOnGoingComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate, IUserObserver<UserDispatchBattleInfoManager.Observer.BroadCastInfo>, IUserObserver
{
	public interface IListener
	{
		void OnClick(ContentUIDispatchBattleOnGoingComponent comp);
	}

	[Serializable]
	public class WinRateColor
	{
		public Color ColorOnNormalWinRate;

		public Color ColorOnLowWinRate;
	}

	public const string PATH_PREFAB = "Resources/Prefab2/Window/DispatchBattle/ContentUIDispatchBattleOnGoingComponent";

	public UIBundleLoader BundleLoaderMissionImg;

	public UILabel LabelMissionName;

	public UILabel LabelWinRate;

	public UILabel LabelState;

	public WinRateColor WinRateColorSet;

	public ContentUIDispatchState StateUpdater;

	public ContentUIDispatchWinLoseTable winLoseTable;

	private IListener _listener;

	private WCL_DispatchBattleInfo _info;

	public WCL_DispatchBattleInfo Info => _info;

	public void Open(WCL_DispatchBattleInfo info, IListener listener)
	{
		_listener = listener;
		SetData(info);
		UpdateUI();
	}

	public void OnClick()
	{
		if (_info == null)
		{
			return;
		}
		switch (DispatchHelper.GetDispatchState(_info))
		{
		case DispatchState.Prepare:
		case DispatchState.Battle:
		case DispatchState.Simulation:
			switch (DispatchHelper.GetDispatchType(_info))
			{
			case DispatchType.Chapter:
				ContentUIDispatchBattle.OpenGlobalChapter(DungeonHelper.GetEnterDungeonData(_info.ChapterIndex, _info.DungeonIndex), _info.DeckIndex);
				break;
			case DispatchType.RedDragon:
			case DispatchType.BlueDragon:
			case DispatchType.GreenDragon:
			case DispatchType.BlackDragon:
			case DispatchType.RedDragon_Hard:
			case DispatchType.BlueDragon_Hard:
			case DispatchType.GreenDragon_Hard:
			case DispatchType.BlackDragon_Hard:
			case DispatchType.Infira:
			case DispatchType.ImperialArmy:
			case DispatchType.Kibeord:
			case DispatchType.BlackKnight:
			case DispatchType.Gargoria:
			case DispatchType.Seigfried:
			case DispatchType.Ascalon:
			{
				RaidData raidDataMultiVersionWithSingle = RaidHelper.GetRaidDataMultiVersionWithSingle(_info.ChapterIndex, _info.DungeonIndex);
				if (raidDataMultiVersionWithSingle == null)
				{
					return;
				}
				ContentUIDispatchBattle.OpenGlobalRaid(raidDataMultiVersionWithSingle, _info.DeckIndex);
				break;
			}
			}
			break;
		case DispatchState.ReadyToComplete:
			DispatchHelper.SendRQRequestCompleteDispatch(_info.SlotIndex, delegate(WCL_DispatchBattleInfo info)
			{
				if (info != null)
				{
					PopupUIDispatchBattleResult.OpenGlobal(info, DispatchHelper.GetPopupOpenData(info), null);
				}
			});
			break;
		}
		if (_listener != null)
		{
			_listener.OnClick(this);
		}
	}

	private void SetData(WCL_DispatchBattleInfo info)
	{
		_info = info;
		if (StateUpdater != null)
		{
			StateUpdater.SetData(_info, useStateColor: true);
		}
		if (winLoseTable != null)
		{
			winLoseTable.SetData(_info, showOnlyRate: true);
		}
	}

	private void UpdateUI()
	{
		if (_info != null)
		{
			UIHelper.SetDungeonPortrait(this, BundleLoaderMissionImg, _info.ChapterIndex, _info.DungeonIndex);
			DispatchUIHelper.UpdateWinRate(_info, LabelWinRate, 0.4f, WinRateColorSet.ColorOnLowWinRate, WinRateColorSet.ColorOnNormalWinRate);
			DispatchUIHelper.UpdateDungeonName(_info, LabelMissionName, postfixDifficulty: true);
		}
	}

	void IUserObserver<UserDispatchBattleInfoManager.Observer.BroadCastInfo>.OnChanged(UserDispatchBattleInfoManager.Observer.BroadCastInfo noti)
	{
		if (noti.Reason == UserDispatchBattleInfoManager.Observer.BroadCastReason.OnChangeState && DispatchHelper.IsSameInfo(_info, noti.Info))
		{
			SetData(noti.Info);
			UpdateUI();
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager != null)
		{
			GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager.ObserverManager.AddObserver(this);
		}
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager != null)
		{
			GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager.ObserverManager.RemoveObserver(this);
		}
	}
}
