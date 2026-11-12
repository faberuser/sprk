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

public class ReserveOperation : RootBase, IKey, IKeyFinalize, IExecute, RuntimeInstance.IRuntimeStart, Holder.ICollector
{
	public class RootVariable : SmallPooling<RootVariable>, IVariable, ISmallPoolingItem
	{
		private bool _reservedMemberVariable;

		private RuntimeInstance _runtime;

		private ValueGenericMember<long> _myselfUid;

		private ValueGenericMember<long> _targetUid;

		private ValueGenericMember<Vector3> _targetPosition;

		private ValueGenericMember<float> _durationTime;

		private ValueGenericMember<float> _fixedDelayTime;

		private ValueGenericMember<long> _operationUid;

		private StringKeyDictionary<IVariableValueBase> _dictionary = new StringKeyDictionary<IVariableValueBase>();

		private void ReserveMemberVariable()
		{
			if (!_reservedMemberVariable)
			{
				_reservedMemberVariable = true;
				_myselfUid = ValueGenericMember<long>.Create(0L);
				_targetUid = ValueGenericMember<long>.Create(0L);
				_targetPosition = ValueGenericMember<Vector3>.Create(Vector3.zero);
				_durationTime = ValueGenericMember<float>.Create(0f);
				_fixedDelayTime = ValueGenericMember<float>.Create(0f);
				_operationUid = ValueGenericMember<long>.Create(0L);
				_dictionary.Add(VariableKeyword.Myself, _myselfUid);
				_dictionary.Add(VariableKeyword.Target, _targetUid);
				_dictionary.Add(VariableKeyword.TargetPosition, _targetPosition);
				_dictionary.Add(VariableKeyword.DurationTime, _durationTime);
				_dictionary.Add(Keyword_FixedDelayTime, _fixedDelayTime);
				_dictionary.Add(VariableKeyword.Frame, new ValueGenericFunc<float>(GetFrameValue));
				_dictionary.Add(VariableKeyword.Direction, new ValueGenericFunc<Vector3>(GetDirectionValue));
				_dictionary.Add(VariableKeyword.OperationUid, _operationUid);
			}
		}

		internal bool Preopen(OperationMessage<NShared.ReserveOperation> message)
		{
			if (message == null)
			{
				return false;
			}
			_myselfUid.Value = message.FireSkillCreatureUid;
			_targetUid.Value = message.TargetCreatureUid;
			_targetPosition.Value = message.TargetPosition;
			_durationTime.Value = (float)message.Callback.DurationTimeMs * 0.001f;
			_fixedDelayTime.Value = (float)message.Callback.FixedDelayTimeMs * 0.001f;
			_operationUid.Value = message.OperationUid;
			return true;
		}

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
			_runtime = null;
		}

		void IVariable.Open(RuntimeInstance runtime)
		{
			ReserveMemberVariable();
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
			jsonData.AddFieldString("Variable Type", "ReserveOperation");
			foreach (KeyValuePair<StringKey, IVariableValueBase> item in dictionary)
			{
				JsonData value = item.Value.DumpJson();
				jsonData.AddField(item.Key.Value, value);
			}
			return jsonData;
		}

		private Vector3 GetDirectionValue()
		{
			Creature battleCreature = _runtime.SceneContainer.GetBattleCreature((int)_myselfUid.Value);
			if (battleCreature == null)
			{
				return Vector3.right;
			}
			Creature battleCreature2 = _runtime.SceneContainer.GetBattleCreature((int)_targetUid.Value);
			if (battleCreature2 == null)
			{
				return battleCreature.Action.GetDirection().ToVec3();
			}
			Vector3 vector = battleCreature2.Position - battleCreature.Position;
			if (vector.magnitude > Mathf.Epsilon)
			{
				return vector.normalized;
			}
			return battleCreature.Action.GetDirection().ToVec3();
		}

