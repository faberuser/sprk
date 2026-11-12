using System;
using System.Collections.Generic;
using LitJson.NInternal;
using UnityEngine;

namespace LitJson;

public class JsonTools : MonoBehaviour
{
	public static void ToList<T>(JsonData i_json, IList<T> i_list, Func<T> createFunc) where T : ILoadJson
	{
		i_list.Clear();
		AddToList(i_json, i_list, createFunc);
	}

	public static void AddToList<T>(JsonData i_json, IList<T> i_list, Func<T> createFunc) where T : ILoadJson
	{
		if (!i_json.IsArray())
		{
			return;
		}
		for (int i = 0; i < i_json.Count; i++)
		{
			T val = Vespa_System_Action_Extension_Method.fire(createFunc);
			if (val != null && val.Load(i_json.GetFromArray(i)))
			{
				i_list.Add(val);
			}
		}
	}

	public static void ToList<T>(JsonData i_json, IList<T> i_list) where T : ILoadJson, new()
	{
		i_list.Clear();
		AddToList(i_json, i_list);
	}

	public static void AddToList<T>(JsonData i_json, IList<T> i_list) where T : ILoadJson, new()
	{
		if (!i_json.IsArray())
		{
			return;
		}
		for (int i = 0; i < i_json.Count; i++)
		{
			T item = new T();
			if (item.Load(i_json.GetFromArray(i)))
			{
				i_list.Add(item);
			}
		}
	}

