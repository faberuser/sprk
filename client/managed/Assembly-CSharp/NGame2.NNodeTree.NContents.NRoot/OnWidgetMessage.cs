using System;
using System.Collections.Generic;
using LitJson;
using NGame2.NMessage;
using NGame2.NMessage.NFastHandler;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NRuntime.NVariable;
using NGame2.NNodeTree.NUtil;
using NGame2.NUI;
using NVespa;

namespace NGame2.NNodeTree.NContents.NRoot;

public class OnWidgetMessage : RootBase, IKey, IKeyFinalize, RuntimeInstance.IRuntimeStart, Holder.ICollector
{
	public class RootVariable : SmallPooling<RootVariable>, IVariable, ISmallPoolingItem
	{
		private UIGame3DWidget _widget;

		private RuntimeInstance _runtime;

		private ValueGenericMember<object> _rootWidget;

		private StringKeyDictionary<IVariableValueBase> _dictionary = new StringKeyDictionary<IVariableValueBase>();

		internal bool Preopen(UIGame3DWidget widget)
		{
			_widget = widget;
			return true;
		}

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
			_dictionary.Clear();
			_runtime = null;
		}

		void IVariable.Open(RuntimeInstance runtime)
		{
			_runtime = runtime;
			_rootWidget = ValueGenericMember<object>.Create(_widget);
			_dictionary.Add(Key_Root, _rootWidget);
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
			return null;
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

	public static string ContentsTypeName = "Root/" + typeof(OnWidgetMessage).Name;

	public static readonly StringKey Key_Root = StringKey.Get("Root");

	public Caller Caller;

	public string EventCode;

	private DrawRuntimeVariableHelper _drawVariableHelper = new DrawRuntimeVariableHelper();

	public OnWidgetMessage(NodeData i_node)
		: base(i_node)
	{
		Caller = new Caller(this, "Caller");
	}

	public override void Wakeup(IVariable variable)
	{
	}

	void Holder.ICollector.Collect(Holder holder)
	{
		holder.Add<WidgetMessage>(new ObjVerifier(this), OnMessageFast);
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

	private void OnMessageFast(WidgetMessage message)
	{
		if (!message.IsValidMessage())
		{
			return;
		}
		PackData pack = this.GetNode().GetTree().GetPack();
		if (pack != null && string.Equals(pack.PathName, message.PackPathName, StringComparison.OrdinalIgnoreCase) && string.Equals(EventCode, message.EventCode, StringComparison.OrdinalIgnoreCase))
		{
			RootVariable rootVariable = SmallPooling<RootVariable>.CreateInstance();
			if (!rootVariable.Preopen(message.Widget))
			{
				SmallPooling<RootVariable>.ReleaseInstance(rootVariable);
				return;
			}
			ILocalTimer parentTimer = ((!(message.Widget != null)) ? UnityLocalTimer.Default : message.Widget.LocalTimer);
			RuntimeInstance runtimeInstance = SmallPooling<RuntimeInstance>.CreateInstance();
			runtimeInstance.Open(this.GetNode().GetTree(), rootVariable, parentTimer, message.Observer, message.Widget.RenderLayer, this);
			message.AddRuntimeInstance(runtimeInstance);
		}
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData json)
	{
		Caller.LOADFROMJSON(json["Caller"]);
		EventCode = json["EventCode"].GETSTRING(EventCode);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("Caller", Caller.MAKEJSON());
		jsonData.AddFieldString("EventCode", EventCode);
		return jsonData;
	}
}
