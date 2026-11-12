using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.B2M_Ping;

public static class JM_NShared_B2M_Ping_Request
{
	public static NShared.B2M_Ping.Request Parse(string s)
	{
		return Parse(s, new NShared.B2M_Ping.Request());
	}

	public static NShared.B2M_Ping.Request Parse(string s, NShared.B2M_Ping.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.B2M_Ping.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.B2M_Ping.Request());
	}

	public static NShared.B2M_Ping.Request Parse(IDictionary d, NShared.B2M_Ping.Request o)
	{
		o.BattleServerName = BaseJsonMarshaler.ParseValueString(d, "BattleServerName");
		o.IntegrityType = BaseJsonMarshaler.ParseValueString(d, "IntegrityType");
		o.Address = BaseJsonMarshaler.ParseValueString(d, "Address");
		o.PublicIp = BaseJsonMarshaler.ParseValueString(d, "PublicIp");
		o.PrivateIp = BaseJsonMarshaler.ParseValueString(d, "PrivateIp");
		o.Port = BaseJsonMarshaler.ParseValueInt(d, "Port");
		o.Weight = BaseJsonMarshaler.ParseValueInt(d, "Weight");
		o.BattleVersion = BaseJsonMarshaler.ParseValueString(d, "BattleVersion");
		o.BattleMinVersion = BaseJsonMarshaler.ParseValueString(d, "BattleMinVersion");
		o.BattleMaxVersion = BaseJsonMarshaler.ParseValueString(d, "BattleMaxVersion");
		o.SensitiveDebugging = BaseJsonMarshaler.ParseValueBool(d, "SensitiveDebugging");
		o.CpuUsage = BaseJsonMarshaler.ParseValueInt(d, "CpuUsage");
		o.IsCheckServer = BaseJsonMarshaler.ParseValueBool(d, "IsCheckServer");
		o.IsCheckSimulationServer = BaseJsonMarshaler.ParseValueBool(d, "IsCheckSimulationServer");
		o.IntegrityRetRequestIds = BaseJsonMarshaler.ParseArrayCast(d, "IntegrityRetRequestIds", BaseJsonMarshaler.CastToString);
		o.IntegrityRetAccountIds = BaseJsonMarshaler.ParseArrayCast(d, "IntegrityRetAccountIds", BaseJsonMarshaler.CastToLong);
		o.IntegrityRetResults = BaseJsonMarshaler.ParseArrayCast(d, "IntegrityRetResults", BaseJsonMarshaler.CastToString);
		o.IntegrityRetExactResult = BaseJsonMarshaler.ParseArrayCast(d, "IntegrityRetExactResult", BaseJsonMarshaler.CastToString);
		o.IntegrityRetClientTotalDamage = BaseJsonMarshaler.ParseArrayCast(d, "IntegrityRetClientTotalDamage", BaseJsonMarshaler.CastToLong);
		return o;
	}

	public static string EncodeJsonString(NShared.B2M_Ping.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.B2M_Ping.Request o)
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
		if (o.IntegrityType != null)
		{
			dictionary.Add("IntegrityType", o.IntegrityType);
		}
		if (o.Address != null)
		{
			dictionary.Add("Address", o.Address);
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
		dictionary.Add("Weight", o.Weight.ToString());
		if (o.BattleVersion != null)
		{
			dictionary.Add("BattleVersion", o.BattleVersion);
		}
		if (o.BattleMinVersion != null)
		{
			dictionary.Add("BattleMinVersion", o.BattleMinVersion);
		}
		if (o.BattleMaxVersion != null)
		{
			dictionary.Add("BattleMaxVersion", o.BattleMaxVersion);
		}
		dictionary.Add("SensitiveDebugging", o.SensitiveDebugging.ToString());
		dictionary.Add("CpuUsage", o.CpuUsage.ToString());
		dictionary.Add("IsCheckServer", o.IsCheckServer.ToString());
		dictionary.Add("IsCheckSimulationServer", o.IsCheckSimulationServer.ToString());
		dictionary.Add("IntegrityRetRequestIds", BaseJsonMarshaler.EncodeArray(o.IntegrityRetRequestIds));
		dictionary.Add("IntegrityRetAccountIds", BaseJsonMarshaler.EncodeArray(o.IntegrityRetAccountIds));
		dictionary.Add("IntegrityRetResults", BaseJsonMarshaler.EncodeArray(o.IntegrityRetResults));
		dictionary.Add("IntegrityRetExactResult", BaseJsonMarshaler.EncodeArray(o.IntegrityRetExactResult));
		dictionary.Add("IntegrityRetClientTotalDamage", BaseJsonMarshaler.EncodeArray(o.IntegrityRetClientTotalDamage));
		return dictionary;
	}
}
