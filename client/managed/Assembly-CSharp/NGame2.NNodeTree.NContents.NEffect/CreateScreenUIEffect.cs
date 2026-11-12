using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NScene;
using NGame2.NUI.NWindow;
using NGameAsset.NEffect;
using NVespa;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NEffect;

public class CreateScreenUIEffect : EffectBase, IKey, IExecute, IKeyFinalize
{
	public class LocalItem : ItemBase
	{
		public bool IsEnd;

		public float BeginTime;

		public ScreenUIEffect EffectContents;

		public GameObject EffectObject;
	}

	public static string ContentsTypeName = "Effect/" + typeof(CreateScreenUIEffect).Name;

	public ChainCall ChainCall;

	public ArgumentSingleGeneric<float> DurationTime;

	public ArgumentSingleGeneric<bool> StopThis;

	public string PrefabName = string.Empty;

	public GameUIRoot.ELayerType LayerType;

	public EStopContentsType StopContentsType;

	public CreateScreenUIEffect(NodeData i_node)
		: base(i_node)
	{
		ChainCall = new ChainCall(this, "ChainCall", ActionStart);
		DurationTime = new ArgumentSingleGeneric<float>(VariableKeyword.DurationTime);
		StopThis = new ArgumentSingleGeneric<bool>(value: false);
	}

	public override void Wakeup(IVariable variable)
	{
		DurationTime.GetValue(variable);
		StopThis.GetValue(variable);
	}

	private IVariable ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.BeginTime = runtime.LocalTimer.time;
		localItem.EffectContents = ScreenUIEffect.PlayEffect(PrefabName, LayerType);
		localItem.EffectObject = localItem.EffectContents.LastEffect;
		runtime.ExecuteContainer.Pending(this, localItem);
		return localItem.Variable;
	}

	private void ActionEnd(LocalItem localItem, ERequestStop requestStop)
	{
		if (!localItem.IsEnd)
		{
			bool flag = StopContentsType == EStopContentsType.StopWithThis || ((StopContentsType == EStopContentsType.StopIfRequestStop) ? (requestStop != ERequestStop.None) : ((StopContentsType == EStopContentsType.StopIfBattleCancel) ? (requestStop == ERequestStop.BattleCancel) : (StopContentsType != EStopContentsType.None)));
			if (flag && localItem.EffectContents != null)
			{
				localItem.EffectContents.Stop(localItem.EffectObject);
				localItem.EffectContents = null;
			}
			localItem.IsEnd = true;
			ChainCall.EndCall(localItem.Runtime, localItem.Variable);
		}
	}

	void IExecute.UpdateExecute(ItemBase itemBase)
	{
		if (itemBase is LocalItem { IsEnd: false } localItem)
		{
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
		Builder builder = SingletonAuto<BuilderContainer>.instance.Get(PrefabName);
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
		PrefabName = json["PrefabName"].GETSTRING(PrefabName);
		LayerType = json["LayerType"].GETENUM(LayerType);
		StopContentsType = json.GetJsonAny("StopContentsType", "StopEffectType").GETENUM(StopContentsType);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("ChainCall", ChainCall.MAKEJSON());
		jsonData.AddField("DurationTime", DurationTime.MakeJson());
		jsonData.AddField("StopThis", StopThis.MakeJson());
		jsonData.AddFieldString("PrefabName", PrefabName);
		jsonData.AddFieldEnum("LayerType", LayerType);
		jsonData.AddFieldEnum("StopContentsType", StopContentsType);
		return jsonData;
	}
}
