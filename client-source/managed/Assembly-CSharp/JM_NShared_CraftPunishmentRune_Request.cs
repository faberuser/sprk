using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.CraftPunishmentRune;

public static class JM_NShared_CraftPunishmentRune_Request
{
	public static NShared.CraftPunishmentRune.Request Parse(string s)
	{
		return Parse(s, new NShared.CraftPunishmentRune.Request());
	}

	public static NShared.CraftPunishmentRune.Request Parse(string s, NShared.CraftPunishmentRune.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.CraftPunishmentRune.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.CraftPunishmentRune.Request());
	}

	public static NShared.CraftPunishmentRune.Request Parse(IDictionary d, NShared.CraftPunishmentRune.Request o)
	{
		o.PunishmentRuneIndex = BaseJsonMarshaler.ParseValueInt(d, "PunishmentRuneIndex");
		o.IngredientRuneIndices = BaseJsonMarshaler.ParseArrayCast(d, "IngredientRuneIndices", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.CraftPunishmentRune.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.CraftPunishmentRune.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("PunishmentRuneIndex", o.PunishmentRuneIndex.ToString());
		dictionary.Add("IngredientRuneIndices", BaseJsonMarshaler.EncodeArray(o.IngredientRuneIndices));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
