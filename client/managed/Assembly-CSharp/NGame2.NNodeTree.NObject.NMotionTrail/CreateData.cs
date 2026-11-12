using LitJson;
using NGame2.NNodeTree.NUtil;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NNodeTree.NObject.NMotionTrail;

public class CreateData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public enum EKeyType
	{
		SampleFrame,
		LifeTime
	}

	public LinkHeroCode LinkHeroCode = new LinkHeroCode();

	public string SampleName;

	public int BeginFrame;

	public int EndFrame;

	public float LifeTime;

	public Vector3 TopOffset;

	public Vector3 BottomOffset;

	public PrefabResAuto<Material> MaterialAuto = new PrefabResAuto<Material>();

	public EKeyType TexCurveKeyType;

	public AnimationCurve TexCurve = new AnimationCurve();

	public AnimationCurve AlphaByCreateFrame = new AnimationCurve();

	public AnimationCurve AlphaByLifetime = new AnimationCurve();

	public float CalcNoTexCurve(float texUFrame)
	{
		return texUFrame;
	}

	public float CalcTexCurve(float texUFrame)
	{
		return TexCurve.Evaluate(texUFrame);
	}

	public float CalcAlphaNoCurve(float createFrame, float lifetimeFrame)
	{
		return 1f;
	}

	public float CalcAlphaByCreateFrame(float createFrame, float lifetimeFrame)
	{
		return AlphaByCreateFrame.Evaluate(createFrame);
	}

	public float CalcAlphaByLifetime(float createFrame, float lifetimeFrame)
	{
		return AlphaByLifetime.Evaluate(lifetimeFrame);
	}

	public float CalcAlphaByAllCurve(float createFrame, float lifetimeFrame)
	{
		return AlphaByCreateFrame.Evaluate(createFrame) * AlphaByLifetime.Evaluate(lifetimeFrame);
	}

	public void ReservePrefab()
	{
		MaterialAuto.ReservePrefab();
	}

	bool ILoadJson.Load(JsonData json)
	{
		LinkHeroCode.LOADFROMJSON(json["LinkHeroCode"]);
		SampleName = json["SampleName"].GETSTRING(SampleName);
		BeginFrame = json["BeginFrame"].GETINT(BeginFrame);
		EndFrame = json["EndFrame"].GETINT(EndFrame);
		LifeTime = json["LifeTime"].GETFLOAT(LifeTime);
		TopOffset = JsonTools.ToVector3(json["TopOffset"], TopOffset);
		BottomOffset = JsonTools.ToVector3(json["BottomOffset"], BottomOffset);
		MaterialAuto.LOADFROMJSON(json.GetJsonAny("MaterialAuto", "MaterialPath"));
		TexCurveKeyType = json["TexCurveKeyType"].GETENUM(TexCurveKeyType);
		TexCurve = JsonTools.ToCurve(json["TexCurve"], TexCurve);
		AlphaByCreateFrame = JsonTools.ToCurve(json["AlphaByCreateFrame"], AlphaByCreateFrame);
		AlphaByLifetime = JsonTools.ToCurve(json["AlphaByLifetime"], AlphaByLifetime);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldJsonMake("LinkHeroCode", LinkHeroCode);
		jsonData.AddFieldString("SampleName", SampleName);
		jsonData.AddFieldLong("BeginFrame", BeginFrame);
		jsonData.AddFieldLong("EndFrame", EndFrame);
		jsonData.AddFieldDouble("LifeTime", LifeTime);
		jsonData.AddField("TopOffset", JsonTools.FromVector3(TopOffset));
		jsonData.AddField("BottomOffset", JsonTools.FromVector3(BottomOffset));
		jsonData.AddFieldJsonMake("MaterialAuto", MaterialAuto);
		jsonData.AddFieldEnum("TexCurveKeyType", TexCurveKeyType);
		jsonData.AddField("TexCurve", JsonTools.FromCurve(TexCurve));
		jsonData.AddField("AlphaByCreateFrame", JsonTools.FromCurve(AlphaByCreateFrame));
		jsonData.AddField("AlphaByLifetime", JsonTools.FromCurve(AlphaByLifetime));
		return jsonData;
	}
}
