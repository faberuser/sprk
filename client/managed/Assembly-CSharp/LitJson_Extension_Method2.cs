using LitJson;
using UnityEngine;

public static class LitJson_Extension_Method2
{
	public static void AddFieldColor(this JsonData i_json, string key, Color value)
	{
		i_json.AddField(key, JsonTools.FromColor(value));
	}

	public static bool TryGetColor(this JsonData i_json, out Color o_value)
	{
		return JsonTools.TryToColor(i_json, out o_value);
	}

	public static Color GETCOLOR(this JsonData i_json, Color i_default)
	{
		return JsonTools.ToColor(i_json, i_default);
	}

	public static void AddFieldVector2(this JsonData i_json, string key, Vector2 value)
	{
		i_json.AddField(key, JsonTools.FromVector2(value));
	}

	public static bool TryGetVector2(this JsonData i_json, out Vector2 o_value)
	{
		return JsonTools.TryToVector2(i_json, out o_value);
	}

	public static Vector2 GETVECTOR2(this JsonData i_json, Vector2 i_default)
	{
		return JsonTools.ToVector2(i_json, i_default);
	}

	public static void AddFieldVector3(this JsonData i_json, string key, Vector3 value)
	{
		i_json.AddField(key, JsonTools.FromVector3(value));
	}

	public static bool TryGetVector3(this JsonData i_json, out Vector3 o_value)
	{
		return JsonTools.TryToVector3(i_json, out o_value);
	}

	public static Vector3 GETVECTOR3(this JsonData i_json, Vector3 i_default)
	{
		return JsonTools.ToVector3(i_json, i_default);
	}

	public static void AddFieldVectorXZ(this JsonData i_json, string key, VectorXZ value)
	{
		i_json.AddField(key, JsonTools.FromVectorXZ(value));
	}

	public static bool TryGetVectorXZ(this JsonData i_json, out VectorXZ o_value)
	{
		return JsonTools.TryToVectorXZ(i_json, out o_value);
	}

	public static VectorXZ GETVECTORXZ(this JsonData i_json, VectorXZ i_default)
	{
		return JsonTools.ToVectorXZ(i_json, i_default);
	}

	public static void AddFieldRect(this JsonData i_json, string key, Rect value)
	{
		i_json.AddField(key, JsonTools.FromRect(value));
	}

	public static bool TryGetRect(this JsonData i_json, out Rect o_value)
	{
		return JsonTools.TryToRect(i_json, out o_value);
	}

	public static Rect GETRECT(this JsonData i_json, Rect i_default)
	{
		return JsonTools.ToRect(i_json, i_default);
	}

	public static void AddFieldQuaternion(this JsonData i_json, string key, Quaternion value)
	{
		i_json.AddField(key, JsonTools.FromQuaternion(value));
	}

	public static bool TryGetQuaternion(this JsonData i_json, out Quaternion o_value)
	{
		return JsonTools.TryToQuaternion(i_json, out o_value);
	}

	public static Quaternion GETQUATERNION(this JsonData i_json, Quaternion i_default)
	{
		return JsonTools.ToQuaternion(i_json, i_default);
	}

	public static void AddFieldAnimationCurve(this JsonData i_json, string key, AnimationCurve value)
	{
		i_json.AddField(key, JsonTools.FromCurve(value));
	}

	public static AnimationCurve GetAnimationCurve(this JsonData i_json, AnimationCurve i_default)
	{
		return JsonTools.ToCurve(i_json, i_default);
	}

	public static void AddFieldGradient(this JsonData i_json, string key, Gradient value)
	{
		i_json.AddField(key, JsonTools.FromGradient(value));
	}

	public static Gradient GetGradient(this JsonData i_json, Gradient i_default)
	{
		return JsonTools.ToGradient(i_json, i_default);
	}
}
