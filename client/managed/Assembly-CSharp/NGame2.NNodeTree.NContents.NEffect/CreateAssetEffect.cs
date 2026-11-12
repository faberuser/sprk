using System;
using LitJson;
using NGame2.NBattleView;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NObject;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NNodeTree.NTypeValue.NModifier;
using NGame2.NScene;
using NGameAsset;
using NGameAsset.NEffect;
using NVespa;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NEffect;

public class CreateAssetEffect : EffectBase, IKey, IExecute, IKeyFinalize
{
	public class LocalItem : ItemBase, TransformModifier.IItem
	{
		public bool IsEnd;

		public float BeginTime;

		public long PivotUid;

		public IAsyncLoad<IEffect> EffectAsync;

		public bool AttachModelRegistered;

		public Vector3 PrevPosition { get; set; }
	}

	public class AttachModelVariable : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public string ModelName = string.Empty;

		public StringKey VariableKey = StringKey.Empty;

		bool ILoadJson.Load(JsonData json)
		{
			ModelName = json["ModelName"].GETSTRING(ModelName);
			VariableKey = StringKey.CreateFromJson(json["VariableKey"]);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddFieldString("ModelName", ModelName);
			jsonData.AddField("VariableKey", VariableKey.MakeJson());
			return jsonData;
		}
	}

	public static string ContentsTypeName = "Effect/" + typeof(CreateAssetEffect).Name;

	public ChainCall ChainCall;

	public ArgumentSingleGeneric<Transform> ParentTransform;

	public bool PivotCreate;

	public TransformModifier TransformModifier;

	public ArgumentSingleGeneric<float> DurationTime;

	public ArgumentSingleGeneric<bool> StopThis;

	public string Code = string.Empty;

	public EStopContentsType StopContentsType;

	public bool DetachParentOnRelease = true;

	public PoolingList<AttachModelVariable> AttachModelList;

	public CreateAssetEffect(NodeData i_node)
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
		if (!CheckShowSkillEffect(runtime, variable))
		{
			return variable;
		}
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.BeginTime = runtime.LocalTimer.time;
		localItem.EffectAsync = SingletonAuto<EffectContainer>.instance.CreateAsync(Code, runtime.Layer);
		localItem.EffectAsync.Value.SetTimer(runtime.LocalTimer);
		runtime.Observer.OnCreate(localItem.EffectAsync);
		if (PivotCreate)
		{
			ObjectBase objectBase = Container.instance.CreatePivotInvisible(ParentTransform.GetValue(variable), runtime);
			TransformModifier.InitTransform(objectBase.transform, variable, localItem);
			localItem.PivotUid = objectBase.Uid;
			localItem.EffectAsync.Value.SetParent(objectBase.transform);
		}
		else
		{
			localItem.EffectAsync.Value.SetParent(ParentTransform.GetValue(variable));
		}
		runtime.ExecuteContainer.Pending(this, localItem);
		return localItem.Variable;
	}

	private void ActionEnd(LocalItem localItem, ERequestStop requestStop)
	{
		if (localItem.IsEnd)
		{
			return;
		}
		if (StopContentsType == EStopContentsType.StopWithThis || ((StopContentsType == EStopContentsType.StopIfRequestStop) ? ((byte)requestStop != 0) : ((StopContentsType == EStopContentsType.StopIfBattleCancel) ? (requestStop == ERequestStop.BattleCancel) : (StopContentsType != EStopContentsType.None))))
		{
			if (localItem.EffectAsync != null)
			{
				localItem.Runtime.Observer.OnRelease(localItem.EffectAsync);
				EffectContainer.ReleaseEffect(localItem.EffectAsync, DetachParentOnRelease);
				localItem.EffectAsync = null;
			}
		}
		else if (localItem.EffectAsync != null)
		{
			Action onEnd = null;
			if (localItem.PivotUid != 0L)
			{
				long pivotUid = localItem.PivotUid;
				onEnd = delegate
				{
					Container.instance.ReleaseObjectUid(pivotUid);
				};
				localItem.PivotUid = 0L;
			}
			localItem.Runtime.Observer.OnRelease(localItem.EffectAsync);
			SingletonUpdater<AssetUpdateManager>.instance.Add(localItem.EffectAsync, DetachParentOnRelease, isGlobal: false, onEnd);
			localItem.EffectAsync = null;
		}
		if (localItem.PivotUid != 0L)
		{
			Container.instance.ReleaseObjectUid(localItem.PivotUid);
			localItem.PivotUid = 0L;
		}
		localItem.IsEnd = true;
		ChainCall.EndCall(localItem.Runtime, localItem.Variable);
	}

	void IExecute.UpdateExecute(ItemBase itemBase)
	{
		if (itemBase is LocalItem { IsEnd: false } localItem)
		{
			UpdatePivot(localItem);
			localItem.EffectAsync.Value.Update();
			if (!localItem.AttachModelRegistered && localItem.EffectAsync.ISVALID())
			{
				localItem.AttachModelRegistered = true;
				RegisterAttachModel(localItem);
			}
			if (localItem.localTimer.time - localItem.BeginTime >= DurationTime.GetValue(localItem.Variable))
			{
				ActionEnd(localItem, ERequestStop.None);
			}
			else if (StopThis.GetValue(localItem.Variable, defaultValue: false))
			{
				ActionEnd(localItem, ERequestStop.Normal);
			}
		}
	}

	private void RegisterAttachModel(LocalItem localItem)
	{
		for (int i = 0; i < AttachModelList.Count; i++)
		{
			AttachModelVariable attachModelVariable = AttachModelList[i];
			Transform transform = localItem.EffectAsync.Value.FindTransform(attachModelVariable.ModelName);
			if (!(transform == null))
			{
				localItem.Variable.AddValue(attachModelVariable.VariableKey, transform);
			}
		}
	}

	private void UpdatePivot(LocalItem localItem)
	{
		if (localItem.PivotUid != 0L)
		{
			ObjectBase objectBase = Container.instance.GetObjectBase(localItem.PivotUid);
			if (objectBase != null)
			{
				TransformModifier.UpdateTransform(objectBase.transform, localItem.Variable, localItem);
			}
		}
	}

	private bool CheckShowSkillEffect(RuntimeInstance runtime, IVariable variable)
	{
		if (!SystemOptionData.Render_ShowSkillEffect)
		{
			return false;
		}
		int uid = (int)variable.GetValue<long>(VariableKeyword.Myself);
		Creature battleCreature = runtime.SceneContainer.GetBattleCreature(uid);
		if (battleCreature != null)
		{
			if (battleCreature.IsLeftTeam)
			{
				return SystemOptionData.Render_ShowAllySkillEffect;
			}
			return SystemOptionData.Render_ShowEnemySkillEffect;
		}
		return true;
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

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void ReserveCache(IResourcePreloader preloader)
	{
		if (!SystemOptionData.Render_ShowSkillEffect)
		{
			return;
		}
		Builder builder = SingletonAuto<BuilderContainer>.instance.Get(Code);
		if (builder != null)
		{
			preloader.ExecuteAction(preloader.EffectPreloadType, delegate
			{
				preloader.AddAsyncLoad(builder.CreateOpenData());
			});
		}
	}

	public override void LoadFromJson(JsonData json)
	{
		ChainCall.LOADFROMJSON(json["ChainCall"]);
		ParentTransform.LoadFromJson(json["ParentTransform"]);
		PivotCreate = json["PivotCreate"].GETBOOLEAN(PivotCreate);
		TransformModifier.LOADFROMJSON(json["TransformModifier"]);
		DurationTime.LoadFromJson(json["DurationTime"]);
		StopThis.LoadFromJson(json["StopThis"]);
		Code = json["Code"].GETSTRING(Code);
		StopContentsType = json.GetJsonAny("StopContentsType", "StopEffectType").GETENUM(StopContentsType);
		DetachParentOnRelease = json["DetachParentOnRelease"].GETBOOLEAN(DetachParentOnRelease);
		PoolingListUtil.LoadFromJson(ref AttachModelList, json["AttachModelList"]);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("ChainCall", ChainCall.MAKEJSON());
		jsonData.AddField("ParentTransform", ParentTransform.MakeJson());
		jsonData.AddFieldBool("PivotCreate", PivotCreate);
		jsonData.AddFieldJsonMake("TransformModifier", TransformModifier);
		jsonData.AddField("DurationTime", DurationTime.MakeJson());
		jsonData.AddField("StopThis", StopThis.MakeJson());
		jsonData.AddFieldString("Code", Code);
		jsonData.AddFieldEnum("StopContentsType", StopContentsType);
		jsonData.AddFieldBool("DetachParentOnRelease", DetachParentOnRelease);
		jsonData.AddFieldList("AttachModelList", ref AttachModelList);
		return jsonData;
	}
}
