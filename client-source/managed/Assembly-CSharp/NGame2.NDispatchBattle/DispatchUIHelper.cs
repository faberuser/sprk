using System;
using NGame2.NBattleContext;
using NGame2.NUI;
using NGame2.NUtil.NDataHelper;
using NGame2.NUtil.NWrappedInfo;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using UnityEngine;

namespace NGame2.NDispatchBattle;

public class DispatchUIHelper
{
	public enum ProgressUIState
	{
		Idle,
		Prepare,
		Battle,
		ReadyToComplete
	}

	public static void UpdateDungeonName(WCL_DispatchBattleInfo info, UILabel labelDungeonName, bool postfixDifficulty = false)
	{
		switch (DispatchHelper.GetContentMode(info))
		{
		case DispatchHelper.ContentMode.Chapter:
			UpdateDungeonNameOnChapter(DungeonHelper.GetEnterDungeonData(info.ChapterIndex, info.DungeonIndex), (NShared.DungeonDifficultyType)info.Difficulty, labelDungeonName);
			break;
		case DispatchHelper.ContentMode.Raid:
			UpdateDungeonNameOnRaid(GameRoot.Instance.RaidDataContainer.GetRaidDataByDungeon(info.ChapterIndex, info.DungeonIndex), labelDungeonName, postfixDifficulty);
			break;
		default:
			UIHelper.SetLabelText(labelDungeonName, "");
			break;
		}
	}

	public static void UpdateDungeonNameOnChapter(EnterDungeonData enterDungeonData, NShared.DungeonDifficultyType difficulty, UILabel labelName, bool postfixDifficulty = false)
	{
		string text = DungeonHelper.GetDungeonName(enterDungeonData);
		if (postfixDifficulty)
		{
			text = text + " " + difficulty;
		}
		UIHelper.SetLabelText(labelName, text);
	}

	public static void UpdateDungeonNameOnRaid(RaidData raidData, UILabel labelName, bool postfixDifficulty = false)
	{
		string text = DungeonHelper.GetDungeonName(raidData);
		if (postfixDifficulty)
		{
			text = text + " " + LocalizationManager.GetStringDirect(raidData.Step_Name);
		}
		UIHelper.SetLabelText(labelName, text);
	}

	public static void UpdateState(UILabel labelState, WCL_DispatchBattleInfo info)
	{
		DispatchState dispatchState = DispatchHelper.GetDispatchState(info);
		UIHelper.SetLabelText(labelState, DispatchHelper.GetDispatchStateString(dispatchState));
	}

	public static void UpdateUIPrepareRandomDescription(WCL_DispatchBattleInfo info, UILabel label = null)
	{
		UIHelper.SetLabelText(label, DispatchHelper.GetPrepareRandomDescription(info));
	}

	public static void UpdateWinRate(WCL_DispatchBattleInfo info, UILabel labelWinRate, float constraintValueOnLow, Color onLow, Color onNormal, bool onlyRate = true)
	{
		if (info != null)
		{
			if (info.State != DispatchState.Battle)
			{
				UIHelper.EnableGameObject(labelWinRate, enable: false);
			}
			else
			{
				UpdateWinRate(info.LastWinCount, info.LastLoseCount, labelWinRate, constraintValueOnLow, onLow, onNormal, onlyRate);
			}
		}
	}

	public static void UpdateWinRate(int winCount, int loseCount, UILabel labelWinRate, float constraintValueOnLow, Color onLow, Color onNormal, bool onlyRate = true)
	{
		float winRate = DispatchHelper.GetWinRate(winCount, loseCount);
		if (winRate < constraintValueOnLow)
		{
			UIHelper.SetLabelColor(labelWinRate, onLow);
		}
		else
		{
			UIHelper.SetLabelColor(labelWinRate, onNormal);
		}
		string text = "";
		string text2 = (winRate * 100f).ToString("N0");
		text = ((!onlyRate) ? LocalizationManager.GetString(DispatchBattle.DISPATCH_COUNT_SUMMARY, winCount, loseCount, text2) : $"{LocalizationManager.GetString(DispatchBattle.DISPATCH_WIN_RATE, text2)}%");
		UIHelper.SetLabelText(labelWinRate, text);
		UIHelper.EnableGameObject(labelWinRate, enable: true);
	}

	public static void UpdateRemainTime(WCL_DispatchBattleInfo info, UILabel labelRemainTime, Enum formattedStr_HH_MM_SS, Enum formattedStr_MM_SS, Enum prefix = null, Enum postfix = null)
	{
		UpdateTimeLabel(DispatchHelper.GetRemainTimeFormattedStr(info, formattedStr_HH_MM_SS, formattedStr_MM_SS), labelRemainTime, formattedStr_HH_MM_SS, formattedStr_MM_SS, prefix, postfix);
	}

	public static void UpdateTotalTime(WCL_DispatchBattleInfo info, UILabel labelRemainTime, Enum formattedStr_HH_MM_SS, Enum formattedStr_MM_SS, Enum prefix = null, Enum postfix = null)
	{
		UpdateTimeLabel(DispatchHelper.GetTotalTimeFormattedStr(info, formattedStr_HH_MM_SS, formattedStr_MM_SS), labelRemainTime, formattedStr_HH_MM_SS, formattedStr_MM_SS, prefix, postfix);
	}

	public static void UpdateTotalTime(TimeSpan totalTime, UILabel labelRemainTime, Enum formattedStr_HH_MM_SS, Enum formattedStr_MM_SS, Enum prefix = null, Enum postfix = null)
	{
		UpdateTimeLabel(DispatchHelper.GetTotalTimeFormattedStr(totalTime, formattedStr_HH_MM_SS, formattedStr_MM_SS), labelRemainTime, formattedStr_HH_MM_SS, formattedStr_MM_SS, prefix, postfix);
	}

	private static void UpdateTimeLabel(string formattedStr, UILabel labelRemainTime, Enum formattedStr_HH_MM, Enum formattedStr_MM_SS, Enum prefix = null, Enum postfix = null)
	{
		if (prefix != null)
		{
			formattedStr = $"{LocalizationManager.GetString(prefix)} {formattedStr}";
		}
		if (postfix != null)
		{
			formattedStr = $"{formattedStr} {LocalizationManager.GetString(postfix)}";
		}
		UIHelper.SetLabelText(labelRemainTime, formattedStr);
	}
}
