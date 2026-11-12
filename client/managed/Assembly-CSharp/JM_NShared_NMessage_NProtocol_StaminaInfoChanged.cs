using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_StaminaInfoChanged
{
	public static StaminaInfoChanged Parse(string s)
	{
		return Parse(s, new StaminaInfoChanged());
	}

	public static StaminaInfoChanged Parse(string s, StaminaInfoChanged o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static StaminaInfoChanged Parse(IDictionary d)
	{
		return Parse(d, new StaminaInfoChanged());
	}

	public static StaminaInfoChanged Parse(IDictionary d, StaminaInfoChanged o)
	{
		o.StaminaType = BaseJsonMarshaler.ParseEnum<StaminaType>(d, "StaminaType");
		o.StaminaCount = BaseJsonMarshaler.ParseValueLong(d, "StaminaCount");
		return o;
	}

	public static string EncodeJsonString(StaminaInfoChanged o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(StaminaInfoChanged o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("StaminaType", o.StaminaType.ToString());
		dictionary.Add("StaminaCount", o.StaminaCount.ToString());
		return dictionary;
	}
}
