using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NWindow;
using NShader;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NNodeTree.NContents.NEffect;

public class ScreenFilterEffect : EffectBase, IKey, IExecute, IKeyFinalize
{
	public class LocalItem : ItemBase
	{
		public bool IsEnd;

		public float BeginTime;
	}

	public static string ContentsTypeName = "Effect/" + typeof(ScreenFilterEffect).Name;

	public ChainCall ChainCall;

	public ArgumentSingleGeneric<float> DurationTime;

	public ChainFilter Filter = new ChainFilter();

	public ScreenFilterEffect(NodeData i_node)
		: base(i_node)
	{
		ChainCall = new ChainCall(this, "ChainCall", ActionStart);
		DurationTime = new ArgumentSingleGeneric<float>(VariableKeyword.DurationTime);
	}

	public override void Wakeup(IVariable variable)
	{
		DurationTime.GetValue(variable);
	}

	private IVariable ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.BeginTime = UnitySystemTimer.time;
		GlobalTimer.PauseTimer(GlobalTimer.EPauseFlag.ScreenFilter);
		SceneAutoBehaviour<BattleManagement>.instance.GetInstance<ScreenFilterWindow>().Open(Filter, null);
		runtime.ExecuteContainer.Pending(this, localItem);
		return localItem.Variable;
	}

	private void ActionEnd(LocalItem localItem)
	{
		if (!localItem.IsEnd)
		{
			SceneAutoBehaviour<BattleManagement>.instance.GetInstance<ScreenFilterWindow>().Close(null);
			GlobalTimer.ResumeTimer(GlobalTimer.EPauseFlag.ScreenFilter);
			localItem.IsEnd = true;
			ChainCall.EndCall(localItem.Runtime, localItem.Variable);
		}
	}

	void IExecute.UpdateExecute(ItemBase itemBase)
	{
		if (itemBase is LocalItem { IsEnd: false } localItem && UnitySystemTimer.time - localItem.BeginTime >= DurationTime.GetValue(localItem.Variable))
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

	void IKeyFinalize.Finalize(ItemBase itemBase)
	{
		if (itemBase is LocalItem localItem)
		{
			ActionEnd(localItem);
		}
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData json)
	{
		ChainCall.LOADFROMJSON(json["ChainCall"]);
		DurationTime.LoadFromJson(json["DurationTime"]);
		Filter.LOADFROMJSON(json["Filter"]);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("ChainCall", ChainCall.MAKEJSON());
		jsonData.AddField("DurationTime", DurationTime.MakeJson());
		jsonData.AddFieldJsonMake("Filter", Filter);
		return jsonData;
	}
}
