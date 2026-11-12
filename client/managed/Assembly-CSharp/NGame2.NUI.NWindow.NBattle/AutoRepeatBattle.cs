using System.Collections;
using System.Collections.Generic;
using NGame2.NBattleContext;
using NGame2.NParty;
using NGame2.NScene;
using NGame2.NTutorial;
using NGame2.NUI.NManager.NBattle;
using NShared;
using NShared.NLocalization;
using NShared.NMessage.NProtocol;
using NShared.NParty;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class AutoRepeatBattle : AutoBattle, IGlobalWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Battle/AutoRepeatBattle";

	private static GlobalWindowPrefab<AutoRepeatBattle> _windowPrefab = new GlobalWindowPrefab<AutoRepeatBattle>("Resources/Prefab2/Window/Battle/AutoRepeatBattle", GlobalUIRoot.ELayerType.GlobalUI);

	public UILabel Label_Stop;

	public static bool IsPartyPlay = false;

	private Coroutine CoBattleRepeatOnLose;

	public UILabel RepeatLog;

	[SerializeField]
	private float AutoCampaignDelay = 2.5f;

	public static bool IsBoosterExpand = false;

	private static List<string> _selectedReward = new List<string>();

	public static AutoRepeatBattle instance => _windowPrefab.instance;

	public static bool IsValidInstance => _windowPrefab.IsOpen();

	public static float AutoCampaignDelayTime => instance.AutoCampaignDelay;

	public static void OpenGlobal(bool _IsPartyPlay = false)
	{
		_windowPrefab.instance.OpenInternal();
		IsPartyPlay = _IsPartyPlay;
		_windowPrefab.instance.SetStopButtonLabel();
	}

	public void SetStopButtonLabel()
	{
		UIHelper.SetLabelText(Label_Stop, LocalizationManager.GetStringDirect("REPEATBATTLE_STOP"));
		if (Label_Stop != null && IsPartyPlay)
		{
			MemberInfo myPartyMemberInfo = GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo();
			if (myPartyMemberInfo != null && !myPartyMemberInfo.IsMaster)
			{
				UIHelper.SetLabelText(Label_Stop, LocalizationManager.GetStringDirect("REPEATBATTLE_ON_GOING"));
			}
		}
	}

	public override void Close()
	{
		base.Close();
		ClearBattleRepeatOnLose();
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
		if (SingletonAuto<BattleContext>.isValidInstance)
		{
			int lastBattleChapterIndex = SingletonAuto<BattleContext>.instance.LastBattleChapterIndex;
			int lastBattleDungeonIndex = SingletonAuto<BattleContext>.instance.LastBattleDungeonIndex;
			NShared.DungeonDifficultyType lastBattleDifficulty = SingletonAuto<BattleContext>.instance.LastBattleDifficulty;
			return BattlePartySetting.AutoRepeatable(new EnterDungeonData(lastBattleChapterIndex, lastBattleDungeonIndex, lastBattleDungeonIndex, lastBattleDifficulty));
		}
		return _windowPrefab.instance._repeatEnabled;
	}

	public static void OnSceneChanged()
	{
		VerifyAutoRepeat();
	}

	public static bool VerifyAutoRepeat()
	{
		if (!_windowPrefab.IsOpen())
		{
			return false;
		}
		if (GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType != LoadingManager.ESceneType.Battle2)
		{
			ClearSelectedReward();
			_windowPrefab.instance.ShowRepeateCancelMessage();
			_windowPrefab.ReleaseWindow();
			return false;
		}
		if (!IsEnabled())
		{
			_windowPrefab.ReleaseWindow();
			return false;
		}
		return true;
	}

	public static void SetSelectedReward(List<string> selectedReward)
	{
		_selectedReward.Clear();
		_selectedReward = selectedReward;
	}

	public static List<string> GetSelectedReward()
	{
		return _selectedReward;
	}

	public static string[] GetSelectedRewardToArray()
	{
		return _selectedReward.ToArray();
	}

	public static void ClearSelectedReward()
	{
		_selectedReward.Clear();
	}

	public override void OnBattleLose()
	{
		CoBattleRepeatOnLose = VespaCoroutine.Start(BattleRepeatOnLose(), ClearBattleRepeatOnLose);
	}

	private IEnumerator BattleRepeatOnLose()
	{
		yield return new WaitForSeconds(10f);
		BattlePartySetting.ExtraOptionData extraOptionData = new BattlePartySetting.ExtraOptionData();
		extraOptionData.AutoRepeat = true;
		extraOptionData.IsEndCampaign = true;
		SceneAutoBehaviour<EndCampaign>.instance.BattleRepeat(extraOptionData);
	}

	private void ClearBattleRepeatOnLose()
	{
		if (CoBattleRepeatOnLose != null)
		{
			VespaCoroutine.Stop(CoBattleRepeatOnLose);
			CoBattleRepeatOnLose = null;
		}
	}

	public override void OnClickStop(bool sendRoomRepeat = false, PartyRoomRepeat.ResultType resultType = PartyRoomRepeat.ResultType.NONE)
	{
		if (IsPartyPlay)
		{
			MemberInfo myPartyMemberInfo = GlobalAutoBehaviour<PartyManager>.instance.GetMyPartyMemberInfo();
			if (myPartyMemberInfo == null)
			{
				return;
			}
			if (!sendRoomRepeat && myPartyMemberInfo.IsMaster)
			{
				GlobalAutoBehaviour<PartyManager>.instance.SendRoomRepeat(IsRepeat: false, PartyRoomRepeat.ResultType.CANCEL_MASTER);
				ClearSelectedReward();
			}
			if (!sendRoomRepeat && !myPartyMemberInfo.IsMaster)
			{
				GlobalCenterMessage.Open(LocalizationManager.GetStringDirect("REPEATBATTLE_MULTI_CANCEL_FAIL"));
				return;
			}
			if (sendRoomRepeat)
			{
				GlobalAutoBehaviour<PartyManager>.instance.SendRoomRepeat(IsRepeat: false, resultType);
				ClearSelectedReward();
			}
		}
		base.OnClickStop();
		if (!IsPartyPlay && SingletonAuto<BattleContext>.isValidInstance)
		{
			int lastBattleChapterIndex = SingletonAuto<BattleContext>.instance.LastBattleChapterIndex;
			int lastBattleDungeonIndex = SingletonAuto<BattleContext>.instance.LastBattleDungeonIndex;
			NShared.DungeonDifficultyType lastBattleDifficulty = SingletonAuto<BattleContext>.instance.LastBattleDifficulty;
			EnterDungeonData enterDungeonData = new EnterDungeonData(lastBattleChapterIndex, lastBattleDungeonIndex, lastBattleDungeonIndex, lastBattleDifficulty);
			if (enterDungeonData.LinkedDungeonData.BattleType == BattleType.UnderPrison && SceneAutoBehaviour<EndCampaign>.isValidInstance)
			{
				ClearSelectedReward();
			}
			else if (enterDungeonData.LinkedDungeonData.BattleType == BattleType.Shakmeh && SceneAutoBehaviour<EndCampaign>.isValidInstance)
			{
				ClearSelectedReward();
			}
		}
		ClearBattleRepeatOnLose();
	}

	public void StopAutoRepeatPartyPlay(string cancelMessage, bool showPopup)
	{
		if (IsPartyPlay)
		{
			EnableRepeat(isEnabled: false);
			if (showPopup)
			{
				MessagePopup.Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), cancelMessage);
			}
			else
			{
				GlobalCenterMessage.Open(cancelMessage);
			}
			ClearSelectedReward();
		}
	}

	protected override void ShowRepeateCancelMessage()
	{
		if (IsEnabled())
		{
			base.ShowRepeateCancelMessage();
			GlobalCenterMessage.Open(LocalizationManager.GetString(NShared.NLocalization.AutoRepeatBattle.AUTO_REPEAT_BATTLE_CANCELED));
		}
	}

	protected override void EnableRepeat(bool isEnabled)
	{
		base.EnableRepeat(isEnabled);
		if (!isEnabled)
		{
			_windowPrefab.ReleaseWindow();
		}
	}
}
