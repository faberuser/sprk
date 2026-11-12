using System;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.B2M_Ping;

public static class JM_NShared_B2M_Ping_QueuedCheckResult
{
	public static QueuedCheckResult Parse(string s)
	{
		return Parse(s, new QueuedCheckResult());
	}

	public static QueuedCheckResult Parse(string s, QueuedCheckResult o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static QueuedCheckResult Parse(IDictionary d)
	{
		return Parse(d, new QueuedCheckResult());
	}

	public static QueuedCheckResult Parse(IDictionary d, QueuedCheckResult o)
	{
		o.CheckInfo = BaseJsonMarshaler.ParseObject(d, "CheckInfo", JM_NShared_B2M_Ping_IntegrityInfo.Parse);
		o.Result = BaseJsonMarshaler.ParseValueString(d, "Result");
		o.ExactResult = BaseJsonMarshaler.ParseValueString(d, "ExactResult");
		o.ClientTotalDamage = BaseJsonMarshaler.ParseValueLong(d, "ClientTotalDamage");
		o.QueuedTime = BaseJsonMarshaler.ParseValue<DateTime>(d, "QueuedTime");
		return o;
	}

	public static string EncodeJsonString(QueuedCheckResult o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(QueuedCheckResult o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CheckInfo", JM_NShared_B2M_Ping_IntegrityInfo.EncodeJsonObject(o.CheckInfo));
		if (o.Result != null)
		{
			dictionary.Add("Result", o.Result);
		}
		if (o.ExactResult != null)
		{
			dictionary.Add("ExactResult", o.ExactResult);
		}
		dictionary.Add("ClientTotalDamage", o.ClientTotalDamage.ToString());
		dictionary.Add("QueuedTime", o.QueuedTime.ToString());
		return dictionary;
	}
}
