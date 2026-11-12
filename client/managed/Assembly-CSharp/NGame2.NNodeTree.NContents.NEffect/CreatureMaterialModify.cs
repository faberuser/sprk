using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NRender;
using NGameAsset;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NEffect;

public class CreatureMaterialModify : EffectBase, IKey, IExecute, IKeyFinalize
{
	public class LocalItem : ItemBase
	{
		public float BeginTime;

		public int Uid;

		public bool IsEnd;

		public CreatureMaterialModifierColor MtrlModifier;
	}

	public static string ContentsTypeName = "Effect/" + typeof(CreatureMaterialModify).Name;

	public ChainCall ChainCall;

	public ArgumentSingleGeneric<long> CreatureUid;

	public ArgumentSingleGeneric<float> DurationTime;

	public ArgumentSingleGeneric<bool> StopThis;

	public string RendererName;

	public string MaterialName;

	public string PropertyName;

	public Gradient ColorGradient = new Gradient();

	public CreatureMaterialModify(NodeData i_node)
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
		localItem.BeginTime = localItem.localTimer.time;
		localItem.Uid = (int)CreatureUid.GetValue(variable, 0L);
		int uid = localItem.Uid;
		RenderFacade renderFacade = runtime.SceneContainer.GetRenderFacade(uid);
		if (renderFacade != null)
		{
			ICreature assetCreature = renderFacade.AssetCreature;
			if (assetCreature != null)
			{
				localItem.MtrlModifier = new CreatureMaterialModifierColor(RendererName, MaterialName, PropertyName);
				assetCreature.AddMaterialModifier(localItem.MtrlModifier);
			}
		}
		runtime.ExecuteContainer.Pending(this, localItem);
		return localItem.Variable;
	}

	private void ActionEnd(LocalItem localItem)
	{
		if (!localItem.IsEnd)
		{
			localItem.IsEnd = true;
			if (localItem.MtrlModifier != null)
			{
				localItem.MtrlModifier.End();
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
			if (localItem.MtrlModifier != null)
			{
				float time = num / Vespa_Float_Extension_Method.NoneZero(value);
				Color fieldValue = ColorGradient.Evaluate(time);
				localItem.MtrlModifier.SetFieldValue(fieldValue);
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
		RendererName = json["RendererName"].GETSTRING(string.Empty);
		MaterialName = json["MaterialName"].GETSTRING(string.Empty);
		PropertyName = json["PropertyName"].GETSTRING(string.Empty);
		ColorGradient = json["ColorGradient"].GetGradient(ColorGradient);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("ChainCall", ChainCall.MAKEJSON());
		jsonData.AddField("CreatureUid", CreatureUid.MakeJson());
		jsonData.AddField("DurationTime", DurationTime.MakeJson());
		jsonData.AddField("StopThis", StopThis.MakeJson());
		jsonData.AddFieldString("RendererName", RendererName);
		jsonData.AddFieldString("MaterialName", MaterialName);
		jsonData.AddFieldString("PropertyName", PropertyName);
		jsonData.AddFieldGradient("ColorGradient", ColorGradient);
		return jsonData;
	}
}
