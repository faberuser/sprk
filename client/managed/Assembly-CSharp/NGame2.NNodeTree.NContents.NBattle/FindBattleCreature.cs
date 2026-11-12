using System;
using LitJson;
using NGame2.NBattleField;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NScene;
using NShared;
using NVespa;

namespace NGame2.NNodeTree.NContents.NBattle;

public class FindBattleCreature : BattleBase, IKey
{
	public class FindCreatureData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public TeamType TeamType;

		public int Index;

		bool ILoadJson.Load(JsonData json)
		{
			TeamType = json["TeamType"].GETENUM(TeamType);
			Index = json["Index"].GETINT(Index);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddFieldEnum("TeamType", TeamType);
			jsonData.AddFieldLong("Index", Index);
			return jsonData;
		}
	}

	public static string ContentsTypeName = "Battle/" + typeof(FindBattleCreature).Name;

	public Callee CalleeStart;

	public Caller CallerEnd;

	public DefineVariable<long> DefineUid = new DefineVariable<long>();

	public FindCreatureData FindCreature = new FindCreatureData();

	public FindBattleCreature(NodeData i_node)
		: base(i_node)
	{
		CalleeStart = new Callee(this, "ContentsStart", ActionStart);
		CallerEnd = new Caller(this, "EndCaller");
	}

	public override void Wakeup(IVariable variable)
	{
	}

	private void ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		IVariable variable2 = variable;
		if (SingletonAuto<BattleFacade>.isValidInstance && !DefineUid.Name.IsNullOrEmpty())
		{
			for (int i = 0; i < SingletonAuto<BattleFacade>.instance.GetCreatureCount(); i++)
			{
				BattleFacade.CreatureCacheData creatureCacheByIndex = SingletonAuto<BattleFacade>.instance.GetCreatureCacheByIndex(i);
				if (creatureCacheByIndex.TeamType == FindCreature.TeamType && creatureCacheByIndex.CreateInfo.HeroInfo.HeroIndex == FindCreature.Index)
				{
					ItemBase itemBase = runtime.ItemContainer.CreateItem<ItemBase>(this, variable);
					DefineUid.SetValue(itemBase, creatureCacheByIndex.Uid);
					variable2 = itemBase.Variable;
				}
			}
		}
		CallerEnd.Call(runtime, variable2);
	}

	internal override string GetContentsTypeName()
	{
		return ContentsTypeName;
	}

	public override void ReserveCache(IResourcePreloader preloader)
	{
		Action action = delegate
		{
		};
		preloader.AddAction(action, string.Empty);
	}

	public override void LoadFromJson(JsonData json)
	{
		CalleeStart.LOADFROMJSON(json["CalleeStart"]);
		CallerEnd.LOADFROMJSON(json["CallerEnd"]);
		DefineUid.LOADFROMJSON(json["DefineUid"]);
		FindCreature.LOADFROMJSON(json["FindCreature"]);
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("CalleeStart", CalleeStart.MAKEJSON());
		jsonData.AddField("CallerEnd", CallerEnd.MAKEJSON());
		jsonData.AddFieldJsonMake("DefineUid", DefineUid);
		jsonData.AddFieldJsonMake("FindCreature", FindCreature);
		return jsonData;
	}
}
