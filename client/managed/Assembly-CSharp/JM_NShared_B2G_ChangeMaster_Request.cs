using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.B2G_ChangeMaster;

public static class JM_NShared_B2G_ChangeMaster_Request
{
	public static NShared.B2G_ChangeMaster.Request Parse(string s)
	{
		return Parse(s, new NShared.B2G_ChangeMaster.Request());
	}

	public static NShared.B2G_ChangeMaster.Request Parse(string s, NShared.B2G_ChangeMaster.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.B2G_ChangeMaster.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.B2G_ChangeMaster.Request());
	}

	public static NShared.B2G_ChangeMaster.Request Parse(IDictionary d, NShared.B2G_ChangeMaster.Request o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.IsMaster = BaseJsonMarshaler.ParseValueBool(d, "IsMaster");
		o.MasterAccountId = BaseJsonMarshaler.ParseValueLong(d, "MasterAccountId");
		o.OldMasterAccountId = BaseJsonMarshaler.ParseValueLong(d, "OldMasterAccountId");
		o.MasterHost = BaseJsonMarshaler.ParseValueString(d, "MasterHost");
		return o;
	}

	public static string EncodeJsonString(NShared.B2G_ChangeMaster.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.B2G_ChangeMaster.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("IsMaster", o.IsMaster.ToString());
		dictionary.Add("MasterAccountId", o.MasterAccountId.ToString());
		dictionary.Add("OldMasterAccountId", o.OldMasterAccountId.ToString());
		if (o.MasterHost != null)
		{
			dictionary.Add("MasterHost", o.MasterHost);
		}
		return dictionary;
	}
}
