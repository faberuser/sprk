using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.AddLog;

public static class JM_NShared_AddLog_Request
{
	public static NShared.AddLog.Request Parse(string s)
	{
		return Parse(s, new NShared.AddLog.Request());
	}

	public static NShared.AddLog.Request Parse(string s, NShared.AddLog.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.AddLog.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.AddLog.Request());
	}

	public static NShared.AddLog.Request Parse(IDictionary d, NShared.AddLog.Request o)
	{
		o.TableName = BaseJsonMarshaler.ParseValueString(d, "TableName");
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.Newbie = BaseJsonMarshaler.ParseValueInt(d, "Newbie");
		o.AssetQuality = BaseJsonMarshaler.ParseValueInt(d, "AssetQuality");
		o.DownloadType = BaseJsonMarshaler.ParseValueInt(d, "DownloadType");
		o.Step = BaseJsonMarshaler.ParseValueInt(d, "Step");
		o.BackgroundUID = BaseJsonMarshaler.ParseValueString(d, "BackgroundUID");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.AddLog.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.AddLog.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.TableName != null)
		{
			dictionary.Add("TableName", o.TableName);
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("Newbie", o.Newbie.ToString());
		dictionary.Add("AssetQuality", o.AssetQuality.ToString());
		dictionary.Add("DownloadType", o.DownloadType.ToString());
		dictionary.Add("Step", o.Step.ToString());
		if (o.BackgroundUID != null)
		{
			dictionary.Add("BackgroundUID", o.BackgroundUID);
		}
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
