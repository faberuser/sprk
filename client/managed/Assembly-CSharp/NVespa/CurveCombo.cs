using System;
using LitJson;
using UnityEngine;

namespace NVespa;

[Serializable]
public class CurveCombo : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public enum EType
	{
		Multiply,
		Add
	}

	public EType m_type;

	public VespaList<AnimationCurve> m_list = new VespaList<AnimationCurve>();

	public float Evaluate(float i_frame, float i_default)
	{
		if (!TryEvaluate(i_frame, out var o_value))
		{
			return i_default;
		}
		return o_value;
	}

	public bool TryEvaluate(float i_frame, out float o_value)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(m_list))
		{
			o_value = 0f;
			return false;
		}
		if (m_type == EType.Multiply)
		{
			o_value = 1f;
		}
		else
		{
			if (m_type != EType.Add)
			{
				o_value = 0f;
				return false;
			}
			o_value = 0f;
		}
		bool result = false;
		for (int i = 0; i < m_list.Count; i++)
		{
			AnimationCurve animationCurve = m_list[i];
			if (Vespa_Unity_AnimationCurve_Extension_Method.IsNullOrEmpty(animationCurve))
			{
				continue;
			}
			if (m_type == EType.Multiply)
			{
				o_value *= animationCurve.Evaluate(i_frame);
			}
			else
			{
				if (m_type != EType.Add)
				{
					o_value = 0f;
					return false;
				}
				o_value += animationCurve.Evaluate(i_frame);
			}
			result = true;
		}
		return result;
	}

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsObject())
		{
			return false;
		}
		JsonData jsonAny = json.GetJsonAny("CurveArray", "curve_array");
		if (jsonAny.IsArray())
		{
			for (int i = 0; i < jsonAny.Count; i++)
			{
				AnimationCurve animationCurve = JsonTools.ToCurve(jsonAny.GetFromArray(i), null);
				if (animationCurve != null)
				{
					m_list.Add(animationCurve);
				}
			}
		}
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		for (int i = 0; i < m_list.Count; i++)
		{
			jsonData.Add(JsonTools.FromCurve(m_list[i]));
		}
		JsonData jsonData2 = new JsonData();
		jsonData2.AddField("CurveArray", jsonData);
		return jsonData2;
	}
}
