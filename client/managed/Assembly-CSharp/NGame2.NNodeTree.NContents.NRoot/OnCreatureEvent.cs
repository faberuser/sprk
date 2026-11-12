using System.Collections.Generic;
using LitJson;
using NGame2.NBattleView;
using NGame2.NMessage;
using NGame2.NMessage.NFastHandler;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NRuntime.NVariable;
using NGame2.NNodeTree.NUtil;
using NVespa;

namespace NGame2.NNodeTree.NContents.NRoot;

public class OnCreatureEvent : RootBase, IKey, IKeyFinalize, IExecute, RuntimeInstance.IRuntimeStart, Holder.ICollector
{
	public class RootVariable : SmallPooling<RootVariable>, IVariable, ISmallPoolingItem
	{
		private bool _reservedMemberVariable;

		private RuntimeInstance _runtime;

		private ValueGenericMember<long> _myself;

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
				_dictionary.Add(VariableKeyword.Myself, _myself);
			}
		}

		internal bool Preopen(long uid)
		{
			_myself.Value = uid;
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
			jsonData.AddFieldString("Variable Type", "OnCreatureEvent");
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
		public bool IsBegin;

		public bool IsEnd;
	}

	public static string ContentsTypeName = "Root/" + typeof(OnCreatureEvent).Name;

	public Caller Caller;

	public CreatureEventMessage.EEventType EventType;

	public string Keyword = string.Empty;

	public bool IsActionRuntime;

	private DrawRuntimeVariableHelper _drawVariableHelper = new DrawRuntimeVariableHelper();

	public OnCreatureEvent(NodeData i_node)
		: base(i_node)
	{
		Caller = new Caller(this, "Caller");
	}

	public override void Wakeup(IVariable variable)
	{
	}

	void Holder.ICollector.Collect(Holder holder)
	{
		holder.Add<CreatureEventMessage>(CreatureManager.Verifier, OnMessageFast);
	}

	bool RuntimeInstance.IRuntimeStart.Start(RuntimeInstance runtime, IVariable variable)
	{
		LocalItem i_executeItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		runtime.ExecuteContainer.Pending(this, i_executeItem);
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
		if (itemBase is LocalItem { IsEnd: false, IsBegin: false } localItem && TryBegin(localItem))
		{
			localItem.IsBegin = true;
			ActionEnd(localItem);
		}
	}

	private bool TryBegin(LocalItem localItem)
	{
		long value = localItem.Variable.GetValue<long>(VariableKeyword.Myself);
		Creature battleCreature = localItem.GetBattleCreature((int)value);
		if (battleCreature == null)
		{
			return false;
		}
		if (!battleCreature.RenderFacade.AssetCreatureAsync.IsDone)
		{
			return false;
		}
		Caller.Call(localItem.Runtime, localItem.Variable);
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
			ActionEnd(localItem);
		}
	}

	private void OnMessageFast(CreatureEventMessage message)
	{
		if (!message.IsValidMessage() || message.EventType != EventType || !string.Equals(Keyword, message.Keyword) || (message.NodeTreePack != null && this.GetNode().GetTree().GetPack() != message.NodeTreePack))
		{
			return;
		}
		Creature creature = BattleViewGenericSingleton<CreatureManager>.instance.GetCreature(message.CreatureUid);
		if (creature == null)
		{
			VespaDebug.LogError($"CreatureEventMessage(Keyword={message.Keyword}) Error: creatureView == null");
			return;
		}
		RootVariable rootVariable = SmallPooling<RootVariable>.CreateInstance();
		if (!rootVariable.Preopen(message.CreatureUid))
		{
			VespaDebug.LogError($"CreatureEventMessage(Keyword={message.Keyword}) Error: runtimeVariable.Preopen");
			SmallPooling<RootVariable>.ReleaseInstance(rootVariable);
			return;
		}
		ILocalTimer localTimer = message.EventTimer;
		if (localTimer == null)
		{
			localTimer = creature.ActionLocalTimer;
		}
		RuntimeInstance runtimeInstance = SmallPooling<RuntimeInstance>.CreateInstance();
		runtimeInstance.Open(this.GetNode().GetTree(), rootVariable, localTimer, null, Game3DRoot.DefaultLayer, this);
		if (IsActionRuntime)
		{
			creature.NodeTreeController.SetMainRuntime(runtimeInstance);
		}
		else
		{
			creature.NodeTreeController.AddRuntime(runtimeInstance, immediatelyStart: true);
		}
		message.AddRuntime(Vespa_SmallPooling_ExtensionMethod.CaptureSmallInstance(runtimeInstance));
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData json)
	{
		Caller.LOADFROMJSON(json["Caller"]);
		EventType = json["EventType"].GETENUM(EventType);
		Keyword = json["Keyword"].GETSTRING(Keyword);
		IsActionRuntime = json["IsActionRuntime"].GETBOOLEAN(IsActionRuntime);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("Caller", Caller.MAKEJSON());
		jsonData.AddFieldEnum("EventType", EventType);
		jsonData.AddFieldString("Keyword", Keyword);
		jsonData.AddFieldBool("IsActionRuntime", IsActionRuntime);
		return jsonData;
	}
}
