using LitJson;
using NGame2.NBattleView;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NUI.NComponent2.NBattle.NSub;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NEffect;

public class CreatureCasting : EffectBase, IKey, IExecute, IKeyFinalize
{
	public class CastingValueData : CastingComponent.ICastingValue
	{
		public CastingComponent.ECastingType CastingType { get; set; }

		public bool IsEnd { get; set; }

		public float Ratio { get; set; }
	}

	public class LocalItem : ItemBase
	{
		public float BeginTime;

		public CastingValueData CastingValue = new CastingValueData();
	}

	public static string ContentsTypeName = "Effect/" + typeof(CreatureCasting).Name;

	public ChainCall ChainCall;

	public ArgumentSingleGeneric<long> CreatureUid;

	public ArgumentSingleGeneric<float> DurationTime;

	public ArgumentSingleGeneric<bool> StopThis;

	public CastingComponent.ECastingType CastingType;

	public AnimationCurve RatioCurve = new AnimationCurve();

	public CreatureCasting(NodeData i_node)
		: base(i_node)
	{
		ChainCall = new ChainCall(this, "ChainCall", ActionStart);
		CreatureUid = new ArgumentSingleGeneric<long>(VariableKeyword.Myself);
		DurationTime = new ArgumentSingleGeneric<float>(VariableKeyword.DurationTime);
		StopThis = new ArgumentSingleGeneric<bool>(value: false);
	}

	public override void Wakeup(IVariable variable)
	{
		CreatureUid.GetValue(variable);
		DurationTime.GetValue(variable);
		StopThis.GetValue(variable);
	}

	private IVariable ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.BeginTime = runtime.LocalTimer.time;
		localItem.CastingValue.CastingType = CastingType;
		localItem.CastingValue.IsEnd = false;
		localItem.CastingValue.Ratio = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(RatioCurve, 0f, 0f);
		long value = CreatureUid.GetValue(variable, 0L);
		Creature bATTLECREATURE = runtime.SceneContainer.GETBATTLECREATURE((int)value);
		if (bATTLECREATURE != null)
		{
			bATTLECREATURE.CastingValue = localItem.CastingValue;
		}
		runtime.ExecuteContainer.Pending(this, localItem);
		return localItem.Variable;
	}

	private void ActionEnd(LocalItem localItem)
	{
		if (!localItem.CastingValue.IsEnd)
		{
			localItem.CastingValue.IsEnd = true;
			ChainCall.EndCall(localItem.Runtime, localItem.Variable);
		}
	}

	void IExecute.UpdateExecute(ItemBase itemBase)
	{
		if (itemBase is LocalItem localItem && !localItem.CastingValue.IsEnd)
		{
			float num = localItem.localTimer.time - localItem.BeginTime;
			float value = DurationTime.GetValue(localItem.Variable);
			float num2 = Mathf.Clamp01(num / Vespa_Float_Extension_Method.NoneZero(value));
			float ratio = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(RatioCurve, num2, num2);
			localItem.CastingValue.Ratio = ratio;
			if (num >= value || StopThis.GetValue(localItem.Variable, defaultValue: false))
			{
				ActionEnd(localItem);
			}
		}
	}

	bool IExecute.IsExecuteEnd(ItemBase itemBase)
	{
		if (!(itemBase is LocalItem localItem))
		{
			return true;
		}
		return localItem.CastingValue.IsEnd;
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
		CreatureUid.LoadFromJson(json["CreatureUid"]);
		DurationTime.LoadFromJson(json["DurationTime"]);
		StopThis.LoadFromJson(json["StopThis"]);
		CastingType = json["CastingType"].GETENUM(CastingType);
		RatioCurve = json["RatioCurve"].GetAnimationCurve(RatioCurve);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("ChainCall", ChainCall.MAKEJSON());
		jsonData.AddField("CreatureUid", CreatureUid.MakeJson());
		jsonData.AddField("DurationTime", DurationTime.MakeJson());
		jsonData.AddField("StopThis", StopThis.MakeJson());
		jsonData.AddFieldEnum("CastingType", CastingType);
		jsonData.AddFieldAnimationCurve("RatioCurve", RatioCurve);
		return jsonData;
	}
}
