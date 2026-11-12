using System.Collections.Generic;
using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NNodeTree.NUtil;
using NGame2.NScene;
using NGameAsset;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NNodeTree.NContents.NExecute;

public class CreateRuntime : ExecuteBase, IKey, IExecute, IKeyFinalize
{
	public class LocalItem : ItemBase
	{
		public bool IsEnd;

		public IAsyncLoad<IEffect> EffectAsync;

		public RuntimeInstance ChildRuntime;
	}

	public static string ContentsTypeName = "Execute/" + typeof(CreateRuntime).Name;

	public Callee CalleeStart;

	public Caller CallerNext;

	public EnableSpecData EnableSpec = new EnableSpecData();

	public ArgumentSingleGeneric<bool> StopThis;

	public LinkNodeTree LinkNodeTree = new LinkNodeTree();

	public EStopContentsType StopRuntimeType = EStopContentsType.None;

	public CreateRuntime(NodeData i_node)
		: base(i_node)
	{
		CalleeStart = new Callee(this, "CalleeStart", ActionStart);
		CallerNext = new Caller(this, "CallerNext");
		StopThis = new ArgumentSingleGeneric<bool>(value: false);
	}

	public override void Wakeup(IVariable variable)
	{
		StopThis.GetValue(variable);
	}

	public void ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		StartRuntime(runtime, variable);
		CallerNext.Call(runtime, variable);
	}

	private void StartRuntime(RuntimeInstance runtime, IVariable variable)
	{
		if (!EnableSpec.CheckEnable())
		{
			return;
		}
		TreeData tree = LinkNodeTree.GetTree();
		if (tree == null)
		{
			return;
		}
		BuildChildRootRuntime buildChildRootRuntime = default(BuildChildRootRuntime);
		if (!buildChildRootRuntime.Open(tree))
		{
			return;
		}
		for (int i = 0; i < LinkNodeTree.MappingDic.Count; i++)
		{
			KeyValuePair<StringKey, ArgumentSingle> keyValuePairByIndex = LinkNodeTree.MappingDic.GetKeyValuePairByIndex(i);
			StringKey key = keyValuePairByIndex.Key;
			ArgumentSingle value = keyValuePairByIndex.Value;
			buildChildRootRuntime.AddTypeValue(key, value.Holder.Value, variable);
		}
		if (!buildChildRootRuntime.TryGetValue<long>(VariableKeyword.Myself, out var _))
		{
			long value3 = runtime.Variable.GetValue<long>(VariableKeyword.Myself);
			if (value3 > 0)
			{
				buildChildRootRuntime.AddValue(VariableKeyword.Myself, value3);
			}
		}
		RuntimeInstance runtimeInstance = buildChildRootRuntime.Build(runtime.SceneContainer, runtime.LocalTimer, runtime.Observer, runtime.Layer);
		if (runtimeInstance != null)
		{
			SceneAutoBehaviour<RuntimeGlobalManager>.instance.AddRuntime(runtimeInstance, immediatelyStart: false);
			if (StopRuntimeType != EStopContentsType.None)
			{
				LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
				localItem.ChildRuntime = Vespa_SmallPooling_ExtensionMethod.CaptureSmallInstance(runtimeInstance);
				runtime.ExecuteContainer.PendingWeak(this, localItem);
			}
		}
	}

	private void ActionEnd(LocalItem localItem, ERequestStop requestStop)
	{
		if (localItem.IsEnd)
		{
			return;
		}
		localItem.IsEnd = true;
		bool flag = StopRuntimeType == EStopContentsType.StopWithThis || ((StopRuntimeType == EStopContentsType.StopIfRequestStop) ? (requestStop != ERequestStop.None) : ((StopRuntimeType == EStopContentsType.StopIfBattleCancel) ? (requestStop == ERequestStop.BattleCancel) : (StopRuntimeType != EStopContentsType.None)));
		if (localItem.ChildRuntime != null)
		{
			if (flag)
			{
				Vespa_Common_Class_Extension_Method.Interface((IRuntime)localItem.ChildRuntime).RequestStop(requestStop);
			}
			SmallPooling<RuntimeInstance>.ReleaseInstance(localItem.ChildRuntime);
			localItem.ChildRuntime = null;
		}
	}

	void IExecute.UpdateExecute(ItemBase itemBase)
	{
		if (itemBase is LocalItem { IsEnd: false } localItem && StopThis.GetValue(localItem.Variable, defaultValue: false))
		{
			ActionEnd(localItem, ERequestStop.Normal);
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
		LinkNodeTree.ReserveCache(preloader);
	}

	public override void LoadFromJson(JsonData json)
	{
		CalleeStart.LOADFROMJSON(json["CalleeStart"]);
		CallerNext.LOADFROMJSON(json["CallerNext"]);
		EnableSpec.LOADFROMJSON(json["EnableSpec"]);
		StopThis.LoadFromJson(json["StopThis"]);
		LinkNodeTree.LoadFromJson(json["LinkNodeTree"], this);
		StopRuntimeType = json["StopRuntimeType"].GETENUM(StopRuntimeType);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddFieldJsonMake("CalleeStart", CalleeStart);
		jsonData.AddFieldJsonMake("CallerNext", CallerNext);
		jsonData.AddFieldJsonMake("EnableSpec", EnableSpec);
		jsonData.AddField("StopThis", StopThis.MakeJson());
		jsonData.AddField("LinkNodeTree", LinkNodeTree.MakeJson());
		jsonData.AddFieldEnum("StopRuntimeType", StopRuntimeType);
		return jsonData;
	}
}
