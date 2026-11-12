using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_ChangePartyRoomInfo
{
	public static ChangePartyRoomInfo Parse(string s)
	{
		return Parse(s, new ChangePartyRoomInfo());
	}

	public static ChangePartyRoomInfo Parse(string s, ChangePartyRoomInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ChangePartyRoomInfo Parse(IDictionary d)
	{
		return Parse(d, new ChangePartyRoomInfo());
	}

	public static ChangePartyRoomInfo Parse(IDictionary d, ChangePartyRoomInfo o)
	{
		o.RaidLevel = BaseJsonMarshaler.ParseValueInt(d, "RaidLevel");
		o.GoalIndex = BaseJsonMarshaler.ParseValueByte(d, "GoalIndex");
		o.IsViolateBanRule = BaseJsonMarshaler.ParseValueBool(d, "IsViolateBanRule");
		o.AffixIndices = BaseJsonMarshaler.ParseArrayCast(d, "AffixIndices", BaseJsonMarshaler.CastToInt);
		return o;
	}

	public static string EncodeJsonString(ChangePartyRoomInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ChangePartyRoomInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RaidLevel", o.RaidLevel.ToString());
		dictionary.Add("GoalIndex", o.GoalIndex.ToString());
		dictionary.Add("IsViolateBanRule", o.IsViolateBanRule.ToString());
		dictionary.Add("AffixIndices", BaseJsonMarshaler.EncodeArray(o.AffixIndices));
		return dictionary;
	}
}
