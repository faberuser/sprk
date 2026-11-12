using System.Collections.Generic;
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

public class ActionModifierList : ExecuteBase, IKey, IExecute
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

	public class ReplaceTargetAnimationInfo : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public AnimationKeyData TargetKey = new AnimationKeyData(DefineCreature.EMotion.NamedAction, string.Empty);

		public NGame2.NRender.NRenderFacade.AnimationData.Modifier actionMod;

		bool ILoadJson.Load(JsonData json)
		{
			TargetKey = AnimationKeyData.CreateFromJson(json["TargetKey"]);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddField("TargetKey", TargetKey.MakeJson());
			return jsonData;
		}
	}

	public static string ContentsTypeName = "Execute/" + typeof(ActionModifierList).Name;

	public const DefineCreature.EMotion NAMED_ACTION = DefineCreature.EMotion.NamedAction;

	public Callee CalleeStart;

	public Caller CallerEnd;

	public List<ReplaceTargetAnimationInfo> ReplaceAnimList = new List<ReplaceTargetAnimationInfo>();

	public ArgumentSingleGeneric<long> MyselfUid;

	public string MotionPostfix = string.Empty;

	public ArgumentSingleGeneric<float> DurationTime;

	public ArgumentSingleGeneric<bool> StopThis;

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public ActionModifierList(NodeData i_node)
		: base(i_node)
	{
		CalleeStart = new Callee(this, "ContentsStart", ActionStart);
		CallerEnd = new Caller(this, "EndCaller");
		MyselfUid = new ArgumentSingleGeneric<long>(VariableKeyword.Myself);
		DurationTime = new ArgumentSingleGeneric<float>(VariableKeyword.DurationTime);
		StopThis = new ArgumentSingleGeneric<bool>(value: false);
	}

	public override void Wakeup(IVariable variable)
	{
		DurationTime.GetValue(variable);
		StopThis.GetValue(variable);
	}

	private void ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		RenderFacade renderFacade = UidCache.FindRenderFacade(variable, MyselfUid);
		if (renderFacade == null)
		{
			return;
		}
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.MyselfCache.SetRenderFacade(renderFacade);
		foreach (ReplaceTargetAnimationInfo replaceAnim in ReplaceAnimList)
		{
			replaceAnim.actionMod.Stack = 1;
			renderFacade.Action.AddModifier(replaceAnim.TargetKey, replaceAnim.actionMod);
		}
		localItem.BeginTime = runtime.LocalTimer.time;
		runtime.ExecuteContainer.Pending(this, localItem);
	}

	private void ActionEnd(LocalItem localItem)
	{
		if (localItem.IsEnd)
		{
			return;
		}
		RenderFacade renderFacade = localItem.MyselfCache.GetRenderFacade(localItem.Variable, MyselfUid);
		if (renderFacade != null)
		{
			foreach (ReplaceTargetAnimationInfo replaceAnim in ReplaceAnimList)
			{
				renderFacade.Action.RemoveModifier(replaceAnim.TargetKey);
			}
		}
		localItem.ClearMessageHolder();
		localItem.IsEnd = true;
		CallerEnd.Call(localItem.Runtime, localItem.Variable);
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

	public override void LoadFromJson(JsonData json)
	{
		CalleeStart.LOADFROMJSON(json["CalleeStart"]);
		CallerEnd.LOADFROMJSON(json["CallerEnd"]);
		if (!MyselfUid.LoadFromJson(json["MyselfUid"]))
		{
			MyselfUid.Holder.Value = SingletonAuto<ValueFactory>.instance.CreateVariable<long>(VariableKeyword.Myself);
		}
		DurationTime.LoadFromJson(json["DurationTime"]);
		StopThis.LoadFromJson(json["StopThis"]);
		MotionPostfix = json["MotionPostfix"].GETSTRING(MotionPostfix);
		ReplaceAnimList.LoadFromJson(json["ReplaceAnimList"], () => new ReplaceTargetAnimationInfo());
		if (ReplaceAnimList == null || ReplaceAnimList.Count <= 0)
		{
			return;
		}
		foreach (ReplaceTargetAnimationInfo replaceAnim in ReplaceAnimList)
		{
			replaceAnim.actionMod = default(NGame2.NRender.NRenderFacade.AnimationData.Modifier);
			string name = $"{replaceAnim.TargetKey.PlayName}_{MotionPostfix}";
			AnimationKeyData animationKey = new AnimationKeyData(DefineCreature.EMotion.NamedAction, name);
			replaceAnim.actionMod.AnimationKey = animationKey;
		}
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("CalleeStart", CalleeStart.MAKEJSON());
		jsonData.AddField("CallerEnd", CallerEnd.MAKEJSON());
		jsonData.AddField("MyselfUid", MyselfUid.MakeJson());
		jsonData.AddFieldString("MotionPostfix", MotionPostfix);
		jsonData.AddFieldList("ReplaceAnimList", ReplaceAnimList);
		jsonData.AddField("DurationTime", DurationTime.MakeJson());
		jsonData.AddField("StopThis", StopThis.MakeJson());
		return jsonData;
	}
}
