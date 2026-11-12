using UnityEngine;

namespace LitJson.NInternal;

public static class AnimationCurveBuilder
{
	private const long Version_20151117 = 20151117L;

	public static JsonData MakeJson(AnimationCurve value)
	{
		return MakeJson_20151117(value);
	}

	public static AnimationCurve LoadFromJson(JsonData json)
	{
		if (!json.IsObject())
		{
			return null;
		}
		long lONG = json["Version"].GETLONG(0L);
		return LoadFromJson_20151117(json, lONG);
	}

	private static AnimationCurve LoadFromJson_20151117(JsonData json, long version)
	{
		if (version != 20151117)
		{
			return LoadFromJson_Old(json);
		}
		JsonData jsonData = json["preWrapMode"];
		JsonData jsonData2 = json["postWrapMode"];
		if (jsonData.isNone() || jsonData2.isNone())
		{
			return null;
		}
		JsonData jsonData3 = json["inTangentArray"];
		JsonData jsonData4 = json["outTangentArray"];
		JsonData jsonData5 = json["tangentModeArray"];
		JsonData jsonData6 = json["timeArray"];
		JsonData jsonData7 = json["valueArray"];
		if (!jsonData3.IsArray() || !jsonData4.IsArray() || !jsonData5.IsArray() || !jsonData6.IsArray() || !jsonData7.IsArray())
		{
			return null;
		}
		if (jsonData3.Count != jsonData4.Count || jsonData3.Count != jsonData5.Count || jsonData3.Count != jsonData6.Count || jsonData3.Count != jsonData7.Count)
		{
			return null;
		}
		Keyframe[] array = new Keyframe[jsonData3.Count];
		for (int i = 0; i < jsonData3.Count; i++)
		{
			JsonData fromArray = jsonData3.GetFromArray(i);
			if (!fromArray.TryGetFloat(out var o_value))
			{
				o_value = ((!fromArray.IsString() || !(fromArray.valueStr == "Infinity")) ? 0f : float.PositiveInfinity);
			}
			array[i].inTangent = o_value;
		}
		for (int j = 0; j < jsonData4.Count; j++)
		{
			JsonData fromArray2 = jsonData4.GetFromArray(j);
			if (!fromArray2.TryGetFloat(out var o_value2))
			{
				o_value2 = ((!fromArray2.IsString() || !(fromArray2.valueStr == "Infinity")) ? 0f : float.PositiveInfinity);
			}
			array[j].outTangent = o_value2;
		}
		for (int k = 0; k < jsonData5.Count; k++)
		{
			array[k].tangentMode = jsonData5.GetFromArray(k).GETINT(1);
		}
		for (int l = 0; l < jsonData6.Count; l++)
		{
			array[l].time = jsonData6.GetFromArray(l).GETFLOAT(0f);
		}
		for (int m = 0; m < jsonData7.Count; m++)
		{
			array[m].value = jsonData7.GetFromArray(m).GETFLOAT(0f);
		}
		return new AnimationCurve(array)
		{
			preWrapMode = (WrapMode)jsonData.valueInt,
			postWrapMode = (WrapMode)jsonData2.valueInt
		};
	}

