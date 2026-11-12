using System.Collections.Generic;
using LitJson;
using NGame2.NBattleView;
using NGame2.NBattleView.NCreature;
using NGame2.NMessage;
using NGame2.NMessage.NFastHandler;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NRuntime.NVariable;
using NGame2.NNodeTree.NUtil;
using NGame2.NScene;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NRoot;

public class BeginOperationSingle : RootBase, IKey, IKeyFinalize, IExecute, RuntimeInstance.IRuntimeStart, Holder.ICollector
{
	public class RootVariable : SmallPooling<RootVariable>, IVariable, ISmallPoolingItem
	{
		private bool _reservedMemberVariable;

		private RuntimeInstance _runtime;

		private SkillData _skillData;

		private OperationData _operationData;

		private ValueGenericMember<long> _targetUid;

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
			_operationData = null;
			_runtime = null;
		}

		private void ReserveMemberVariable()
		{
			if (!_reservedMemberVariable)
			{
				_reservedMemberVariable = true;
				_targetUid = ValueGenericMember<long>.Create(0L);
				_targetPosition = ValueGenericMember<Vector3>.Create(Vector3.zero);
				_dictionary.Add(VariableKeyword.Target, _targetUid);
				_dictionary.Add(VariableKeyword.TargetPosition, _targetPosition);
			}
		}

		internal bool Preopen(OperationMessage<NShared.BeginOperation> message)
		{
			_skillData = GameRoot.Instance.SkillDataContainer.GetData(message.FireSkillId);
			if (_skillData == null)
			{
				return false;
			}
			_operationData = _skillData.GetOperationDataUnsafe(message.OperationNo);
			if (_operationData == null)
			{
				return false;
			}
			_targetUid.Value = message.TargetCreatureUid;
			_targetPosition.Value = message.TargetPosition;
			return true;
		}

		void IVariable.Open(RuntimeInstance runtime)
		{
			_runtime = runtime;
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
			jsonData.AddFieldString("Variable Type", "BeginOperation");
			foreach (KeyValuePair<StringKey, IVariableValueBase> item in dictionary)
			{
				JsonData value = item.Value.DumpJson();
				jsonData.AddField(item.Key.Value, value);
			}
			return jsonData;
		}
	}

	public class LocalItem : ItemBase
	{
		public bool IsEnd;

		public float BeginTime;
	}

	public static string ContentsTypeName = "Root/" + typeof(BeginOperationSingle).Name;

	public int SkillIndex;

	public int OperationNo;

	public Caller OnBeginCaller;

	private DrawRuntimeVariableHelper _drawVariableHelper = new DrawRuntimeVariableHelper();

	public BeginOperationSingle(NodeData i_node)
		: base(i_node)
	{
		OnBeginCaller = new Caller(this, "OnBeginCaller");
	}

	public override void Wakeup(IVariable variable)
	{
	}

	void Holder.ICollector.Collect(Holder holder)
	{
		holder.AddBySkillIndex<OperationMessage<NShared.BeginOperation>>(CreatureManager.Verifier, OnMessageFast, SkillIndex);
		holder.AddBySkillIndex<OperationMessage<EndOperation>>(CreatureManager.Verifier, OnMessageFast, SkillIndex);
		holder.AddBySkillIndex<OperationMessage<CancelOperation>>(CreatureManager.Verifier, OnMessageFast, SkillIndex);
	}

	bool RuntimeInstance.IRuntimeStart.Start(RuntimeInstance runtime, IVariable variable)
	{
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.BeginTime = runtime.LocalTimer.time;
		runtime.ExecuteContainer.Pending(this, localItem);
		OnBeginCaller.Call(runtime, variable);
		return true;
	}

	private void ActionEnd(LocalItem localItem)
	{
		if (!localItem.IsEnd)
		{
			localItem.IsEnd = true;
		}
	}

	void IKeyFinalize.Finalize(ItemBase itemBase)
	{
	}

	void IExecute.UpdateExecute(ItemBase itemBase)
	{
		if (itemBase is LocalItem localItem)
		{
			_ = localItem.IsEnd;
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

	private void OnMessageFast(OperationMessage<NShared.BeginOperation> message)
	{
		if (!message.IsValidMessage() || SkillIndex != message.FireSkillId || OperationNo != message.OperationNo)
		{
			return;
		}
		Creature creature = BattleViewGenericSingleton<CreatureManager>.instance.GetCreature(message.TargetCreatureUid);
		if (creature == null)
		{
			return;
		}
		NodeTreeController nodeTreeController = creature.NodeTreeController;
		NodeTreeController.RuntimeLock runtimeLock = nodeTreeController.GetRuntimeLock(this);
		if (runtimeLock != null)
		{
			runtimeLock.Lock(message.OperationUid);
			return;
		}
		IRuntime runtime = CreateRuntimeInstance(message);
		if (runtime != null)
		{
			nodeTreeController.LockRuntime(this, message.OperationUid, runtime, immediatelyStart: false);
		}
	}

	private void OnMessageFast(OperationMessage<EndOperation> message)
	{
		if (message.IsValidMessage() && SkillIndex == message.FireSkillId && OperationNo == message.OperationNo)
		{
			ReleaseRuntimeLock(message.TargetCreatureUid, message.OperationUid);
		}
	}

	private void OnMessageFast(OperationMessage<CancelOperation> message)
	{
		if (message.IsValidMessage() && SkillIndex == message.FireSkillId && OperationNo == message.OperationNo)
		{
			ReleaseRuntimeLock(message.TargetCreatureUid, message.OperationUid);
		}
	}

	private void ReleaseRuntimeLock(int creatureUid, long operationUid)
	{
		Creature creature = BattleViewGenericSingleton<CreatureManager>.instance.GetCreature(creatureUid);
		if (!(creature == null))
		{
			creature.NodeTreeController.ReleaseRuntimeLock(this, operationUid);
		}
	}

	private IRuntime CreateRuntimeInstance(OperationMessage<NShared.BeginOperation> message)
	{
		RootVariable rootVariable = SmallPooling<RootVariable>.CreateInstance();
		if (!rootVariable.Preopen(message))
		{
			SmallPooling<RootVariable>.ReleaseInstance(rootVariable);
			return null;
		}
		RuntimeInstance runtimeInstance = SmallPooling<RuntimeInstance>.CreateInstance();
		runtimeInstance.Open(this.GetNode().GetTree(), rootVariable, message.OperationTimer, null, Game3DRoot.DefaultLayer, this);
		return runtimeInstance;
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
		SkillIndex = json["SkillIndex"].GETINT(SkillIndex);
		OperationNo = json["OperationNo"].GETINT(OperationNo);
		OnBeginCaller.LOADFROMJSON(json["OnBeginCaller"]);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddFieldLong("SkillIndex", SkillIndex);
		jsonData.AddFieldLong("OperationNo", OperationNo);
		jsonData.AddFieldJsonMake("OnBeginCaller", OnBeginCaller);
		return jsonData;
	}
}
