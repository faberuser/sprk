using System;
using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NObject;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NNodeTree.NTypeValue.NModifier;
using NGame2.NNodeTree.NUtil;
using NGame2.NScene;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NEffect;

public class CreateModel : EffectBase, IKey, IExecute, IKeyFinalize
{
	public enum ELifetimeType
	{
		Default,
		DestroyOnFinalize,
		CaptureExist
	}

	public class LocalItem : ItemBase, TransformModifier.IItem
	{
		public bool IsEnd;

		public float BeginTime;

		public long Uid;

		public Vector3 PrevPosition { get; set; }
	}

	public static string ContentsTypeName = "Effect/" + typeof(CreateModel).Name;

	public ChainCall ChainCall;

	public ELifetimeType LifetimeType;

	public ArgumentSingleGeneric<long> CaptureUid;

	public ArgumentSingleGeneric<Transform> ParentTransform;

	public TransformModifier TransformModifier;

	public ArgumentSingleGeneric<float> DurationTime;

	public ArgumentSingleGeneric<bool> StopThis;

	public DefineVariable<long> DefineUid = new DefineVariable<long>();

	public DefineVariable<float> DefineFrame = new DefineVariable<float>();

	public PrefabGoAuto PrefabAuto = new PrefabGoAuto(string.Empty, isGlobal: false, EPrefabType.Default, isPooling: true, touchMaterial: true);

	public MaterialData MaterialData = new MaterialData();

	public CreateModel(NodeData i_node)
		: base(i_node)
	{
		ChainCall = new ChainCall(this, "ChainCall", ActionStart);
		CaptureUid = new ArgumentSingleGeneric<long>(StringKey.Empty);
		ParentTransform = new ArgumentSingleGeneric<Transform>(StringKey.Empty);
		TransformModifier = new TransformModifier();
		DurationTime = new ArgumentSingleGeneric<float>(VariableKeyword.DurationTime);
		StopThis = new ArgumentSingleGeneric<bool>(value: false);
	}

	public override void Wakeup(IVariable variable)
	{
		CaptureUid.GetValue(variable);
		ParentTransform.GetValue(variable);
		TransformModifier.Wakeup(variable);
		DurationTime.GetValue(variable);
		StopThis.GetValue(variable);
	}

