using System;
using System.Collections.Generic;
using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NRuntime.NVariable;
using NGame2.NNodeTree.NUtil;
using NVespa;

namespace NGame2.NNodeTree.NContents.NRoot;

public class ChildRoot : RootBase, IKey, IKeyFinalize, RuntimeInstance.IRuntimeStart
{
	public class RuntimeVariable : SmallPooling<RuntimeVariable>, IVariable, ISmallPoolingItem
	{
		private RuntimeInstance _runtime;

		private StringKeyDictionary<IVariableValueBase> _valueDic = new StringKeyDictionary<IVariableValueBase>();

		void ISmallPoolingItem.OnCreateNew()
		{
		}

		void ISmallPoolingItem.OnDestroy()
		{
		}

		void ISmallPoolingItem.OnCreate()
		{
		}

		void ISmallPoolingItem.OnRelease()
		{
			_valueDic.Clear();
			_runtime = null;
		}

		public void AddValue<T>(StringKey key, T value)
		{
			_valueDic.ADD(key, ValueGenericMember<T>.Create(value));
		}

		public void AddFunc<T>(StringKey key, Func<T> func)
		{
			_valueDic.ADD(key, new ValueGenericFunc<T>(func));
		}

		public void AddTypeValue(StringKey key, ITypeValueBase typeValue, IVariable variable)
		{
			IVariableValueBase variableValueBase = SingletonAuto<TypeFactory>.instance.CreateVariableValue(typeValue, variable, isDynamic: false);
			if (variableValueBase != null)
			{
				_valueDic.Add(key, variableValueBase);
			}
		}

		void IVariable.Open(RuntimeInstance runtime)
		{
			_runtime = runtime;
		}

		IVariable IVariable.CaptureSelf()
		{
			return SmallPooling<RuntimeVariable>.CaptureInstance(this);
		}

		void IVariable.ReleaseSelf()
		{
			SmallPooling<RuntimeVariable>.ReleaseInstance(this);
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
			if (_valueDic.TryGetValue(key, out var value))
			{
				return value;
			}
			return SingletonAuto<EnvironmentVariable>.instance.GetValueBase(key);
		}

		JsonData IVariable.DumpJson()
		{
			Dictionary<StringKey, IVariableValueBase> dictionary = _valueDic.CreateDictionary();
			JsonData jsonData = new JsonData();
			jsonData.AddFieldString("Variable Type", "LocalVariable");
			foreach (KeyValuePair<StringKey, IVariableValueBase> item in dictionary)
			{
				JsonData value = item.Value.DumpJson();
				jsonData.AddField(item.Key.Value, value);
			}
			return jsonData;
		}
	}

	public class MemberVariable : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public StringKey Name = StringKey.Empty;

		public string VariableType = Lookup.GetTypeName<long>();

		bool ILoadJson.Load(JsonData json)
		{
			Name = StringKey.CreateFromJson(json["Name"]);
			VariableType = json["VariableType"].GETSTRING(VariableType);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddField("Name", Name.MakeJson());
			jsonData.AddFieldString("VariableType", VariableType);
			return jsonData;
		}
	}

	public static string ContentsTypeName = "Root/" + typeof(ChildRoot).Name;

	public Caller Caller;

	public List<MemberVariable> MemberList = new List<MemberVariable>();

	private DrawRuntimeVariableHelper _drawVariableHelper = new DrawRuntimeVariableHelper();

	public ChildRoot(NodeData i_node)
		: base(i_node)
	{
		Caller = new Caller(this, "Caller");
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

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData i_json)
	{
		Caller.LOADFROMJSON(i_json["Caller"]);
		JsonTools.ToList(i_json.GetJsonAny("MemberList", "DefineList"), MemberList);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddFieldJsonMake("Caller", Caller);
		jsonData.AddField("MemberList", JsonTools.FromList(MemberList));
		return jsonData;
	}
}
