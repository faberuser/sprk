using LitJson;
using NGame2.NGameSetting.NUi;
using UnityEngine;

namespace NGame2.NGameSetting;

public class UiData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public float CoinIncreaseTime = 1f;

	public GetItemAnimationData GetItemAnimation = new GetItemAnimationData();

	public GetItemAnimationData GetRareItemAnimation = new GetItemAnimationData();

	public GetItemAnimationData GetUniqueItemAnimation = new GetItemAnimationData();

	public GetItemAnimationData GetUniqueItemAwesome = new GetItemAnimationData();

	public GetItemAnimationData GetUniqueItemPerfect = new GetItemAnimationData();

	public GetItemAnimationData GetClassUniqueItemAnimation = new GetItemAnimationData();

	public PartySettingData PartySetting = new PartySettingData();

	public LoginSettingData LoginSetting = new LoginSettingData();

	public Hero3dViewData Hero3dView = new Hero3dViewData();

	public HeroDetailViewData HeroDetailView = new HeroDetailViewData();

	public float PurifyHero_SceneFOV = 45f;

	public Vector3 PurifyHero_CameraTarget = new Vector3(0f, 1.2f, 0f);

	public Vector3 PurifyHero_CameraPos = new Vector3(0.386f, 1.488f, -1.598f);

	public float PurifyHero_GaugeScrollSpeed = 0.2f;

	public float PurifyHero_TrailTimeLength = 1f;

	public float PurifyHero_TrailBeginForward = 0.1f;

	public float PurifyHero_TrailBeginUp = -0.025f;

	public AnimationCurve PurifyHero_TrailUpCurve = new AnimationCurve();

	public float PurifyHero_TrailUpCurveLength = 1f;

	public AnimationCurve PurifyHero_TrailRightCurve = new AnimationCurve();

	public float PurifyHero_TrailRightCurveLength = 1f;

	public float PurifyHero_AwakeCameraTimeLength = 2f;

	public AnimationCurve PurifyHero_AwakeCameraRotate = new AnimationCurve();

	public float PurifyHero_AwakeCameraRotateSpeed = 360f;

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsObject())
		{
			return false;
		}
		CoinIncreaseTime = json["CoinIncreaseTime"].GETFLOAT(CoinIncreaseTime);
		GetItemAnimation.LOADFROMJSON(json["GetItemAnimation"]);
		GetRareItemAnimation.LOADFROMJSON(json["GetRareItemAnimation"]);
		GetUniqueItemAnimation.LOADFROMJSON(json["GetUniqueItemAnimation"]);
		GetUniqueItemAwesome.LOADFROMJSON(json["GetUniqueItemAwesome"]);
		GetUniqueItemPerfect.LOADFROMJSON(json["GetUniqueItemPerfect"]);
		GetClassUniqueItemAnimation.LOADFROMJSON(json["GetClassUniqueItemAnimation"]);
		PartySetting.LOADFROMJSON(json["PartySetting"]);
		LoginSetting.LOADFROMJSON(json["LoginSetting"]);
		Hero3dView.LOADFROMJSON(json["Hero3dView"]);
		HeroDetailView.LOADFROMJSON(json["HeroDetailView"]);
		PurifyHero_SceneFOV = json["PurifyHero_SceneFOV"].GETFLOAT(PurifyHero_SceneFOV);
		PurifyHero_CameraTarget = json["PurifyHero_CameraTarget"].GETVECTOR3(PurifyHero_CameraTarget);
		PurifyHero_CameraPos = json["PurifyHero_CameraPos"].GETVECTOR3(PurifyHero_CameraPos);
		PurifyHero_GaugeScrollSpeed = json["PurifyHero_GaugeScrollSpeed"].GETFLOAT(PurifyHero_GaugeScrollSpeed);
		PurifyHero_TrailTimeLength = json["PurifyHero_TrailTimeLength"].GETFLOAT(PurifyHero_TrailTimeLength);
		PurifyHero_TrailBeginForward = json["PurifyHero_TrailBeginForward"].GETFLOAT(PurifyHero_TrailBeginForward);
		PurifyHero_TrailBeginUp = json["PurifyHero_TrailBeginUp"].GETFLOAT(PurifyHero_TrailBeginUp);
		PurifyHero_TrailUpCurve = json["PurifyHero_TrailUpCurve"].GetAnimationCurve(PurifyHero_TrailUpCurve);
		PurifyHero_TrailUpCurveLength = json["PurifyHero_TrailUpCurveLength"].GETFLOAT(PurifyHero_TrailUpCurveLength);
		PurifyHero_TrailRightCurve = json["PurifyHero_TrailRightCurve"].GetAnimationCurve(PurifyHero_TrailRightCurve);
		PurifyHero_TrailRightCurveLength = json["PurifyHero_TrailRightCurveLength"].GETFLOAT(PurifyHero_TrailRightCurveLength);
		PurifyHero_AwakeCameraTimeLength = json["PurifyHero_AwakeCameraTimeLength"].GETFLOAT(PurifyHero_AwakeCameraTimeLength);
		PurifyHero_AwakeCameraRotate = json["PurifyHero_AwakeCameraRotate"].GetAnimationCurve(PurifyHero_AwakeCameraRotate);
		PurifyHero_AwakeCameraRotateSpeed = json["PurifyHero_AwakeCameraRotateSpeed"].GETFLOAT(PurifyHero_AwakeCameraRotateSpeed);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldDouble("CoinIncreaseTime", CoinIncreaseTime);
		jsonData.AddFieldJsonMake("GetItemAnimation", GetItemAnimation);
		jsonData.AddFieldJsonMake("GetRareItemAnimation", GetRareItemAnimation);
		jsonData.AddFieldJsonMake("GetUniqueItemAnimation", GetUniqueItemAnimation);
		jsonData.AddFieldJsonMake("GetUniqueItemAwesome", GetUniqueItemAwesome);
		jsonData.AddFieldJsonMake("GetUniqueItemPerfect", GetUniqueItemPerfect);
		jsonData.AddFieldJsonMake("GetClassUniqueItemAnimation", GetClassUniqueItemAnimation);
		jsonData.AddFieldJsonMake("PartySetting", PartySetting);
		jsonData.AddFieldJsonMake("LoginSetting", LoginSetting);
		jsonData.AddFieldJsonMake("Hero3dView", Hero3dView);
		jsonData.AddFieldJsonMake("HeroDetailView", HeroDetailView);
		jsonData.AddFieldDouble("PurifyHero_SceneFOV", PurifyHero_SceneFOV);
		jsonData.AddFieldVector3("PurifyHero_CameraTarget", PurifyHero_CameraTarget);
		jsonData.AddFieldVector3("PurifyHero_CameraPos", PurifyHero_CameraPos);
		jsonData.AddFieldDouble("PurifyHero_GaugeScrollSpeed", PurifyHero_GaugeScrollSpeed);
		jsonData.AddFieldDouble("PurifyHero_TrailTimeLength", PurifyHero_TrailTimeLength);
		jsonData.AddFieldDouble("PurifyHero_TrailBeginForward", PurifyHero_TrailBeginForward);
		jsonData.AddFieldDouble("PurifyHero_TrailBeginUp", PurifyHero_TrailBeginUp);
		jsonData.AddFieldAnimationCurve("PurifyHero_TrailUpCurve", PurifyHero_TrailUpCurve);
		jsonData.AddFieldDouble("PurifyHero_TrailUpCurveLength", PurifyHero_TrailUpCurveLength);
		jsonData.AddFieldAnimationCurve("PurifyHero_TrailRightCurve", PurifyHero_TrailRightCurve);
		jsonData.AddFieldDouble("PurifyHero_TrailRightCurveLength", PurifyHero_TrailRightCurveLength);
		jsonData.AddFieldDouble("PurifyHero_AwakeCameraTimeLength", PurifyHero_AwakeCameraTimeLength);
		jsonData.AddFieldAnimationCurve("PurifyHero_AwakeCameraRotate", PurifyHero_AwakeCameraRotate);
		jsonData.AddFieldDouble("PurifyHero_AwakeCameraRotateSpeed", PurifyHero_AwakeCameraRotateSpeed);
		return jsonData;
	}
}
