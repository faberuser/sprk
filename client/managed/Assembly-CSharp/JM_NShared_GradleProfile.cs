using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GradleProfile
{
	public static GradleProfile Parse(string s)
	{
		return Parse(s, new GradleProfile());
	}

	public static GradleProfile Parse(string s, GradleProfile o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GradleProfile Parse(IDictionary d)
	{
		return Parse(d, new GradleProfile());
	}

	public static GradleProfile Parse(IDictionary d, GradleProfile o)
	{
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		o.AppNameConfig = BaseJsonMarshaler.ParseValueString(d, "AppNameConfig");
		o.ApplicaionId = BaseJsonMarshaler.ParseValueString(d, "ApplicaionId");
		o.GoogleAppId = BaseJsonMarshaler.ParseValueString(d, "GoogleAppId");
		o.IconPath = BaseJsonMarshaler.ParseValueString(d, "IconPath");
		o.Tasks = BaseJsonMarshaler.ParseValueString(d, "Tasks");
		o.ReleaseTasks = BaseJsonMarshaler.ParseValueString(d, "ReleaseTasks");
		o.ExcludeJars = BaseJsonMarshaler.ParseArrayCast(d, "ExcludeJars", BaseJsonMarshaler.CastToString);
		o.ExcludeModules = BaseJsonMarshaler.ParseArrayCast(d, "ExcludeModules", BaseJsonMarshaler.CastToString);
		o.ExcludeArchitectures = BaseJsonMarshaler.ParseArrayCast(d, "ExcludeArchitectures", BaseJsonMarshaler.CastToString);
		o.AppendSecretKey = BaseJsonMarshaler.ParseValueBool(d, "AppendSecretKey");
		return o;
	}

	public static string EncodeJsonString(GradleProfile o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GradleProfile o)
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
		if (o.AppNameConfig != null)
		{
			dictionary.Add("AppNameConfig", o.AppNameConfig);
		}
		if (o.ApplicaionId != null)
		{
			dictionary.Add("ApplicaionId", o.ApplicaionId);
		}
		if (o.GoogleAppId != null)
		{
			dictionary.Add("GoogleAppId", o.GoogleAppId);
		}
		if (o.IconPath != null)
		{
			dictionary.Add("IconPath", o.IconPath);
		}
		if (o.Tasks != null)
		{
			dictionary.Add("Tasks", o.Tasks);
		}
		if (o.ReleaseTasks != null)
		{
			dictionary.Add("ReleaseTasks", o.ReleaseTasks);
		}
		dictionary.Add("ExcludeJars", BaseJsonMarshaler.EncodeArray(o.ExcludeJars));
		dictionary.Add("ExcludeModules", BaseJsonMarshaler.EncodeArray(o.ExcludeModules));
		dictionary.Add("ExcludeArchitectures", BaseJsonMarshaler.EncodeArray(o.ExcludeArchitectures));
		dictionary.Add("AppendSecretKey", o.AppendSecretKey.ToString());
		return dictionary;
	}
}
