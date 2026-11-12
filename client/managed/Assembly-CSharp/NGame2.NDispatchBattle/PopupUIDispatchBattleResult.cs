using System;
using NGame2.NBattleContext;
using NGame2.NUI;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUtil.NDataHelper;
using NGame2.NUtil.NWrappedInfo;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NDispatchBattle;

public class PopupUIDispatchBattleResult : BaseRewardPopup
{
	public const string PREFAB_PATH = "Resources/Prefab2/Window/DispatchBattle/PopupUIDispatchBattleResult";

	public UIPanel RootPanel;

	public UILabel LabelDungeonLocation;

	public UILabel LabelTime;

	public UILabel LabelWinCount;

	public UILabel LabelLoseCount;

	public UILabel LabelNoReward;

	public float ItemRewardFxDelayTime = 2f;

	private DispatchHelper.ContentMode _mode = DispatchHelper.ContentMode.Chapter;

	private WCL_DispatchBattleInfo _info;

	private EnterDungeonData _enterDungeonData;

	private RaidData _raidData;

	private Action _onClose;

	public static void OpenGlobal(WCL_DispatchBattleInfo info, PopupOpenData popupOpenData, Action onClose)
	{
		WindowPrefab<PopupUIDispatchBattleResult> windowPrefab = new WindowPrefab<PopupUIDispatchBattleResult>("Resources/Prefab2/Window/DispatchBattle/PopupUIDispatchBattleResult", GameUIRoot.ELayerType.UI);
		if (windowPrefab != null)
		{
			GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
			windowPrefab.instance.Open(info, popupOpenData, onClose);
		}
	}

	public void Open(WCL_DispatchBattleInfo info, PopupOpenData popupOpenData, Action onClose)
	{
		_info = info;
		_onClose = onClose;
		_popupOpen = popupOpenData;
		ChangeMode(DispatchHelper.GetContentMode(_info));
		OpenWithData(_popupOpen, isGlobal: false, _onClose, isOpenWithNoReward: true);
	}

	public void OnClickGetReward()
	{
		Close();
	}

	private void ChangeMode(DispatchHelper.ContentMode mode)
	{
		_mode = mode;
		switch (_mode)
		{
		case DispatchHelper.ContentMode.Chapter:
			_enterDungeonData = DungeonHelper.GetEnterDungeonData(_info.ChapterIndex, _info.DungeonIndex);
			break;
		case DispatchHelper.ContentMode.Raid:
			_raidData = GameRoot.Instance.RaidDataContainer.GetRaidDataByDungeon(_info.ChapterIndex, _info.DungeonIndex);
			break;
		default:
			VespaDebug.LogError($"PopupUIDispatchBattleOnGoing::ChangeMode - Incorrect data : content mode: {_mode.ToString()}");
			Close();
			break;
		}
		UpdateUI();
	}

	private void UpdateUI()
	{
		UpdateUILocation();
		UpdateUITime();
		UpdateUIWinCount();
		UpdateUILoseCount();
		UIHelper.EnableGameObject(LabelNoReward, _popupOpen.CalcRewardCount() == 0);
	}

	private void UpdateUILocation()
	{
		DispatchUIHelper.UpdateDungeonName(_info, LabelDungeonLocation, postfixDifficulty: true);
	}

	private void UpdateUITime()
	{
		long num = NGame2.NUI.NWindow.Common.GetUTCNow().Ticks - _info.ConvBeginTime.Ticks;
		long num2 = _info.ConvCompleteTime.Ticks - _info.ConvBeginTime.Ticks;
		if (num > num2)
		{
			num = num2;
		}
		DispatchUIHelper.UpdateTotalTime(new TimeSpan(num), LabelTime, DispatchBattle.DISPATCH_REMAIN_TIME_HH_MM_SS, DispatchBattle.DISPATCH_REMAIN_TIME_MM_SS);
	}

	private void UpdateUIWinCount()
	{
		string text = "0";
		if (_info != null)
		{
			text = _info.LastWinCount.ToString();
		}
		UIHelper.SetLabelText(LabelWinCount, text);
	}

	private void UpdateUILoseCount()
	{
		string text = "0";
		if (_info != null)
		{
			text = _info.LastLoseCount.ToString();
		}
		UIHelper.SetLabelText(LabelLoseCount, text);
	}
}
