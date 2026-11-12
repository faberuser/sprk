using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_UseSkillReq
{
	public static UseSkillReq Parse(string s)
	{
		return Parse(s, new UseSkillReq());
	}

	public static UseSkillReq Parse(string s, UseSkillReq o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static UseSkillReq Parse(IDictionary d)
	{
		return Parse(d, new UseSkillReq());
	}

	public static UseSkillReq Parse(IDictionary d, UseSkillReq o)
	{
		o.CreatureUid = BaseJsonMarshaler.ParseValueInt(d, "CreatureUid");
		o.SkillSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "SkillSlotIndex");
		o.RequestId = BaseJsonMarshaler.ParseValueInt(d, "RequestId");
		return o;
	}

	public static string EncodeJsonString(UseSkillReq o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(UseSkillReq o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("CreatureUid", o.CreatureUid.ToString());
		dictionary.Add("SkillSlotIndex", o.SkillSlotIndex.ToString());
		dictionary.Add("RequestId", o.RequestId.ToString());
		return dictionary;
	}
}
