using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NSceneContainer;
using NGame2.NNodeTree.NTypeValue;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NEffect;

public class CameraShakeSimple : EffectBase, IKey
{
	public static string ContentsTypeName = "Effect/" + typeof(CameraShakeSimple).Name;

	public ChainCall ChainCall;

	public ArgumentSingleGeneric<Vector3> ShakeForce;

	public ArgumentSingleGeneric<float> RepeatTime;

	public ArgumentSingleGeneric<float> LifeTime;

	public ArgumentSingleGeneric<long> UniqueNo;

	public bool IsRandomForce;

	public bool UsePowerByLifeTime;

	public AnimationCurve PowerByLifeTime = new AnimationCurve();

	public CameraShakeSimple(NodeData i_node)
		: base(i_node)
	{
		ChainCall = new ChainCall(this, "ChainCall", ActionStart);
		ShakeForce = new ArgumentSingleGeneric<Vector3>(Vector3.up);
		RepeatTime = new ArgumentSingleGeneric<float>(0.1f);
		LifeTime = new ArgumentSingleGeneric<float>(1f);
		UniqueNo = new ArgumentSingleGeneric<long>(0L);
	}

	public override void Wakeup(IVariable variable)
	{
		ShakeForce.GetValue(variable);
		RepeatTime.GetValue(variable);
		LifeTime.GetValue(variable);
		UniqueNo.GetValue(variable);
	}

	private IVariable ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		Vector3 value = ShakeForce.GetValue(variable);
		float value2 = RepeatTime.GetValue(variable);
		float value3 = LifeTime.GetValue(variable);
		long value4 = UniqueNo.GetValue(variable);
		ICamera cAMERA = runtime.SCENECONTAINER().GETCAMERA();
		if (cAMERA != null && cAMERA.GetCameraShake() != null)
		{
			cAMERA.GetCameraShake().AddForce(value4, value, IsRandomForce, value2, value3, UsePowerByLifeTime ? PowerByLifeTime : null, runtime.LocalTimer);
		}
		return variable;
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData json)
	{
		ChainCall.LOADFROMJSON(json["ChainCall"]);
		ShakeForce.LoadFromJson(json["ShakeForce"]);
		RepeatTime.LoadFromJson(json["RepeatTime"]);
		LifeTime.LoadFromJson(json["LifeTime"]);
		UniqueNo.LoadFromJson(json["UniqueNo"]);
		IsRandomForce = json["IsRandomForce"].GETBOOLEAN(IsRandomForce);
		UsePowerByLifeTime = json["UsePowerByLifeTime"].GETBOOLEAN(UsePowerByLifeTime);
		PowerByLifeTime = JsonTools.ToCurve(json["PowerByLifeTime"], PowerByLifeTime);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("ChainCall", ChainCall.MAKEJSON());
		jsonData.AddField("ShakeForce", ShakeForce.MakeJson());
		jsonData.AddField("RepeatTime", RepeatTime.MakeJson());
		jsonData.AddField("LifeTime", LifeTime.MakeJson());
		jsonData.AddField("UniqueNo", UniqueNo.MakeJson());
		jsonData.AddFieldBool("IsRandomForce", IsRandomForce);
		jsonData.AddFieldBool("UsePowerByLifeTime", UsePowerByLifeTime);
		jsonData.AddField("PowerByLifeTime", JsonTools.FromCurve(PowerByLifeTime));
		return jsonData;
	}
}
