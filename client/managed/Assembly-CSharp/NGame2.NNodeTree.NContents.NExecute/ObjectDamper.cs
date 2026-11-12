using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NObject;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NExecute;

public class ObjectDamper : ExecuteBase, IKey, IExecute
{
	public class LocalItem : ItemBase
	{
		public bool IsEnd;

		public float BeginTime;

		public Vector3 Velocity;
	}

	public static string ContentsTypeName = "Execute/" + typeof(ObjectDamper).Name;

	public static readonly StringKey Key_DestRotation = StringKey.Get("DestRotation");

	public Callee CalleeStart;

	public Caller CallerEnd;

	public ArgumentSingleGeneric<long> ObjectUid;

	public ArgumentSingleGeneric<Vector3> Destination;

	public ArgumentSingleGeneric<Quaternion> DestRotation;

	public ArgumentSingleGeneric<float> DurationTime;

	public ArgumentSingleGeneric<bool> StopThis;

	public bool IsLocalTransform;

	public float DistanceSpring = 1f;

	public float DistanceDamper = 1f;

	public float DirectionDamper = 1f;

	public Vector2 DirectionBySpeed = new Vector2(0.3f, 1f);

	public float DirectionBlend = 1f;

	public ObjectDamper(NodeData i_node)
		: base(i_node)
	{
		CalleeStart = new Callee(this, "ContentsStart", ActionStart);
		CallerEnd = new Caller(this, "EndCaller");
		ObjectUid = new ArgumentSingleGeneric<long>(VariableKeyword.DummyDefaultName);
		Destination = new ArgumentSingleGeneric<Vector3>(VariableKeyword.Destination);
		DestRotation = new ArgumentSingleGeneric<Quaternion>(Key_DestRotation);
		DurationTime = new ArgumentSingleGeneric<float>(VariableKeyword.DurationTime);
		StopThis = new ArgumentSingleGeneric<bool>(value: false);
	}

	public override void Wakeup(IVariable variable)
	{
		ObjectUid.GetValue(variable);
		Destination.GetValue(variable);
		DestRotation.GetValue(variable);
		DurationTime.GetValue(variable);
		StopThis.GetValue(variable);
	}

	private void ActionStart(RuntimeInstance i_runtime, IVariable variable)
	{
		LocalItem localItem = i_runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.BeginTime = i_runtime.LocalTimer.time;
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
		if (!(objectBase == null) && Destination.TryGetValue(localItem.Variable, out var value2))
		{
			Vector3 vector = (IsLocalTransform ? objectBase.transform.localPosition : objectBase.transform.position);
			Vector3 vector2 = value2 - vector;
			Vector3 vector3 = vector2 * DistanceSpring;
			float num = Mathf.Clamp01((Vector3.Dot(vector2.normalized, localItem.Velocity) + 1f) * 0.5f * DirectionDamper * localItem.localTimer.deltaTime);
			Vector3 vector4 = localItem.Velocity + vector3 * localItem.localTimer.deltaTime - localItem.Velocity * num;
			float magnitude = vector2.magnitude;
			float num2 = DistanceDamper * localItem.localTimer.deltaTime / Vespa_Float_Extension_Method.NoneZero(magnitude);
			float num3 = 1f / (num2 + 1f);
			localItem.Velocity = vector4 * num3;
			Vector3 vector5 = vector + localItem.Velocity * localItem.localTimer.deltaTime;
			Quaternion value3 = DestRotation.GetValue(localItem.Variable, Quaternion.identity);
			Vector3 normalized = (vector5 - vector).normalized;
			Quaternion b2;
			if (normalized.sqrMagnitude > Mathf.Epsilon)
			{
				Quaternion b = Quaternion.LookRotation(normalized);
				float t = Mathf.InverseLerp(DirectionBySpeed.x, DirectionBySpeed.y, localItem.Velocity.magnitude);
				b2 = Quaternion.Lerp(value3, b, t);
			}
			else
			{
				b2 = value3;
			}
			b2 = Quaternion.Lerp(IsLocalTransform ? objectBase.transform.localRotation : objectBase.transform.rotation, b2, localItem.localTimer.deltaTime / Mathf.Max(Mathf.Epsilon, DirectionBlend));
			if (IsLocalTransform)
			{
				objectBase.transform.localPosition = vector5;
				objectBase.transform.localRotation = b2;
			}
			else
			{
				objectBase.transform.position = vector5;
				objectBase.transform.rotation = b2;
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
		DestRotation.LoadFromJson(json["DestRotation"]);
		DurationTime.LoadFromJson(json["DurationTime"]);
		StopThis.LoadFromJson(json["StopThis"]);
		IsLocalTransform = json["IsLocalTransform"].GETBOOLEAN(IsLocalTransform);
		DistanceSpring = json["DistanceSpring"].GETFLOAT(DistanceSpring);
		DistanceDamper = json["DistanceDamper"].GETFLOAT(DistanceDamper);
		DirectionDamper = json["DirectionDamper"].GETFLOAT(DirectionDamper);
		DirectionBySpeed = JsonTools.ToVector2(json["DirectionBySpeed"], DirectionBySpeed);
		DirectionBlend = json["DirectionBlend"].GETFLOAT(DirectionBlend);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("CalleeStart", CalleeStart.MAKEJSON());
		jsonData.AddField("CallerEnd", CallerEnd.MAKEJSON());
		jsonData.AddField("ObjectUid", ObjectUid.MakeJson());
		jsonData.AddField("Destination", Destination.MakeJson());
		jsonData.AddField("DestRotation", DestRotation.MakeJson());
		jsonData.AddField("DurationTime", DurationTime.MakeJson());
		jsonData.AddField("StopThis", StopThis.MakeJson());
		jsonData.AddFieldBool("IsLocalTransform", IsLocalTransform);
		jsonData.AddFieldDouble("DistanceSpring", DistanceSpring);
		jsonData.AddFieldDouble("DistanceDamper", DistanceDamper);
		jsonData.AddFieldDouble("DirectionDamper", DirectionDamper);
		jsonData.AddField("DirectionBySpeed", JsonTools.FromVector2(DirectionBySpeed));
		jsonData.AddFieldDouble("DirectionBlend", DirectionBlend);
		return jsonData;
	}
}
