using System.Collections.Generic;
using LitJson;
using NGame2.NBattleView;
using NGame2.NMessage;
using NGame2.NMessage.NFastHandler;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NRuntime.NVariable;
using NGame2.NNodeTree.NUtil;
using NGame2.NScene;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NRoot;

public class FireSkill : RootBase, IKey, IKeyFinalize, RuntimeInstance.IRuntimeStart, Holder.ICollector
{
	public class RootVariable : SmallPooling<RootVariable>, IVariable, ISmallPoolingItem
	{
		private bool _reservedMemberVariable;

		private RuntimeInstance _runtime;

		private NShared.FireSkill _packet;

		private SkillData _skillData;

		private ValueGenericMember<long> _myself;

		private ValueGenericMember<long> _target;

		private ValueGenericMember<Vector3> _targetPosition;

		private StringKeyDictionary<IVariableValueBase> _dictionary = new StringKeyDictionary<IVariableValueBase>();

		void ISmallPoolingItem.OnCreateNew()
		{
			ReserveMemberVariable();
		}

		void ISmallPoolingItem.OnDestroy()
		{
		}

		void ISmallPoolingItem.OnCreate()
		{
		}

		void ISmallPoolingItem.OnRelease()
		{
			_skillData = null;
			_packet = null;
			_runtime = null;
		}

		private void ReserveMemberVariable()
		{
			if (!_reservedMemberVariable)
			{
				_reservedMemberVariable = true;
				_myself = ValueGenericMember<long>.Create(0L);
				_target = ValueGenericMember<long>.Create(0L);
				_targetPosition = ValueGenericMember<Vector3>.Create(Vector3.zero);
				_dictionary.Add(VariableKeyword.Myself, _myself);
				_dictionary.Add(VariableKeyword.Target, _target);
				_dictionary.Add(VariableKeyword.TargetPosition, _targetPosition);
			}
		}

		internal bool Preopen(NShared.FireSkill packet)
		{
			_skillData = GameRoot.Instance.SkillDataContainer.GetData(packet.SkillId);
			if (_skillData == null)
			{
				return false;
			}
			_packet = packet;
			return true;
		}

		void IVariable.Open(RuntimeInstance runtime)
		{
			_runtime = runtime;
			_myself.Value = _packet.CreatureUid;
			_target.Value = _packet.FirstTargetCreatureUid;
			_targetPosition.Value = _packet.TargetPosition;
		}

		IVariable IVariable.CaptureSelf()
		{
			return SmallPooling<RootVariable>.CaptureInstance(this);
		}

		void IVariable.ReleaseSelf()
		{
			SmallPooling<RootVariable>.ReleaseInstance(this);
		}

		RuntimeInstance IVariable.GetRuntimeInstance()
		{
			return _runtime;
		}

		IVariableValueBase IVariable.GetValueBase(StringKey key)
		{
			if (key.IsNullOrEmpty())
			{
				return null;
			}
			if (_dictionary.TryGetValue(key, out var value))
			{
				return value;
			}
			return SingletonAuto<EnvironmentVariable>.instance.GetValueBase(key);
		}

		JsonData IVariable.DumpJson()
		{
			Dictionary<StringKey, IVariableValueBase> dictionary = _dictionary.CreateDictionary();
			JsonData jsonData = new JsonData();
			jsonData.AddFieldString("Variable Type", "FireSkill");
			foreach (KeyValuePair<StringKey, IVariableValueBase> item in dictionary)
			{
				JsonData value = item.Value.DumpJson();
				jsonData.AddField(item.Key.Value, value);
			}
			return jsonData;
		}
	}

	public static string ContentsTypeName = "Root/" + typeof(FireSkill).Name;

	public Caller Caller;

	public int SkillIndex;

	public bool FilterByTickIndex;

	public int TickIndex;

	private DrawRuntimeVariableHelper _drawVariableHelper = new DrawRuntimeVariableHelper();

	public FireSkill(NodeData i_node)
		: base(i_node)
	{
		Caller = new Caller(this, "Caller");
	}

	void Holder.ICollector.Collect(Holder holder)
	{
		holder.AddBySkillIndex<CallbackMessage<NShared.FireSkill>>(CreatureManager.Verifier, OnMessageFast, SkillIndex);
	}

	public override void Wakeup(IVariable variable)
	{
	}

	bool RuntimeInstance.IRuntimeStart.Start(RuntimeInstance runtime, IVariable variable)
	{
		runtime.ItemContainer.CreateItem<ItemBase>(this, variable);
		Caller.Call(runtime, variable);
		return true;
	}

	void IKeyFinalize.Finalize(ItemBase itemBase)
	{
	}

	private void OnMessageFast(CallbackMessage<NShared.FireSkill> message)
	{
		if (!message.IsValidMessage())
		{
			return;
		}
		NShared.FireSkill callback = message.Callback;
		Creature creature = BattleViewGenericSingleton<CreatureManager>.instance.GetCreature(callback.CreatureUid);
		if (!(creature == null) && SkillIndex == callback.SkillId && (!FilterByTickIndex || TickIndex == callback.TickIndex))
		{
			RootVariable rootVariable = SmallPooling<RootVariable>.CreateInstance();
			if (!rootVariable.Preopen(callback))
			{
				SmallPooling<RootVariable>.ReleaseInstance(rootVariable);
				return;
			}
			RuntimeInstance runtimeInstance = SmallPooling<RuntimeInstance>.CreateInstance();
			runtimeInstance.Open(this.GetNode().GetTree(), rootVariable, creature.ActionLocalTimer, null, Game3DRoot.DefaultLayer, this);
			SceneAutoBehaviour<RuntimeGlobalManager>.instance.AddRuntime(runtimeInstance, immediatelyStart: true);
		}
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void ReserveCache(IResourcePreloader preloader)
	{
	}

	public override void LoadFromJson(JsonData json)
	{
		Caller.LOADFROMJSON(json["Caller"]);
		SkillIndex = json["SkillIndex"].GETINT(SkillIndex);
		FilterByTickIndex = json["FilterByTickIndex"].GETBOOLEAN(FilterByTickIndex);
		TickIndex = json["TickIndex"].GETINT(TickIndex);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("Caller", Caller.MAKEJSON());
		jsonData.AddFieldLong("SkillIndex", SkillIndex);
		jsonData.AddFieldBool("FilterByTickIndex", FilterByTickIndex);
		jsonData.AddFieldLong("TickIndex", TickIndex);
		return jsonData;
	}
}
