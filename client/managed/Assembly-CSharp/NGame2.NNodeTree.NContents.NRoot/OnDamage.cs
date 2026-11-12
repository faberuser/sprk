using System.Collections.Generic;
using LitJson;
using NGame2.NBattleView;
using NGame2.NMessage;
using NGame2.NMessage.NFastHandler;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NRuntime.NVariable;
using NGame2.NNodeTree.NUtil;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NNodeTree.NContents.NRoot;

public class OnDamage : RootBase, IKey, IKeyFinalize, RuntimeInstance.IRuntimeStart, Holder.ICollector
{
	public class RootVariable : SmallPooling<RootVariable>, IVariable, ISmallPoolingItem
	{
		private bool _reservedMemberVariable;

		private RuntimeInstance _runtime;

		private ValueGenericMember<long> _myself;

		private ValueGenericMember<long> _attacker;

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
			_runtime = null;
		}

		private void ReserveMemberVariable()
		{
			if (!_reservedMemberVariable)
			{
				_reservedMemberVariable = true;
				_myself = ValueGenericMember<long>.Create(0L);
				_attacker = ValueGenericMember<long>.Create(0L);
				_dictionary.Add(VariableKeyword.Myself, _myself);
				_dictionary.Add(Key_Attacker, _attacker);
			}
		}

		internal bool Preopen(Damage packet)
		{
			OperationRuntime operationRuntime = SceneAutoBehaviour<BattleCallbackManager>.instance.GetOperationRuntime(packet.OperationUid);
			if (operationRuntime == null || operationRuntime.FireSkillCallback == null)
			{
				return false;
			}
			_myself.Value = packet.CreatureUid;
			_attacker.Value = operationRuntime.FireSkillCallback.CreatureUid;
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
			jsonData.AddFieldString("Variable Type", "BeginFireSkill");
			foreach (KeyValuePair<StringKey, IVariableValueBase> item in dictionary)
			{
				JsonData value = item.Value.DumpJson();
				jsonData.AddField(item.Key.Value, value);
			}
			return jsonData;
		}
	}

	public enum EDamageType
	{
		DontCare,
		Physical,
		Magical
	}

	public enum ECriticalType
	{
		DontCare,
		Critical,
		NoCritical
	}

	public enum EBlockType
	{
		DontCare,
		Blocked,
		NotBlocked
	}

	public enum EDodgeType
	{
		DontCare,
		Dodged,
		NotDodged
	}

	public enum EDotType
	{
		DontCare,
		IsDot,
		IsNotDot
	}

	public static string ContentsTypeName = "Root/" + typeof(OnDamage).Name;

	public static readonly StringKey Key_Attacker = StringKey.Get("Attacker");

	public Caller Caller;

	public EDamageType DamageType;

	public ECriticalType CriticalType;

	public EBlockType BlockType;

	public EDodgeType DodgeType;

	public EDotType DotType;

	private DrawRuntimeVariableHelper _drawVariableHelper = new DrawRuntimeVariableHelper();

	public OnDamage(NodeData i_node)
		: base(i_node)
	{
		Caller = new Caller(this, "Caller");
	}

	public override void Wakeup(IVariable variable)
	{
	}

	void Holder.ICollector.Collect(Holder holder)
	{
		holder.Add<CallbackMessage<Damage>>(CreatureManager.Verifier, OnMessageFast);
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

	private void OnMessageFast(CallbackMessage<Damage> message)
	{
		if (!message.IsValidMessage())
		{
			return;
		}
		Damage callback = message.Callback;
		if (DamageType == EDamageType.Physical)
		{
			if ((callback.DamageFlag & DamageFlag.Physical) == 0)
			{
				return;
			}
		}
		else if (DamageType == EDamageType.Magical && (callback.DamageFlag & DamageFlag.Magical) == 0)
		{
			return;
		}
		if (CriticalType == ECriticalType.Critical)
		{
			if ((callback.DamageFlag & DamageFlag.Critical) == 0)
			{
				return;
			}
		}
		else if (CriticalType == ECriticalType.NoCritical && (callback.DamageFlag & DamageFlag.Critical) != DamageFlag.None)
		{
			return;
		}
		if (BlockType == EBlockType.Blocked)
		{
			if ((callback.DamageFlag & DamageFlag.Blocked) == 0)
			{
				return;
			}
		}
		else if (BlockType == EBlockType.NotBlocked && (callback.DamageFlag & DamageFlag.Blocked) != DamageFlag.None)
		{
			return;
		}
		if (DodgeType == EDodgeType.Dodged)
		{
			if ((callback.DamageFlag & DamageFlag.Dodged) == 0)
			{
				return;
			}
		}
		else if (DodgeType == EDodgeType.NotDodged && (callback.DamageFlag & DamageFlag.Dodged) != DamageFlag.None)
		{
			return;
		}
		if (DotType == EDotType.IsDot)
		{
			if ((callback.DamageFlag & DamageFlag.Dot) == 0)
			{
				return;
			}
		}
		else if (DotType == EDotType.IsNotDot && (callback.DamageFlag & DamageFlag.Dot) != DamageFlag.None)
		{
			return;
		}
		RootVariable rootVariable = SmallPooling<RootVariable>.CreateInstance();
		if (!rootVariable.Preopen(callback))
		{
			SmallPooling<RootVariable>.ReleaseInstance(rootVariable);
			return;
		}
		ILocalTimer parentTimer = null;
		OperationRuntime operationRuntime = SceneAutoBehaviour<BattleCallbackManager>.instance.GetOperationRuntime(callback.OperationUid);
		if (operationRuntime != null)
		{
			parentTimer = operationRuntime.OperationTimer;
		}
		RuntimeInstance runtimeInstance = SmallPooling<RuntimeInstance>.CreateInstance();
		runtimeInstance.Open(this.GetNode().GetTree(), rootVariable, parentTimer, null, Game3DRoot.DefaultLayer, this);
		SceneAutoBehaviour<RuntimeGlobalManager>.instance.AddRuntime(runtimeInstance, immediatelyStart: false);
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData json)
	{
		Caller.LOADFROMJSON(json["Caller"]);
		DamageType = json["DamageType"].GETENUM(DamageType);
		CriticalType = json["CriticalType"].GETENUM(CriticalType);
		BlockType = json["BlockType"].GETENUM(BlockType);
		DodgeType = json["DodgeType"].GETENUM(DodgeType);
		DotType = json["DotType"].GETENUM(DotType);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("Caller", Caller.MAKEJSON());
		jsonData.AddFieldEnum("DamageType", DamageType);
		jsonData.AddFieldEnum("CriticalType", CriticalType);
		jsonData.AddFieldEnum("BlockType", BlockType);
		jsonData.AddFieldEnum("DodgeType", DodgeType);
		jsonData.AddFieldEnum("DotType", DotType);
		return jsonData;
	}
}
