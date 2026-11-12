using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ExecPetGacha;

public static class JM_NShared_ExecPetGacha_Request
{
	public static NShared.ExecPetGacha.Request Parse(string s)
	{
		return Parse(s, new NShared.ExecPetGacha.Request());
	}

	public static NShared.ExecPetGacha.Request Parse(string s, NShared.ExecPetGacha.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ExecPetGacha.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ExecPetGacha.Request());
	}

	public static NShared.ExecPetGacha.Request Parse(IDictionary d, NShared.ExecPetGacha.Request o)
	{
		o.GachaIndex = BaseJsonMarshaler.ParseValueInt(d, "GachaIndex");
		o.ItemIndex = BaseJsonMarshaler.ParseValueInt(d, "ItemIndex");
		o.Free = BaseJsonMarshaler.ParseValueBool(d, "Free");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ExecPetGacha.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ExecPetGacha.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GachaIndex", o.GachaIndex.ToString());
		dictionary.Add("ItemIndex", o.ItemIndex.ToString());
		dictionary.Add("Free", o.Free.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
