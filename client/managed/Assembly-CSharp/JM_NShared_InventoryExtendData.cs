using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_InventoryExtendData
{
	public static InventoryExtendData Parse(string s)
	{
		return Parse(s, new InventoryExtendData());
	}

	public static InventoryExtendData Parse(string s, InventoryExtendData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static InventoryExtendData Parse(IDictionary d)
	{
		return Parse(d, new InventoryExtendData());
	}

	public static InventoryExtendData Parse(IDictionary d, InventoryExtendData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.EquipItemExtendCount = BaseJsonMarshaler.ParseValueInt(d, "EquipItemExtendCount");
		o.EquipItemExtendPrice = BaseJsonMarshaler.ParseValueInt(d, "EquipItemExtendPrice");
		o.EquipItemExtendSize = BaseJsonMarshaler.ParseValueInt(d, "EquipItemExtendSize");
		o.ProductIndex = BaseJsonMarshaler.ParseArrayCast(d, "ProductIndex", BaseJsonMarshaler.CastToInt);
		o.InventoryType = BaseJsonMarshaler.ParseEnum<InventoryType>(d, "InventoryType");
		return o;
	}

	public static string EncodeJsonString(InventoryExtendData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(InventoryExtendData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("EquipItemExtendCount", o.EquipItemExtendCount.ToString());
		dictionary.Add("EquipItemExtendPrice", o.EquipItemExtendPrice.ToString());
		dictionary.Add("EquipItemExtendSize", o.EquipItemExtendSize.ToString());
		dictionary.Add("ProductIndex", BaseJsonMarshaler.EncodeArray(o.ProductIndex));
		dictionary.Add("InventoryType", o.InventoryType.ToString());
		return dictionary;
	}
}
