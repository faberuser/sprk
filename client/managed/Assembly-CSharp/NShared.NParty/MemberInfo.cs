using System.Collections.Generic;
using NShared.NMessage.NProtocol;
using NVespa;

namespace NShared.NParty;

[JsonSerializable]
public class MemberInfo : CachedUserInfo
{
	public const int ProgressingRewardItemNo = -1;

	public const int AbandonedRewardItemNo = -2;

	public bool IsMaster { get; set; }

	public bool IsBattleReady { get; set; }

	public Dictionary<int, CachedHeroInfo> DeckHeros { get; set; }

	public Dictionary<int, CachedHeroInfo> SubDeckHeros { get; set; }

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public int MemberIndex { get; set; }

	public int SelectedRewardItemNo { get; set; }

	public MemberStatus Status { get; set; }

	public bool UseStamina { get; set; }

	public HeroExpResultInfo[] HeroExpResults { get; set; }

	public FlaskEndCampaignResultInfo[] FlaskResults { get; set; }

	public ItemResultInfo[] FlaskItemResults { get; set; }

	public CurrencyResultInfo3[] CurrencyResults { get; set; }

	public ItemResultInfo[] ItemResults { get; set; }

	public EquipItemInfo[] EquipItemInfos { get; set; }

	public EquipItemBoostInfo[] EquipItemBoostInfos { get; set; }

	public CurrencyResultInfo3[] CurrencyResults2 { get; set; }

	public ItemResultInfo[] ItemResults2 { get; set; }

	public EquipItemInfo[] EquipItemInfos2 { get; set; }

	public EquipItemBoostInfo[] EquipItemBoostInfos2 { get; set; }

	public bool IsReceivedResult { get; set; }

	public bool UseOnetimeBooster { get; set; }

	public MemberInfo()
	{
		DeckHeros = new Dictionary<int, CachedHeroInfo>();
		SubDeckHeros = new Dictionary<int, CachedHeroInfo>();
	}

	public MemberInfo(UserInfo userInfo)
		: this()
	{
		base.AccountId = userInfo.AccountId;
		base.Nick = userInfo.Nick;
		base.AvatarHeroIndex = userInfo.AvatarHeroIndex;
		base.TeamLevel = userInfo.TeamLevel;
		SelectedRewardItemNo = -1;
	}

	public void SetBattleCreate()
	{
	}

	public void SetBattleLeave()
	{
		IsMaster = false;
		IsBattleReady = false;
		Status = MemberStatus.PartyRoom;
		DeckHeros.Clear();
	}

	public CachedHeroInfo SetDeckHeroCostume(int heroIndex, CachedHeroCostumeInfo costumeInfo)
	{
		if (costumeInfo == null)
		{
			return null;
		}
		if (!DeckHeros.ContainsKey(heroIndex))
		{
			VespaDebug.LogError($"SetDeckHeroCostume :: DeckHeros.ContainKey({heroIndex}) == false");
			return null;
		}
		DeckHeros[heroIndex].SetCostumeInfo(costumeInfo);
		return DeckHeros[heroIndex];
	}

	public void SetDeckHero(int heroIndex, CachedHeroInfo heroInfo, bool isMainDeck = true)
	{
		if (heroInfo == null)
		{
			RemoveDeckHero(isMainDeck ? DeckHeros : SubDeckHeros, heroIndex);
		}
		else
		{
			InsertDeckHero(isMainDeck ? DeckHeros : SubDeckHeros, heroIndex, heroInfo);
		}
	}

	private void InsertDeckHero(Dictionary<int, CachedHeroInfo> deckHeros, int heroIndex, CachedHeroInfo heroInfo)
	{
		if (deckHeros.ContainsKey(heroIndex))
		{
			deckHeros[heroIndex] = heroInfo;
		}
		else
		{
			deckHeros.Add(heroIndex, heroInfo);
		}
	}

	private void RemoveDeckHero(Dictionary<int, CachedHeroInfo> deckHeros, int heroIndex)
	{
		deckHeros.Remove(heroIndex);
	}

	public CachedHeroInfo GetDeckHero(int heroIndex)
	{
		if (DeckHeros == null || DeckHeros.Count == 0)
		{
			return null;
		}
		if (DeckHeros.ContainsKey(heroIndex))
		{
			return DeckHeros[heroIndex];
		}
		return null;
	}

	public void SetBattleReady(bool isReady)
	{
		IsBattleReady = isReady;
	}

	public bool DidSelectReward()
	{
		if (SelectedRewardItemNo < 0)
		{
			return SelectedRewardItemNo == -2;
		}
		return true;
	}

	public void OnBattleStart()
	{
		SetBattleReady(isReady: false);
		SelectedRewardItemNo = -1;
	}

	public void SetMemberResults(MemberResultsRes results)
	{
		IsReceivedResult = true;
		UseStamina = results.UseStamina;
		HeroExpResults = results.HeroExpResults;
		FlaskResults = results.FlaskResults;
		FlaskItemResults = results.FlaskItemResults;
		CurrencyResults = results.CurrencyResults;
		ItemResults = results.ItemResults;
		EquipItemInfos = results.EquipItemInfos;
		EquipItemBoostInfos = results.EquipItemBoostInfos;
		CurrencyResults2 = results.CurrencyResults2;
		ItemResults2 = results.ItemResults2;
		EquipItemInfos2 = results.EquipItemInfos2;
		EquipItemBoostInfos2 = results.EquipItemBoostInfos2;
	}
}
