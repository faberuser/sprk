using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ChangePetLayout;

public static class JM_NShared_ChangePetLayout_Request
{
	public static NShared.ChangePetLayout.Request Parse(string s)
	{
		return Parse(s, new NShared.ChangePetLayout.Request());
	}

	public static NShared.ChangePetLayout.Request Parse(string s, NShared.ChangePetLayout.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ChangePetLayout.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ChangePetLayout.Request());
	}

	public static NShared.ChangePetLayout.Request Parse(IDictionary d, NShared.ChangePetLayout.Request o)
	{
		o.PetIndices = BaseJsonMarshaler.ParseArrayCast(d, "PetIndices", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ChangePetLayout.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ChangePetLayout.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("PetIndices", BaseJsonMarshaler.EncodeArray(o.PetIndices));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
