using LitJson;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NCutScene.NModifier;

public class BackgroundColorModifier : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool IsEnable = true;

	public int BeginTick;

	public int EndTick;

	public Gradient ColorGradient = new Gradient();

	public bool IsEnd { get; private set; }

	public void Begin()
	{
		if (!IsEnable)
		{
			IsEnd = true;
			return;
		}
		SceneAutoBehaviour<CutSceneManager>.instance.BackgroundControl.BackgroundColor = Vespa_Unity_Gradient_Extension_Method.EVALUATE(ColorGradient, 0f, Color.white);
		IsEnd = false;
	}

	public void Update(int elapsedTick)
	{
		if (IsEnable && !IsEnd)
		{
			if (BeginTick < EndTick)
			{
				float i_frame = Math.InverseLerpInc(BeginTick, EndTick, elapsedTick);
				Color backgroundColor = Vespa_Unity_Gradient_Extension_Method.EVALUATE(ColorGradient, i_frame, Color.white);
				SceneAutoBehaviour<CutSceneManager>.instance.BackgroundControl.BackgroundColor = backgroundColor;
			}
			if (elapsedTick >= EndTick)
			{
				IsEnd = true;
			}
		}
	}

	public void Skip()
	{
		IsEnd = true;
		if (IsEnable)
		{
			Color backgroundColor = Vespa_Unity_Gradient_Extension_Method.EVALUATE(ColorGradient, 1f, Color.white);
			SceneAutoBehaviour<CutSceneManager>.instance.BackgroundControl.BackgroundColor = backgroundColor;
		}
	}

	bool ILoadJson.Load(JsonData json)
	{
		IsEnable = json["IsEnable"].GETBOOLEAN(IsEnable);
		BeginTick = json["BeginTick"].GETINT(BeginTick);
		EndTick = json["EndTick"].GETINT(EndTick);
		ColorGradient = json["ColorGradient"].GetGradient(ColorGradient);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldBool("IsEnable", IsEnable);
		jsonData.AddFieldLong("BeginTick", BeginTick);
		jsonData.AddFieldLong("EndTick", EndTick);
		jsonData.AddFieldGradient("ColorGradient", ColorGradient);
		return jsonData;
	}
}
