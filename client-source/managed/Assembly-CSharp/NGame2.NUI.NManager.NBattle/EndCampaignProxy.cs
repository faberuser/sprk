using System.Collections.Generic;
using NShared;
using NShared.EndCampaign;
using NShared.EndContent;

namespace NGame2.NUI.NManager.NBattle;

public class EndCampaignProxy
{
	public string ForwardHost { get; private set; }

	public ExpResultInfo ExpResult { get; private set; }

	public ExpResultInfo[] ExpResultsByGetHero { get; private set; }

	public HeroExpResultInfo[] HeroExpResults { get; private set; }

	public ItemResultInfo[] ItemResults { get; private set; }

	public EquipItemInfo[] EquipItemInfos { get; private set; }

	public EquipItemBoostInfo[] EquipItemBoostInfos { get; set; }

	public HeroInfo[] HeroInfos { get; private set; }

	public FlaskEndCampaignResultInfo[] FlaskResults { get; private set; }

	public ItemResultInfo[] FlaskItemResults { get; set; }

	public IndexedRewardItemInfo[] GuildRaidBootyItemRewardInfos { get; private set; }

	public GuildRaidDungeonInfo[] GuildRaidInfos { get; private set; }

	public GuildPointResultInfo GuildPointResult { get; set; }

	public WorldBossPointResultInfo[] WorldBossPointResults { get; set; }

	public bool WorldBossKilledByMe { get; set; }

	public PolledRewardInfo WorldBossKilledRewardInfo { get; set; }

	public EventDungeonPointResultInfo[] EventDungeonPointResultInfos { get; set; }

	public EventDungeonPoint2ResultInfo[] EventDungeonPoint2ResultInfos { get; set; }

	public CurrencyResultInfo2[] CurrencyResultInfos { get; set; }

	public CurrencyResultInfo3[] CurrencyResultInfos3 { get; set; }

	public ItemBoosterResultInfo ItemBoosterResultInfo { get; set; }

	public MultiplayMemberResultInfo[] MultiplayMemberResults { get; set; }

	public bool UseStamina { get; set; }

	public CurrencyResultInfo3[] CurrencyResults2 { get; set; }

	public ItemResultInfo[] ItemResults2 { get; set; }

	public EquipItemInfo[] EquipItemInfos2 { get; set; }

	public EquipItemBoostInfo[] EquipItemBoostInfos2 { get; set; }

	public CurrencyResultInfo3[] GuildCurrencyResults { get; set; }

	public string[] SelectedRewardItemCodes { get; set; }

	public long[] MultiplayMemberIds { get; private set; }

	public void OnResponse(NShared.EndContent.Request request, NShared.EndContent.Response response)
	{
		ForwardHost = response.ForwardHost;
		ItemResults = response.ItemResults;
		EquipItemInfos = response.EquipItemInfos;
		CurrencyResultInfos3 = response.CurrencyResults;
	}

	public void OnResponse(NShared.EndCampaign.Request request, NShared.EndCampaign.Response response)
	{
		ForwardHost = response.ForwardHost;
		ExpResult = SprkTeamExpRefresh.BattleExp(response);
		ExpResultsByGetHero = response.ExpResultsByGetHero;
		HeroExpResults = response.HeroExpResults;
		ItemResults = response.ItemResults;
		EquipItemInfos = response.EquipItemInfos;
		EquipItemBoostInfos = response.EquipItemBoostInfos;
		HeroInfos = response.HeroInfos;
		FlaskResults = response.FlaskResults;
		FlaskItemResults = response.FlaskItemResults;
		MultiplayMemberIds = response.MultiplayMemberIds;
		GuildRaidBootyItemRewardInfos = response.GuildRaidBootyItemRewardInfos;
		GuildRaidInfos = response.GuildRaidInfos;
		GuildPointResult = response.GuildPointResult;
		WorldBossKilledByMe = response.WorldBossKilledByMe;
		WorldBossKilledRewardInfo = response.WorldBossKilledRewardInfo;
		CurrencyResultInfos3 = response.CurrencyResults;
		ItemBoosterResultInfo = response.ItemBoosterResult;
		CurrencyResults2 = response.CurrencyResults2;
		ItemResults2 = response.ItemResults2;
		EquipItemInfos2 = response.EquipItemInfos2;
		EquipItemBoostInfos2 = response.EquipItemBoostInfos2;
		GuildCurrencyResults = response.GuildCurrencyResults;
		SelectedRewardItemCodes = response.SelectedRewardItemCodes;
	}

	public void TutorialResponse(HeroExpResultInfo[] heroExpResults, ItemResultInfo[] itemResults, EquipItemInfo[] equipInfos, GoldResultInfo[] goldResults, GemResultInfo[] gemResults, RaidPointResultInfo[] raidPointResults = null)
	{
		HeroExpResults = heroExpResults;
		ItemResults = itemResults;
		EquipItemInfos = equipInfos;
		List<CurrencyResultInfo3> list = new List<CurrencyResultInfo3>();
		if (goldResults != null)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(goldResults); i++)
			{
				CurrencyResultInfo3 item = Create(goldResults[i]);
				list.Add(item);
			}
		}
		if (gemResults != null)
		{
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(gemResults); j++)
			{
				CurrencyResultInfo3 item2 = Create(gemResults[j]);
				list.Add(item2);
			}
		}
		if (raidPointResults != null)
		{
			for (int k = 0; k < __Extension_Method_ICollection__.COUNT(raidPointResults); k++)
			{
				CurrencyResultInfo3 item3 = Create(raidPointResults[k]);
				list.Add(item3);
			}
		}
		CurrencyResultInfos3 = list.ToArray();
	}

	public CurrencyResultInfo3 Create(ICurrencyResultInfo info)
	{
		CurrencyResultInfo3 currencyResultInfo = new CurrencyResultInfo3
		{
			CurrencyType = info.CurrencyType,
			AddValue = info.AddValue,
			NewValue = info.NewValue
		};
		if (info.CurrencyType == CurrencyType.Gem && ((GemResultInfo)info).FirstDiffClear == 1)
		{
			currencyResultInfo.Field1 = "FirstDiffClear";
			currencyResultInfo.Value1 = 1L;
		}
		return currencyResultInfo;
	}
}
