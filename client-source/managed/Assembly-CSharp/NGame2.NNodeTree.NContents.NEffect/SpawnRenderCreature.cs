using LitJson;
using NGame2.N3D;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NRender;
using NGame2.NScene;
using NGameAsset;
using NGameAsset.NCreature;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NEffect;

public class SpawnRenderCreature : EffectBase, IKey
{
	public static string ContentsTypeName = "Effect/" + typeof(SpawnRenderCreature).Name;

	public ChainCall ChainCall;

	public DefineVariable<long> DefineUid = new DefineVariable<long>();

	public string CreatureCode;

	public string BodyPrefix;

	public string WeaponPrefix;

	public int Star;

	private ArgumentSingleGeneric<Vector3> _position;

	public SpawnRenderCreature(NodeData i_node)
		: base(i_node)
	{
		ChainCall = new ChainCall(this, "ChainCall", ActionStart);
		_position = new ArgumentSingleGeneric<Vector3>(Vector3.zero);
	}

	public override void Wakeup(IVariable variable)
	{
		_position.GetValue(variable);
	}

	private IVariable ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		ISceneContainer sceneContainer = runtime.SCENECONTAINER();
		if (sceneContainer == null)
		{
			return variable;
		}
		RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(null, TeamType.Left, CreatureCode, Star);
		renderFacadeBuilder.SetModelPrefix(BodyPrefix, WeaponPrefix);
		renderFacadeBuilder.LoadLevel = CreatureStock.ELoadLevel.IncludeFacial;
		ItemBase itemBase = runtime.ItemContainer.CreateItem<ItemBase>(this, variable);
		int num = sceneContainer.SpawnRenderFacade(DefineUid.Name, renderFacadeBuilder);
		if (num != 0 && !DefineUid.Name.IsNullOrEmpty())
		{
			DefineUid.SetValue(itemBase, num);
		}
		RenderFacade rENDERFACADE = sceneContainer.GETRENDERFACADE(num);
		if (rENDERFACADE != null)
		{
			if (!_position.TryGetValue(variable, out var value))
			{
				value = Vector3.zero;
			}
			rENDERFACADE.transform.localPosition = value;
		}
		return itemBase.Variable;
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void ReserveCache(IResourcePreloader preloader)
	{
		Builder builder = SingletonAuto<BuilderContainer>.instance.Get(CreatureCode);
		if (builder != null)
		{
			preloader.ExecuteAction(preloader.EffectPreloadType, delegate
			{
				CreatureStock stock = new CreatureStock
				{
					Code = CreatureCode,
					Star = Star,
					BodyPrefix = BodyPrefix,
					WeaponStock = 
					{
						WeaponPrefix = WeaponPrefix
					},
					LoadLevel = CreatureStock.ELoadLevel.IncludeFacial
				};
				preloader.AddAsyncLoad(builder.CreateOpenData(stock));
			});
		}
	}

	public override void LoadFromJson(JsonData json)
	{
		ChainCall.LOADFROMJSON(json["ChainCall"]);
		DefineUid.LOADFROMJSON(json["DefineUid"]);
		CreatureCode = json["CreatureCode"].GETSTRING(CreatureCode);
		BodyPrefix = json["BodyPrefix"].GETSTRING(BodyPrefix);
		WeaponPrefix = json["WeaponPrefix"].GETSTRING(WeaponPrefix);
		Star = json["Star"].GETINT(Star);
		_position.LoadFromJson(json.GetJsonAny("Position", "LocalPosition"));
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("ChainCall", ChainCall.MAKEJSON());
		jsonData.AddFieldJsonMake("DefineUid", DefineUid);
		jsonData.AddFieldString("CreatureCode", CreatureCode);
		jsonData.AddFieldString("BodyPrefix", BodyPrefix);
		jsonData.AddFieldString("WeaponPrefix", WeaponPrefix);
		jsonData.AddFieldLong("Star", Star);
		jsonData.AddField("Position", _position.MakeJson());
		return jsonData;
	}
}
