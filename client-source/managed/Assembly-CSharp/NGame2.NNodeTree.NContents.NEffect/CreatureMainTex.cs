using System;
using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NRender;
using NGame2.NScene;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NEffect;

public class CreatureMainTex : EffectBase, IKey, IExecute, IKeyFinalize
{
	public class LocalItem : ItemBase
	{
		public bool IsEnd;

		public float BeginTime;
	}

	public static string ContentsTypeName = "Effect/" + typeof(CreatureMainTex).Name;

	public ChainCall ChainCall;

	public ArgumentSingleGeneric<long> CreatureUid;

	public ArgumentSingleGeneric<float> DurationTime;

	public ArgumentSingleGeneric<bool> StopThis;

	public int TexPriority;

	public PrefabResAuto<Texture2D> TexAuto = new PrefabResAuto<Texture2D>();

	public CreatureMainTex(NodeData i_node)
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
		localItem.BeginTime = runtime.LocalTimer.time;
		long value = CreatureUid.GetValue(variable, 0L);
		RenderFacade renderFacade = runtime.SceneContainer.GetRenderFacade((int)value);
		if (renderFacade != null)
		{
			renderFacade.RenderGroupModifier.ReplaceTexture(TexAuto.Res, localItem, TexPriority);
		}
		runtime.ExecuteContainer.Pending(this, localItem);
		return localItem.Variable;
	}

	private void ActionEnd(LocalItem localItem)
	{
		if (!localItem.IsEnd)
		{
			long value = CreatureUid.GetValue(localItem.Variable, 0L);
			RenderFacade renderFacade = localItem.GetRenderFacade((int)value);
			if (renderFacade != null)
			{
				renderFacade.RenderGroupModifier.RemoveReplaceTexture(localItem);
			}
			localItem.IsEnd = true;
			ChainCall.EndCall(localItem.Runtime, localItem.Variable);
		}
	}

	void IExecute.UpdateExecute(ItemBase itemBase)
	{
		if (itemBase is LocalItem { IsEnd: false } localItem && (localItem.localTimer.time - localItem.BeginTime >= DurationTime.GetValue(localItem.Variable) || StopThis.GetValue(localItem.Variable, defaultValue: false)))
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

	public override void ReserveCache(IResourcePreloader preloader)
	{
		Action action = delegate
		{
			TexAuto.ReservePrefab();
		};
		preloader.AddAction(action, string.Empty);
	}

	public override void LoadFromJson(JsonData json)
	{
		ChainCall.LOADFROMJSON(json["ChainCall"]);
		CreatureUid.LoadFromJson(json["CreatureUid"]);
		DurationTime.LoadFromJson(json["DurationTime"]);
		StopThis.LoadFromJson(json["StopThis"]);
		TexPriority = json["TexPriority"].GETINT(TexPriority);
		TexAuto.LOADFROMJSON(json.GetJsonAny("TexAuto", "TexPath"));
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("ChainCall", ChainCall.MAKEJSON());
		jsonData.AddField("CreatureUid", CreatureUid.MakeJson());
		jsonData.AddField("DurationTime", DurationTime.MakeJson());
		jsonData.AddField("StopThis", StopThis.MakeJson());
		jsonData.AddFieldLong("TexPriority", TexPriority);
		jsonData.AddFieldJsonMake("TexAuto", TexAuto);
		return jsonData;
	}
}
