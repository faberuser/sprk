using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NRender3D;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class GuildArenaTotalInfo : BaseWindow
{
	public interface Listener
	{
	}

	public const string PrefabPath = "Resources/Prefab2/Window/GuildArena/GuildArenaTotalInfo";

	public UIToggle Toggle_Guide;

	public UIToggle Toggle_SeasonReward;

	public UIToggle Toggle_Contribution;

	public UIToggle Toggle_BuffReward;

	public GameObject GuidePage;

	public GameObject SeasonRankPage;

	public GameObject ContributionPage;

	public GameObject BuffRewardPage;

	public UITable Table_Guilde;

	public UILabel Label_TimeDesc;

	public UIGrid Grid_Contribution;

	public UIScrollGrid Grid_Reward;

	public UIScrollGrid BuffRewardGrid;

	private List<GuildArenaTierRewardComponent> RewardComponents = new List<GuildArenaTierRewardComponent>();

	private List<GuildArenaBuffRewardComponent> _listBuffComponent = new List<GuildArenaBuffRewardComponent>();

	private HeroPreview _heroPreview;

	private bool _showReward;

	private bool IsFirstUpdate;

	private void LateUpdate()
	{
		if (IsFirstUpdate)
		{
			FirstLateUpdate();
			IsFirstUpdate = false;
		}
	}

	protected void FirstLateUpdate()
	{
		if ((bool)Table_Guilde)
		{
			Table_Guilde.repositionNow = true;
		}
		if ((bool)Grid_Contribution)
		{
			Grid_Contribution.repositionNow = true;
		}
	}

	public void Open(Listener listener)
	{
		UIHelper.EnableGameObject(GuidePage, enable: false);
		UIHelper.EnableGameObject(SeasonRankPage, enable: false);
		UIHelper.EnableGameObject(ContributionPage, enable: false);
		Open(delegate
		{
			Init();
			IsFirstUpdate = true;
			Reposition();
		}, null);
	}

	public override void OnClosing()
	{
		base.OnClosing();
		FactoryBase.ReleaseAndClearList(RewardComponents);
		FactoryBase.ReleaseAndClearList(_listBuffComponent);
	}

	public void Init()
	{
		ResetToggle();
		InitTimeDescLabel();
		InitTierRewardPage();
		InitBuffRewardPage();
		OnClickToggle();
	}

	private void ResetToggle()
	{
		Toggle_Guide.Set(state: true);
		Toggle_SeasonReward.Set(state: false);
		Toggle_Contribution.Set(state: false);
		Toggle_BuffReward.Set(state: false);
	}

	public void InitTierRewardPage()
	{
		FactoryBase.ReleaseAndClearList(RewardComponents);
		IList<GuildArenaSeasonRewardData> allData = GameRoot.Instance.GuildArenaSeasonRewardDataContainer.GetAllData();
		for (int num = allData.Count - 1; num >= 0; num--)
		{
			GuildArenaSeasonRewardData seasonRewardData = allData[num];
			GuildArenaTierRewardComponent guildArenaTierRewardComponent = UIHelper.AddGridItem(Grid_Reward, GuildArenaTierRewardComponent.PrefabAuto);
			guildArenaTierRewardComponent.Open(seasonRewardData);
			RewardComponents.Add(guildArenaTierRewardComponent);
		}
		Grid_Reward.RepositionGrid();
		Grid_Reward.ScrollView.ResetPosition();
	}

	public void InitBuffRewardPage()
	{
		FactoryBase.ReleaseAndClearList(_listBuffComponent);
		IList<GuildArenaServerBuffRewardData> allData = GameRoot.Instance.GuildArenaServerBuffRewardDataContainer.GetAllData();
		for (int i = 0; i < allData.Count; i++)
		{
			GuildArenaServerBuffRewardData data = allData[i];
			GuildArenaBuffRewardComponent guildArenaBuffRewardComponent = UIHelper.AddGridItem(BuffRewardGrid, GuildArenaBuffRewardComponent.PrefabAuto);
			guildArenaBuffRewardComponent.Open(data);
			_listBuffComponent.Add(guildArenaBuffRewardComponent);
		}
		UIHelper.RepositionGrid(BuffRewardGrid);
	}

	public void OnClickToggle()
	{
		UIHelper.EnableGameObject(GuidePage, Toggle_Guide.value);
		UIHelper.EnableGameObject(SeasonRankPage, Toggle_SeasonReward.value);
		UIHelper.EnableGameObject(ContributionPage, Toggle_Contribution.value);
		UIHelper.EnableGameObject(BuffRewardPage, Toggle_BuffReward.value);
		Reposition();
	}

	public void InitTimeDescLabel()
	{
		string[] valueArray = GameRoot.Instance.ConstantDataContainer.GetValueArray<string>("GlobalGuildArenaApplyTime");
		string[] valueArray2 = GameRoot.Instance.ConstantDataContainer.GetValueArray<string>("GlobalGuildArenaProgressTime");
		string[] valueArray3 = GameRoot.Instance.ConstantDataContainer.GetValueArray<string>("GlobalGuildArenaApplyTime2");
		string[] valueArray4 = GameRoot.Instance.ConstantDataContainer.GetValueArray<string>("GlobalGuildArenaProgressTime2");
		string text = "-";
		string text2 = "-";
		string text3 = "-";
		string text4 = "-";
		if (valueArray.Length == 4)
		{
			string localTimeRange = GetLocalTimeRange(valueArray[0], valueArray[1], valueArray[2], valueArray[3]);
			text = LocalizationManager.GetStringDirect("PUNITIVE_EXPEDITION_INFO_APPLY_DESC", localTimeRange);
		}
		if (valueArray3.Length == 4)
		{
			string localTimeRange2 = GetLocalTimeRange(valueArray3[0], valueArray3[1], valueArray3[2], valueArray3[3]);
			text3 = LocalizationManager.GetStringDirect("PUNITIVE_EXPEDITION_INFO_APPLY_DESC", localTimeRange2);
		}
		if (valueArray2.Length == 4)
		{
			string localTimeRange3 = GetLocalTimeRange(valueArray2[0], valueArray2[1], valueArray2[2], valueArray2[3]);
			text2 = LocalizationManager.GetStringDirect("PUNITIVE_EXPEDITION_INFO_PLAY_DESC", localTimeRange3);
		}
		if (valueArray4.Length == 4)
		{
			string localTimeRange4 = GetLocalTimeRange(valueArray4[0], valueArray4[1], valueArray4[2], valueArray4[3]);
			text4 = LocalizationManager.GetStringDirect("PUNITIVE_EXPEDITION_INFO_PLAY_DESC", localTimeRange4);
		}
		string text5 = string.Format("{0} : {1}, {2}\n{3} : {4}, {5}", LocalizationManager.GetStringDirect("GUILD_ARENA_APPLY_TITLE"), text, text3, LocalizationManager.GetStringDirect("GUILD_ARENA_PLAY_TITLE"), text2, text4);
		UIHelper.SetLabelText(Label_TimeDesc, text5);
	}

	private string GetLocalTimeRange(string startDowUtc, string startUtc, string endDowUtc, string endUtc)
	{
		DateTime dateTime = MakeUtcTime(startUtc);
		DateTime dateTime2 = MakeUtcTime(endUtc);
		DateTime clientTime = UIHelper.GetClientTime(dateTime);
		DateTime clientTime2 = UIHelper.GetClientTime(dateTime2);
		DayOfWeek localDayOfWeek = GetLocalDayOfWeek(startDowUtc, clientTime, dateTime);
		DayOfWeek localDayOfWeek2 = GetLocalDayOfWeek(endDowUtc, clientTime2, dateTime2);
		string text = LocalizationManager.GetString(NGame2.NUI.Common.GetDayOfWeekKey(localDayOfWeek));
		string text2 = LocalizationManager.GetString(NGame2.NUI.Common.GetDayOfWeekKey(localDayOfWeek2));
		return text + " " + clientTime.ToString("HH:mm:ss") + " ~ " + text2 + " " + clientTime2.ToString("HH:mm:ss");
	}

	private DateTime MakeUtcTime(string utcTime)
	{
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		DateTime dateTime = Convert.ToDateTime(utcTime);
		return new DateTime(currentServerUTCTime.Year, currentServerUTCTime.Month, currentServerUTCTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
	}

	private DayOfWeek GetLocalDayOfWeek(string baseDoW, DateTime localTime, DateTime utcTime)
	{
		DayOfWeek dayOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), baseDoW);
		int diffDay = GetDiffDay(localTime, utcTime);
		if (diffDay < 0)
		{
			dayOfWeek--;
			if (dayOfWeek < DayOfWeek.Sunday)
			{
				dayOfWeek = DayOfWeek.Saturday;
			}
		}
		else if (diffDay > 0)
		{
			dayOfWeek++;
			if (dayOfWeek > DayOfWeek.Saturday)
			{
				dayOfWeek = DayOfWeek.Sunday;
			}
		}
		return dayOfWeek;
	}

	private int GetDiffDay(DateTime t1, DateTime t2)
	{
		DateTime t3 = new DateTime(t1.Year, t1.Month, t1.Day);
		DateTime t4 = new DateTime(t2.Year, t2.Month, t2.Day);
		return DateTime.Compare(t3, t4);
	}

	private void Reposition()
	{
		Table_Guilde.Reposition();
		Grid_Contribution.Reposition();
		Grid_Reward.RepositionGrid();
		Grid_Reward.ScrollView.ResetPosition();
		UIHelper.RepositionGrid(BuffRewardGrid);
	}
}
