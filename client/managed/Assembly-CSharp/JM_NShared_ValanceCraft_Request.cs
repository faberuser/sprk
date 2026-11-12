using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ValanceCraft;

public static class JM_NShared_ValanceCraft_Request
{
	public static NShared.ValanceCraft.Request Parse(string s)
	{
		return Parse(s, new NShared.ValanceCraft.Request());
	}

	public static NShared.ValanceCraft.Request Parse(string s, NShared.ValanceCraft.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ValanceCraft.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.ValanceCraft.Request());
	}

	public static NShared.ValanceCraft.Request Parse(IDictionary d, NShared.ValanceCraft.Request o)
	{
		o.SetType = BaseJsonMarshaler.ParseEnum<EquipItemSetGroupType>(d, "SetType");
		o.CreatureTagType = BaseJsonMarshaler.ParseEnum<CreatureTagType>(d, "CreatureTagType");
		o.PartType = BaseJsonMarshaler.ParseEnum<EquipPartType>(d, "PartType");
		o.SubPartType = BaseJsonMarshaler.ParseEnum<EquipItemSubType>(d, "SubPartType");
		o.SelectEquipOptionIndices = BaseJsonMarshaler.ParseArrayCast(d, "SelectEquipOptionIndices", BaseJsonMarshaler.CastToInt);
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.ValanceCraft.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.ValanceCraft.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SetType", o.SetType.ToString());
		dictionary.Add("CreatureTagType", o.CreatureTagType.ToString());
		dictionary.Add("PartType", o.PartType.ToString());
		dictionary.Add("SubPartType", o.SubPartType.ToString());
		dictionary.Add("SelectEquipOptionIndices", BaseJsonMarshaler.EncodeArray(o.SelectEquipOptionIndices));
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
