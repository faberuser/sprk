using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PlayerArchiveInfo
{
	public static PlayerArchiveInfo Parse(string s)
	{
		return Parse(s, new PlayerArchiveInfo());
	}

	public static PlayerArchiveInfo Parse(string s, PlayerArchiveInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PlayerArchiveInfo Parse(IDictionary d)
	{
		return Parse(d, new PlayerArchiveInfo());
	}

	public static PlayerArchiveInfo Parse(IDictionary d, PlayerArchiveInfo o)
	{
		o.ArchiveMainIndex = BaseJsonMarshaler.ParseValueInt(d, "ArchiveMainIndex");
		o.ArchiveSubIndex = BaseJsonMarshaler.ParseValueInt(d, "ArchiveSubIndex");
		o.ArchiveValue = BaseJsonMarshaler.ParseValueString(d, "ArchiveValue");
		o.UpdatedTime = BaseJsonMarshaler.ParseValueString(d, "UpdatedTime");
		return o;
	}

	public static string EncodeJsonString(PlayerArchiveInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PlayerArchiveInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ArchiveMainIndex", o.ArchiveMainIndex.ToString());
		dictionary.Add("ArchiveSubIndex", o.ArchiveSubIndex.ToString());
		if (o.ArchiveValue != null)
		{
			dictionary.Add("ArchiveValue", o.ArchiveValue);
		}
		if (o.UpdatedTime != null)
		{
			dictionary.Add("UpdatedTime", o.UpdatedTime);
		}
		return dictionary;
	}
}
