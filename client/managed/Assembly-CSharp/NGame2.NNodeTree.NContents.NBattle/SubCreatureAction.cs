using LitJson;
using NGame2.NBattleView;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NRender.NRenderFacade;
using NGameAsset;
using NGameAsset.NCreature;
using NVespa;

namespace NGame2.NNodeTree.NContents.NBattle;

public class SubCreatureAction : ExecuteBase, IKey, IExecute
{
	public class LocalItem : ItemBase, IVerifier
	{
		public bool IsEnd;

		public float BeginTime;

		public NGame2.NRender.NRenderFacade.AnimationData.StockData MotionStock;

		bool IVerifier.IsValid()
		{
			return !IsEnd;
		}
	}

	public static string ContentsTypeName = "Battle/" + typeof(SubCreatureAction).Name;

	public Callee CalleeStart;

	public Caller CallerEnd;

	public bool UseMyselfDirect;

	public ArgumentSingleGeneric<long> MyselfUid;

	public ArgumentSingleGeneric<object> MyselfDirect;

	public AnimationKeyData AnimationKey = new AnimationKeyData(DefineCreature.EMotion.NamedAction, string.Empty);

	public DefineCreature.EMotionEnd MotionEnd;

	public bool BlendMotion = true;

	public ArgumentSingleGeneric<float> DurationTime;

	public ArgumentSingleGeneric<bool> StopThis;

	public SubCreatureAction(NodeData i_node)
		: base(i_node)
	{
		CalleeStart = new Callee(this, "ContentsStart", ActionStart);
		CallerEnd = new Caller(this, "EndCaller");
		MyselfUid = new ArgumentSingleGeneric<long>(VariableKeyword.Myself);
		MyselfDirect = new ArgumentSingleGeneric<object>(VariableKeyword.Myself);
		DurationTime = new ArgumentSingleGeneric<float>(VariableKeyword.DurationTime);
		StopThis = new ArgumentSingleGeneric<bool>(value: false);
	}

	public override void Wakeup(IVariable variable)
	{
		MyselfUid.GetValue(variable);
		MyselfDirect.GetValue(variable);
		DurationTime.GetValue(variable);
		StopThis.GetValue(variable);
	}

	private SubCreature GetMyselfSubCreature(LocalItem localItem, IVariable variable)
	{
		SubCreature subCreature = null;
		if (UseMyselfDirect)
		{
			subCreature = MyselfDirect.GetValue(variable) as SubCreature;
		}
		else
		{
			Creature bATTLECREATURE = variable.RUNTIMEINSTANCE().SCENECONTAINER().GETBATTLECREATURE((int)MyselfUid.GetValue(variable));
			if (bATTLECREATURE == null)
			{
				return null;
			}
			subCreature = bATTLECREATURE.SubCreature;
		}
		if (subCreature == null || subCreature.ActionSubCreature == null)
		{
			return null;
		}
		return subCreature;
	}

	private void ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		SubCreature myselfSubCreature = GetMyselfSubCreature(localItem, variable);
		if (myselfSubCreature != null)
		{
			float value = DurationTime.GetValue(variable, 1f);
			NGame2.NRender.NRenderFacade.AnimationData.StockData stockData = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
			stockData.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Action;
			stockData.AnimationKey = AnimationKey;
			stockData.MotionEnd = MotionEnd;
			stockData.TimeLength = value;
			localItem.BeginTime = runtime.LocalTimer.time;
			localItem.MotionStock = stockData;
			myselfSubCreature.ActionSubCreature.Motion(localItem.MotionStock, BlendMotion, runtime.LocalTimer);
			runtime.ExecuteContainer.Pending(this, localItem);
		}
	}

	private void ActionEnd(LocalItem localItem)
	{
		if (!localItem.IsEnd)
		{
			SubCreature myselfSubCreature = GetMyselfSubCreature(localItem, localItem.Variable);
			if (myselfSubCreature != null && Vespa_Common_Class_Extension_Method.EQUALS(myselfSubCreature.ActionSubCreature.CurrentMotionStock, localItem.MotionStock) && MotionEnd == DefineCreature.EMotionEnd.End)
			{
				myselfSubCreature.ActionSubCreature.Idle();
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
		UseMyselfDirect = json["UseMyselfDirect"].GETBOOLEAN(UseMyselfDirect);
		if (!MyselfUid.LoadFromJson(json["MyselfUid"]))
		{
			MyselfUid.Holder.Value = SingletonAuto<ValueFactory>.instance.CreateVariable<long>(VariableKeyword.Myself);
		}
		if (!MyselfDirect.LoadFromJson(json["MyselfDirect"]))
		{
			MyselfDirect.Holder.Value = SingletonAuto<ValueFactory>.instance.CreateVariable<long>(VariableKeyword.Myself);
		}
		AnimationKey = AnimationKeyData.CreateFromJson(json["AnimationKey"]);
		BlendMotion = json["BlendMotion"].GETBOOLEAN(BlendMotion);
		MotionEnd = json["MotionEnd"].GETENUM(MotionEnd);
		DurationTime.LoadFromJson(json["DurationTime"]);
		StopThis.LoadFromJson(json["StopThis"]);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddFieldBool("UseMyselfDirect", UseMyselfDirect);
		jsonData.AddField("MyselfDirect", MyselfDirect.MakeJson());
		jsonData.AddField("MyselfUid", MyselfUid.MakeJson());
		jsonData.AddField("CalleeStart", CalleeStart.MAKEJSON());
		jsonData.AddField("CallerEnd", CallerEnd.MAKEJSON());
		jsonData.AddField("AnimationKey", AnimationKey.MakeJson());
		jsonData.AddFieldBool("BlendMotion", BlendMotion);
		jsonData.AddFieldEnum("MotionEnd", MotionEnd);
		jsonData.AddField("DurationTime", DurationTime.MakeJson());
		jsonData.AddField("StopThis", StopThis.MakeJson());
		return jsonData;
	}
}
