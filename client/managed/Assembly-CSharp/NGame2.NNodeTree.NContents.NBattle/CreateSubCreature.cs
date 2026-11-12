using LitJson;
using NGame2.NBattleView;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NObject;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NNodeTree.NTypeValue.NModifier;
using NGame2.NNodeTree.NUtil;
using NVespa;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NBattle;

public class CreateSubCreature : BattleBase, IKey, IExecute, IKeyFinalize
{
	public enum ESubCreatureType
	{
		SoulWeapon
	}

	public class LocalItem : ItemBase, IVerifier, TransformModifier.IItem
	{
		public bool IsEnd;

		public SubCreature SubCreature;

		public float BeginTime;

		public Vector3 PrevPosition { get; set; }

		bool IVerifier.IsValid()
		{
			return !IsEnd;
		}
	}

	public static string ContentsTypeName = "Battle/" + typeof(CreateSubCreature).Name;

	public ChainCall ChainCall;

	public string Code = string.Empty;

	public ArgumentSingleGeneric<Transform> ParentTransform;

	public bool PivotCreate;

	public TransformModifier TransformModifier;

	public ArgumentSingleGeneric<long> CreatureUid;

	public ArgumentSingleGeneric<float> DurationTime;

	public LinkNodeTree2 OnDestroyLinkNodeTree;

	public ESubCreatureType SubCreatureType;

	public CreateSubCreature(NodeData i_node)
		: base(i_node)
	{
		ChainCall = new ChainCall(this, "ChainCall", ActionStart);
		ParentTransform = new ArgumentSingleGeneric<Transform>(StringKey.Empty);
		TransformModifier = new TransformModifier();
		OnDestroyLinkNodeTree = new LinkNodeTree2();
		CreatureUid = new ArgumentSingleGeneric<long>(StringKey.Empty);
		DurationTime = new ArgumentSingleGeneric<float>(VariableKeyword.DurationTime);
	}

	public override void Wakeup(IVariable variable)
	{
		TransformModifier.Wakeup(variable);
		ParentTransform.GetValue(variable);
		CreatureUid.GetValue(variable);
		DurationTime.GetValue(variable);
	}

	private IVariable ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		Transform transform = null;
		long pivotUid = 0L;
		if (PivotCreate)
		{
			ObjectBase objectBase = Container.instance.CreatePivotInvisible(ParentTransform.GetValue(variable), runtime);
			TransformModifier.InitTransform(objectBase.transform, variable, localItem);
			transform = objectBase.transform;
			pivotUid = objectBase.Uid;
		}
		else
		{
			transform = ParentTransform.GetValue(variable);
		}
		Creature bATTLECREATURE = runtime.SCENECONTAINER().GETBATTLECREATURE((int)CreatureUid.GetValue(variable));
		if (bATTLECREATURE != null)
		{
			bATTLECREATURE.CreateSubCreature(Code, transform, pivotUid);
			localItem.SubCreature = bATTLECREATURE.SubCreature;
		}
		localItem.BeginTime = runtime.LocalTimer.time;
		runtime.ExecuteContainer.Pending(this, localItem);
		return localItem.Variable;
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
			ActionEnd(localItem, requestStop);
		}
	}

	void IKeyFinalize.Finalize(ItemBase itemBase)
	{
		if (itemBase is LocalItem localItem)
		{
			ActionEnd(localItem, ERequestStop.None);
		}
	}

	void IExecute.UpdateExecute(ItemBase itemBase)
	{
		if (itemBase is LocalItem { IsEnd: false } localItem)
		{
			float num = localItem.localTimer.time - localItem.BeginTime;
			float value = DurationTime.GetValue(localItem.Variable, 1f);
			if (num > value)
			{
				ActionEnd(localItem, ERequestStop.None);
			}
			else
			{
				UpdatePivot(localItem);
			}
		}
	}

	private void UpdatePivot(LocalItem localItem)
	{
		if (localItem.SubCreature != null)
		{
			ObjectBase objectBase = Container.instance.GetObjectBase(localItem.SubCreature.PivotUid);
			if (objectBase != null)
			{
				TransformModifier.UpdateTransform(objectBase.transform, localItem.Variable, localItem);
			}
		}
	}

	private void ActionEnd(LocalItem localItem, ERequestStop requestStop)
	{
		if (!localItem.IsEnd)
		{
			Creature bATTLECREATURE = localItem.Runtime.SCENECONTAINER().GETBATTLECREATURE((int)CreatureUid.GetValue(localItem.Variable));
			if (!(bATTLECREATURE == null))
			{
				bATTLECREATURE.ReleaseSubCreature(OnDestroyLinkNodeTree, localItem.Runtime);
				localItem.IsEnd = true;
				ChainCall.EndCall(localItem.Runtime, localItem.Variable);
			}
		}
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData json)
	{
		OnDestroyLinkNodeTree.LOADFROMJSON(json["OnDestroyLinkNodeTree"]);
		ChainCall.LOADFROMJSON(json["ChainCall"]);
		Code = json["Code"].GETSTRING(Code);
		ParentTransform.LoadFromJson(json["ParentTransform"]);
		PivotCreate = json["PivotCreate"].GETBOOLEAN(PivotCreate);
		TransformModifier.LOADFROMJSON(json["TransformModifier"]);
		CreatureUid.LoadFromJson(json["CreatureUid"]);
		DurationTime.LoadFromJson(json["DurationTime"]);
		SubCreatureType = json["SubCreatureType"].GETENUM(SubCreatureType);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddFieldString("Code", Code);
		jsonData.AddField("ParentTransform", ParentTransform.MakeJson());
		jsonData.AddFieldBool("PivotCreate", PivotCreate);
		jsonData.AddFieldJsonMake("TransformModifier", TransformModifier);
		jsonData.AddField("OnDestroyLinkNodeTree", OnDestroyLinkNodeTree.MAKEJSON());
		jsonData.AddField("ChainCall", ChainCall.MAKEJSON());
		jsonData.AddField("CreatureUid", CreatureUid.MakeJson());
		jsonData.AddField("DurationTime", DurationTime.MakeJson());
		jsonData.AddFieldEnum("SubCreatureType", SubCreatureType);
		return jsonData;
	}
}
