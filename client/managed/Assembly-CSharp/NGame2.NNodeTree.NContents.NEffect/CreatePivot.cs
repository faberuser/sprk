using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NObject;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NNodeTree.NTypeValue.NModifier;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NEffect;

public class CreatePivot : EffectBase, IKey, IExecute, IKeyFinalize
{
	public class LocalItem : ItemBase, TransformModifier.IItem
	{
		public bool IsEnd;

		public float BeginTime;

		public long Uid;

		public Vector3 PrevPosition { get; set; }
	}

	public static string ContentsTypeName = "Effect/" + typeof(CreatePivot).Name;

	public ChainCall ChainCall;

	public ArgumentSingleGeneric<Transform> ParentTransform;

	public TransformModifier TransformModifier;

	public ArgumentSingleGeneric<float> DurationTime;

	public ArgumentSingleGeneric<bool> StopThis;

	public DefineVariable<long> DefineUid = new DefineVariable<long>(VariableKeyword.DummyDefaultName);

	public DefineVariable<float> DefineFrame = new DefineVariable<float>();

	public CreatePivot(NodeData i_node)
		: base(i_node)
	{
		ChainCall = new ChainCall(this, "ChainCall", ActionStart);
		ParentTransform = new ArgumentSingleGeneric<Transform>(StringKey.Empty);
		TransformModifier = new TransformModifier();
		DurationTime = new ArgumentSingleGeneric<float>(VariableKeyword.DurationTime);
		StopThis = new ArgumentSingleGeneric<bool>(value: false);
	}

	public override void Wakeup(IVariable variable)
	{
		ParentTransform.GetValue(variable);
		TransformModifier.Wakeup(variable);
		DurationTime.GetValue(variable);
		StopThis.GetValue(variable);
	}

	private IVariable ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.BeginTime = runtime.LocalTimer.time;
		if (!DefineUid.Name.IsNullOrEmpty())
		{
			ObjectBase objectBase = Container.instance.CreatePivotInvisible(ParentTransform.GetValue(variable), runtime);
			if (objectBase != null)
			{
				localItem.Uid = objectBase.Uid;
				DefineUid.SetValue(localItem, localItem.Uid);
				TransformModifier.InitTransform(objectBase.transform, variable, localItem);
			}
		}
		DefineFrame.SetFunc(localItem, () => Mathf.Clamp01((localItem.localTimer.time - localItem.BeginTime) / Vespa_Float_Extension_Method.NoneZero(DurationTime.GetValue(localItem.Variable))));
		runtime.ExecuteContainer.Pending(this, localItem);
		return localItem.Variable;
	}

	private void ActionEnd(LocalItem localItem)
	{
		if (!localItem.IsEnd)
		{
			localItem.IsEnd = true;
			ChainCall.EndCall(localItem.Runtime, localItem.Variable);
		}
	}

	void IExecute.UpdateExecute(ItemBase itemBase)
	{
		if (itemBase is LocalItem { IsEnd: false } localItem)
		{
			UpdatePivot(localItem);
			if (localItem.localTimer.time - localItem.BeginTime >= DurationTime.GetValue(localItem.Variable) || StopThis.GetValue(localItem.Variable, defaultValue: false))
			{
				ActionEnd(localItem);
			}
		}
	}

	private void UpdatePivot(LocalItem localItem)
	{
		if (localItem.Uid != 0L)
		{
			ObjectBase objectBase = Container.instance.GetObjectBase(localItem.Uid);
			if (objectBase != null)
			{
				TransformModifier.UpdateTransform(objectBase.transform, localItem.Variable, localItem);
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
			if (localItem.Uid != 0L)
			{
				Container.instance.ReleaseObjectUid(localItem.Uid);
				localItem.Uid = 0L;
			}
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
		ParentTransform.LoadFromJson(json["ParentTransform"]);
		TransformModifier.LOADFROMJSON(json["TransformModifier"]);
		DurationTime.LoadFromJson(json["DurationTime"]);
		StopThis.LoadFromJson(json["StopThis"]);
		DefineUid.LOADFROMJSON(json["DefineUid"]);
		DefineFrame.LOADFROMJSON(json["DefineFrame"]);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("ChainCall", ChainCall.MAKEJSON());
		jsonData.AddField("ParentTransform", ParentTransform.MakeJson());
		jsonData.AddFieldJsonMake("TransformModifier", TransformModifier);
		jsonData.AddField("DurationTime", DurationTime.MakeJson());
		jsonData.AddField("StopThis", StopThis.MakeJson());
		jsonData.AddFieldJsonMake("DefineUid", DefineUid);
		jsonData.AddFieldJsonMake("DefineFrame", DefineFrame);
		return jsonData;
	}
}