	public static JsonData FromList<T>(IList<T> i_list) where T : IMakeJson
	{
		JsonData jsonData = new JsonData();
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(i_list))
		{
			for (int i = 0; i < i_list.Count; i++)
			{
				jsonData.Add(i_list[i].MakeJson());
			}
		}
		return jsonData;
	}

	public static JsonData FromColor(Color i_value)
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldDouble("r", i_value.r);
		jsonData.AddFieldDouble("g", i_value.g);
		jsonData.AddFieldDouble("b", i_value.b);
		jsonData.AddFieldDouble("a", i_value.a);
		return jsonData;
	}

	public static bool TryToColor(JsonData i_json, out Color o_value)
	{
		if (!i_json.IsObject())
		{
			o_value = Color.white;
			return false;
		}
		JsonData jsonData = i_json["r"];
		JsonData jsonData2 = i_json["g"];
		JsonData jsonData3 = i_json["b"];
		JsonData jsonData4 = i_json["a"];
		if (jsonData.isNone() || jsonData2.isNone() || jsonData3.isNone() || jsonData4.isNone())
		{
			o_value = Color.white;
			return false;
		}
		o_value.r = (float)jsonData.valueDouble;
		o_value.g = (float)jsonData2.valueDouble;
		o_value.b = (float)jsonData3.valueDouble;
		o_value.a = (float)jsonData4.valueDouble;
		return true;
	}

	public static Color ToColor(JsonData i_json, Color i_default)
	{
		if (!TryToColor(i_json, out var o_value))
		{
			return i_default;
		}
		return o_value;
	}

	public static JsonData FromVector2(Vector2 i_value)
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldDouble("x", i_value.x);
		jsonData.AddFieldDouble("y", i_value.y);
		return jsonData;
	}

	public static bool TryToVector2(JsonData i_json, out Vector2 o_value)
	{
		if (!i_json.IsObject())
		{
			o_value = Vector2.zero;
			return false;
		}
		JsonData jsonData = i_json["x"];
		JsonData jsonData2 = i_json["y"];
		if (jsonData.isNone() || jsonData2.isNone())
		{
			o_value = Vector2.zero;
			return false;
		}
		o_value.x = (float)jsonData.valueDouble;
		o_value.y = (float)jsonData2.valueDouble;
		return true;
	}

	public static Vector2 ToVector2(JsonData i_json, Vector2 i_default)
	{
		if (!TryToVector2(i_json, out var o_value))
		{
			return i_default;
		}
		return o_value;
	}

	public static JsonData FromVector3(Vector3 i_value)
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldDouble("x", i_value.x);
		jsonData.AddFieldDouble("y", i_value.y);
		jsonData.AddFieldDouble("z", i_value.z);
		return jsonData;
	}

	public static bool TryToVector3(JsonData i_json, out Vector3 o_value)
	{
		if (!i_json.IsObject())
		{
			o_value = Vector3.zero;
			return false;
		}
		JsonData jsonData = i_json["x"];
		JsonData jsonData2 = i_json["y"];
		JsonData jsonData3 = i_json["z"];
		if (jsonData.isNone() || jsonData2.isNone() || jsonData3.isNone())
		{
			o_value = Vector3.zero;
			return false;
		}
		o_value.x = (float)jsonData.valueDouble;
		o_value.y = (float)jsonData2.valueDouble;
		o_value.z = (float)jsonData3.valueDouble;
		return true;
	}

	public static Vector3 ToVector3(JsonData i_json, Vector3 i_default)
	{
		if (!TryToVector3(i_json, out var o_value))
		{
			return i_default;
		}
		return o_value;
	}

	public static JsonData FromVector4(Vector4 i_value)
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldDouble("x", i_value.x);
		jsonData.AddFieldDouble("y", i_value.y);
		jsonData.AddFieldDouble("z", i_value.z);
		jsonData.AddFieldDouble("w", i_value.w);
		return jsonData;
	}

	public static bool TryToVector4(JsonData i_json, out Vector4 o_value)
	{
		if (!i_json.IsObject())
		{
			o_value = Vector4.zero;
			return false;
		}
		o_value.x = i_json["x"].GETFLOAT(0f);
		o_value.y = i_json["y"].GETFLOAT(0f);
		o_value.z = i_json["z"].GETFLOAT(0f);
		o_value.w = i_json["w"].GETFLOAT(0f);
		return true;
	}

	public static Vector4 ToVector4(JsonData i_json, Vector4 i_default)
	{
		if (!TryToVector4(i_json, out var o_value))
		{
			return i_default;
		}
		return o_value;
	}

	public static JsonData FromVectorXZ(VectorXZ i_value)
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldDouble("x", i_value.x);
		jsonData.AddFieldDouble("z", i_value.z);
		return jsonData;
	}

	public static bool TryToVectorXZ(JsonData i_json, out VectorXZ o_value)
	{
		if (!i_json.IsObject())
		{
			o_value = VectorXZ.zero;
			return false;
		}
		JsonData jsonData = i_json["x"];
		JsonData jsonData2 = i_json["z"];
		if (jsonData.isNone() || jsonData2.isNone())
		{
			o_value = VectorXZ.zero;
			return false;
		}
		o_value.x = (float)jsonData.valueDouble;
		o_value.z = (float)jsonData2.valueDouble;
		return true;
	}

	public static VectorXZ ToVectorXZ(JsonData i_json, VectorXZ i_default)
	{
		if (!TryToVectorXZ(i_json, out var o_value))
		{
			return i_default;
		}
		return o_value;
	}

	public static JsonData FromRect(Rect value)
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldVector2("pos", value.position);
		jsonData.AddFieldVector2("size", value.size);
		return jsonData;
	}

	public static bool TryToRect(JsonData json, out Rect o_value)
	{
		if (!json.IsObject())
		{
			o_value = default(Rect);
			return false;
		}
		if (!json["pos"].TryGetVector2(out var o_value2) || !json["size"].TryGetVector2(out var o_value3))
		{
			o_value = default(Rect);
			return false;
		}
		o_value = new Rect(o_value2, o_value3);
		return true;
	}

	public static Rect ToRect(JsonData i_json, Rect i_default)
	{
		if (!TryToRect(i_json, out var o_value))
		{
			return i_default;
		}
		return o_value;
	}

	public static JsonData FromQuaternion(Quaternion i_value)
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldDouble("w", i_value.w);
		jsonData.AddFieldDouble("x", i_value.x);
		jsonData.AddFieldDouble("y", i_value.y);
		jsonData.AddFieldDouble("z", i_value.z);
		return jsonData;
	}

	public static bool TryToQuaternion(JsonData i_json, out Quaternion o_value)
	{
		if (!i_json.IsObject())
		{
			o_value = Quaternion.identity;
			return false;
		}
		JsonData jsonData = i_json["w"];
		JsonData jsonData2 = i_json["x"];
		JsonData jsonData3 = i_json["y"];
		JsonData jsonData4 = i_json["z"];
		if (jsonData.isNone() || jsonData2.isNone() || jsonData3.isNone() || jsonData4.isNone())
		{
			o_value = Quaternion.identity;
			return false;
		}
		o_value.w = (float)jsonData.valueDouble;
		o_value.x = (float)jsonData2.valueDouble;
		o_value.y = (float)jsonData3.valueDouble;
		o_value.z = (float)jsonData4.valueDouble;
		return true;
	}

	public static Quaternion ToQuaternion(JsonData i_json, Quaternion i_default)
	{
		if (!TryToQuaternion(i_json, out var o_value))
		{
			return i_default;
		}
		return o_value;
	}

	public static JsonData FromCurve(AnimationCurve i_value)
	{
		return AnimationCurveBuilder.MakeJson(i_value);
	}

	public static AnimationCurve ToCurve(JsonData i_json, AnimationCurve i_default)
	{
		AnimationCurve animationCurve = AnimationCurveBuilder.LoadFromJson(i_json);
		if (animationCurve != null)
		{
			return animationCurve;
		}
		return i_default;
	}

	public static AnimationCurve ToCurve(JsonData i_json)
	{
		AnimationCurve animationCurve = AnimationCurveBuilder.LoadFromJson(i_json);
		if (animationCurve != null)
		{
			return animationCurve;
		}
		return new AnimationCurve();
	}

	public static JsonData FromGradient(Gradient i_value)
	{
		JsonData jsonData = new JsonData();
		if (i_value == null)
		{
			return jsonData;
		}
		JsonData jsonData2 = new JsonData();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(i_value.colorKeys); i++)
		{
			GradientColorKey gradientColorKey = i_value.colorKeys[i];
			JsonData jsonData3 = new JsonData();
			jsonData3.AddField("color", FromColor(gradientColorKey.color));
			jsonData3.AddFieldDouble("time", gradientColorKey.time);
			jsonData2.Add(jsonData3);
		}
		JsonData jsonData4 = new JsonData();
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(i_value.alphaKeys); j++)
		{
			GradientAlphaKey gradientAlphaKey = i_value.alphaKeys[j];
			JsonData jsonData5 = new JsonData();
			jsonData5.AddFieldDouble("alpha", gradientAlphaKey.alpha);
			jsonData5.AddFieldDouble("time", gradientAlphaKey.time);
			jsonData4.Add(jsonData5);
		}
		jsonData.AddField("colorkeys_array", jsonData2);
		jsonData.AddField("alphakeys_array", jsonData4);
		return jsonData;
	}

	public static Gradient ToGradient(JsonData i_json, Gradient i_default)
	{
		Gradient gradient = ToGradientInternal(i_json);
		if (gradient != null)
		{
			return gradient;
		}
		return i_default;
	}

	public static Gradient ToGradient(JsonData i_json)
	{
		Gradient gradient = ToGradientInternal(i_json);
		if (gradient != null)
		{
			return gradient;
		}
		return new Gradient();
	}

	private static Gradient ToGradientInternal(JsonData i_json)
	{
		if (!i_json.IsObject())
		{
			return null;
		}
		JsonData jsonData = i_json["colorkeys_array"];
		JsonData jsonData2 = i_json["alphakeys_array"];
		if (!jsonData.IsArray() || !jsonData2.IsArray())
		{
			return null;
		}
		GradientColorKey[] array = new GradientColorKey[jsonData.Count];
		for (int i = 0; i < jsonData.Count; i++)
		{
			JsonData fromArray = jsonData.GetFromArray(i);
			array[i] = new GradientColorKey
			{
				color = ToColor(fromArray["color"], Color.white),
				time = fromArray["time"].GETFLOAT(0f)
			};
		}
		GradientAlphaKey[] array2 = new GradientAlphaKey[jsonData2.Count];
		for (int j = 0; j < jsonData2.Count; j++)
		{
			JsonData fromArray2 = jsonData2.GetFromArray(j);
			array2[j] = new GradientAlphaKey
			{
				alpha = fromArray2["alpha"].GETFLOAT(1f),
				time = fromArray2["time"].GETFLOAT(0f)
			};
		}
		Gradient gradient = new Gradient();
		gradient.SetKeys(array, array2);
		return gradient;
	}
}
