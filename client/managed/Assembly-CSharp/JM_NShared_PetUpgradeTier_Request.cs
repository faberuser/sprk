using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.PetUpgradeTier;

public static class JM_NShared_PetUpgradeTier_Request
{
	public static NShared.PetUpgradeTier.Request Parse(string s)
	{
		return Parse(s, new NShared.PetUpgradeTier.Request());
	}

	public static NShared.PetUpgradeTier.Request Parse(string s, NShared.PetUpgradeTier.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.PetUpgradeTier.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.PetUpgradeTier.Request());
	}

	public static NShared.PetUpgradeTier.Request Parse(IDictionary d, NShared.PetUpgradeTier.Request o)
	{
		o.PetSouls = BaseJsonMarshaler.ParseArrayCast(d, "PetSouls", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.PetUpgradeTier.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.PetUpgradeTier.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("PetSouls", BaseJsonMarshaler.EncodeArray(o.PetSouls));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
