using LitJson;
using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NBattleField;
using NGame2.NBattleView;
using NGame2.NNodeTree.NLink;
using NGame2.NNodeTree.NRuntime;
using NGame2.NNodeTree.NTypeValue;
using NGame2.NScene;
using NGameAsset;
using NGameAsset.NCreature;
using NShared;
using NShared.NUtil;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NNodeTree.NContents.NBattle;

public class CreateBattleCreature : BattleBase, IKey, IExecute
{
	public class CreateCreatureData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public int Uid;

		public TeamType TeamType;

		public int Index;

		public int Star;

		public int CostumeIndex;

		public int Level = 1;

		public int RandomSeed = 1;

		public int Tier;

		public MonsterGradeType Grade;

		public int SkillOpen;

		public int WeaponIndex;

		public bool UseSoulWeapon;

		public bool IsAI = true;

		public bool IsDummy = true;

		public int DefaultMaxMp = 6000;

		bool ILoadJson.Load(JsonData json)
		{
			Uid = json["Uid"].GETINT(Uid);
			TeamType = json["TeamType"].GETENUM(TeamType);
			Index = json["Index"].GETINT(Index);
			Star = json["Star"].GETINT(Star);
			CostumeIndex = json["CostumeIndex"].GETINT(CostumeIndex);
			Level = json["Level"].GETINT(Level);
			RandomSeed = json["RandomSeed"].GETINT(RandomSeed);
			Tier = json["Tier"].GETINT(Tier);
			Grade = json["Grade"].GETENUM(Grade);
			SkillOpen = json["SkillOpen"].GETINT(SkillOpen);
			WeaponIndex = json["WeaponIndex"].GETINT(WeaponIndex);
			UseSoulWeapon = json["UseSoulWeapon"].GETBOOLEAN(UseSoulWeapon);
			IsAI = json["IsAI"].GETBOOLEAN(IsAI);
			IsDummy = json["IsDummy"].GETBOOLEAN(IsDummy);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddFieldLong("Uid", Uid);
			jsonData.AddFieldEnum("TeamType", TeamType);
			jsonData.AddFieldLong("Index", Index);
			jsonData.AddFieldLong("Star", Star);
			jsonData.AddFieldLong("CostumeIndex", CostumeIndex);
			jsonData.AddFieldLong("Level", Level);
			jsonData.AddFieldLong("RandomSeed", RandomSeed);
			jsonData.AddFieldLong("Tier", Tier);
			jsonData.AddFieldEnum("Grade", Grade);
			jsonData.AddFieldLong("SkillOpen", SkillOpen);
			jsonData.AddFieldLong("WeaponIndex", WeaponIndex);
			jsonData.AddFieldBool("UseSoulWeapon", UseSoulWeapon);
			jsonData.AddFieldBool("IsAI", IsAI);
			jsonData.AddFieldBool("IsDummy", IsDummy);
			return jsonData;
		}
	}

	public class LocalItem : ItemBase
	{
		public bool IsEnd;

		public bool IsCreated;

		public int Uid;
	}

	public static string ContentsTypeName = "Battle/" + typeof(CreateBattleCreature).Name;

	public Callee CalleeStart;

	public Caller CallerEnd;

	public DefineVariable<long> DefineUid = new DefineVariable<long>();

	public CreateCreatureData CreateCreature = new CreateCreatureData();

	public ArgumentSingleGeneric<UnityEngine.Vector3> Position;

	public bool UseMyHeroShape = true;

	public CreateBattleCreature(NodeData i_node)
		: base(i_node)
	{
		CalleeStart = new Callee(this, "ContentsStart", ActionStart);
		CallerEnd = new Caller(this, "EndCaller");
		Position = new ArgumentSingleGeneric<UnityEngine.Vector3>(UnityEngine.Vector3.zero);
	}

	public override void Wakeup(IVariable variable)
	{
	}

	private void ActionStart(RuntimeInstance runtime, IVariable variable)
	{
		LocalItem localItem = runtime.ItemContainer.CreateItem<LocalItem>(this, variable);
		if (!SingletonAuto<BattleFacade>.isValidInstance)
		{
			localItem.IsCreated = true;
		}
		else
		{
			BattleCreatureCreateInfo battleCreatureCreateInfo = BattleCreatureCreateInfo.Create(CreateCreature.Index, CreateCreature.Star, CreateCreature.Level, CreateCreature.RandomSeed, CreateCreature.Tier, CreateCreature.Grade, CreateCreature.SkillOpen, 1000, CreateCreature.WeaponIndex, CreateCreature.UseSoulWeapon);
			battleCreatureCreateInfo.HeroInfo.CostumeIndex = CreateCreature.CostumeIndex;
			battleCreatureCreateInfo.IsAi = CreateCreature.IsAI;
			battleCreatureCreateInfo.IsDummy = CreateCreature.IsDummy;
			battleCreatureCreateInfo.Position = new NShared.NUtil.Vector3(Position.GetValue(variable));
			if (battleCreatureCreateInfo.StatData.GetStat(StatType.MaxMp) <= 0)
			{
				battleCreatureCreateInfo.StatData.SetStat(StatType.MaxMp, CreateCreature.DefaultMaxMp);
			}
			if (UseMyHeroShape)
			{
				ApplyMyHeroShape(battleCreatureCreateInfo);
			}
			SingletonAuto<BattleFacade>.instance.CreateBattleCreature(CreateCreature.TeamType, battleCreatureCreateInfo, CreateCreature.Uid, delegate(int creatureUid)
			{
				localItem.Uid = creatureUid;
				localItem.IsCreated = true;
				if (creatureUid != 0 && !DefineUid.Name.IsNullOrEmpty())
				{
					DefineUid.SetValue(localItem, creatureUid);
				}
			});
		}
		runtime.ExecuteContainer.Pending(this, localItem);
	}

	private void ApplyMyHeroShape(BattleCreatureCreateInfo createInfo)
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance || GlobalAutoBehaviour<AccountManager>.instance.HeroManager == null)
		{
			return;
		}
		CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(createInfo.HeroInfo.HeroIndex);
		if (cachedHeroInfo == null)
		{
			return;
		}
		createInfo.HeroInfo.Star = cachedHeroInfo.Star;
		createInfo.HeroInfo.CostumeIndex = cachedHeroInfo.CostumeIndex;
		createInfo.HeroInfo.HairCostumeIndex = cachedHeroInfo.HairCostumeIndex;
		createInfo.HeroInfo.HideUniqueWeapon = cachedHeroInfo.HideUniqueWeapon;
		createInfo.HeroInfo.WeaponCostumeIndex = cachedHeroInfo.WeaponCostumeIndex;
		for (int i = 1; i <= 5; i++)
		{
			EquipItemInfo equipItemInfoByPartIndex = cachedHeroInfo.GetEquipItemInfoByPartIndex(i);
			if (equipItemInfoByPartIndex != null)
			{
				createInfo.HeroInfo.SetEquipItemInfo(i, equipItemInfoByPartIndex);
			}
		}
		foreach (AccessoryPartType item in AccessoryPartTypeHelper.PartTypes())
		{
			if (cachedHeroInfo.GetAccessoryIndex(item) != 0)
			{
				int accessoryIndex = cachedHeroInfo.GetAccessoryIndex(item);
				PositionInfo accessoryPositionInfo = cachedHeroInfo.GetAccessoryPositionInfo(item);
				createInfo.HeroInfo.SetAccessoryCostume(item, accessoryIndex, accessoryPositionInfo);
			}
		}
	}

	private void ActionEnd(LocalItem localItem)
	{
		if (!localItem.IsEnd)
		{
			localItem.IsEnd = true;
			CallerEnd.Call(localItem.Runtime, localItem.Variable);
		}
	}

	public void UpdateExecute(ItemBase itemBase)
	{
		if (!(itemBase is LocalItem { IsEnd: false, IsCreated: not false } localItem))
		{
			return;
		}
		if (localItem.Uid != 0)
		{
			Creature bATTLECREATURE = localItem.Variable.RUNTIMEINSTANCE().SCENECONTAINER().GETBATTLECREATURE(localItem.Uid);
			if (bATTLECREATURE == null)
			{
				return;
			}
			bATTLECREATURE.NodeTreeController.SyncPosition.DisableLock++;
		}
		ActionEnd(localItem);
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
		if (GameRoot.Instance.CreatureDataContainer.GetData(CreateCreature.Index) == null)
		{
			return;
		}
		BattleCreatureCreateInfo battleCreatureCreateInfo = BattleCreatureCreateInfo.Create(CreateCreature.Index, CreateCreature.Star, CreateCreature.Level, CreateCreature.RandomSeed, CreateCreature.Tier, CreateCreature.Grade, CreateCreature.SkillOpen, 1000, CreateCreature.WeaponIndex, CreateCreature.UseSoulWeapon);
		battleCreatureCreateInfo.HeroInfo.CostumeIndex = CreateCreature.CostumeIndex;
		battleCreatureCreateInfo.IsAi = CreateCreature.IsAI;
		battleCreatureCreateInfo.IsDummy = CreateCreature.IsDummy;
		if (battleCreatureCreateInfo.StatData.GetStat(StatType.MaxMp) <= 0)
		{
			battleCreatureCreateInfo.StatData.SetStat(StatType.MaxMp, CreateCreature.DefaultMaxMp);
		}
		if (UseMyHeroShape)
		{
			ApplyMyHeroShape(battleCreatureCreateInfo);
		}
		RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(null, TeamType.Left, battleCreatureCreateInfo.HeroInfo);
		CreatureStock stock = renderFacadeBuilder.GetStock();
		Builder creatureBuilder = SingletonAuto<BuilderContainer>.instance.Get(stock.Code);
		if (creatureBuilder != null)
		{
			preloader.ExecuteAction(preloader.CreaturePreloadType, delegate
			{
				preloader.AddAsyncLoad(creatureBuilder.CreateOpenData(stock));
			});
		}
	}

	public override void LoadFromJson(JsonData json)
	{
		CalleeStart.LOADFROMJSON(json["CalleeStart"]);
		CallerEnd.LOADFROMJSON(json["CallerEnd"]);
		UseMyHeroShape = json["UseMyHeroShape"].GETBOOLEAN(UseMyHeroShape);
		DefineUid.LOADFROMJSON(json["DefineUid"]);
		CreateCreature.LOADFROMJSON(json["CreateCreature"]);
		Position.LoadFromJson(json["Position"]);
		if (!json.Contains("UseMyHeroShape"))
		{
			UseMyHeroShape = CreateCreature.TeamType == TeamType.Left;
		}
	}

	public override JsonData MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("type", this.GETCONTENTSTYPENAME());
		jsonData.AddField("CalleeStart", CalleeStart.MAKEJSON());
		jsonData.AddField("CallerEnd", CallerEnd.MAKEJSON());
		jsonData.AddFieldBool("UseMyHeroShape", UseMyHeroShape);
		jsonData.AddFieldJsonMake("DefineUid", DefineUid);
		jsonData.AddFieldJsonMake("CreateCreature", CreateCreature);
		jsonData.AddField("Position", Position.MakeJson());
		return jsonData;
	}
}
