using LitJson;
using NGame2.NBattleView;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NRender;
using NGame2.NRender.NRenderFacade;
using NGameAsset;
using NGameAsset.NCreature;
using NVespa;

namespace NGame2.NNodeTree.NContents.NBattle;

public class FieldMapCreatureAction : ExecuteBase, IKey, IExecute
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

	public static string ContentsTypeName = "Battle/" + typeof(FieldMapCreatureAction).Name;

	public Callee CalleeStart;

	public Caller CallerEnd;

	public ArgumentSingleGeneric<long> FieldMapCreatureUid;

	public AnimationKeyData AnimationKey = new AnimationKeyData(DefineCreature.EMotion.NamedAction, string.Empty);

	public DefineCreature.EMotionEnd MotionEnd;

	public bool BlendMotion = true;

	public bool UseCustomFacial;

	public string[] FacialNameArray = new string[2]
	{
		string.Empty,
		string.Empty
	};

	public ArgumentSingleGeneric<float> DurationTime;

	public ArgumentSingleGeneric<bool> StopThis;

	public FieldMapCreatureAction(NodeData i_node)
		: base(i_node)
	{
		CalleeStart = new Callee(this, "ContentsStart", ActionStart);
		CallerEnd = new Caller(this, "EndCaller");
		FieldMapCreatureUid = new ArgumentSingleGeneric<long>(VariableKeyword.FieldMapCreature);
		DurationTime = new ArgumentSingleGeneric<float>(VariableKeyword.DurationTime);
		StopThis = new ArgumentSingleGeneric<bool>(value: false);
	}

	public override void Wakeup(IVariable variable)
	{
		FieldMapCreatureUid.GetValue(variable);
		DurationTime.GetValue(variable);
		StopThis.GetValue(variable);
	}

	private RenderFacade GetFieldMapCreature(LocalItem localItem, IVariable variable)
	{
		if (!BattleViewGenericSingleton<FieldMapManager2>.IsValidInstance)
		{
			return null;
		}
		return BattleViewGenericSingleton<FieldMapManager2>.instance.GetRenderFacade(FieldMapCreatureUid.GetValue(variable));
	}

	private void ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		RenderFacade fieldMapCreature = GetFieldMapCreature(localItem, variable);
		if (!(fieldMapCreature == null))
		{
			float value = DurationTime.GetValue(variable, 1f);
			NGame2.NRender.NRenderFacade.AnimationData.StockData stock = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
			stock.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Action;
			stock.AnimationKey = AnimationKey;
			stock.MotionEnd = MotionEnd;
			stock.TimeLength = value;
			fieldMapCreature.Action.ApplyModifier(ref stock);
			CreatureMotion(fieldMapCreature, stock, localItem.localTimer);
			localItem.MotionStock = stock;
			localItem.BeginTime = runtime.LocalTimer.time;
			runtime.ExecuteContainer.Pending(this, localItem);
		}
	}

	private void CreatureMotion(RenderFacade fieldMapCreature, NGame2.NRender.NRenderFacade.AnimationData.StockData motionStock, ILocalTimer localTimer)
	{
		FacialAnimationData facialAnimationData = null;
		if (UseCustomFacial && !__Extension_Method_ICollection__.IsNullOrEmpty(FacialNameArray) && (!String_Extension_Method.IsNullOrEmpty(FacialNameArray[0]) || !String_Extension_Method.IsNullOrEmpty(FacialNameArray[1])))
		{
			facialAnimationData = new FacialAnimationData();
			facialAnimationData.StaticShapeList.AddRange(FacialNameArray);
		}
		fieldMapCreature.Action.Motion(motionStock, BlendMotion, localTimer, facialAnimationData);
	}

	private void ActionEnd(LocalItem localItem)
	{
		if (!localItem.IsEnd)
		{
			RenderFacade fieldMapCreature = GetFieldMapCreature(localItem, localItem.Variable);
			if (fieldMapCreature != null && Vespa_Common_Class_Extension_Method.EQUALS(fieldMapCreature.Action.CurrentMotionStock, localItem.MotionStock) && MotionEnd == DefineCreature.EMotionEnd.End)
			{
				fieldMapCreature.Action.Idle();
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
		if (!FieldMapCreatureUid.LoadFromJson(json["FieldMapCreatureUid"]))
		{
			FieldMapCreatureUid.Holder.Value = SingletonAuto<ValueFactory>.instance.CreateVariable<long>(VariableKeyword.FieldMapCreature);
		}
		AnimationKey = AnimationKeyData.CreateFromJson(json["AnimationKey"]);
		BlendMotion = json["BlendMotion"].GETBOOLEAN(BlendMotion);
		MotionEnd = json["MotionEnd"].GETENUM(MotionEnd);
		UseCustomFacial = json["UseCustomFacial"].GETBOOLEAN(UseCustomFacial);
		FacialNameArray.LoadFromJsonEx(json["FacialNameArray"]);
		DurationTime.LoadFromJson(json["DurationTime"]);
		StopThis.LoadFromJson(json["StopThis"]);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("FieldMapCreatureUid", FieldMapCreatureUid.MakeJson());
		jsonData.AddField("CalleeStart", CalleeStart.MAKEJSON());
		jsonData.AddField("CallerEnd", CallerEnd.MAKEJSON());
		jsonData.AddField("AnimationKey", AnimationKey.MakeJson());
		jsonData.AddFieldBool("BlendMotion", BlendMotion);
		jsonData.AddFieldEnum("MotionEnd", MotionEnd);
		jsonData.AddFieldBool("UseCustomFacial", UseCustomFacial);
		jsonData.AddField("FacialNameArray", Json_Extension_Method_IList.MakeJson(FacialNameArray));
		jsonData.AddField("DurationTime", DurationTime.MakeJson());
		jsonData.AddField("StopThis", StopThis.MakeJson());
		return jsonData;
	}
}
