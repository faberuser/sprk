using LitJson;
using NGame2.N3D;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NRender;
using NGameAsset;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NEffect;

public class SpawnRenderCreature2 : EffectBase, IKey
{
	public static string ContentsTypeName = "Effect/" + typeof(SpawnRenderCreature2).Name;

	public ChainCall ChainCall;

	public DefineVariable<long> DefineUid = new DefineVariable<long>();

	public string CreatureCode;

	public ArgumentSingleGeneric<string> BodyPrefix;

	public ArgumentSingleGeneric<string> WeaponPrefix;

	public ArgumentSingleGeneric<long> Star;

	private ArgumentSingleGeneric<Vector3> _position;

	public SpawnRenderCreature2(NodeData i_node)
		: base(i_node)
	{
		ChainCall = new ChainCall(this, "ChainCall", ActionStart);
		_position = new ArgumentSingleGeneric<Vector3>(Vector3.zero);
		BodyPrefix = new ArgumentSingleGeneric<string>(string.Empty);
		WeaponPrefix = new ArgumentSingleGeneric<string>(string.Empty);
		Star = new ArgumentSingleGeneric<long>(0L);
	}

	public override void Wakeup(IVariable variable)
	{
		_position.GetValue(variable);
		BodyPrefix.GetValue(variable);
		WeaponPrefix.GetValue(variable);
		Star.GetValue(variable);
	}

	private IVariable ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		ISceneContainer sceneContainer = runtime.SCENECONTAINER();
		if (sceneContainer == null)
		{
			return variable;
		}
		string value = BodyPrefix.GetValue(variable);
		string value2 = WeaponPrefix.GetValue(variable);
		long value3 = Star.GetValue(variable);
		RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(null, TeamType.Left, CreatureCode, (int)value3);
		renderFacadeBuilder.SetModelPrefix(value, value2);
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
			if (!_position.TryGetValue(variable, out var value4))
			{
				value4 = Vector3.zero;
			}
			rENDERFACADE.transform.localPosition = value4;
		}
		return itemBase.Variable;
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData json)
	{
		ChainCall.LOADFROMJSON(json["ChainCall"]);
		DefineUid.LOADFROMJSON(json["DefineUid"]);
		CreatureCode = json["CreatureCode"].GETSTRING(CreatureCode);
		if (!BodyPrefix.LoadFromJson(json["BodyPrefix"]))
		{
			BodyPrefix.Holder.Value = SingletonAuto<ValueFactory>.instance.CreateConstant(string.Empty);
		}
		if (!WeaponPrefix.LoadFromJson(json["WeaponPrefix"]))
		{
			WeaponPrefix.Holder.Value = SingletonAuto<ValueFactory>.instance.CreateConstant(string.Empty);
		}
		if (!Star.LoadFromJson(json["Star"]))
		{
			Star.Holder.Value = SingletonAuto<ValueFactory>.instance.CreateConstant(0);
		}
		_position.LoadFromJson(json.GetJsonAny("Position", "LocalPosition"));
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("ChainCall", ChainCall.MAKEJSON());
		jsonData.AddFieldJsonMake("DefineUid", DefineUid);
		jsonData.AddFieldString("CreatureCode", CreatureCode);
		jsonData.AddField("BodyPrefix", BodyPrefix.MakeJson());
		jsonData.AddField("WeaponPrefix", WeaponPrefix.MakeJson());
		jsonData.AddField("Star", Star.MakeJson());
		jsonData.AddField("Position", _position.MakeJson());
		return jsonData;
	}
}
