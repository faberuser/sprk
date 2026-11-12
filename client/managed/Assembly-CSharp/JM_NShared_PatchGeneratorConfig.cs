using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PatchGeneratorConfig
{
	public static PatchGeneratorConfig Parse(string s)
	{
		return Parse(s, new PatchGeneratorConfig());
	}

	public static PatchGeneratorConfig Parse(string s, PatchGeneratorConfig o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PatchGeneratorConfig Parse(IDictionary d)
	{
		return Parse(d, new PatchGeneratorConfig());
	}

	public static PatchGeneratorConfig Parse(IDictionary d, PatchGeneratorConfig o)
	{
		o.ArchivePrefix = BaseJsonMarshaler.ParseValueString(d, "ArchivePrefix");
		o.CreateArchive = BaseJsonMarshaler.ParseValueBool(d, "CreateArchive");
		o.MinArchiveSize = BaseJsonMarshaler.ParseValueInt(d, "MinArchiveSize");
		o.Profiles = BaseJsonMarshaler.ParseArray(d, "Profiles", JM_NShared_PatchProfileData.Parse);
		o.ExceptPattern = BaseJsonMarshaler.ParseArrayCast(d, "ExceptPattern", BaseJsonMarshaler.CastToString);
		o.ExceptHeroPattern = BaseJsonMarshaler.ParseArrayCast(d, "ExceptHeroPattern", BaseJsonMarshaler.CastToString);
		o.IncludePattern = BaseJsonMarshaler.ParseArrayCast(d, "IncludePattern", BaseJsonMarshaler.CastToString);
		o.AppstoreReviewExceptPattern = BaseJsonMarshaler.ParseArrayCast(d, "AppstoreReviewExceptPattern", BaseJsonMarshaler.CastToString);
		return o;
	}

	public static string EncodeJsonString(PatchGeneratorConfig o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PatchGeneratorConfig o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.ArchivePrefix != null)
		{
			dictionary.Add("ArchivePrefix", o.ArchivePrefix);
		}
		dictionary.Add("CreateArchive", o.CreateArchive.ToString());
		dictionary.Add("MinArchiveSize", o.MinArchiveSize.ToString());
		dictionary.Add("Profiles", BaseJsonMarshaler.EncodeArray(o.Profiles, JM_NShared_PatchProfileData.EncodeJsonObject));
		dictionary.Add("ExceptPattern", BaseJsonMarshaler.EncodeArray(o.ExceptPattern));
		dictionary.Add("ExceptHeroPattern", BaseJsonMarshaler.EncodeArray(o.ExceptHeroPattern));
		dictionary.Add("IncludePattern", BaseJsonMarshaler.EncodeArray(o.IncludePattern));
		dictionary.Add("AppstoreReviewExceptPattern", BaseJsonMarshaler.EncodeArray(o.AppstoreReviewExceptPattern));
		return dictionary;
	}
}
