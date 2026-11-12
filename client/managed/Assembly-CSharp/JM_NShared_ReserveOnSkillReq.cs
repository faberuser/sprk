using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ReserveOnSkillReq
{
	public static ReserveOnSkillReq Parse(string s)
	{
		return Parse(s, new ReserveOnSkillReq());
	}

	public static ReserveOnSkillReq Parse(string s, ReserveOnSkillReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ReserveOnSkillReq Parse(IDictionary d)
	{
		return Parse(d, new ReserveOnSkillReq());
	}

	public static ReserveOnSkillReq Parse(IDictionary d, ReserveOnSkillReq o)
	{
		o.SkillSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "SkillSlotIndex");
		o.ManualTargetUid = BaseJsonMarshaler.ParseValueInt(d, "ManualTargetUid");
		o.AutoReserved = BaseJsonMarshaler.ParseValueBool(d, "AutoReserved");
		o.CreatureUid = BaseJsonMarshaler.ParseValueInt(d, "CreatureUid");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(ReserveOnSkillReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ReserveOnSkillReq o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SkillSlotIndex", o.SkillSlotIndex.ToString());
		dictionary.Add("ManualTargetUid", o.ManualTargetUid.ToString());
		dictionary.Add("AutoReserved", o.AutoReserved.ToString());
		dictionary.Add("CreatureUid", o.CreatureUid.ToString());
		dictionary.Add("RequestId", o.RequestId.ToString());
		return dictionary;
	}
}
