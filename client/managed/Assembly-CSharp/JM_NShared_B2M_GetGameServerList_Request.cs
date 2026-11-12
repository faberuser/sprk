using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.B2M_GetGameServerList;

public static class JM_NShared_B2M_GetGameServerList_Request
{
	public static NShared.B2M_GetGameServerList.Request Parse(string s)
	{
		return Parse(s, new NShared.B2M_GetGameServerList.Request());
	}

	public static NShared.B2M_GetGameServerList.Request Parse(string s, NShared.B2M_GetGameServerList.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.B2M_GetGameServerList.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.B2M_GetGameServerList.Request());
	}

	public static NShared.B2M_GetGameServerList.Request Parse(IDictionary d, NShared.B2M_GetGameServerList.Request o)
	{
		o.BattleServerName = BaseJsonMarshaler.ParseValueString(d, "BattleServerName");
		o.PublicIp = BaseJsonMarshaler.ParseValueString(d, "PublicIp");
		o.PrivateIp = BaseJsonMarshaler.ParseValueString(d, "PrivateIp");
		o.Port = BaseJsonMarshaler.ParseValueInt(d, "Port");
		o.SessionCu = BaseJsonMarshaler.ParseValueInt(d, "SessionCu");
		o.RealtimeCu = BaseJsonMarshaler.ParseValueInt(d, "RealtimeCu");
		o.ServerVersion = BaseJsonMarshaler.ParseValueString(d, "ServerVersion");
		o.RunningTime = BaseJsonMarshaler.ParseValueInt(d, "RunningTime");
		o.CpuUsage = BaseJsonMarshaler.ParseValueInt(d, "CpuUsage");
		o.FreeMem = BaseJsonMarshaler.ParseValueInt(d, "FreeMem");
		o.FreeMemPercentage = BaseJsonMarshaler.ParseValueInt(d, "FreeMemPercentage");
		return o;
	}

	public static string EncodeJsonString(NShared.B2M_GetGameServerList.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.B2M_GetGameServerList.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.BattleServerName != null)
		{
			dictionary.Add("BattleServerName", o.BattleServerName);
		}
		if (o.PublicIp != null)
		{
			dictionary.Add("PublicIp", o.PublicIp);
		}
		if (o.PrivateIp != null)
		{
			dictionary.Add("PrivateIp", o.PrivateIp);
		}
		dictionary.Add("Port", o.Port.ToString());
		dictionary.Add("SessionCu", o.SessionCu.ToString());
		dictionary.Add("RealtimeCu", o.RealtimeCu.ToString());
		if (o.ServerVersion != null)
		{
			dictionary.Add("ServerVersion", o.ServerVersion);
		}
		dictionary.Add("RunningTime", o.RunningTime.ToString());
		dictionary.Add("CpuUsage", o.CpuUsage.ToString());
		dictionary.Add("FreeMem", o.FreeMem.ToString());
		dictionary.Add("FreeMemPercentage", o.FreeMemPercentage.ToString());
		return dictionary;
	}
}
