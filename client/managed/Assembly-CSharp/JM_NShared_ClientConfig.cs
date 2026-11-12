using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ClientConfig
{
	public static ClientConfig Parse(string s)
	{
		return Parse(s, new ClientConfig());
	}

	public static ClientConfig Parse(string s, ClientConfig o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ClientConfig Parse(IDictionary d)
	{
		return Parse(d, new ClientConfig());
	}

	public static ClientConfig Parse(IDictionary d, ClientConfig o)
	{
		o.Version = BaseJsonMarshaler.ParseValueString(d, "Version");
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		o.Build = BaseJsonMarshaler.ParseEnum<ClientBuildType>(d, "Build");
		o.BuildStep = BaseJsonMarshaler.ParseValueString(d, "BuildStep");
		o.Platform = BaseJsonMarshaler.ParseValueString(d, "Platform");
		o.BundleIdentifier = BaseJsonMarshaler.ParseValueString(d, "BundleIdentifier");
		o.ProductName = BaseJsonMarshaler.ParseValueString(d, "ProductName");
		o.QueryHost = BaseJsonMarshaler.ParseValueString(d, "QueryHost");
		o.ForcedServerName = BaseJsonMarshaler.ParseValueString(d, "ForcedServerName");
		o.Define = BaseJsonMarshaler.ParseValueString(d, "Define");
		o.GcmRegisterId = BaseJsonMarshaler.ParseValueString(d, "GcmRegisterId");
		o.ShowEditorGui = BaseJsonMarshaler.ParseValueBool(d, "ShowEditorGui");
		o.BattleVersion = BaseJsonMarshaler.ParseValueString(d, "BattleVersion");
		o.ExpireDate = BaseJsonMarshaler.ParseValueString(d, "ExpireDate");
		o.EditorDefault = BaseJsonMarshaler.ParseValueBool(d, "EditorDefault");
		o.ShowClientConfig = BaseJsonMarshaler.ParseValueBool(d, "ShowClientConfig");
		o.ForceBuildVersion = BaseJsonMarshaler.ParseValueString(d, "ForceBuildVersion");
		o.IsOBB = BaseJsonMarshaler.ParseValueBool(d, "IsOBB");
		o.IsPTS = BaseJsonMarshaler.ParseValueBool(d, "IsPTS");
		o.SecretKey = BaseJsonMarshaler.ParseValueString(d, "SecretKey");
		o.GradleProfile = BaseJsonMarshaler.ParseArrayCast(d, "GradleProfile", BaseJsonMarshaler.CastToString);
		return o;
	}

	public static string EncodeJsonString(ClientConfig o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ClientConfig o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Version != null)
		{
			dictionary.Add("Version", o.Version);
		}
		if (o.Name != null)
		{
			dictionary.Add("Name", o.Name);
		}
		dictionary.Add("Build", o.Build.ToString());
		if (o.BuildStep != null)
		{
			dictionary.Add("BuildStep", o.BuildStep);
		}
		if (o.Platform != null)
		{
			dictionary.Add("Platform", o.Platform);
		}
		if (o.BundleIdentifier != null)
		{
			dictionary.Add("BundleIdentifier", o.BundleIdentifier);
		}
		if (o.ProductName != null)
		{
			dictionary.Add("ProductName", o.ProductName);
		}
		if (o.QueryHost != null)
		{
			dictionary.Add("QueryHost", o.QueryHost);
		}
		if (o.ForcedServerName != null)
		{
			dictionary.Add("ForcedServerName", o.ForcedServerName);
		}
		if (o.Define != null)
		{
			dictionary.Add("Define", o.Define);
		}
		if (o.GcmRegisterId != null)
		{
			dictionary.Add("GcmRegisterId", o.GcmRegisterId);
		}
		dictionary.Add("ShowEditorGui", o.ShowEditorGui.ToString());
		if (o.BattleVersion != null)
		{
			dictionary.Add("BattleVersion", o.BattleVersion);
		}
		if (o.ExpireDate != null)
		{
			dictionary.Add("ExpireDate", o.ExpireDate);
		}
		dictionary.Add("EditorDefault", o.EditorDefault.ToString());
		dictionary.Add("ShowClientConfig", o.ShowClientConfig.ToString());
		if (o.ForceBuildVersion != null)
		{
			dictionary.Add("ForceBuildVersion", o.ForceBuildVersion);
		}
		dictionary.Add("IsOBB", o.IsOBB.ToString());
		dictionary.Add("IsPTS", o.IsPTS.ToString());
		if (o.SecretKey != null)
		{
			dictionary.Add("SecretKey", o.SecretKey);
		}
		dictionary.Add("GradleProfile", BaseJsonMarshaler.EncodeArray(o.GradleProfile));
		return dictionary;
	}
}