	private IVariable ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		ModelObject modelObject;
		if (LifetimeType == ELifetimeType.Default || LifetimeType == ELifetimeType.DestroyOnFinalize)
		{
			Transform value = ParentTransform.GetValue(variable);
			modelObject = NGame2.NNodeTree.NObject.Container.instance.CreateModel(PrefabAuto, value, runtime);
		}
		else
		{
			if (LifetimeType != ELifetimeType.CaptureExist)
			{
				return variable;
			}
			modelObject = NGame2.NNodeTree.NObject.Container.instance.GetObjectBase(CaptureUid.GetValue(variable)) as ModelObject;
			if (modelObject != null)
			{
				NGame2.NNodeTree.NObject.Container.instance.CaptureObject(modelObject);
			}
		}
		if (modelObject == null)
		{
			return variable;
		}
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.BeginTime = runtime.LocalTimer.time;
		localItem.Uid = modelObject.Uid;
		if (LifetimeType == ELifetimeType.Default || LifetimeType == ELifetimeType.DestroyOnFinalize)
		{
			DefineUid.SetValue(localItem, localItem.Uid);
		}
		else
		{
			_ = LifetimeType;
			_ = 2;
		}
		DefineFrame.SetFunc(localItem, () => Mathf.Clamp01((localItem.localTimer.time - localItem.BeginTime) / Vespa_Float_Extension_Method.NoneZero(DurationTime.GetValue(localItem.Variable))));
		TransformModifier.InitTransform(modelObject.transform, localItem.Variable, localItem);
		for (int num = 0; num < modelObject.RenderList.Count; num++)
		{
			Material[] materials = modelObject.RenderList[num].materials;
			for (int num2 = 0; num2 < __Extension_Method_ICollection__.COUNT(materials); num2++)
			{
				Material material = materials[num];
				if (!(material == null))
				{
					MaterialData.InitMaterial(material, 0f);
				}
			}
		}
		runtime.ExecuteContainer.Pending(this, localItem);
		return localItem.Variable;
	}

	private void ActionEnd(LocalItem localItem)
	{
		if (!localItem.IsEnd)
		{
			if (LifetimeType == ELifetimeType.Default || LifetimeType == ELifetimeType.CaptureExist)
			{
				NGame2.NNodeTree.NObject.Container.instance.ReleaseObjectUid(localItem.Uid);
				localItem.Uid = 0L;
			}
			else if (LifetimeType != ELifetimeType.DestroyOnFinalize)
			{
				NGame2.NNodeTree.NObject.Container.instance.ReleaseObjectUid(localItem.Uid);
				localItem.Uid = 0L;
			}
			localItem.IsEnd = true;
			ChainCall.EndCall(localItem.Runtime, localItem.Variable);
		}
	}

	void IExecute.UpdateExecute(ItemBase itemBase)
	{
		if (itemBase is LocalItem { IsEnd: false } localItem)
		{
			UpdateModel(localItem);
			if (localItem.localTimer.time - localItem.BeginTime >= DurationTime.GetValue(localItem.Variable) || StopThis.GetValue(localItem.Variable, defaultValue: false))
			{
				ActionEnd(localItem);
			}
		}
	}

	private void UpdateModel(LocalItem localItem)
	{
		ModelObject modelObject = NGame2.NNodeTree.NObject.Container.instance.GetObjectBase(localItem.Uid) as ModelObject;
		if (modelObject == null)
		{
			return;
		}
		TransformModifier.UpdateTransform(modelObject.transform, localItem.Variable, localItem);
		float frame = Mathf.Clamp01((localItem.localTimer.time - localItem.BeginTime) / DurationTime.GetValue(localItem.Variable));
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(modelObject.RenderList); i++)
		{
			Material[] materials = modelObject.RenderList[i].materials;
			if (__Extension_Method_ICollection__.IsNullOrEmpty(materials))
			{
				continue;
			}
			foreach (Material material in materials)
			{
				if (!(material == null))
				{
					MaterialData.UpdateMaterial(material, frame);
				}
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
			if (LifetimeType == ELifetimeType.DestroyOnFinalize)
			{
				NGame2.NNodeTree.NObject.Container.instance.ReleaseObjectUid(localItem.Uid);
				localItem.Uid = 0L;
			}
			ActionEnd(localItem);
		}
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void ReserveCache(IResourcePreloader preloader)
	{
		Action action = delegate
		{
			PrefabAuto.ReservePrefab();
			MaterialData.ReservePrefab();
		};
		preloader.AddAction(action, string.Empty);
	}

	public override void LoadFromJson(JsonData json)
	{
		ChainCall.LOADFROMJSON(json["ChainCall"]);
		LifetimeType = json["LifetimeType"].GETENUM(LifetimeType);
		CaptureUid.LoadFromJson(json["CaptureUid"]);
		ParentTransform.LoadFromJson(json["ParentTransform"]);
		TransformModifier.LOADFROMJSON(json["TransformModifier"]);
		DurationTime.LoadFromJson(json["DurationTime"]);
		StopThis.LoadFromJson(json["StopThis"]);
		DefineUid.LOADFROMJSON(json["DefineUid"]);
		DefineFrame.LOADFROMJSON(json["DefineFrame"]);
		PrefabAuto.LOADFROMJSON(json.GetJsonAny("PrefabAuto", "PrefabPath"));
		MaterialData.LOADFROMJSON(json["MaterialData"]);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("ChainCall", ChainCall.MAKEJSON());
		jsonData.AddFieldEnum("LifetimeType", LifetimeType);
		jsonData.AddField("CaptureUid", CaptureUid.MakeJson());
		jsonData.AddField("ParentTransform", ParentTransform.MakeJson());
		jsonData.AddFieldJsonMake("TransformModifier", TransformModifier);
		jsonData.AddField("DurationTime", DurationTime.MakeJson());
		jsonData.AddField("StopThis", StopThis.MakeJson());
		jsonData.AddFieldJsonMake("DefineUid", DefineUid);
		jsonData.AddFieldJsonMake("DefineFrame", DefineFrame);
		jsonData.AddFieldJsonMake("PrefabAuto", PrefabAuto);
		jsonData.AddFieldJsonMake("MaterialData", MaterialData);
		return jsonData;
	}
}
