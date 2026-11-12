using System.Collections.Generic;
using LitJson;

namespace NVespa;

public static class PoolingListUtil
{
	public static bool LoadFromJson<T>(ref PoolingList<T> list, JsonData json) where T : new()
	{
		if (!json.IsArray())
		{
			return false;
		}
		if (json.Count == 0)
		{
			return false;
		}
		List<T> list2 = list.PopList(forceCreate: true);
		bool result = list2.LoadFromJson(json);
		list.PushList(list2);
		return result;
	}
}