	private static JsonData MakeJson_20151117(AnimationCurve value)
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldLong("Version", 20151117L);
		if (value == null)
		{
			return jsonData;
		}
		jsonData.AddFieldLong("preWrapMode", (long)value.preWrapMode);
		jsonData.AddFieldLong("postWrapMode", (long)value.postWrapMode);
		JsonData jsonData2 = new JsonData();
		JsonData jsonData3 = new JsonData();
		JsonData jsonData4 = new JsonData();
		JsonData jsonData5 = new JsonData();
		JsonData jsonData6 = new JsonData();
		jsonData2.ReserveArrayCapacity(value.length);
		jsonData3.ReserveArrayCapacity(value.length);
		jsonData4.ReserveArrayCapacity(value.length);
		jsonData5.ReserveArrayCapacity(value.length);
		jsonData6.ReserveArrayCapacity(value.length);
		for (int i = 0; i < value.length; i++)
		{
			Keyframe keyframe = value[i];
			if (float.IsInfinity(keyframe.inTangent))
			{
				jsonData2.Add(new JsonData("Infinity"));
			}
			else
			{
				jsonData2.Add(new JsonData(keyframe.inTangent));
			}
			if (float.IsInfinity(keyframe.outTangent))
			{
				jsonData3.Add(new JsonData("Infinity"));
			}
			else
			{
				jsonData3.Add(new JsonData(keyframe.outTangent));
			}
			jsonData4.Add(new JsonData(keyframe.tangentMode));
			jsonData5.Add(new JsonData(keyframe.time));
			jsonData6.Add(new JsonData(keyframe.value));
		}
		jsonData.AddField("inTangentArray", jsonData2);
		jsonData.AddField("outTangentArray", jsonData3);
		jsonData.AddField("tangentModeArray", jsonData4);
		jsonData.AddField("timeArray", jsonData5);
		jsonData.AddField("valueArray", jsonData6);
		return jsonData;
	}

	private static AnimationCurve LoadFromJson_Old(JsonData json)
	{
		if (!json.IsObject())
		{
			return null;
		}
		JsonData jsonData = json["keys"];
		JsonData jsonData2 = json["preWrapMode"];
		JsonData jsonData3 = json["postWrapMode"];
		if (jsonData2.isNone() || jsonData3.isNone())
		{
			return null;
		}
		AnimationCurve animationCurve;
		if (jsonData.isNone())
		{
			animationCurve = new AnimationCurve();
		}
		else
		{
			if (!jsonData.IsArray())
			{
				return null;
			}
			Keyframe[] array = new Keyframe[jsonData.Count];
			for (int i = 0; i < jsonData.Count; i++)
			{
				JsonData fromArray = jsonData.GetFromArray(i);
				if (!fromArray.IsObject())
				{
					return null;
				}
				JsonData jsonData4 = fromArray["inTangent"];
				JsonData jsonData5 = fromArray["outTangent"];
				if (jsonData4.IsString() && jsonData4.valueStr == "Infinity")
				{
					array[i].inTangent = float.PositiveInfinity;
				}
				else
				{
					array[i].inTangent = jsonData4.GETFLOAT(0f);
				}
				if (jsonData5.IsString() && jsonData5.valueStr == "Infinity")
				{
					array[i].outTangent = float.PositiveInfinity;
				}
				else
				{
					array[i].outTangent = jsonData5.GETFLOAT(0f);
				}
				array[i].tangentMode = fromArray["tangentMode"].GETINT(1);
				array[i].time = fromArray["time"].GETFLOAT(0f);
				array[i].value = fromArray["value"].GETFLOAT(0f);
			}
			animationCurve = new AnimationCurve(array);
		}
		animationCurve.preWrapMode = (WrapMode)jsonData2.valueInt;
		animationCurve.postWrapMode = (WrapMode)jsonData3.valueInt;
		return animationCurve;
	}

	private static JsonData MakeJson_Old(AnimationCurve value)
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldLong("preWrapMode", (long)value.preWrapMode);
		jsonData.AddFieldLong("postWrapMode", (long)value.postWrapMode);
		JsonData jsonData2 = new JsonData();
		for (int i = 0; i < value.length; i++)
		{
			Keyframe keyframe = value[i];
			JsonData jsonData3 = new JsonData();
			if (float.IsInfinity(keyframe.inTangent))
			{
				jsonData3.AddFieldString("inTangent", "Infinity");
			}
			else
			{
				jsonData3.AddFieldDouble("inTangent", keyframe.inTangent);
			}
			if (float.IsInfinity(keyframe.outTangent))
			{
				jsonData3.AddFieldString("outTangent", "Infinity");
			}
			else
			{
				jsonData3.AddFieldDouble("outTangent", keyframe.outTangent);
			}
			jsonData3.AddFieldLong("tangentMode", keyframe.tangentMode);
			jsonData3.AddFieldDouble("time", keyframe.time);
			jsonData3.AddFieldDouble("value", keyframe.value);
			jsonData2.Add(jsonData3);
		}
		jsonData.AddField("keys", jsonData2);
		return jsonData;
	}
}
