using System;
using System.Collections.Generic;

namespace NShared.NParty;

[JsonSerializable]
public class PartyInfo
{
	public int RaidIndex { get; set; }

	public int RaidLevel { get; set; }

	public bool IsRepeatBattle { get; set; }

	public List<MemberInfo> PartyMembers { get; set; }

	public RewardItemInfo[] RewardItems { get; set; }

	public string MasterServerPrivateIp { get; set; }

	public int MasterServerPort { get; set; }

	public DateTime CreatedTime { get; set; }

	public long[] Bannedids { get; set; }

	public int ChapterIndex { get; set; }

	public int DungeonIndex { get; set; }

	public PartyDungeonDifficultyType Difficulty { get; set; }

	public int GuildSuppressDungeonIndex { get; set; }

	public RaidRoomInfo PartyRoom { get; set; }

	public void Reset()
	{
		DestroyRoom();
		PartyMembers.Clear();
		SetRepeatBattle(IsRepeat: false);
		Bannedids = null;
	}

	public bool IsValid()
	{
		if (PartyMembers != null)
		{
			return PartyMembers.Count > 0;
		}
		return false;
	}

	public void DestroyRoom()
	{
		foreach (MemberInfo partyMember in PartyMembers)
		{
			partyMember.SetBattleLeave();
		}
		SetRaid(0, 0);
		SetCampaignInfo(null, 0, null);
		PartyRoom = null;
	}

	public void SetRepeatBattle(bool IsRepeat)
	{
		IsRepeatBattle = IsRepeat;
	}

	public long GetMasterId()
	{
		foreach (MemberInfo partyMember in PartyMembers)
		{
			if (partyMember.IsMaster)
			{
				return partyMember.AccountId;
			}
		}
		return 0L;
	}

	public long[] GetMemberIds()
	{
		List<long> list = new List<long>();
		foreach (MemberInfo partyMember in PartyMembers)
		{
			list.Add(partyMember.AccountId);
		}
		return list.ToArray();
	}

	public void SetRaid(int raidIndex, int raidLevel)
	{
		RaidIndex = raidIndex;
		RaidLevel = raidLevel;
		ChapterIndex = GetChapterIndex();
		DungeonIndex = GetDungeonIndex();
	}

	public void SetMulti(int chapterIndex, int dungeonIndex, PartyDungeonDifficultyType difficulty)
	{
		ChapterIndex = chapterIndex;
		DungeonIndex = dungeonIndex;
		Difficulty = difficulty;
	}

	public int GetChapterIndex()
	{
		if (RaidIndex == 0 && RaidLevel == 0)
		{
			return 0;
		}
		return GameRoot.Instance.RaidDataContainer.GetData(RaidIndex, RaidLevel)?.ChapterIndex ?? 0;
	}

	public int GetDungeonIndex()
	{
		if (RaidIndex == 0 && RaidLevel == 0)
		{
			return 0;
		}
		return GameRoot.Instance.RaidDataContainer.GetData(RaidIndex, RaidLevel)?.DungeonIndex ?? 0;
	}

	public void SetCampaignInfo(string masterServerPrivateIp, int masterServerPort, RewardItemInfo[] rewardItems)
	{
		MasterServerPrivateIp = masterServerPrivateIp;
		MasterServerPort = masterServerPort;
		RewardItems = rewardItems;
	}

	public bool IsBattleStarted()
	{
		if (MasterServerPrivateIp != null)
		{
			return MasterServerPrivateIp.Length > 0;
		}
		return false;
	}

	public void AddBannedId(long bannedId)
	{
		List<long> list = new List<long>();
		if (Bannedids != null && Bannedids.Length != 0)
		{
			for (int i = 0; i < Bannedids.Length; i++)
			{
				list.Add(Bannedids[i]);
			}
		}
		list.Add(bannedId);
		Bannedids = list.ToArray();
	}

	public bool RemoveBannedId(long bannedId)
	{
		if (Bannedids == null)
		{
			return false;
		}
		for (int i = 0; i < Bannedids.Length; i++)
		{
			if (Bannedids[i] == bannedId)
			{
				List<long> list = new List<long>(Bannedids);
				list.Remove(bannedId);
				Bannedids = list.ToArray();
				return true;
			}
		}
		return false;
	}

	public bool IsPublicRoom()
	{
		if (PartyRoom == null)
		{
			return false;
		}
		if (PartyRoom.Opened == 0)
		{
			return false;
		}
		return true;
	}

	public void OnReturnRoom()
	{
		MasterServerPrivateIp = "";
		MasterServerPort = 0;
	}

	public MemberInfo GetPartyMemberInfo(long accountId)
	{
		if (PartyMembers == null || PartyMembers.Count == 0)
		{
			return null;
		}
		return PartyMembers.Find((MemberInfo e) => e.AccountId == accountId);
	}

	public bool IsOverlapHero(int heroIndex)
	{
		foreach (MemberInfo partyMember in PartyMembers)
		{
			if (partyMember != null && partyMember.GetDeckHero(heroIndex) != null)
			{
				return true;
			}
		}
		return false;
	}

	public int GetDeckCount()
	{
		int num = 0;
		foreach (MemberInfo partyMember in PartyMembers)
		{
			if (partyMember != null && partyMember.DeckHeros != null)
			{
				num += partyMember.DeckHeros.Count;
			}
		}
		return num;
	}

	public bool IsRaid()
	{
		if (!IsValid())
		{
			return false;
		}
		if (RaidIndex != 0)
		{
			return true;
		}
		return false;
	}

	public void ResetReady()
	{
		foreach (MemberInfo partyMember in PartyMembers)
		{
			if (partyMember != null)
			{
				partyMember.IsBattleReady = false;
			}
		}
	}

	public int[] GetAffixIndices()
	{
		if (PartyRoom == null)
		{
			return null;
		}
		return PartyRoom.AffixIndices;
	}
}
