using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PatchInfo
{
	public static PatchInfo Parse(string s)
	{
		return Parse(s, new PatchInfo());
	}

	public static PatchInfo Parse(string s, PatchInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PatchInfo Parse(IDictionary d)
	{
		return Parse(d, new PatchInfo());
	}

	public static PatchInfo Parse(IDictionary d, PatchInfo o)
	{
		o.Archives = BaseJsonMarshaler.ParseList<ArchiveInfo, List<ArchiveInfo>>(d, "Archives", JM_NShared_ArchiveInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(PatchInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PatchInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Archives", BaseJsonMarshaler.EncodeArray(o.Archives, JM_NShared_ArchiveInfo.EncodeJsonObject));
		return dictionary;
	}
}