		private float GetFrameValue()
		{
			if (!_runtime.Variable.TryGetValue<float>(VariableKeyword.BeginTime, out var o_value))
			{
				return 0f;
			}
			return Mathf.Clamp01((_runtime.LocalTimer.time - o_value) / Vespa_Float_Extension_Method.NoneZero(_durationTime.Value));
		}
	}

	public class LocalItem : ItemBase, IVerifier
	{
		public bool IsEnd;

		public float BeginTime;

		public float DurationTime;

		bool IVerifier.IsValid()
		{
			return !IsEnd;
		}
	}

	public static string ContentsTypeName = "Root/" + typeof(ReserveOperation).Name;

	public static readonly StringKey Keyword_FixedDelayTime = StringKey.Get("FixedDelayTime");

	public int SkillIndex;

	public int OperationNo;

	public bool StopOnBeginOperation;

	public Caller OnReserveCaller;

	private DrawRuntimeVariableHelper _drawVariableHelper = new DrawRuntimeVariableHelper();

	public ReserveOperation(NodeData i_node)
		: base(i_node)
	{
		OnReserveCaller = new Caller(this, "OnReserveCaller");
	}

	public override void Wakeup(IVariable variable)
	{
	}

	void Holder.ICollector.Collect(Holder holder)
	{
		holder.AddBySkillIndex<OperationMessage<NShared.ReserveOperation>>(CreatureManager.Verifier, OnMessageFast, SkillIndex);
	}

	bool RuntimeInstance.IRuntimeStart.Start(RuntimeInstance runtime, IVariable variable)
	{
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.BeginTime = runtime.LocalTimer.time;
		localItem.DurationTime = variable.GetValue<float>(VariableKeyword.DurationTime);
		runtime.ExecuteContainer.PendingWeak(this, localItem);
		OnReserveCaller.Call(runtime, localItem.Variable);
		long value = localItem.Variable.GetValue<long>(VariableKeyword.OperationUid);
		localItem.AddOnMessageByOperationUid<LocalItem, OperationMessage<NShared.BeginOperation>>(localItem, LocalItemOnMessageFast, value);
		localItem.AddOnMessageByOperationUid<LocalItem, OperationMessage<NShared.FlyingOperation>>(localItem, LocalItemOnMessageFast, value);
		localItem.AddOnMessageByOperationUid<LocalItem, OperationMessage<ResistOperation>>(localItem, LocalItemOnMessageFast, value);
		localItem.AddOnMessageByOperationUid<LocalItem, OperationMessage<CancelOperation>>(localItem, LocalItemOnMessageFast, value);
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
		if (itemBase is LocalItem { IsEnd: false } localItem && localItem.localTimer.time - localItem.BeginTime >= localItem.DurationTime)
		{
			ActionEnd(localItem);
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

	private void OnMessageFast(OperationMessage<NShared.ReserveOperation> message)
	{
		if (message.IsValidMessage() && SkillIndex == message.FireSkillId && OperationNo == message.OperationNo)
		{
			RootVariable rootVariable = SmallPooling<RootVariable>.CreateInstance();
			if (!rootVariable.Preopen(message))
			{
				SmallPooling<RootVariable>.ReleaseInstance(rootVariable);
				return;
			}
			RuntimeInstance runtimeInstance = SmallPooling<RuntimeInstance>.CreateInstance();
			runtimeInstance.Open(this.GetNode().GetTree(), rootVariable, message.OperationTimer, null, Game3DRoot.DefaultLayer, this);
			SceneAutoBehaviour<RuntimeGlobalManager>.instance.AddRuntime(runtimeInstance, immediatelyStart: true);
		}
	}

	private void LocalItemOnMessageFast(LocalItem localItem, OperationMessage<CancelOperation> message)
	{
		if (localItem != null && message.IsValidMessage() && localItem.Variable.GetValue<long>(VariableKeyword.OperationUid) == message.OperationUid)
		{
			localItem.Runtime.RequestStopFromInternal(ERequestStop.BattleCancel);
		}
	}

	private void LocalItemOnMessageFast(LocalItem localItem, OperationMessage<NShared.FlyingOperation> message)
	{
		if (localItem != null && message.IsValidMessage() && localItem.Variable.GetValue<long>(VariableKeyword.OperationUid) == message.OperationUid)
		{
			localItem.DurationTime = (float)message.Callback.ReserveDurationTimeMs * 0.001f;
			localItem.Variable.AddValue(VariableKeyword.Target, message.Callback.TargetCreatureUid);
			localItem.Variable.AddValue(VariableKeyword.DurationTime, localItem.DurationTime);
		}
	}

	private void LocalItemOnMessageFast(LocalItem localItem, OperationMessage<NShared.BeginOperation> message)
	{
		if (localItem != null && message.IsValidMessage() && localItem.Variable.GetValue<long>(VariableKeyword.OperationUid) == message.OperationUid && StopOnBeginOperation)
		{
			localItem.Runtime.RequestStopFromInternal(ERequestStop.Normal);
		}
	}

	private void LocalItemOnMessageFast(LocalItem localItem, OperationMessage<ResistOperation> message)
	{
		if (localItem != null && message.IsValidMessage() && localItem.Variable.GetValue<long>(VariableKeyword.OperationUid) == message.OperationUid && StopOnBeginOperation)
		{
			localItem.Runtime.RequestStopFromInternal(ERequestStop.BattleCancel);
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
		SkillIndex = json["SkillIndex"].GETINT(SkillIndex);
		OperationNo = json["OperationNo"].GETINT(OperationNo);
		StopOnBeginOperation = json["StopOnBeginOperation"].GETBOOLEAN(StopOnBeginOperation);
		OnReserveCaller.LOADFROMJSON(json.GetJsonAny("OnReserveCaller", "OnFlyingCaller"));
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddFieldLong("SkillIndex", SkillIndex);
		jsonData.AddFieldLong("OperationNo", OperationNo);
		jsonData.AddFieldBool("StopOnBeginOperation", StopOnBeginOperation);
		jsonData.AddFieldJsonMake("OnReserveCaller", OnReserveCaller);
		return jsonData;
	}
}
