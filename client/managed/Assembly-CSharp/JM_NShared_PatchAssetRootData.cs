using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_PatchAssetRootData
{
	public static PatchAssetRootData Parse(string s)
	{
		return Parse(s, new PatchAssetRootData());
	}

	public static PatchAssetRootData Parse(string s, PatchAssetRootData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PatchAssetRootData Parse(IDictionary d)
	{
		return Parse(d, new PatchAssetRootData());
	}

	public static PatchAssetRootData Parse(IDictionary d, PatchAssetRootData o)
	{
		o.Path = BaseJsonMarshaler.ParseValueString(d, "Path");
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		o.CheckIntegrity = BaseJsonMarshaler.ParseValueBool(d, "CheckIntegrity");
		o.ExportSubPath = BaseJsonMarshaler.ParseArrayCast(d, "ExportSubPath", BaseJsonMarshaler.CastToString);
		o.ExportFiles = BaseJsonMarshaler.ParseArrayCast(d, "ExportFiles", BaseJsonMarshaler.CastToString);
		o.ExcludeFiles = BaseJsonMarshaler.ParseArrayCast(d, "ExcludeFiles", BaseJsonMarshaler.CastToString);
		o.Recurse = BaseJsonMarshaler.ParseValueBool(d, "Recurse");
		o.EncryptType = BaseJsonMarshaler.ParseEnum<EncryptType>(d, "EncryptType");
		o.CookType = BaseJsonMarshaler.ParseEnum<CookType>(d, "CookType");
		o.ArchiveType = BaseJsonMarshaler.ParseEnum<PatchArchiveType>(d, "ArchiveType");
		return o;
	}

	public static string EncodeJsonString(PatchAssetRootData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PatchAssetRootData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Path != null)
		{
			dictionary.Add("Path", o.Path);
		}
		if (o.Name != null)
		{
			dictionary.Add("Name", o.Name);
		}
		dictionary.Add("CheckIntegrity", o.CheckIntegrity.ToString());
		dictionary.Add("ExportSubPath", BaseJsonMarshaler.EncodeArray(o.ExportSubPath));
		dictionary.Add("ExportFiles", BaseJsonMarshaler.EncodeArray(o.ExportFiles));
		dictionary.Add("ExcludeFiles", BaseJsonMarshaler.EncodeArray(o.ExcludeFiles));
		dictionary.Add("Recurse", o.Recurse.ToString());
		dictionary.Add("EncryptType", o.EncryptType.ToString());
		dictionary.Add("CookType", o.CookType.ToString());
		dictionary.Add("ArchiveType", o.ArchiveType.ToString());
		return dictionary;
	}
}
