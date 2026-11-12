using System.Collections.Generic;
using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NNodeTree.NUtil;
using NGame2.NRender;
using NGame2.NScene;
using NGameAsset;
using NGameAsset.NCreature;
using NVespa;

namespace NGame2.NNodeTree.NContents.NExecute;

public class CreatureLoadAction : ExecuteBase, IKey, IExecute
{
	public class LocalItem : ItemBase, IVerifier
	{
		public bool IsEnd;

		public float BeginTime;

		public ICreatureLoad CreatureLoad;

		public IAsyncLoadBase AsyncLoad;

		bool IVerifier.IsValid()
		{
			return !IsEnd;
		}
	}

	public static string ContentsTypeName = "Execute/" + typeof(CreatureLoadAction).Name;

	public ChainCall ChainCall;

	public ArgumentSingleGeneric<long> MyselfUid;

	public bool LoadFacialModel;

	public bool LoadByMotionType;

	public DefineCreature.EMotion MotionType = DefineCreature.EMotion.EmotiMotion;

	public List<AnimationKeyData> AnimationKeyList = new List<AnimationKeyData>();

	public CreatureLoadAction(NodeData i_node)
		: base(i_node)
	{
		ChainCall = new ChainCall(this, "ChainCall", ActionStart);
		MyselfUid = new ArgumentSingleGeneric<long>(VariableKeyword.Myself);
	}

	public override void Wakeup(IVariable variable)
	{
	}

	private IVariable ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		RenderFacade renderFacade = UidCache.FindRenderFacade(variable, MyselfUid);
		if (renderFacade == null)
		{
			return variable;
		}
		ICreatureLoad assetCreatureAsync = renderFacade.AssetCreatureAsync;
		if (assetCreatureAsync == null)
		{
			return variable;
		}
		IAsyncLoadBase asyncLoad = LoadExtraAnimation(assetCreatureAsync);
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		localItem.BeginTime = runtime.LocalTimer.time;
		localItem.AsyncLoad = asyncLoad;
		localItem.CreatureLoad = assetCreatureAsync;
		runtime.ExecuteContainer.Pending(this, localItem);
		return localItem.Variable;
	}

	private IAsyncLoadBase LoadExtraAnimation(ICreatureLoad creatureAsync)
	{
		if (LoadFacialModel)
		{
			creatureAsync.LoadFacialModelAsync();
		}
		if (LoadByMotionType)
		{
			return creatureAsync.LoadExtraAnimation(MotionType);
		}
		return creatureAsync.LoadExtraAnimation(AnimationKeyList);
	}

	private void ActionEnd(LocalItem localItem)
	{
		if (!localItem.IsEnd)
		{
			localItem.IsEnd = true;
			localItem.AsyncLoad = null;
			ChainCall.EndCall(localItem.Runtime, localItem.Variable);
		}
	}

	public void UpdateExecute(ItemBase itemBase)
	{
		if (itemBase is LocalItem { IsEnd: false } localItem && localItem.AsyncLoad.ISDONE())
		{
			if (LoadFacialModel && localItem.CreatureLoad != null)
			{
				localItem.CreatureLoad.OpenFacialModel();
			}
			ActionEnd(localItem);
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

	public override void ReserveCache(IResourcePreloader preloader)
	{
	}

	public override void LoadFromJson(JsonData json)
	{
		ChainCall.LOADFROMJSON(json["ChainCall"]);
		if (!MyselfUid.LoadFromJson(json["MyselfUid"]))
		{
			MyselfUid.Holder.Value = SingletonAuto<ValueFactory>.instance.CreateVariable<long>(VariableKeyword.Myself);
		}
		LoadFacialModel = json["LoadFacialModel"].GETBOOLEAN(LoadFacialModel);
		LoadByMotionType = json["LoadByMotionType"].GETBOOLEAN(LoadByMotionType);
		MotionType = json["MotionType"].GETENUM(MotionType);
		AnimationKeyList.LoadFromJson(json["AnimationKeyList"]);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("ChainCall", ChainCall.MAKEJSON());
		jsonData.AddField("MyselfUid", MyselfUid.MakeJson());
		jsonData.AddFieldBool("LoadFacialModel", LoadFacialModel);
		jsonData.AddFieldBool("LoadByMotionType", LoadByMotionType);
		jsonData.AddFieldEnum("MotionType", MotionType);
		jsonData.AddField("AnimationKeyList", Json_Extension_Method_IList.MakeJson(AnimationKeyList));
		return jsonData;
	}
}
