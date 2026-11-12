using System.Collections.Generic;
using LitJson;
using NGame2.NBattleField;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NNodeTree.NTypeValue.NModifier;
using NGame2.NScene;
using NGame2.NUI.NComponent2.NBattle;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NWindow.NBattle;
using NGameAsset;
using NGameAsset.NEffect;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NEffect;

public class CreateSkillUiEffect : EffectBase, IKey, IExecute, IKeyFinalize
{
	public class LocalItem : ItemBase, TransformModifier.IItem
	{
		public bool IsEnd;

		public float BeginTime;

		public IAsyncLoad<IEffect> EffectAsync;

		public Vector3 PrevPosition { get; set; }
	}

	public static string ContentsTypeName = "Effect/" + typeof(CreateSkillUiEffect).Name;

	public ChainCall ChainCall;

	public ArgumentSingleGeneric<float> DurationTime;

	public ArgumentSingleGeneric<bool> StopThis;

	public ArgumentSingleGeneric<long> CreatureIndex;

	public ArgumentSingleGeneric<long> SkillSlotIndex;

	public ArgumentSingleGeneric<long> CreatureUid;

	public string Code = string.Empty;

	public EStopContentsType StopContentsType;

	public bool DetachParentOnRelease = true;

	public CreateSkillUiEffect(NodeData i_node)
		: base(i_node)
	{
		ChainCall = new ChainCall(this, "ChainCall", ActionStart);
		DurationTime = new ArgumentSingleGeneric<float>(VariableKeyword.DurationTime);
		StopThis = new ArgumentSingleGeneric<bool>(value: false);
		CreatureIndex = new ArgumentSingleGeneric<long>(VariableKeyword.CreatureIndex);
		SkillSlotIndex = new ArgumentSingleGeneric<long>(VariableKeyword.SkillSlotIndex);
		CreatureUid = new ArgumentSingleGeneric<long>(VariableKeyword.Myself);
	}

	public override void Wakeup(IVariable variable)
	{
		DurationTime.GetValue(variable);
		StopThis.GetValue(variable);
		CreatureIndex.GetValue(variable);
		SkillSlotIndex.GetValue(variable);
		CreatureUid.GetValue(variable);
	}

	private IVariable ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		if (!SingletonAuto<BattleFacade>.instance.IsValidBattle())
		{
			return variable;
		}
		long value = CreatureUid.GetValue(variable);
		BattleFacade.CreatureCacheData creatureCache = SingletonAuto<BattleFacade>.instance.GetCreatureCache((int)value);
		if (creatureCache == null || creatureCache.Player == null || !creatureCache.Player.IsMe)
		{
			return variable;
		}
		if (!SceneAutoBehaviour<BattleManagement>.isValidInstance)
		{
			return variable;
		}
		BattleManagement instance = SceneAutoBehaviour<BattleManagement>.instance;
		if (instance == null)
		{
			return variable;
		}
		HeroParty instance2 = instance.GetInstance<HeroParty>(openedOnly: true);
		if (instance2 == null)
		{
			return variable;
		}
		long value2 = CreatureIndex.GetValue(variable);
		long value3 = SkillSlotIndex.GetValue(variable);
		BaseSkillComponent skillComponent = GetSkillComponent(instance2.PartySlotDic, value2, value3);
		if (skillComponent == null && instance2.EnabledSwapParty)
		{
			skillComponent = GetSkillComponent(instance2.SubPartySlotDic, value2, value3);
		}
		if (skillComponent == null)
		{
			return variable;
		}
		int layer = skillComponent.gameObject.layer;
		Transform transform = skillComponent.transform;
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.BeginTime = runtime.LocalTimer.time;
		localItem.EffectAsync = SingletonAuto<EffectContainer>.instance.CreateAsync(Code, layer);
		localItem.EffectAsync.Value.SetTimer(runtime.LocalTimer);
		runtime.Observer.OnCreate(localItem.EffectAsync);
		localItem.EffectAsync.Value.SetParent(transform);
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
			localItem.Runtime.Observer.OnRelease(localItem.EffectAsync);
			SingletonUpdater<AssetUpdateManager>.instance.Add(localItem.EffectAsync, DetachParentOnRelease, isGlobal: false, null);
			localItem.EffectAsync = null;
		}
		localItem.IsEnd = true;
		ChainCall.EndCall(localItem.Runtime, localItem.Variable);
	}

	void IExecute.UpdateExecute(ItemBase itemBase)
	{
		if (itemBase is LocalItem { IsEnd: false } localItem)
		{
			localItem.EffectAsync.Value.Update();
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
		DurationTime.LoadFromJson(json["DurationTime"]);
		StopThis.LoadFromJson(json["StopThis"]);
		CreatureIndex.LoadFromJson(json["CreatureIndex"]);
		SkillSlotIndex.LoadFromJson(json["SkillSlotIndex"]);
		CreatureUid.LoadFromJson(json["CreatureUid"]);
		Code = json["Code"].GETSTRING(Code);
		StopContentsType = json.GetJsonAny("StopContentsType", "StopEffectType").GETENUM(StopContentsType);
		DetachParentOnRelease = json["DetachParentOnRelease"].GETBOOLEAN(DetachParentOnRelease);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("ChainCall", ChainCall.MAKEJSON());
		jsonData.AddField("DurationTime", DurationTime.MakeJson());
		jsonData.AddField("StopThis", StopThis.MakeJson());
		jsonData.AddField("CreatureIndex", CreatureIndex.MakeJson());
		jsonData.AddField("SkillSlotIndex", SkillSlotIndex.MakeJson());
		jsonData.AddField("CreatureUid", CreatureUid.MakeJson());
		jsonData.AddFieldString("Code", Code);
		jsonData.AddFieldEnum("StopContentsType", StopContentsType);
		jsonData.AddFieldBool("DetachParentOnRelease", DetachParentOnRelease);
		return jsonData;
	}

	private BaseSkillComponent GetSkillComponent(IncreaseDic<int, PartySlotComponent2> partySlotDic, long findingCreatureIndex, long findingSkillSlotIndex)
	{
		foreach (KeyValuePair<int, PartySlotComponent2> item in (IEnumerable<KeyValuePair<int, PartySlotComponent2>>)partySlotDic)
		{
			int key = item.Key;
			PartySlotComponent2 value = item.Value;
			if (value == null)
			{
				continue;
			}
			BattleFacade.CreatureCacheData creatureCache = SingletonAuto<BattleFacade>.instance.GetCreatureCache(key);
			if (creatureCache != null && creatureCache.Uid != 0 && creatureCache.CreatureData.Index == findingCreatureIndex)
			{
				return value.FindSkillComponent((BaseSkillComponent e) => e.SkillSlotIndex == findingSkillSlotIndex);
			}
		}
		return null;
	}
}
