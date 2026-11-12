using System.Collections.Generic;
using LitJson;
using NVespa;

namespace NGame2.NNodeTree;

public class StringKey
{
	public class EqualityComparer : IEqualityComparer<StringKey>
	{
		public bool Equals(StringKey x, StringKey y)
		{
			return x.Index == y.Index;
		}

		public int GetHashCode(StringKey obj)
		{
			return obj.Index;
		}
	}

	public static EqualityComparer Comparer;

	public static StringKey Empty;

	public static IncreaseDic<string, StringKey> Dic;

	public string Value { get; internal set; }

	public int Index { get; internal set; }

	internal StringKey()
	{
	}

	public override string ToString()
	{
		return Value;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is StringKey stringKey))
		{
			return false;
		}
		return Index == stringKey.Index;
	}

	public override int GetHashCode()
	{
		return Index;
	}

	public static StringKey Get(string value)
	{
		if (!Dic.TryGetValue(value, out var o_value))
		{
			o_value = new StringKey();
			o_value.Value = value;
			o_value.Index = Dic.Count + 1;
			Dic.Add(value, o_value);
		}
		return o_value;
	}

	public static StringKey CreateFromJson(JsonData json)
	{
		string sTRING = json.GETSTRING(string.Empty);
		if (String_Extension_Method.IsNullOrEmpty(sTRING))
		{
			return Empty;
		}
		return Get(sTRING);
	}

	public static JsonData MakeJson(StringKey key)
	{
		return new JsonData(key.Value);
	}

	static StringKey()
	{
		Comparer = new EqualityComparer();
		Empty = new StringKey
		{
			Value = string.Empty,
			Index = 0
		};
		Dic = new IncreaseDic<string, StringKey>();
		Get("Myself");
		Get("Target");
		Get("TargetPosition");
		Get("Direction");
		Get("BeginTime");
		Get("CurrentTime");
		Get("ElapsedTime");
		Get("DurationTime");
		Get("Frame");
		Get("FlyingDurationTime");
		Get("FlyingTarget");
		Get("OperationUid");
		Get("Dummy01");
		Get("FlowNumber");
		Get("CostumeIndex");
		Get("SoulWeaponGrade");
		Get("FieldMapCreature");
		Get("ItemIndex");
	}
}
