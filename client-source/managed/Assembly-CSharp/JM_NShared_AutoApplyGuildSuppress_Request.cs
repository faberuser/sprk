using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.AutoApplyGuildSuppress;

public static class JM_NShared_AutoApplyGuildSuppress_Request
{
	public static NShared.AutoApplyGuildSuppress.Request Parse(string s)
	{
		return Parse(s, new NShared.AutoApplyGuildSuppress.Request());
	}

	public static NShared.AutoApplyGuildSuppress.Request Parse(string s, NShared.AutoApplyGuildSuppress.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.AutoApplyGuildSuppress.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.AutoApplyGuildSuppress.Request());
	}

	public static NShared.AutoApplyGuildSuppress.Request Parse(IDictionary d, NShared.AutoApplyGuildSuppress.Request o)
	{
		o.IsApply = BaseJsonMarshaler.ParseValueBool(d, "IsApply");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.AutoApplyGuildSuppress.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.AutoApplyGuildSuppress.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("IsApply", o.IsApply.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
