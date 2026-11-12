using LitJson;
using NShader;
using UnityEngine;

namespace NVespa.NGameSharedSetting;

public class ShaderData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public class ShaderVariableData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public Vector3 RimLightDirection = Vector3.back;

		public Color RimLightColor = Color.yellow;

		public Vector3 ShadowDirection = new Vector3(-0.5f, 1f, -1f);

		public Color OutlineColor = Color.black;

		public float OutlineWidth = 0.6f;

		bool ILoadJson.Load(JsonData json)
		{
			RimLightDirection = json["RimLightDirection"].GETVECTOR3(RimLightDirection);
			RimLightColor = json["RimLightColor"].GETCOLOR(RimLightColor);
			ShadowDirection = json["ShadowDirection"].GETVECTOR3(ShadowDirection);
			OutlineColor = json["OutlineColor"].GETCOLOR(OutlineColor);
			OutlineWidth = json["OutlineWidth"].GETFLOAT(OutlineWidth);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddFieldVector3("RimLightDirection", RimLightDirection);
			jsonData.AddFieldColor("RimLightColor", RimLightColor);
			jsonData.AddFieldVector3("ShadowDirection", ShadowDirection);
			jsonData.AddFieldColor("OutlineColor", OutlineColor);
			jsonData.AddFieldDouble("OutlineWidth", OutlineWidth);
			return jsonData;
		}
	}

	public ShaderVariableData ShaderVariable = new ShaderVariableData();

	public ChainFilter CameraFilter_BattleScene = new ChainFilter();

	public ChainFilter CameraFilter_HeroPreview = new ChainFilter();

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsObject())
		{
			return false;
		}
		ShaderVariable.LOADFROMJSON(json["ShaderVariable"]);
		CameraFilter_BattleScene.LOADFROMJSON(json.GetJsonAny("CameraFilter_BattleScene", "Camera_BloomFilter"));
		CameraFilter_HeroPreview.LOADFROMJSON(json["CameraFilter_HeroPreview"]);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldJsonMake("ShaderVariable", ShaderVariable);
		jsonData.AddFieldJsonMake("CameraFilter_BattleScene", CameraFilter_BattleScene);
		jsonData.AddFieldJsonMake("CameraFilter_HeroPreview", CameraFilter_HeroPreview);
		return jsonData;
	}
}
