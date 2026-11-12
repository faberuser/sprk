using LitJson;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NCutScene.NScript;

public class PlayCameraShake : IScript, IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public GameUIRoot.ELayerType LayerType = GameUIRoot.ELayerType.CutSceneUI;

	public Vector3 ShakeForce = Vector3.up;

	public float RepeatTime = 0.1f;

	public float LifeTime = 1f;

	public bool IsRandomForce;

	public bool UsePowerByLifeTime;

	public AnimationCurve PowerByLifeTime = new AnimationCurve();

	private ILocalTimer _localTimer;

	private int _elapsedTick;

	private bool _isEnd;

	bool IScript.IsEnd => _isEnd;

	void IScript.Visit(IVisitor visitor)
	{
		visitor.TryVisit(this);
	}

	void IScript.Begin(ILocalTimer localTimer, bool isSkip)
	{
		_localTimer = localTimer;
		SceneAutoBehaviour<CutSceneManager>.instance.GetCameraShake(LayerType).AddForce(0L, ShakeForce, IsRandomForce, RepeatTime, LifeTime, UsePowerByLifeTime ? PowerByLifeTime : null, _localTimer);
		_elapsedTick = 0;
		_isEnd = false;
	}

	void IScript.Update()
	{
		if (_localTimer == null)
		{
			_isEnd = true;
			return;
		}
		_elapsedTick += _localTimer.deltaTick;
		if (_elapsedTick >= Mathf.RoundToInt(LifeTime * 1000f))
		{
			_isEnd = true;
		}
	}

	void IScript.End()
	{
	}

	void IScript.FinalizeScript(FinalizeType finalizeType)
	{
		_isEnd = true;
		if (_localTimer != null)
		{
			SceneAutoBehaviour<CutSceneManager>.instance.GetCameraShake(LayerType).Clear();
		}
	}

	void IScript.Reset()
	{
	}

	bool ILoadJson.Load(JsonData json)
	{
		LayerType = json["LayerType"].GETENUM(LayerType);
		ShakeForce = json["ShakeForce"].GETVECTOR3(ShakeForce);
		RepeatTime = json["RepeatTime"].GETFLOAT(RepeatTime);
		LifeTime = json["LifeTime"].GETFLOAT(LifeTime);
		IsRandomForce = json["IsRandomForce"].GETBOOLEAN(IsRandomForce);
		UsePowerByLifeTime = json["UsePowerByLifeTime"].GETBOOLEAN(UsePowerByLifeTime);
		PowerByLifeTime = json["PowerByLifeTime"].GetAnimationCurve(PowerByLifeTime);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldEnum("LayerType", LayerType);
		jsonData.AddFieldVector3("ShakeForce", ShakeForce);
		jsonData.AddFieldDouble("RepeatTime", RepeatTime);
		jsonData.AddFieldDouble("LifeTime", LifeTime);
		jsonData.AddFieldBool("IsRandomForce", IsRandomForce);
		jsonData.AddFieldBool("UsePowerByLifeTime", UsePowerByLifeTime);
		jsonData.AddFieldAnimationCurve("PowerByLifeTime", PowerByLifeTime);
		return jsonData;
	}
}
