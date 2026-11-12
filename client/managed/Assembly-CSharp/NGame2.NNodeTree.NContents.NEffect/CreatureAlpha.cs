using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NRender;
using NGame2.NRender.NRenderFacade;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NEffect;

public class CreatureAlpha : EffectBase, IKey, IExecute, IKeyFinalize
{
	public class LocalItem : ItemBase
	{
		public float BeginTime;

		public int Uid;

		public bool IsEnd;
	}

	public static string ContentsTypeName = "Effect/" + typeof(CreatureAlpha).Name;

	public ChainCall ChainCall;

	public ArgumentSingleGeneric<long> CreatureUid;

	public ArgumentSingleGeneric<float> DurationTime;

	public ArgumentSingleGeneric<bool> StopThis;

	public AnimationCurve AlphaCurve = new AnimationCurve();

	public float ResetDelay = 0.5f;

	public float ResetDuration = 0.5f;

	public CreatureAlpha(NodeData i_node)
		: base(i_node)
	{
		ChainCall = new ChainCall(this, "ChainCall", ActionStart);
		CreatureUid = new ArgumentSingleGeneric<long>(StringKey.Empty);
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
		localItem.BeginTime = localItem.localTimer.time;
		localItem.Uid = (int)CreatureUid.GetValue(variable, 0L);
		runtime.ExecuteContainer.Pending(this, localItem);
		return localItem.Variable;
	}

	private void ActionEnd(LocalItem localItem)
	{
		if (!localItem.IsEnd)
		{
			localItem.IsEnd = true;
			RenderFacade renderFacade = localItem.GetRenderFacade(localItem.Uid);
			if (renderFacade != null)
			{
				renderFacade.ExtraAlphaController.ReleaseAlpha(ExtraAlphaController.EAlphaType.NodeTree, ResetDelay, ResetDuration);
			}
			ChainCall.EndCall(localItem.Runtime, localItem.Variable);
		}
	}

	void IExecute.UpdateExecute(ItemBase itemBase)
	{
		if (itemBase is LocalItem { IsEnd: false } localItem)
		{
			float num = localItem.localTimer.time - localItem.BeginTime;
			float value = DurationTime.GetValue(localItem.Variable);
			RenderFacade renderFacade = localItem.GetRenderFacade(localItem.Uid);
			if (renderFacade != null)
			{
				float i_frame = num / Vespa_Float_Extension_Method.NoneZero(value);
				float alpha = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(AlphaCurve, i_frame, 1f);
				renderFacade.ExtraAlphaController.SetAlpha(ExtraAlphaController.EAlphaType.NodeTree, alpha);
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
		CreatureUid.LoadFromJson(json["CreatureUid"]);
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
		jsonData.AddField("CreatureUid", CreatureUid.MakeJson());
		jsonData.AddField("DurationTime", DurationTime.MakeJson());
		jsonData.AddField("StopThis", StopThis.MakeJson());
		jsonData.AddFieldAnimationCurve("AlphaCurve", AlphaCurve);
		jsonData.AddFieldDouble("ResetDelay", ResetDelay);
		jsonData.AddFieldDouble("ResetDuration", ResetDuration);
		return jsonData;
	}
}
