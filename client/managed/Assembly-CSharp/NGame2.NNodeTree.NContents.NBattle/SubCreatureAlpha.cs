using LitJson;
using NGame2.NBattleView;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NRender.NRenderFacade;
using NVespa;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NBattle;

public class SubCreatureAlpha : ExecuteBase, IKey, IExecute, IKeyFinalize
{
	public class LocalItem : ItemBase
	{
		public float BeginTime;

		public SubCreature SubCreature;

		public bool IsEnd;
	}

	public static string ContentsTypeName = "Battle/" + typeof(SubCreatureAlpha).Name;

	public ChainCall ChainCall;

	public bool UseMyselfDirect;

	public ArgumentSingleGeneric<long> MyselfUid;

	public ArgumentSingleGeneric<object> MyselfDirect;

	public ArgumentSingleGeneric<float> DurationTime;

	public ArgumentSingleGeneric<bool> StopThis;

	public AnimationCurve AlphaCurve = new AnimationCurve();

	public float ResetDelay = 0.5f;

	public float ResetDuration = 0.5f;

	public SubCreatureAlpha(NodeData i_node)
		: base(i_node)
	{
		ChainCall = new ChainCall(this, "ChainCall", ActionStart);
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

	private IVariable ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.BeginTime = localItem.localTimer.time;
		localItem.SubCreature = GetMyselfSubCreature(localItem, variable);
		runtime.ExecuteContainer.Pending(this, localItem);
		return localItem.Variable;
	}

	private void ActionEnd(LocalItem localItem)
	{
		if (!localItem.IsEnd)
		{
			localItem.IsEnd = true;
			localItem.SubCreature?.ExtraAlphaController.ReleaseAlpha(ExtraAlphaController.EAlphaType.NodeTree, ResetDelay, ResetDuration);
			ChainCall.EndCall(localItem.Runtime, localItem.Variable);
		}
	}

	void IExecute.UpdateExecute(ItemBase itemBase)
	{
		if (itemBase is LocalItem { IsEnd: false } localItem)
		{
			float num = localItem.localTimer.time - localItem.BeginTime;
			float value = DurationTime.GetValue(localItem.Variable);
			SubCreature subCreature = localItem.SubCreature;
			if (subCreature != null)
			{
				float i_frame = num / Vespa_Float_Extension_Method.NoneZero(value);
				float alpha = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(AlphaCurve, i_frame, 1f);
				subCreature.ExtraAlphaController.SetAlpha(ExtraAlphaController.EAlphaType.NodeTree, alpha);
			}
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
		UseMyselfDirect = json["UseMyselfDirect"].GETBOOLEAN(UseMyselfDirect);
		if (!MyselfUid.LoadFromJson(json["MyselfUid"]))
		{
			MyselfUid.Holder.Value = SingletonAuto<ValueFactory>.instance.CreateVariable<long>(VariableKeyword.Myself);
		}
		if (!MyselfDirect.LoadFromJson(json["MyselfDirect"]))
		{
			MyselfDirect.Holder.Value = SingletonAuto<ValueFactory>.instance.CreateVariable<long>(VariableKeyword.Myself);
		}
		DurationTime.LoadFromJson(json["DurationTime"]);
		StopThis.LoadFromJson(json["StopThis"]);
		AlphaCurve = json["AlphaCurve"].GetAnimationCurve(AlphaCurve);
		ResetDelay = json["ResetDelay"].GETFLOAT(ResetDelay);
		ResetDuration = json["ResetDuration"].GETFLOAT(ResetDuration);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("ChainCall", ChainCall.MAKEJSON());
		jsonData.AddFieldBool("UseMyselfDirect", UseMyselfDirect);
		jsonData.AddField("MyselfDirect", MyselfDirect.MakeJson());
		jsonData.AddField("MyselfUid", MyselfUid.MakeJson());
		jsonData.AddField("DurationTime", DurationTime.MakeJson());
		jsonData.AddField("StopThis", StopThis.MakeJson());
		jsonData.AddFieldAnimationCurve("AlphaCurve", AlphaCurve);
		jsonData.AddFieldDouble("ResetDelay", ResetDelay);
		jsonData.AddFieldDouble("ResetDuration", ResetDuration);
		return jsonData;
	}
}
