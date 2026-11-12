using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NObject;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NExecute;

public class ObjectApproach : ExecuteBase, IKey, IExecute
{
	public class LocalItem : ItemBase
	{
		public bool IsEnd;

		public float BeginTime;
	}

	public static string ContentsTypeName = "Execute/" + typeof(ObjectApproach).Name;

	public Callee CalleeStart;

	public Caller CallerEnd;

	public ArgumentSingleGeneric<long> ObjectUid;

	public ArgumentSingleGeneric<Vector3> Destination;

	public ArgumentSingleGeneric<float> DurationTime;

	public ArgumentSingleGeneric<bool> StopThis;

	public bool IsLocalTransform;

	public DefineVariable<float> DefineFrame = new DefineVariable<float>();

	public ObjectApproach(NodeData i_node)
		: base(i_node)
	{
		CalleeStart = new Callee(this, "ContentsStart", ActionStart);
		CallerEnd = new Caller(this, "EndCaller");
		ObjectUid = new ArgumentSingleGeneric<long>(VariableKeyword.DummyDefaultName);
		Destination = new ArgumentSingleGeneric<Vector3>(VariableKeyword.Destination);
		DurationTime = new ArgumentSingleGeneric<float>(VariableKeyword.DurationTime);
		StopThis = new ArgumentSingleGeneric<bool>(value: false);
	}

	public override void Wakeup(IVariable variable)
	{
		ObjectUid.GetValue(variable);
		Destination.GetValue(variable);
		DurationTime.GetValue(variable);
		StopThis.GetValue(variable);
	}

	private void ActionStart(RuntimeInstance i_runtime, IVariable variable)
	{
		LocalItem localItem = i_runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.BeginTime = i_runtime.LocalTimer.time;
		DefineFrame.SetFunc(localItem, () => Mathf.Clamp01((localItem.localTimer.time - localItem.BeginTime) / Vespa_Float_Extension_Method.NoneZero(DurationTime.GetValue(localItem.Variable))));
		i_runtime.ExecuteContainer.Pending(this, localItem);
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
		if (objectBase == null || !Destination.TryGetValue(localItem.Variable, out var value2))
		{
			return;
		}
		float num = localItem.BeginTime + DurationTime.GetValue(localItem.Variable) - localItem.localTimer.time;
		if (num < Mathf.Epsilon)
		{
			if (IsLocalTransform)
			{
				objectBase.transform.localPosition = value2;
			}
			else
			{
				objectBase.transform.position = value2;
			}
			return;
		}
		Vector3 vector = (IsLocalTransform ? objectBase.transform.localPosition : objectBase.transform.position);
		float t = localItem.localTimer.deltaTime / num;
		Vector3 vector2 = Vector3.Lerp(vector, value2, t);
		if (!(Vector3.Distance(vector, vector2) < Mathf.Epsilon))
		{
			Quaternion quaternion = Quaternion.FromToRotation(Vector3.forward, (vector2 - vector).normalized);
			if (IsLocalTransform)
			{
				objectBase.transform.localRotation = quaternion;
				objectBase.transform.localPosition = vector2;
			}
			else
			{
				objectBase.transform.rotation = quaternion;
				objectBase.transform.position = vector2;
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

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData json)
	{
		CalleeStart.LOADFROMJSON(json["CalleeStart"]);
		CallerEnd.LOADFROMJSON(json["CallerEnd"]);
		ObjectUid.LoadFromJson(json["ObjectUid"]);
		Destination.LoadFromJson(json["Destination"]);
		DurationTime.LoadFromJson(json["DurationTime"]);
		StopThis.LoadFromJson(json["StopThis"]);
		IsLocalTransform = json["IsLocalTransform"].GETBOOLEAN(IsLocalTransform);
		DefineFrame.LOADFROMJSON(json["DefineFrame"]);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("CalleeStart", CalleeStart.MAKEJSON());
		jsonData.AddField("CallerEnd", CallerEnd.MAKEJSON());
		jsonData.AddField("ObjectUid", ObjectUid.MakeJson());
		jsonData.AddField("Destination", Destination.MakeJson());
		jsonData.AddField("DurationTime", DurationTime.MakeJson());
		jsonData.AddField("StopThis", StopThis.MakeJson());
		jsonData.AddFieldBool("IsLocalTransform", IsLocalTransform);
		jsonData.AddFieldJsonMake("DefineFrame", DefineFrame);
		return jsonData;
	}
}
