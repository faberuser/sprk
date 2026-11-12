using LitJson;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NRender;
using NGameAsset;

namespace NGame2.NNodeTree.NContents.NBattle;

public class ModifyWeaponPrefix : BattleBase, IKey
{
	public static string ContentsTypeName = "Battle/" + typeof(ModifyWeaponPrefix).Name;

	public Callee CalleeStart;

	public Caller CallerEnd;

	public ArgumentSingleGeneric<long> CreatureUid;

	public string WeaponPrefix = string.Empty;

	public ModifyWeaponPrefix(NodeData node)
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
			rENDERFACADE.AssetCreatureAsync.ChangeWeaponStock(new WeaponStock(WeaponPrefix));
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
		WeaponPrefix = json["WeaponPrefix"].GETSTRING(WeaponPrefix);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("CalleeStart", CalleeStart.MAKEJSON());
		jsonData.AddField("CallerEnd", CallerEnd.MAKEJSON());
		jsonData.AddField("CreatureUid", CreatureUid.MakeJson());
		jsonData.AddFieldString("WeaponPrefix", WeaponPrefix);
		return jsonData;
	}
}
