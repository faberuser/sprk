using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NNodeTree.NUtil;
using NGame2.NRender;
using NGame2.NRender.NRenderFacade;
using NGameAsset;
using NGameAsset.NCreature;
using NVespa;

namespace NGame2.NNodeTree.NContents.NExecute;

public class ActionModifier : ExecuteBase, IKey, IExecute
{
	public class LocalItem : ItemBase, IVerifier
	{
		public bool IsEnd;

		public float BeginTime;

		public UidCache MyselfCache = new UidCache();

		bool IVerifier.IsValid()
		{
			return !IsEnd;
		}
	}

	public static string ContentsTypeName = "Execute/" + typeof(ActionModifier).Name;

	public Callee CalleeStart;

	public Caller CallerEnd;

	public ArgumentSingleGeneric<long> MyselfUid;

	public AnimationKeyData TargetKey = new AnimationKeyData(DefineCreature.EMotion.NamedAction, string.Empty);

	public AnimationKeyData ReplaceKey = new AnimationKeyData(DefineCreature.EMotion.NamedAction, string.Empty);

	public bool UseCustomTimeValue;

	public NGame2.NRender.NRenderFacade.AnimationData.StockData.ETimeType ReplaceTimeType;

	public ArgumentSingleGeneric<float> ReplaceTimeValue;

	public ArgumentSingleGeneric<float> DurationTime;

	public ArgumentSingleGeneric<bool> StopThis;

	public ActionModifier(NodeData i_node)
		: base(i_node)
	{
		CalleeStart = new Callee(this, "ContentsStart", ActionStart);
		CallerEnd = new Caller(this, "EndCaller");
		MyselfUid = new ArgumentSingleGeneric<long>(VariableKeyword.Myself);
		DurationTime = new ArgumentSingleGeneric<float>(VariableKeyword.DurationTime);
		StopThis = new ArgumentSingleGeneric<bool>(value: false);
		ReplaceTimeValue = new ArgumentSingleGeneric<float>(1f);
	}

	public override void Wakeup(IVariable variable)
	{
		DurationTime.GetValue(variable);
		StopThis.GetValue(variable);
	}

	private void ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		RenderFacade renderFacade = UidCache.FindRenderFacade(variable, MyselfUid);
		if (!(renderFacade == null))
		{
			LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
			localItem.MyselfCache.SetRenderFacade(renderFacade);
			NGame2.NRender.NRenderFacade.AnimationData.Modifier modifier = new NGame2.NRender.NRenderFacade.AnimationData.Modifier
			{
				AnimationKey = ReplaceKey,
				UseCustomTimeValue = UseCustomTimeValue,
				TimeType = ReplaceTimeType,
				TimeValue = ReplaceTimeValue.GetValue(variable),
				Stack = 1
			};
			renderFacade.Action.AddModifier(TargetKey, modifier);
			localItem.BeginTime = runtime.LocalTimer.time;
			runtime.ExecuteContainer.Pending(this, localItem);
		}
	}

	private void ActionEnd(LocalItem localItem)
	{
		if (!localItem.IsEnd)
		{
			RenderFacade renderFacade = localItem.MyselfCache.GetRenderFacade(localItem.Variable, MyselfUid);
			if (renderFacade != null)
			{
				renderFacade.Action.RemoveModifier(TargetKey);
			}
			localItem.ClearMessageHolder();
			localItem.IsEnd = true;
			CallerEnd.Call(localItem.Runtime, localItem.Variable);
		}
	}

	public void UpdateExecute(ItemBase itemBase)
	{
		if (itemBase is LocalItem { IsEnd: false } localItem)
		{
			float num = localItem.localTimer.time - localItem.BeginTime;
			float value = DurationTime.GetValue(localItem.Variable, 1f);
			if (num > value || StopThis.GetValue(localItem.Variable, defaultValue: false))
			{
				ActionEnd(localItem);
			}
		}
	}

	public bool IsExecuteEnd(ItemBase itemBase)
	{
		if (!(itemBase is LocalItem localItem))
		{
			return true;
		}
		return localItem.IsEnd;
	}

	public void RequestStopExecute(ItemBase itemBase, ERequestStop requestStop)
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
		CalleeStart.LOADFROMJSON(json["CalleeStart"]);
		CallerEnd.LOADFROMJSON(json["CallerEnd"]);
		if (!MyselfUid.LoadFromJson(json["MyselfUid"]))
		{
			MyselfUid.Holder.Value = SingletonAuto<ValueFactory>.instance.CreateVariable<long>(VariableKeyword.Myself);
		}
		TargetKey = AnimationKeyData.CreateFromJson(json["TargetKey"]);
		ReplaceKey = AnimationKeyData.CreateFromJson(json["ReplaceKey"]);
		UseCustomTimeValue = json["UseCustomTimeValue"].GETBOOLEAN(UseCustomTimeValue);
		ReplaceTimeType = json["ReplaceTimeType"].GETENUM(ReplaceTimeType);
		ReplaceTimeValue.LoadFromJson(json["ReplaceTimeValue"]);
		DurationTime.LoadFromJson(json["DurationTime"]);
		StopThis.LoadFromJson(json["StopThis"]);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("CalleeStart", CalleeStart.MAKEJSON());
		jsonData.AddField("CallerEnd", CallerEnd.MAKEJSON());
		jsonData.AddField("MyselfUid", MyselfUid.MakeJson());
		jsonData.AddField("TargetKey", TargetKey.MakeJson());
		jsonData.AddField("ReplaceKey", ReplaceKey.MakeJson());
		jsonData.AddFieldBool("UseCustomTimeValue", UseCustomTimeValue);
		jsonData.AddFieldEnum("ReplaceTimeType", ReplaceTimeType);
		jsonData.AddField("ReplaceTimeValue", ReplaceTimeValue.MakeJson());
		jsonData.AddField("DurationTime", DurationTime.MakeJson());
		jsonData.AddField("StopThis", StopThis.MakeJson());
		return jsonData;
	}
}
