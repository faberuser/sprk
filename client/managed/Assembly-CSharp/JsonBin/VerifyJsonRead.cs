using System.Collections.Generic;

namespace JsonBin;

public class VerifyJsonRead
{
	public static int VerifyCount;

	public static bool VerifyBegin(JsonRead lhs, JsonRead rhs)
	{
		VerifyCount = 0;
		return Verify(lhs, rhs);
	}

	private static bool Verify(JsonRead lhs, JsonRead rhs)
	{
		VerifyCount++;
		if (lhs.GetJsonType() != rhs.GetJsonType())
		{
			return false;
		}
		switch (lhs.GetJsonType())
		{
		case JsonType.None:
			return true;
		case JsonType.Boolean:
		{
			if (lhs.TryGetBoolean(out var value3) != rhs.TryGetBoolean(out var value4))
			{
				return false;
			}
			if (value3 != value4)
			{
				return false;
			}
			return true;
		}
		case JsonType.Long:
		{
			if (lhs.TryGetLong(out var value) != rhs.TryGetLong(out var value2))
			{
				return false;
			}
			if (value != value2)
			{
				return false;
			}
			return true;
		}
		case JsonType.ULong:
		{
			if (lhs.TryGetULong(out var value9) != rhs.TryGetULong(out var value10))
			{
				return false;
			}
			if (value9 != value10)
			{
				return false;
			}
			return true;
		}
		case JsonType.Float:
		{
			if (lhs.TryGetFloat(out var value5) != rhs.TryGetFloat(out var value6))
			{
				return false;
			}
			if (value5 != value6)
			{
				return false;
			}
			return true;
		}
		case JsonType.String:
		{
			if (lhs.TryGetString(out var value7) != rhs.TryGetString(out var value8))
			{
				return false;
			}
			if (value7 != value8)
			{
				return false;
			}
			return true;
		}
		case JsonType.Array:
		{
			int count3 = lhs.Count;
			int count4 = rhs.Count;
			if (count3 != count4)
			{
				return false;
			}
			for (int j = 0; j < count3; j++)
			{
				if (!Verify(lhs.GetFromArray(j), rhs.GetFromArray(j)))
				{
					return false;
				}
			}
			return true;
		}
		case JsonType.Object:
		{
			int count = lhs.Count;
			int count2 = rhs.Count;
			if (count != count2)
			{
				return false;
			}
			List<JsonRead.JsonReadFieldInfo> fieldInfoList = lhs.GetFieldInfoList();
			List<JsonRead.JsonReadFieldInfo> fieldInfoList2 = rhs.GetFieldInfoList();
			if (__Extension_Method_ICollection__.COUNT(fieldInfoList) != __Extension_Method_ICollection__.COUNT(fieldInfoList2))
			{
				return false;
			}
			for (int i = 0; i < fieldInfoList.Count; i++)
			{
				JsonRead.JsonReadFieldInfo jsonReadFieldInfo = fieldInfoList[i];
				JsonRead.JsonReadFieldInfo jsonReadFieldInfo2 = fieldInfoList2[i];
				if (jsonReadFieldInfo.Key != jsonReadFieldInfo2.Key)
				{
					return false;
				}
				if (jsonReadFieldInfo.ValueLength != jsonReadFieldInfo2.ValueLength)
				{
					return false;
				}
				if (!Verify(jsonReadFieldInfo.ValueJsonRead, jsonReadFieldInfo2.ValueJsonRead))
				{
					return false;
				}
			}
			return true;
		}
		default:
			return false;
		}
	}
}
