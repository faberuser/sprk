using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetDungeonInfo;

public static class JM_NShared_GetDungeonInfo_Request
{
	public static NShared.GetDungeonInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetDungeonInfo.Request());
	}

	public static NShared.GetDungeonInfo.Request Parse(string s, NShared.GetDungeonInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetDungeonInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetDungeonInfo.Request());
	}

	public static NShared.GetDungeonInfo.Request Parse(IDictionary d, NShared.GetDungeonInfo.Request o)
	{
		o.BattleType = BaseJsonMarshaler.ParseEnum<BattleType>(d, "BattleType");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetDungeonInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetDungeonInfo.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("BattleType", o.BattleType.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
