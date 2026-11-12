using System;
using System.Collections;
using NGame2.NBattleContext;
using NGame2.NTutorial;
using NShared;
using NShared.NLocalization;
using NShared.NMessage.NProtocol;
using NVespa;
using NVespa.NGlobal;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class AutoNextBattle : AutoBattle, IGlobalWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Battle/AutoNextBattle";

	private static GlobalWindowPrefab<AutoNextBattle> _windowPrefab = new GlobalWindowPrefab<AutoNextBattle>("Resources/Prefab2/Window/Battle/AutoNextBattle", GlobalUIRoot.ELayerType.GlobalUI);

	public UILabel Label_Stop;

	public BattleType AutoNextBattleType;

	public NShared.DungeonDifficultyType DungeonDifficulty;

	[SerializeField]
	private int sprkPrefabBoosterCount;

	[NonSerialized]
	public long ItemBoosterCount;

	private Coroutine CoBattleRepeatOnLose;

	[SerializeField]
	private float AutoCampaignDelay = 2.5f;

	public static AutoNextBattle instance => _windowPrefab.instance;

	public static bool IsValidInstance => _windowPrefab.IsOpen();

	public static float AutoCampaignDelayTime => instance.AutoCampaignDelay;

	public static void OpenGlobal()
	{
		_windowPrefab.instance.OpenInternal();
		_windowPrefab.instance.SetStopButtonLabel();
	}

	public static void OpenGlobal(BattleType battleType = BattleType.None, NShared.DungeonDifficultyType difficulty = NShared.DungeonDifficultyType.Easy, long itemBoosterCount = 0)
	{
		_windowPrefab.instance.AutoNextBattleType = battleType;
		_windowPrefab.instance.DungeonDifficulty = difficulty;
		_windowPrefab.instance.ItemBoosterCount = itemBoosterCount;
		OpenGlobal();
	}

	public override void Close()
	{
		base.Close();
		if (CoBattleRepeatOnLose != null)
		{
			VespaCoroutine.Stop(CoBattleRepeatOnLose);
			CoBattleRepeatOnLose = null;
		}
	}

	public void SetStopButtonLabel()
	{
		if (_windowPrefab.instance.AutoNextBattleType == BattleType.Conquest || _windowPrefab.instance.AutoNextBattleType == BattleType.Hideout)
		{
			UIHelper.SetLabelText(Label_Stop, LocalizationManager.GetStringDirect("REPEAT_CONTINUE_BATTLE_END"));
		}
		else
		{
			UIHelper.SetLabelText(Label_Stop, LocalizationManager.GetStringDirect("TOWER_CONTINUOUS_BATTLE_STOP"));
		}
	}

	public static bool IsEnabled()
	{
		if (!_windowPrefab.IsOpen())
		{
			return false;
		}
		if (SingletonUpdater<TutorialManager>.instance.IsProgressing)
		{
			return false;
		}
		return _windowPrefab.instance._repeatEnabled;
	}

	public override void OnBattleLose()
	{
		if (!SprkStoryAuto.DeferOpeningStop(this))
		{
			SprkStoryAuto.Cancelled();
			if (AutoNextBattleType == BattleType.Conquest || AutoNextBattleType == BattleType.Hideout)
			{
				CoBattleRepeatOnLose = VespaCoroutine.Start(BattleRepeatOnLose());
				return;
			}
			base.OnBattleLose();
			string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string stringDirect = LocalizationManager.GetStringDirect("CONTINUOUS_BATTLE_FAIL_LOSE");
			GlobalMessagePopup.OpenGlobal(text, stringDirect);
		}
	}

	private IEnumerator BattleRepeatOnLose()
	{
		yield return new WaitForSeconds(10f);
		SingletonAuto<BattleContext>.instance.RepeatCampaign(null);
		CoBattleRepeatOnLose = null;
	}

	public override void OnClickStop(bool sendRoonRepeat = false, PartyRoomRepeat.ResultType resultType = PartyRoomRepeat.ResultType.NONE)
	{
		if (!SprkStoryAuto.DeferOpeningStop(this))
		{
			SprkStoryAuto.Cancelled();
			base.OnClickStop();
			if (CoBattleRepeatOnLose != null)
			{
				VespaCoroutine.Stop(CoBattleRepeatOnLose);
				CoBattleRepeatOnLose = null;
			}
		}
	}

	public void OnClickStopWithoutMessage()
	{
		if (!SprkStoryAuto.DeferOpeningStop(this))
		{
			SprkStoryAuto.Cancelled();
			EnableRepeat(isEnabled: false);
		}
	}

	protected override void ShowRepeateCancelMessage()
	{
		base.ShowRepeateCancelMessage();
		string empty = string.Empty;
		empty = ((AutoNextBattleType != BattleType.Conquest && AutoNextBattleType != BattleType.Hideout) ? LocalizationManager.GetStringDirect("TOWER_CONTINUOUS_STOP_MESSAGE") : LocalizationManager.GetStringDirect("REPEAT_CONTINUE_BATTLE_END_MESSAGE"));
		GlobalCenterMessage.Open(empty);
	}

	protected override void EnableRepeat(bool isEnabled)
	{
		base.EnableRepeat(isEnabled);
		if (!isEnabled)
		{
			Close();
			Close2(immediately: true);
			_windowPrefab.ReleaseWindow();
		}
	}

	public static void StopWithClickButton()
	{
		if (IsEnabled())
		{
			_windowPrefab.instance.OnClickStop();
		}
	}

	public static void StopWithoutMessage()
	{
		if (IsEnabled())
		{
			_windowPrefab.instance.OnClickStopWithoutMessage();
		}
	}
}
