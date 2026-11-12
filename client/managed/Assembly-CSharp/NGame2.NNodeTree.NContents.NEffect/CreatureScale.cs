using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NRender;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NEffect;

public class CreatureScale : EffectBase, IKey, IExecute, IKeyFinalize
{
	public class LocalItem : ItemBase
	{
		public float BeginTime;

		public int Uid;

		public bool IsEnd;
	}

	public static string ContentsTypeName = "Effect/" + typeof(CreatureScale).Name;

	public ChainCall ChainCall;

	public ArgumentSingleGeneric<long> CreatureUid;

	public ArgumentSingleGeneric<float> DurationTime;

	public ArgumentSingleGeneric<bool> StopThis;

	public string Keyword = "NodeTree";

	public float TargetScale = 1f;

	public float BlendTime = 0.5f;

	public AnimationCurve BlendCurve = new AnimationCurve();

	public bool RestoreOnEnd;

	public CreatureScale(NodeData i_node)
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
		RenderFacade renderFacade = localItem.GetRenderFacade(localItem.Uid);
		if (renderFacade != null)
		{
			float runtimeScale = renderFacade.GetRuntimeScale(Keyword);
			renderFacade.ExtraScaleController.SetScale(Keyword, runtimeScale, TargetScale, BlendTime, BlendCurve);
		}
		return localItem.Variable;
	}

	private void ActionEnd(LocalItem localItem)
	{
		if (!localItem.IsEnd)
		{
			localItem.IsEnd = true;
			RenderFacade renderFacade = localItem.GetRenderFacade(localItem.Uid);
			if (renderFacade != null && RestoreOnEnd)
			{
				float runtimeScale = renderFacade.GetRuntimeScale(Keyword);
				renderFacade.ExtraScaleController.SetScale(Keyword, runtimeScale, 1f, BlendTime, BlendCurve);
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
		Keyword = json["Keyword"].GETSTRING(Keyword);
		TargetScale = json["TargetScale"].GETFLOAT(TargetScale);
		BlendTime = json["BlendTime"].GETFLOAT(BlendTime);
		BlendCurve = json["BlendCurve"].GetAnimationCurve(BlendCurve);
		RestoreOnEnd = json["RestoreOnEnd"].GETBOOLEAN(RestoreOnEnd);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("ChainCall", ChainCall.MAKEJSON());
		jsonData.AddField("CreatureUid", CreatureUid.MakeJson());
		jsonData.AddField("DurationTime", DurationTime.MakeJson());
		jsonData.AddField("StopThis", StopThis.MakeJson());
		jsonData.AddFieldString("Keyword", Keyword);
		jsonData.AddFieldDouble("TargetScale", TargetScale);
		jsonData.AddFieldDouble("BlendTime", BlendTime);
		jsonData.AddFieldAnimationCurve("BlendCurve", BlendCurve);
		jsonData.AddFieldBool("RestoreOnEnd", RestoreOnEnd);
		return jsonData;
	}
}
