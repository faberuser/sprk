using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.BeginCampaign;

public static class JM_NShared_BeginCampaign_Request
{
	public static NShared.BeginCampaign.Request Parse(string s)
	{
		return Parse(s, new NShared.BeginCampaign.Request());
	}

	public static NShared.BeginCampaign.Request Parse(string s, NShared.BeginCampaign.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.BeginCampaign.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.BeginCampaign.Request());
	}

	public static NShared.BeginCampaign.Request Parse(IDictionary d, NShared.BeginCampaign.Request o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.LinkedDungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "LinkedDungeonIndex");
		o.DungeonDifficulty = BaseJsonMarshaler.ParseValueInt(d, "DungeonDifficulty");
		o.ScenarioDungeon = BaseJsonMarshaler.ParseValueBool(d, "ScenarioDungeon");
		o.HeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "HeroIndices", BaseJsonMarshaler.CastToInt);
		o.GroupHeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "GroupHeroIndices", BaseJsonMarshaler.CastToInt);
		o.AiHeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "AiHeroIndices", BaseJsonMarshaler.CastToInt);
		o.LeaderHeroIndex = BaseJsonMarshaler.ParseValueInt(d, "LeaderHeroIndex");
		o.AccountIds = BaseJsonMarshaler.ParseArrayCast(d, "AccountIds", BaseJsonMarshaler.CastToLong);
		o.Repeat = BaseJsonMarshaler.ParseValueBool(d, "Repeat");
		o.BuyKey = BaseJsonMarshaler.ParseValueInt(d, "BuyKey");
		o.WorldMapEventIndex = BaseJsonMarshaler.ParseValueInt(d, "WorldMapEventIndex");
		o.WorldMapNodeIndex = BaseJsonMarshaler.ParseValueInt(d, "WorldMapNodeIndex");
		o.MultiplayMasterId = BaseJsonMarshaler.ParseValueLong(d, "MultiplayMasterId");
		o.MultiplayMemberIds = BaseJsonMarshaler.ParseArrayCast(d, "MultiplayMemberIds", BaseJsonMarshaler.CastToLong);
		o.MultiplayObserverIds = BaseJsonMarshaler.ParseArrayCast(d, "MultiplayObserverIds", BaseJsonMarshaler.CastToLong);
		o.MultiplayMasterServerIp = BaseJsonMarshaler.ParseValueString(d, "MultiplayMasterServerIp");
		o.MultiplayMasterServerPort = BaseJsonMarshaler.ParseValueInt(d, "MultiplayMasterServerPort");
		o.TowerIndex = BaseJsonMarshaler.ParseValueInt(d, "TowerIndex");
		o.TowerFloor = BaseJsonMarshaler.ParseValueInt(d, "TowerFloor");
		o.RaidIndex = BaseJsonMarshaler.ParseValueInt(d, "RaidIndex");
		o.RaidLevel = BaseJsonMarshaler.ParseValueInt(d, "RaidLevel");
		o.WorldBossIndex = BaseJsonMarshaler.ParseValueInt(d, "WorldBossIndex");
		o.EventWorldBossIndex = BaseJsonMarshaler.ParseValueInt(d, "EventWorldBossIndex");
		o.EventWorldBossLevel = BaseJsonMarshaler.ParseValueInt(d, "EventWorldBossLevel");
		o.RaidRoomNo = BaseJsonMarshaler.ParseValueLong(d, "RaidRoomNo");
		o.MultiRoomNo = BaseJsonMarshaler.ParseValueLong(d, "MultiRoomNo");
		o.ItemBoosterCount = BaseJsonMarshaler.ParseValueLong(d, "ItemBoosterCount");
		o.UseStamina = BaseJsonMarshaler.ParseValueBool(d, "UseStamina");
		o.SelectedRewardItemCodes = BaseJsonMarshaler.ParseArrayCast(d, "SelectedRewardItemCodes", BaseJsonMarshaler.CastToString);
		o.OnlineGameSpeedRatio = BaseJsonMarshaler.ParseValueInt(d, "OnlineGameSpeedRatio");
		o.EnterTicketCount = BaseJsonMarshaler.ParseValueInt(d, "EnterTicketCount");
		o.DeltaHeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "DeltaHeroIndices", BaseJsonMarshaler.CastToInt);
		o.ZetaHeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "ZetaHeroIndices", BaseJsonMarshaler.CastToInt);
		o.AffixIndices = BaseJsonMarshaler.ParseArrayCast(d, "AffixIndices", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.BeginCampaign.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.BeginCampaign.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		int chapterIndex = o.ChapterIndex;
		dictionary.Add("ChapterIndex", ((long)chapterIndex).ToString());
		int dungeonIndex = o.DungeonIndex;
		dictionary.Add("DungeonIndex", ((long)dungeonIndex).ToString());
		int linkedDungeonIndex = o.LinkedDungeonIndex;
		dictionary.Add("LinkedDungeonIndex", ((long)linkedDungeonIndex).ToString());
		int dungeonDifficulty = o.DungeonDifficulty;
		dictionary.Add("DungeonDifficulty", ((long)dungeonDifficulty).ToString());
		dictionary.Add("ScenarioDungeon", o.ScenarioDungeon.ToString());
		dictionary.Add("HeroIndices", BaseJsonMarshaler.EncodeArray(o.HeroIndices));
		dictionary.Add("GroupHeroIndices", BaseJsonMarshaler.EncodeArray(o.GroupHeroIndices));
		dictionary.Add("AiHeroIndices", BaseJsonMarshaler.EncodeArray(o.AiHeroIndices));
		int leaderHeroIndex = o.LeaderHeroIndex;
		dictionary.Add("LeaderHeroIndex", ((long)leaderHeroIndex).ToString());
		dictionary.Add("AccountIds", BaseJsonMarshaler.EncodeArray(o.AccountIds));
		dictionary.Add("Repeat", o.Repeat.ToString());
		int buyKey = o.BuyKey;
		dictionary.Add("BuyKey", ((long)buyKey).ToString());
		int worldMapEventIndex = o.WorldMapEventIndex;
		dictionary.Add("WorldMapEventIndex", ((long)worldMapEventIndex).ToString());
		int worldMapNodeIndex = o.WorldMapNodeIndex;
		dictionary.Add("WorldMapNodeIndex", ((long)worldMapNodeIndex).ToString());
		dictionary.Add("MultiplayMasterId", o.MultiplayMasterId.ToString());
		dictionary.Add("MultiplayMemberIds", BaseJsonMarshaler.EncodeArray(o.MultiplayMemberIds));
		dictionary.Add("MultiplayObserverIds", BaseJsonMarshaler.EncodeArray(o.MultiplayObserverIds));
		if (o.MultiplayMasterServerIp != null)
		{
			dictionary.Add("MultiplayMasterServerIp", o.MultiplayMasterServerIp);
		}
		int multiplayMasterServerPort = o.MultiplayMasterServerPort;
		dictionary.Add("MultiplayMasterServerPort", ((long)multiplayMasterServerPort).ToString());
		int towerIndex = o.TowerIndex;
		dictionary.Add("TowerIndex", ((long)towerIndex).ToString());
		int towerFloor = o.TowerFloor;
		dictionary.Add("TowerFloor", ((long)towerFloor).ToString());
		int raidIndex = o.RaidIndex;
		dictionary.Add("RaidIndex", ((long)raidIndex).ToString());
		int raidLevel = o.RaidLevel;
		dictionary.Add("RaidLevel", ((long)raidLevel).ToString());
		int worldBossIndex = o.WorldBossIndex;
		dictionary.Add("WorldBossIndex", ((long)worldBossIndex).ToString());
		int eventWorldBossIndex = o.EventWorldBossIndex;
		dictionary.Add("EventWorldBossIndex", ((long)eventWorldBossIndex).ToString());
		int eventWorldBossLevel = o.EventWorldBossLevel;
		dictionary.Add("EventWorldBossLevel", ((long)eventWorldBossLevel).ToString());
		dictionary.Add("RaidRoomNo", o.RaidRoomNo.ToString());
		dictionary.Add("MultiRoomNo", o.MultiRoomNo.ToString());
		dictionary.Add("ItemBoosterCount", o.ItemBoosterCount.ToString());
		dictionary.Add("UseStamina", o.UseStamina.ToString());
		dictionary.Add("SelectedRewardItemCodes", BaseJsonMarshaler.EncodeArray(o.SelectedRewardItemCodes));
		int onlineGameSpeedRatio = o.OnlineGameSpeedRatio;
		dictionary.Add("OnlineGameSpeedRatio", ((long)onlineGameSpeedRatio).ToString());
		int enterTicketCount = o.EnterTicketCount;
		dictionary.Add("EnterTicketCount", ((long)enterTicketCount).ToString());
		dictionary.Add("DeltaHeroIndices", BaseJsonMarshaler.EncodeArray(o.DeltaHeroIndices));
		dictionary.Add("ZetaHeroIndices", BaseJsonMarshaler.EncodeArray(o.ZetaHeroIndices));
		dictionary.Add("AffixIndices", BaseJsonMarshaler.EncodeArray(o.AffixIndices));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
