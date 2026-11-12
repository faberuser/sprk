using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NObject;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NNodeTree.NTypeValue.NModifier;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NExecute;

public class ObjectTransform : ExecuteBase, IKey, IExecute
{
	public class LocalItem : ItemBase, TransformModifier.IItem
	{
		public bool IsEnd;

		public float BeginTime;

		public Vector3 PrevPosition { get; set; }
	}

	public static string ContentsTypeName = "Execute/" + typeof(ObjectTransform).Name;

	public Callee CalleeStart;

	public Caller CallerEnd;

	public ArgumentSingleGeneric<long> ObjectUid;

	public ArgumentSingleGeneric<float> DurationTime;

	public ArgumentSingleGeneric<bool> StopThis;

	public TransformModifier TransformModifier;

	public DefineVariable<float> DefineFrame = new DefineVariable<float>();

	public ObjectTransform(NodeData i_node)
		: base(i_node)
	{
		CalleeStart = new Callee(this, "ContentsStart", ActionStart);
		CallerEnd = new Caller(this, "EndCaller");
		ObjectUid = new ArgumentSingleGeneric<long>(VariableKeyword.DummyDefaultName);
		DurationTime = new ArgumentSingleGeneric<float>(VariableKeyword.DurationTime);
		StopThis = new ArgumentSingleGeneric<bool>(value: false);
		TransformModifier = new TransformModifier();
	}

	public override void Wakeup(IVariable variable)
	{
		ObjectUid.GetValue(variable);
		DurationTime.GetValue(variable);
		StopThis.GetValue(variable);
		TransformModifier.Wakeup(variable);
	}

	private void ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		long value = ObjectUid.GetValue(variable);
		ObjectBase objectBase = Container.instance.GetObjectBase(value);
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.BeginTime = runtime.LocalTimer.time;
		DefineFrame.SetFunc(localItem, () => Mathf.Clamp01((localItem.localTimer.time - localItem.BeginTime) / Vespa_Float_Extension_Method.NoneZero(DurationTime.GetValue(localItem.Variable))));
		TransformModifier.InitTransform(objectBase.transform, localItem.Variable, localItem);
		runtime.ExecuteContainer.Pending(this, localItem);
	}

	private void ActionEnd(LocalItem localItem)
	{
		if (!localItem.IsEnd)
		{
			localItem.IsEnd = true;
			CallerEnd.Call(localItem.Runtime, localItem.Variable);
		}
	}

	void IExecute.UpdateExecute(ItemBase itemBase)
	{
		if (itemBase is LocalItem { IsEnd: false } localItem)
		{
			UpdateObject(localItem);
			if (localItem.localTimer.time - localItem.BeginTime >= DurationTime.GetValue(localItem.Variable) || StopThis.GetValue(localItem.Variable, defaultValue: false))
			{
				ActionEnd(localItem);
			}
		}
	}

	private void UpdateObject(LocalItem localItem)
	{
		long value = ObjectUid.GetValue(localItem.Variable);
		ObjectBase objectBase = Container.instance.GetObjectBase(value);
		if (!(objectBase == null))
		{
			TransformModifier.UpdateTransform(objectBase.transform, localItem.Variable, localItem);
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

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData json)
	{
		CalleeStart.LOADFROMJSON(json["CalleeStart"]);
		CallerEnd.LOADFROMJSON(json["CallerEnd"]);
		ObjectUid.LoadFromJson(json["ObjectUid"]);
		DurationTime.LoadFromJson(json["DurationTime"]);
		StopThis.LoadFromJson(json["StopThis"]);
		TransformModifier.LOADFROMJSON(json["TransformModifier"]);
		DefineFrame.LOADFROMJSON(json["DefineFrame"]);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("CalleeStart", CalleeStart.MAKEJSON());
		jsonData.AddField("CallerEnd", CallerEnd.MAKEJSON());
		jsonData.AddField("ObjectUid", ObjectUid.MakeJson());
		jsonData.AddField("DurationTime", DurationTime.MakeJson());
		jsonData.AddField("StopThis", StopThis.MakeJson());
		jsonData.AddFieldJsonMake("TransformModifier", TransformModifier);
		jsonData.AddFieldJsonMake("DefineFrame", DefineFrame);
		return jsonData;
	}
}
