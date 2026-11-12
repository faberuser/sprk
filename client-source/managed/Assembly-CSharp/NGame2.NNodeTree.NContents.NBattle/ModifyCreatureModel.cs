using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NRender;
using NGameAsset;

namespace NGame2.NNodeTree.NContents.NBattle;

public class ModifyCreatureModel : BattleBase, IKey
{
	public static string ContentsTypeName = "Battle/" + typeof(ModifyCreatureModel).Name;

	public Callee CalleeStart;

	public Caller CallerEnd;

	public ArgumentSingleGeneric<long> CreatureUid;

	public bool ChangeStar;

	public int HeroStar = 5;

	public bool ChangeBodyPrefix;

	public string BodyPrefix = string.Empty;

	public bool ChangeWeaponPrefix;

	public string WeaponPrefix = string.Empty;

	public bool ChangeHairPrefix;

	public string HairPrefix = string.Empty;

	public ModifyCreatureModel(NodeData node)
		: base(node)
	{
		CalleeStart = new Callee(this, "ContentsStart", ActionStart);
		CallerEnd = new Caller(this, "EndCaller");
		CreatureUid = new ArgumentSingleGeneric<long>(StringKey.Empty);
	}

	public override void Wakeup(IVariable variable)
	{
	}

	private void ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		int uid = (int)CreatureUid.GetValue(variable);
		RenderFacade rENDERFACADE = runtime.SCENECONTAINER().GETRENDERFACADE(uid);
		if (!(rENDERFACADE == null) && rENDERFACADE.AssetCreatureAsync != null)
		{
			CreatureStock stock = rENDERFACADE.AssetCreatureAsync.Stock;
			if (ChangeStar)
			{
				stock.Star = HeroStar;
			}
			if (ChangeBodyPrefix)
			{
				stock.BodyPrefix = BodyPrefix;
			}
			if (ChangeWeaponPrefix)
			{
				stock.WeaponStock.WeaponPrefix = WeaponPrefix;
			}
			if (ChangeHairPrefix)
			{
				stock.HairPrefix = HairPrefix;
			}
			rENDERFACADE.AssetCreatureAsync.ChangeStock(stock);
			CallerEnd.Call(runtime, variable);
		}
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void LoadFromJson(JsonData json)
	{
		CalleeStart.LOADFROMJSON(json["CalleeStart"]);
		CallerEnd.LOADFROMJSON(json["CallerEnd"]);
		CreatureUid.LoadFromJson(json["CreatureUid"]);
		ChangeStar = json["ChangeStar"].GETBOOLEAN(ChangeStar);
		HeroStar = json["HeroStar"].GETINT(HeroStar);
		ChangeBodyPrefix = json["ChangeBodyPrefix"].GETBOOLEAN(ChangeBodyPrefix);
		BodyPrefix = json["BodyPrefix"].GETSTRING(BodyPrefix);
		ChangeWeaponPrefix = json["ChangeWeaponPrefix"].GETBOOLEAN(ChangeWeaponPrefix);
		WeaponPrefix = json["WeaponPrefix"].GETSTRING(WeaponPrefix);
		ChangeHairPrefix = json["ChangeHairPrefix"].GETBOOLEAN(ChangeHairPrefix);
		HairPrefix = json["HairPrefix"].GETSTRING(HairPrefix);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("CalleeStart", CalleeStart.MAKEJSON());
		jsonData.AddField("CallerEnd", CallerEnd.MAKEJSON());
		jsonData.AddField("CreatureUid", CreatureUid.MakeJson());
		jsonData.AddFieldBool("ChangeStar", ChangeStar);
		jsonData.AddFieldLong("HeroStar", HeroStar);
		jsonData.AddFieldBool("ChangeBodyPrefix", ChangeBodyPrefix);
		jsonData.AddFieldString("BodyPrefix", BodyPrefix);
		jsonData.AddFieldBool("ChangeWeaponPrefix", ChangeWeaponPrefix);
		jsonData.AddFieldString("WeaponPrefix", WeaponPrefix);
		jsonData.AddFieldBool("ChangeHairPrefix", ChangeHairPrefix);
		jsonData.AddFieldString("HairPrefix", HairPrefix);
		return jsonData;
	}
}
