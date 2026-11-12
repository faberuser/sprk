using System;
using System.Collections.Generic;
using LitJson;
using NGame2.NBattleView;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NScene;
using NGameAsset.NCreature;

namespace NGame2.NNodeTree.NContents.NBattle;

public class FindFieldMapCreature : BattleBase, IKey
{
	public class FindCreatureData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public string Code;

		bool ILoadJson.Load(JsonData json)
		{
			Code = json["Code"].GETSTRING(Code);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddFieldString("Code", Code);
			return jsonData;
		}
	}

	public static string ContentsTypeName = "Battle/" + typeof(FindFieldMapCreature).Name;

	public Callee CalleeStart;

	public Caller CallerEnd;

	public DefineVariable<long> DefineUid = new DefineVariable<long>();

	public FindCreatureData FindCreature = new FindCreatureData();

	public FindFieldMapCreature(NodeData i_node)
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
		if (BattleViewGenericSingleton<FieldMapManager2>.IsValidInstance && !DefineUid.Name.IsNullOrEmpty())
		{
			List<FieldMapManager2.Creature> creatureContainer = BattleViewGenericSingleton<FieldMapManager2>.instance.GetCreatureContainer();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(creatureContainer); i++)
			{
				FieldMapManager2.Creature creature = creatureContainer[i];
				if (!(creature.RenderFacade == null) && creature.RenderFacade.AssetCreature is CreatureInstance creatureInstance && !(creatureInstance.OpenData.GetStock().Code != FindCreature.Code))
				{
					ItemBase itemBase = runtime.ItemContainer.CreateItem<ItemBase>(this, variable);
					DefineUid.SetValue(itemBase, creature.Uid);
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
