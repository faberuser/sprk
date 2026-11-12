using LitJson;
using NGame2.NRender.NRenderFacade;
using NVespa;
using UnityEngine;

namespace NGame2.NCutScene.NModifier;

public class ExtraAlphaModifier : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool IsEnable = true;

	public int BeginTick;

	public int EndTick;

	public AnimationCurve Curve = new AnimationCurve();

	public bool IsEnd { get; private set; }

	public void Begin(CreatureModifier creatureMod)
	{
		if (!IsEnable || creatureMod == null)
		{
			IsEnd = true;
			return;
		}
		creatureMod.RenderFacade.ExtraAlphaController.SetAlpha(ExtraAlphaController.EAlphaType.CutScene, Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(Curve, 0f, 1f));
		IsEnd = false;
	}

	public void Update(CreatureModifier creatureMod, int elapsedTick)
	{
		if (IsEnable && !IsEnd)
		{
			if (BeginTick < EndTick)
			{
				float i_frame = Math.InverseLerpInc(BeginTick, EndTick, elapsedTick);
				float alpha = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(Curve, i_frame, 1f);
				creatureMod.RenderFacade.ExtraAlphaController.SetAlpha(ExtraAlphaController.EAlphaType.CutScene, alpha);
			}
			if (elapsedTick >= EndTick)
			{
				IsEnd = true;
			}
		}
	}

	public void Skip(CreatureModifier creatureMod)
	{
		IsEnd = true;
		if (IsEnable)
		{
			float alpha = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(Curve, 1f, 1f);
			creatureMod.RenderFacade.ExtraAlphaController.SetAlpha(ExtraAlphaController.EAlphaType.CutScene, alpha);
		}
	}

	bool ILoadJson.Load(JsonData json)
	{
		IsEnable = json["IsEnable"].GETBOOLEAN(IsEnable);
		BeginTick = json["BeginTick"].GETINT(BeginTick);
		EndTick = json["EndTick"].GETINT(EndTick);
		Curve = JsonTools.ToCurve(json["Curve"], Curve);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldBool("IsEnable", IsEnable);
		jsonData.AddFieldLong("BeginTick", BeginTick);
		jsonData.AddFieldLong("EndTick", EndTick);
		jsonData.AddField("Curve", JsonTools.FromCurve(Curve));
		return jsonData;
	}
}
