using System.Collections.Generic;
using JsonBin;
using LitJson;

namespace NGameAsset.NCreature;

public class AnimationBlendDefault : JsonBin.ILoadJson, IBuilder, LitJson.ILoadJson, IJsonInterface, IMakeJson
{
	public struct BlendKey
	{
		public class Comparer : IEqualityComparer<BlendKey>
		{
			public static Comparer Default = new Comparer();

			bool IEqualityComparer<BlendKey>.Equals(BlendKey x, BlendKey y)
			{
				return (x.MotionFrom == y.MotionFrom) & (x.MotionTo == y.MotionTo);
			}

			int IEqualityComparer<BlendKey>.GetHashCode(BlendKey obj)
			{
				return (int)(obj.MotionFrom + (int)obj.MotionTo * 7777);
			}
		}

		public DefineCreature.EMotion MotionFrom { get; private set; }

		public DefineCreature.EMotion MotionTo { get; private set; }

		public BlendKey(DefineCreature.EMotion from, DefineCreature.EMotion to)
		{
			this = default(BlendKey);
			MotionFrom = from;
			MotionTo = to;
		}

		public static BlendKey CreateFromJson(JsonRead json)
		{
			BlendKey result = default(BlendKey);
			result.MotionFrom = json.GetJsonAny("MotionFrom", "From").GETENUM(result.MotionFrom);
			result.MotionTo = json.GetJsonAny("MotionTo", "To").GETENUM(result.MotionTo);
			return result;
		}

		public static BlendKey CreateFromLitJson(JsonData json)
		{
			BlendKey result = default(BlendKey);
			result.MotionFrom = json.GetJsonAny("MotionFrom", "From").GETENUM(result.MotionFrom);
			result.MotionTo = json.GetJsonAny("MotionTo", "To").GETENUM(result.MotionTo);
			return result;
		}

		public static JsonData MakeJson(BlendKey key)
		{
			JsonData jsonData = new JsonData();
			jsonData.AddFieldEnum("MotionFrom", key.MotionFrom);
			jsonData.AddFieldEnum("MotionTo", key.MotionTo);
			return jsonData;
		}
	}

	public Dictionary<BlendKey, float> TimeDic = new Dictionary<BlendKey, float>(new BlendKey.Comparer());

	public bool TryBlendTime(DefineCreature.EMotion motionFrom, DefineCreature.EMotion motionTo, out float blendTime)
	{
		BlendKey key = new BlendKey(motionFrom, motionTo);
		return TimeDic.TryGetValue(key, out blendTime);
	}

	bool JsonBin.ILoadJson.Load(JsonRead json)
	{
		TimeDic.LoadDictionaryFromJson(json["TimeDic"]);
		return true;
	}

	bool LitJson.ILoadJson.Load(JsonData json)
	{
		TimeDic.LoadFromJson(json["TimeDic"]);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddField("TimeDic", Json_Extension_Method_IDictionary.MakeJson(TimeDic));
		return jsonData;
	}
}
