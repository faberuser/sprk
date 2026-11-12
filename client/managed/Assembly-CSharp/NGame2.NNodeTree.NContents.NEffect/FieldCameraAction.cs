using LitJson;
using NGame2.N3D;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NEffect;

public class FieldCameraAction : EffectBase, IKey, IExecute, IKeyFinalize
{
	public class LocalItem : ItemBase
	{
		public bool IsEnd;

		public float BeginTime;

		public CameraStat.CameraStockOffset Offset;
	}

	public static string ContentsTypeName = "Effect/" + typeof(FieldCameraAction).Name;

	public ChainCall ChainCall;

	public ArgumentSingleGeneric<float> DurationTime;

	public ArgumentSingleGeneric<bool> StopThis;

	public ArgumentSingleGeneric<float> LookAngle;

	public ArgumentSingleGeneric<float> HorizontalAngle;

	public ArgumentSingleGeneric<float> FieldOfView;

	public ArgumentSingleGeneric<float> MinHeight;

	public ArgumentSingleGeneric<Vector3> BoundsMargin;

	public ArgumentSingleGeneric<Vector3> BoundsOffset;

	public AnimationCurve WeightCurve = new AnimationCurve();

	public FieldCameraAction(NodeData i_node)
		: base(i_node)
	{
		ChainCall = new ChainCall(this, "ChainCall", ActionStart);
		DurationTime = new ArgumentSingleGeneric<float>(VariableKeyword.DurationTime);
		StopThis = new ArgumentSingleGeneric<bool>(value: false);
		LookAngle = new ArgumentSingleGeneric<float>(0f);
		HorizontalAngle = new ArgumentSingleGeneric<float>(0f);
		FieldOfView = new ArgumentSingleGeneric<float>(0f);
		MinHeight = new ArgumentSingleGeneric<float>(0f);
		BoundsMargin = new ArgumentSingleGeneric<Vector3>(Vector3.zero);
		BoundsOffset = new ArgumentSingleGeneric<Vector3>(Vector3.zero);
	}

	public override void Wakeup(IVariable variable)
	{
		DurationTime.GetValue(variable);
		StopThis.GetValue(variable);
		LookAngle.GetValue(variable);
		HorizontalAngle.GetValue(variable);
		FieldOfView.GetValue(variable);
		MinHeight.GetValue(variable);
		BoundsMargin.GetValue(variable);
		BoundsOffset.GetValue(variable);
	}

	private IVariable ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.BeginTime = runtime.LocalTimer.time;
		Vector3 value = BoundsMargin.GetValue(variable);
		Vector3 value2 = BoundsOffset.GetValue(variable);
		float value3 = LookAngle.GetValue(variable);
		float value4 = HorizontalAngle.GetValue(variable);
		float value5 = FieldOfView.GetValue(variable);
		float value6 = MinHeight.GetValue(variable);
		if (CameraStat.BasicIsValidInstance)
		{
			localItem.Offset = new CameraStat.CameraStockOffset
			{
				FieldOfView = value5 - CameraStat.Basic.FieldOfView,
				HorizonAngle = value4 - CameraStat.Basic.HorizonAngle,
				LookAngle = value3 - CameraStat.Basic.LookAngle,
				MinHeight = value6 - CameraStat.Basic.MinHeight,
				BoundsMargin = value - CameraStat.Basic.BoundsMargin,
				BoundsOffset = value2 - CameraStat.Basic.BoundsOffset
			};
		}
		runtime.ExecuteContainer.Pending(this, localItem);
		return localItem.Variable;
	}

	private void ActionEnd(LocalItem localItem)
	{
		if (!localItem.IsEnd)
		{
			localItem.IsEnd = true;
			EndFieldCameraData();
			ChainCall.EndCall(localItem.Runtime, localItem.Variable);
		}
	}

	private void EndFieldCameraData()
	{
		if (CameraStat.BasicIsValidInstance)
		{
			CameraStat.Basic.ChangeOffset(CameraStat.CameraStockOffset.Empty);
			CameraStat.Basic.ResetRotation();
		}
	}

	void IExecute.UpdateExecute(ItemBase itemBase)
	{
		if (itemBase is LocalItem { IsEnd: false } localItem)
		{
			UpdateFieldCameraData(localItem);
			if (localItem.localTimer.time - localItem.BeginTime >= DurationTime.GetValue(localItem.Variable) || StopThis.GetValue(localItem.Variable, defaultValue: false))
			{
				ActionEnd(localItem);
			}
		}
	}

	private void UpdateFieldCameraData(LocalItem localItem)
	{
		if (CameraStat.BasicIsValidInstance)
		{
			CameraStat.CameraStockOffset offset = localItem.Offset;
			float i_frame = Mathf.Clamp01((localItem.localTimer.time - localItem.BeginTime) / Vespa_Float_Extension_Method.NoneZero(DurationTime.GetValue(localItem.Variable)));
			float num = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(WeightCurve, i_frame, 1f);
			CameraStat.CameraStockOffset offset2 = new CameraStat.CameraStockOffset
			{
				FieldOfView = offset.FieldOfView * num,
				HorizonAngle = offset.HorizonAngle * num,
				LookAngle = offset.LookAngle * num,
				MinHeight = offset.MinHeight * num,
				BoundsMargin = offset.BoundsMargin * num,
				BoundsOffset = offset.BoundsOffset * num
			};
			CameraStat.Basic.ChangeOffset(offset2);
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
		_ = itemBase is LocalItem;
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData json)
	{
		ChainCall.LOADFROMJSON(json["ChainCall"]);
		LookAngle.LoadFromJson(json["LookAngle"]);
		HorizontalAngle.LoadFromJson(json["HorizontalAngle"]);
		FieldOfView.LoadFromJson(json["FieldOfView"]);
		MinHeight.LoadFromJson(json["MinHeight"]);
		BoundsMargin.LoadFromJson(json["BoundsMargin"]);
		BoundsOffset.LoadFromJson(json["BoundsOffset"]);
		WeightCurve = json["WeightCurve"].GetAnimationCurve(WeightCurve);
		DurationTime.LoadFromJson(json["DurationTime"]);
		StopThis.LoadFromJson(json["StopThis"]);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("ChainCall", ChainCall.MAKEJSON());
		jsonData.AddField("LookAngle", LookAngle.MakeJson());
		jsonData.AddField("HorizontalAngle", HorizontalAngle.MakeJson());
		jsonData.AddField("FieldOfView", FieldOfView.MakeJson());
		jsonData.AddField("MinHeight", MinHeight.MakeJson());
		jsonData.AddField("BoundsMargin", BoundsMargin.MakeJson());
		jsonData.AddField("BoundsOffset", BoundsOffset.MakeJson());
		jsonData.AddFieldAnimationCurve("WeightCurve", WeightCurve);
		jsonData.AddField("DurationTime", DurationTime.MakeJson());
		jsonData.AddField("StopThis", StopThis.MakeJson());
		return jsonData;
	}
}
