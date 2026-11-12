using UnityEngine;

namespace JsonBin;

public static class AnimationCurveBuilder
{
	private const long Version_20151117 = 20151117L;

	public static AnimationCurve LoadFromJson(JsonRead json)
	{
		if (!json.IsObject())
		{
			return null;
		}
		long lONG = json["Version"].GETLONG(0L);
		return LoadFromJson_20151117(json, lONG);
	}

	private static AnimationCurve LoadFromJson_20151117(JsonRead json, long version)
	{
		if (version != 20151117)
		{
			return LoadFromJson_Old(json);
		}
		JsonRead jsonRead = json["preWrapMode"];
		JsonRead jsonRead2 = json["postWrapMode"];
		if (jsonRead.IsNone() || jsonRead2.IsNone())
		{
			return null;
		}
		JsonRead jsonRead3 = json["inTangentArray"];
		JsonRead jsonRead4 = json["outTangentArray"];
		JsonRead jsonRead5 = json["tangentModeArray"];
		JsonRead jsonRead6 = json["timeArray"];
		JsonRead jsonRead7 = json["valueArray"];
		if (!jsonRead3.IsArray() || !jsonRead4.IsArray() || !jsonRead5.IsArray() || !jsonRead6.IsArray() || !jsonRead7.IsArray())
		{
			return null;
		}
		if (jsonRead3.Count != jsonRead4.Count || jsonRead3.Count != jsonRead5.Count || jsonRead3.Count != jsonRead6.Count || jsonRead3.Count != jsonRead7.Count)
		{
			return null;
		}
		Keyframe[] array = new Keyframe[jsonRead3.Count];
		for (int i = 0; i < jsonRead3.Count; i++)
		{
			JsonRead fromArray = jsonRead3.GetFromArray(i);
			if (!fromArray.TryGetFloat(out var value))
			{
				value = ((!fromArray.IsString() || !(fromArray.GETSTRING(string.Empty) == "Infinity")) ? 0f : float.PositiveInfinity);
			}
			array[i].inTangent = value;
		}
		for (int j = 0; j < jsonRead4.Count; j++)
		{
			JsonRead fromArray2 = jsonRead4.GetFromArray(j);
			if (!fromArray2.TryGetFloat(out var value2))
			{
				value2 = ((!fromArray2.IsString() || !(fromArray2.GETSTRING(string.Empty) == "Infinity")) ? 0f : float.PositiveInfinity);
			}
			array[j].outTangent = value2;
		}
		for (int k = 0; k < jsonRead5.Count; k++)
		{
			array[k].tangentMode = jsonRead5.GetFromArray(k).GETINT(1);
		}
		for (int l = 0; l < jsonRead6.Count; l++)
		{
			array[l].time = jsonRead6.GetFromArray(l).GETFLOAT(0f);
		}
		for (int m = 0; m < jsonRead7.Count; m++)
		{
			array[m].value = jsonRead7.GetFromArray(m).GETFLOAT(0f);
		}
		return new AnimationCurve(array)
		{
			preWrapMode = (WrapMode)jsonRead.GETINT(0),
			postWrapMode = (WrapMode)jsonRead2.GETINT(0)
		};
	}

	private static AnimationCurve LoadFromJson_Old(JsonRead json)
	{
		if (!json.IsObject())
		{
			return null;
		}
		JsonRead jsonRead = json["keys"];
		JsonRead jsonRead2 = json["preWrapMode"];
		JsonRead jsonRead3 = json["postWrapMode"];
		if (jsonRead2.IsNone() || jsonRead3.IsNone())
		{
			return null;
		}
		AnimationCurve animationCurve;
		if (jsonRead.IsNone())
		{
			animationCurve = new AnimationCurve();
		}
		else
		{
			if (!jsonRead.IsArray())
			{
				return null;
			}
			Keyframe[] array = new Keyframe[jsonRead.Count];
			for (int i = 0; i < jsonRead.Count; i++)
			{
				JsonRead fromArray = jsonRead.GetFromArray(i);
				if (!fromArray.IsObject())
				{
					return null;
				}
				JsonRead jsonRead4 = fromArray["inTangent"];
				JsonRead jsonRead5 = fromArray["outTangent"];
				if (jsonRead4.IsString() && jsonRead4.GETSTRING(string.Empty) == "Infinity")
				{
					array[i].inTangent = float.PositiveInfinity;
				}
				else
				{
					array[i].inTangent = jsonRead4.GETFLOAT(0f);
				}
				if (jsonRead5.IsString() && jsonRead5.GETSTRING(string.Empty) == "Infinity")
				{
					array[i].outTangent = float.PositiveInfinity;
				}
				else
				{
					array[i].outTangent = jsonRead5.GETFLOAT(0f);
				}
				array[i].tangentMode = fromArray["tangentMode"].GETINT(1);
				array[i].time = fromArray["time"].GETFLOAT(0f);
				array[i].value = fromArray["value"].GETFLOAT(0f);
			}
			animationCurve = new AnimationCurve(array);
		}
		animationCurve.preWrapMode = (WrapMode)jsonRead2.GETINT(0);
		animationCurve.postWrapMode = (WrapMode)jsonRead3.GETINT(0);
		return animationCurve;
	}
}
