using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.RestoreArtifact;

public static class JM_NShared_RestoreArtifact_Request
{
	public static NShared.RestoreArtifact.Request Parse(string s)
	{
		return Parse(s, new NShared.RestoreArtifact.Request());
	}

	public static NShared.RestoreArtifact.Request Parse(string s, NShared.RestoreArtifact.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.RestoreArtifact.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.RestoreArtifact.Request());
	}

	public static NShared.RestoreArtifact.Request Parse(IDictionary d, NShared.RestoreArtifact.Request o)
	{
		o.ArtifactRestoreIndex = BaseJsonMarshaler.ParseValueInt(d, "ArtifactRestoreIndex");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.RestoreArtifact.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.RestoreArtifact.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ArtifactRestoreIndex", o.ArtifactRestoreIndex.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
