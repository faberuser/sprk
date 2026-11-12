using LitJson;
using NGameAsset.NWorldMap;
using UnityEngine;

namespace NGame2.NGameSetting;

public class WorldMapData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public class EntryMenuAnimationData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public float BackgroundBlendTime = 0.5f;

		public float PopInterval = 0.1f;

		public float PopTimeLength = 0.1f;

		public AnimationCurve PopCurve = new AnimationCurve();

		bool ILoadJson.Load(JsonData json)
		{
			if (!json.IsObject())
			{
				return false;
			}
			BackgroundBlendTime = json["BackgroundBlendTime"].GETFLOAT(BackgroundBlendTime);
			PopInterval = json["PopInterval"].GETFLOAT(PopInterval);
			PopTimeLength = json["PopTimeLength"].GETFLOAT(PopTimeLength);
			PopCurve = JsonTools.ToCurve(json["PopCurve"], PopCurve);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddFieldDouble("BackgroundBlendTime", BackgroundBlendTime);
			jsonData.AddFieldDouble("PopInterval", PopInterval);
			jsonData.AddFieldDouble("PopTimeLength", PopTimeLength);
			jsonData.AddField("PopCurve", JsonTools.FromCurve(PopCurve));
			return jsonData;
		}
	}

	public class PlayerChaseData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public float Time = 1f;

		public AnimationCurve Curve = new AnimationCurve();

		bool ILoadJson.Load(JsonData json)
		{
			if (!json.IsObject())
			{
				return false;
			}
			Time = json["Time"].GETFLOAT(Time);
			Curve = JsonTools.ToCurve(json["Curve"], Curve);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddFieldDouble("Time", Time);
			jsonData.AddField("Curve", JsonTools.FromCurve(Curve));
			return jsonData;
		}
	}

	public class NodeMenuModeData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public float CameraOrthoSize = 0.3f;

		public Vector3 CameraOffset = new Vector3(100f, 250f, 0f);

		public float CameraAngle = -40f;

		bool ILoadJson.Load(JsonData json)
		{
			if (!json.IsObject())
			{
				return false;
			}
			CameraOrthoSize = json["CameraOrthoSize"].GETFLOAT(CameraOrthoSize);
			CameraOffset = JsonTools.ToVector3(json["CameraOffset"], CameraOffset);
			CameraAngle = json["CameraAngle"].GETFLOAT(CameraAngle);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddFieldDouble("CameraOrthoSize", CameraOrthoSize);
			jsonData.AddField("CameraOffset", JsonTools.FromVector3(CameraOffset));
			jsonData.AddFieldDouble("CameraAngle", CameraAngle);
			return jsonData;
		}
	}

	public class EnterNodeMenuData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public float TimeForCameraBlend = 1f;

		public AnimationCurve CurveForZoom = new AnimationCurve();

		public AnimationCurve CurveForPosition = new AnimationCurve();

		public AnimationCurve CurveForRotation = new AnimationCurve();

		bool ILoadJson.Load(JsonData json)
		{
			if (!json.IsObject())
			{
				return false;
			}
			TimeForCameraBlend = json["TimeForCameraBlend"].GETFLOAT(TimeForCameraBlend);
			CurveForZoom = JsonTools.ToCurve(json["CurveForZoom"], CurveForZoom);
			CurveForPosition = JsonTools.ToCurve(json["CurveForPosition"], CurveForPosition);
			CurveForRotation = JsonTools.ToCurve(json["CurveForRotation"], CurveForRotation);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddFieldDouble("TimeForCameraBlend", TimeForCameraBlend);
			jsonData.AddField("CurveForZoom", JsonTools.FromCurve(CurveForZoom));
			jsonData.AddField("CurveForPosition", JsonTools.FromCurve(CurveForPosition));
			jsonData.AddField("CurveForRotation", JsonTools.FromCurve(CurveForRotation));
			return jsonData;
		}
	}

	public EntryMenuAnimationData EntryMenuAnimation = new EntryMenuAnimationData();

	public PlayerChaseData PlayerChase = new PlayerChaseData();

	public NodeMenuModeData NodeMenuMode = new NodeMenuModeData();

	public EnterNodeMenuData EnterNodeMenu = new EnterNodeMenuData();

	public ScaleData Scale = new ScaleData();

	public float NodeShrinkRatio = 0.7f;

	public float NodeShadowScale = 1.2f;

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsObject())
		{
			return false;
		}
		EntryMenuAnimation.LOADFROMJSON(json.GetJsonAny("EntryMenuAnimation", "CityAnimation"));
		PlayerChase.LOADFROMJSON(json["PlayerChase"]);
		NodeMenuMode.LOADFROMJSON(json.GetJsonAny("NodeMenuMode", "CityMode"));
		EnterNodeMenu.LOADFROMJSON(json.GetJsonAny("EnterNodeMenu", "EnterCity"));
		Scale.LOADFROMJSON(json["Scale"]);
		NodeShrinkRatio = json["NodeShrinkRatio"].GETFLOAT(NodeShrinkRatio);
		NodeShadowScale = json["NodeShadowScale"].GETFLOAT(NodeShadowScale);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldJsonMake("EntryMenuAnimation", EntryMenuAnimation);
		jsonData.AddFieldJsonMake("PlayerChase", PlayerChase);
		jsonData.AddFieldJsonMake("NodeMenuMode", NodeMenuMode);
		jsonData.AddFieldJsonMake("EnterNodeMenu", EnterNodeMenu);
		jsonData.AddFieldJsonMake("Scale", Scale);
		jsonData.AddFieldDouble("NodeShrinkRatio", NodeShrinkRatio);
		jsonData.AddFieldDouble("NodeShadowScale", NodeShadowScale);
		return jsonData;
	}
}
