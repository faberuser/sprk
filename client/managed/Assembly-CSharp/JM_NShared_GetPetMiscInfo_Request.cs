using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetPetMiscInfo;

public static class JM_NShared_GetPetMiscInfo_Request
{
	public static NShared.GetPetMiscInfo.Request Parse(string s)
	{
		return Parse(s, new NShared.GetPetMiscInfo.Request());
	}

	public static NShared.GetPetMiscInfo.Request Parse(string s, NShared.GetPetMiscInfo.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetPetMiscInfo.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetPetMiscInfo.Request());
	}

	public static NShared.GetPetMiscInfo.Request Parse(IDictionary d, NShared.GetPetMiscInfo.Request o)
	{
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetPetMiscInfo.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetPetMiscInfo.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
