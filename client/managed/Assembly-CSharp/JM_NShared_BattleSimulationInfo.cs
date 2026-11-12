using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_BattleSimulationInfo
{
	public static BattleSimulationInfo Parse(string s)
	{
		return Parse(s, new BattleSimulationInfo());
	}

	public static BattleSimulationInfo Parse(string s, BattleSimulationInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleSimulationInfo Parse(IDictionary d)
	{
		return Parse(d, new BattleSimulationInfo());
	}

	public static BattleSimulationInfo Parse(IDictionary d, BattleSimulationInfo o)
	{
		o.RequestServer = BaseJsonMarshaler.ParseValueString(d, "RequestServer");
		o.GroupId = BaseJsonMarshaler.ParseValueString(d, "GroupId");
		o.Uid = BaseJsonMarshaler.ParseValueString(d, "Uid");
		o.AccountInfos = BaseJsonMarshaler.ParseArray(d, "AccountInfos", JM_NShared_BattleSimulationAccountInfo.Parse);
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.DungeonDifficulty = BaseJsonMarshaler.ParseValueInt(d, "DungeonDifficulty");
		o.RepeatCount = BaseJsonMarshaler.ParseValueInt(d, "RepeatCount");
		o.RemainCount = BaseJsonMarshaler.ParseValueInt(d, "RemainCount");
		o.ExtraData = BaseJsonMarshaler.ParseObject(d, "ExtraData", JM_NShared_BattleInstanceExtraData.Parse);
		return o;
	}

	public static string EncodeJsonString(BattleSimulationInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleSimulationInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.RequestServer != null)
		{
			dictionary.Add("RequestServer", o.RequestServer);
		}
		if (o.GroupId != null)
		{
			dictionary.Add("GroupId", o.GroupId);
		}
		if (o.Uid != null)
		{
			dictionary.Add("Uid", o.Uid);
		}
		dictionary.Add("AccountInfos", BaseJsonMarshaler.EncodeArray(o.AccountInfos, JM_NShared_BattleSimulationAccountInfo.EncodeJsonObject));
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("DungeonDifficulty", o.DungeonDifficulty.ToString());
		dictionary.Add("RepeatCount", o.RepeatCount.ToString());
		dictionary.Add("RemainCount", o.RemainCount.ToString());
		dictionary.Add("ExtraData", JM_NShared_BattleInstanceExtraData.EncodeJsonObject(o.ExtraData));
		return dictionary;
	}
}
