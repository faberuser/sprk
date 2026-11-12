using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.MaxUpgradeEquipOption;

public static class JM_NShared_MaxUpgradeEquipOption_Request
{
	public static NShared.MaxUpgradeEquipOption.Request Parse(string s)
	{
		return Parse(s, new NShared.MaxUpgradeEquipOption.Request());
	}

	public static NShared.MaxUpgradeEquipOption.Request Parse(string s, NShared.MaxUpgradeEquipOption.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.MaxUpgradeEquipOption.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.MaxUpgradeEquipOption.Request());
	}

	public static NShared.MaxUpgradeEquipOption.Request Parse(IDictionary d, NShared.MaxUpgradeEquipOption.Request o)
	{
		o.EquipItemSlotIndex = BaseJsonMarshaler.ParseValueInt(d, "EquipItemSlotIndex");
		o.MaterialItemIndex = BaseJsonMarshaler.ParseValueInt(d, "MaterialItemIndex");
		o.MaterialItemCount = BaseJsonMarshaler.ParseValueLong(d, "MaterialItemCount");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.MaxUpgradeEquipOption.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.MaxUpgradeEquipOption.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		int equipItemSlotIndex = o.EquipItemSlotIndex;
		dictionary.Add("EquipItemSlotIndex", ((long)equipItemSlotIndex).ToString());
		int materialItemIndex = o.MaterialItemIndex;
		dictionary.Add("MaterialItemIndex", ((long)materialItemIndex).ToString());
		dictionary.Add("MaterialItemCount", o.MaterialItemCount.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
