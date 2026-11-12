using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ArchiveInfo
{
	public static ArchiveInfo Parse(string s)
	{
		return Parse(s, new ArchiveInfo());
	}

	public static ArchiveInfo Parse(string s, ArchiveInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ArchiveInfo Parse(IDictionary d)
	{
		return Parse(d, new ArchiveInfo());
	}

	public static ArchiveInfo Parse(IDictionary d, ArchiveInfo o)
	{
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		o.SrcVersion = BaseJsonMarshaler.ParseValueInt(d, "SrcVersion");
		o.Zip = BaseJsonMarshaler.ParseValueBool(d, "Zip");
		o.AssetInfos = BaseJsonMarshaler.ParseList<AssetInfo, List<AssetInfo>>(d, "AssetInfos", JM_NShared_AssetInfo.Parse);
		o.ArchiveType = BaseJsonMarshaler.ParseEnum<PatchArchiveType>(d, "ArchiveType");
		o.Cat = BaseJsonMarshaler.ParseValueInt(d, "Cat");
		o.Pri = BaseJsonMarshaler.ParseValueInt(d, "Pri");
		o.Length = BaseJsonMarshaler.ParseValueInt(d, "Length");
		o.Md5 = BaseJsonMarshaler.ParseValueString(d, "Md5");
		o.Crc = BaseJsonMarshaler.ParseValueUInt(d, "Crc");
		o.Time = BaseJsonMarshaler.ParseValueLong(d, "Time");
		return o;
	}

	public static string EncodeJsonString(ArchiveInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ArchiveInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Name != null)
		{
			dictionary.Add("Name", o.Name);
		}
		dictionary.Add("SrcVersion", o.SrcVersion.ToString());
		dictionary.Add("Zip", o.Zip.ToString());
		dictionary.Add("AssetInfos", BaseJsonMarshaler.EncodeArray(o.AssetInfos, JM_NShared_AssetInfo.EncodeJsonObject));
		dictionary.Add("ArchiveType", o.ArchiveType.ToString());
		dictionary.Add("Cat", o.Cat.ToString());
		dictionary.Add("Pri", o.Pri.ToString());
		dictionary.Add("Length", o.Length.ToString());
		if (o.Md5 != null)
		{
			dictionary.Add("Md5", o.Md5);
		}
		dictionary.Add("Crc", o.Crc.ToString());
		dictionary.Add("Time", o.Time.ToString());
		return dictionary;
	}
}
