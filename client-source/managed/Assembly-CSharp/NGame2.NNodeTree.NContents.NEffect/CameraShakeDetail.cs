using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NSceneContainer;
using NGame2.NNodeTree.NTypeValue;
using NVespa;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NEffect;

public class CameraShakeDetail : EffectBase, IKey, IExecute, IKeyFinalize
{
	public class LocalItem : ItemBase
	{
		public bool IsEnd;

		public float BeginTime;

		public ShakeMaker.ShakeHolder Holder;
	}

	public static string ContentsTypeName = "Effect/" + typeof(CameraShakeDetail).Name;

	public ChainCall ChainCall;

	public ArgumentSingleGeneric<float> DurationTime;

	public ArgumentSingleGeneric<Vector3> ShakeValue;

	public ArgumentSingleGeneric<bool> StopThis;

	public DefineVariable<float> DefineFrame = new DefineVariable<float>();

	public CameraShakeDetail(NodeData i_node)
		: base(i_node)
	{
		ChainCall = new ChainCall(this, "ChainCall", ActionStart);
		DurationTime = new ArgumentSingleGeneric<float>(VariableKeyword.DurationTime);
		ShakeValue = new ArgumentSingleGeneric<Vector3>(Vector3.zero);
		StopThis = new ArgumentSingleGeneric<bool>(value: false);
	}

	public override void Wakeup(IVariable variable)
	{
		DurationTime.GetValue(variable);
		ShakeValue.GetValue(variable);
		StopThis.GetValue(variable);
	}

	private IVariable ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.BeginTime = runtime.LocalTimer.time;
		ICamera cAMERA = runtime.SCENECONTAINER().GETCAMERA();
		if (cAMERA != null && cAMERA.GetCameraShake() != null)
		{
			ShakeMaker.ShakeHolder shakeHolder = cAMERA.GetCameraShake().CreateHolder();
			localItem.Holder = Vespa_SmallPooling_ExtensionMethod.CaptureSmallInstance(shakeHolder);
		}
		DefineFrame.SetFunc(localItem, () => Mathf.Clamp01((localItem.localTimer.time - localItem.BeginTime) / Vespa_Float_Extension_Method.NoneZero(DurationTime.GetValue(localItem.Variable))));
		runtime.ExecuteContainer.Pending(this, localItem);
		return localItem.Variable;
	}

	private void ActionEnd(LocalItem localItem)
	{
		if (!localItem.IsEnd)
		{
			if (localItem.Holder != null)
			{
				localItem.Holder.IsDone = true;
				SmallPooling<ShakeMaker.ShakeHolder>.ReleaseInstance(localItem.Holder);
				localItem.Holder = null;
			}
			localItem.IsEnd = true;
			ChainCall.EndCall(localItem.Runtime, localItem.Variable);
		}
	}

	void IExecute.UpdateExecute(ItemBase itemBase)
	{
		if (itemBase is LocalItem { IsEnd: false } localItem)
		{
			if (localItem.Holder != null)
			{
				localItem.Holder.Value = ShakeValue.GetValue(localItem.Variable);
			}
			if (localItem.localTimer.time - localItem.BeginTime >= DurationTime.GetValue(localItem.Variable) || StopThis.GetValue(localItem.Variable, defaultValue: false))
			{
				ActionEnd(localItem);
			}
		}
	}

	bool IExecute.IsExecuteEnd(ItemBase itemBase)
	{
		if (!(itemBase is LocalItem localItem))
		{
			return true;
		}
		return localItem.IsEnd;
	}

	void IExecute.RequestStopExecute(ItemBase itemBase, ERequestStop requestStop)
	{
		if (itemBase is LocalItem localItem)
		{
			ActionEnd(localItem);
		}
	}

	void IKeyFinalize.Finalize(ItemBase itemBase)
	{
		if (itemBase is LocalItem localItem)
		{
			ActionEnd(localItem);
		}
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData json)
	{
		ChainCall.LOADFROMJSON(json["ChainCall"]);
		DurationTime.LoadFromJson(json["DurationTime"]);
		ShakeValue.LoadFromJson(json["ShakeValue"]);
		StopThis.LoadFromJson(json["StopThis"]);
		DefineFrame.LOADFROMJSON(json["DefineFrame"]);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("ChainCall", ChainCall.MAKEJSON());
		jsonData.AddField("DurationTime", DurationTime.MakeJson());
		jsonData.AddField("ShakeValue", ShakeValue.MakeJson());
		jsonData.AddField("StopThis", StopThis.MakeJson());
		jsonData.AddFieldJsonMake("DefineFrame", DefineFrame);
		return jsonData;
	}
}
