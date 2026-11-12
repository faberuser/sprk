using System.Collections.Generic;
using NGame2.NBattleContext;
using NGame2.NMessage;
using NGame2.NNodeTree;
using NGame2.NNodeTree.NContents.NRoot;
using NGame2.NNodeTree.NRuntime;
using NGame2.NUI.NManager.NBattle;
using NGameAsset;
using NGameAsset.NBattleFieldMap2;
using NGameAsset.NEffect;
using NGameAsset.NEffect.NPlaySequence;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NScene.NPreloader;

public class BattleScenePreloader : RootPreloader
{
	public static LocalStorageEnum<EAsyncPreloadType> CreaturePreloadValue = new LocalStorageEnum<EAsyncPreloadType>(EAsyncPreloadType.Normal, typeof(BattleScenePreloader).FullName + "DisableCreaturePreloadValue");

	public static LocalStorageEnum<EAsyncPreloadType> EffectPreloadValue = new LocalStorageEnum<EAsyncPreloadType>(EAsyncPreloadType.Delay, typeof(BattleScenePreloader).FullName + "DisableEffectPreloadValue");

	public static LocalStorageEnum<EAsyncPreloadType> SoundPreloadValue = new LocalStorageEnum<EAsyncPreloadType>(EAsyncPreloadType.Delay, typeof(BattleScenePreloader).FullName + "DisableSoundPreloadValue");

	public EnterDungeonData EnterDungeon;

	public string FieldName = string.Empty;

	public bool PreloadDungeonMonster;

	public override void Begin()
	{
		base.Begin();
		ReserveSmallPool();
		AudioManager.PrefabAuto.Factory.ReserveComponent(30);
		CreaturePreloadType = CreaturePreloadValue.Value;
		EffectPreloadType = EffectPreloadValue.Value;
		SoundPreloadType = SoundPreloadValue.Value;
		for (int i = 0; i < DefineEffect.CodeArray_Battle.Length; i++)
		{
			string requestCode = DefineEffect.CodeArray_Battle[i];
			NGameAsset.NEffect.Builder builder = SingletonAuto<NGameAsset.NEffect.BuilderContainer>.instance.Get(requestCode);
			if (builder != null)
			{
				base.Preloader.ExecuteAction(CreaturePreloadType, delegate
				{
					base.Preloader.AddAsyncLoad(builder.CreateOpenData());
				});
			}
		}
		AddUiPreload(SceneAutoBehaviour<BattleManagement>.instance.ReserveCache, "LobbyManagement.instance.ReserveCache");
		base.Preloader.AddNodeTree("Common/Auto", -1L);
		base.Preloader.AddNodeTree("Common/DropItem", -1L);
		base.Preloader.AddAsyncLoadList(SingletonAuto<SoundContainer>.instance.ReserveSound("UI.Battle.BattleStart"));
		base.Preloader.ExecuteAction(SoundPreloadType, delegate
		{
			base.Preloader.AddAsyncLoadList(SingletonAuto<SoundContainer>.instance.ReserveSound("UI.WorldMap.Coin"));
			base.Preloader.AddAsyncLoadList(SingletonAuto<SoundContainer>.instance.ReserveSound("UI.Battle.UsingSkill"));
			base.Preloader.AddAsyncLoadList(SingletonAuto<SoundContainer>.instance.ReserveSound("UI.Campaign.Victory"));
			base.Preloader.AddAsyncLoadList(SingletonAuto<SoundContainer>.instance.ReserveSound("UI.Campaign.Lose"));
			base.Preloader.AddAsyncLoadList(SingletonAuto<SoundContainer>.instance.ReserveSound("UI.Campaign.GetExp"));
			base.Preloader.AddAsyncLoadList(SingletonAuto<SoundContainer>.instance.ReserveSound("UI.Campaign.LevelUp"));
			base.Preloader.AddAsyncLoadList(SingletonAuto<SoundContainer>.instance.ReserveSound("UI.Campaign.RewardItem"));
			base.Preloader.AddAsyncLoadList(SingletonAuto<SoundContainer>.instance.ReserveSound("UI.Boss.Warning"));
			base.Preloader.AddAsyncLoad(SingletonAuto<BgmManager2>.instance.ReserveLock("UI.Campaign.VictoryBGM"));
			base.Preloader.AddAsyncLoad(SingletonAuto<BgmManager2>.instance.ReserveLock("UI.Campaign.LoseBGM"));
		});
		LoadDungeonBgm();
		LoadBattleFieldMap();
		if (PreloadDungeonMonster)
		{
			CollectDungeonData();
		}
	}

	private void ReserveSmallPool()
	{
		SmallPooling<NGame2.NNodeTree.NContents.NRoot.BeginFireSkill.RootVariable>.ReserveInstance(30);
		SmallPooling<NGame2.NNodeTree.NContents.NRoot.FireSkill.RootVariable>.ReserveInstance(50);
		SmallPooling<NGame2.NNodeTree.NContents.NRoot.FlyingOperation.RootVariable>.ReserveInstance(50);
		SmallPooling<NGame2.NNodeTree.NContents.NRoot.ReserveOperation.RootVariable>.ReserveInstance(100);
		SmallPooling<NGame2.NNodeTree.NContents.NRoot.BeginOperation.RootVariable>.ReserveInstance(50);
		SmallPooling<ChildRoot.RuntimeVariable>.ReserveInstance(100);
		SmallPooling<OnCreatureEvent.RootVariable>.ReserveInstance(10);
		SmallPooling<NGame2.NNodeTree.NContents.NRoot.OnDamage.RootVariable>.ReserveInstance(20);
		SmallPooling<OnSpawnCreature.RootVariable>.ReserveInstance(5);
		SmallPooling<RuntimeInstance>.ReserveInstance(100);
		SmallPooling<CreatureVariable>.ReserveInstance(20);
		SmallPooling<LocalVariable>.ReserveInstance(1000);
		SmallPooling<SequenceClamp>.ReserveInstance(20);
		SmallPooling<SequenceEnd>.ReserveInstance(20);
		SmallPooling<SequenceLoop>.ReserveInstance(20);
		SmallPooling<SequenceNone>.ReserveInstance(20);
		SmallPooling<SequenceReady>.ReserveInstance(20);
		SmallPooling<CallbackMessage<SetPosition>>.ReserveInstance(50);
		SmallPooling<CallbackMessage<BeginMove>>.ReserveInstance(50);
		SmallPooling<CallbackMessage<Damage>>.ReserveInstance(50);
		SmallPooling<CallbackMessage<Heal>>.ReserveInstance(50);
		SmallPooling<CallbackMessage<SpawnCreature>>.ReserveInstance(50);
		SmallPooling<CallbackMessage<DestroyCreature>>.ReserveInstance(50);
		SmallPooling<CallbackMessage<ChangeState>>.ReserveInstance(50);
		SmallPooling<CallbackMessage<ChangeMp>>.ReserveInstance(50);
		SmallPooling<CallbackMessage<NShared.BeginFireSkill>>.ReserveInstance(50);
		SmallPooling<CallbackMessage<NShared.FireSkill>>.ReserveInstance(50);
		SmallPooling<CallbackMessage<EndFireSkill>>.ReserveInstance(50);
		SmallPooling<OperationMessage<NShared.BeginOperation>>.ReserveInstance(20);
		SmallPooling<OperationMessage<CancelOperation>>.ReserveInstance(20);
		SmallPooling<OperationMessage<EndOperation>>.ReserveInstance(20);
		SmallPooling<OperationMessage<NShared.FlyingOperation>>.ReserveInstance(20);
		SmallPooling<OperationMessage<NShared.ReserveOperation>>.ReserveInstance(20);
		SmallPooling<OperationMessage<ResistOperation>>.ReserveInstance(20);
		SmallPooling<BuffIconMessage>.ReserveInstance(20);
		SmallPooling<OperationRuntime>.ReserveInstance(20);
		SmallPooling<RenderGroupModifier2>.ReserveInstance(100);
	}

	private string GetFieldName()
	{
		if (!String_Extension_Method.IsNullOrEmpty(FieldName))
		{
			return FieldName;
		}
		if (EnterDungeon == null)
		{
			return string.Empty;
		}
		CampaignDungeonData linkedDungeonData = EnterDungeon.LinkedDungeonData;
		if (linkedDungeonData == null)
		{
			return string.Empty;
		}
		return linkedDungeonData.BattleField;
	}

	private void LoadDungeonBgm()
	{
		if (EnterDungeon != null)
		{
			CampaignDungeonData linkedDungeonData = EnterDungeon.LinkedDungeonData;
			if (linkedDungeonData != null && !linkedDungeonData.BgmCode.IsNullOrEmpty())
			{
				SingletonAuto<BgmManager2>.instance.Request(linkedDungeonData.BgmCode, isEvent: false);
			}
		}
	}

	private void LoadBattleFieldMap()
	{
		string fieldName = GetFieldName();
		if (String_Extension_Method.IsNullOrEmpty(fieldName))
		{
			return;
		}
		NGameAsset.NBattleFieldMap2.Builder builder = SingletonAuto<NGameAsset.NBattleFieldMap2.BuilderContainer>.instance.Get(fieldName);
		if (builder != null)
		{
			BattleFieldMapStock stock = new BattleFieldMapStock(fieldName, string.Empty);
			NGameAsset.NBattleFieldMap2.OpenData openData = builder.CreateOpenData(stock);
			if (openData != null)
			{
				base.Preloader.AddAsyncLoad(openData);
			}
		}
	}

	private void CollectDungeonData()
	{
		if (EnterDungeon == null)
		{
			return;
		}
		CampaignDungeonData linkedDungeonData = EnterDungeon.LinkedDungeonData;
		if (linkedDungeonData != null && linkedDungeonData.BattleType == BattleType.KarmaRaid)
		{
			int chapter = EnterDungeon.Chapter;
			int linkedDungeon = EnterDungeon.LinkedDungeon;
			int punishmentGroupIndex = GameRoot.Instance.KarmaDungeonDataContainer.GetPunishmentGroupIndex(chapter, linkedDungeon);
			IList<KarmaDungeonData> allData = GameRoot.Instance.KarmaDungeonDataContainer.GetAllData();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allData); i++)
			{
				KarmaDungeonData karmaDungeonData = allData[i];
				if (punishmentGroupIndex == karmaDungeonData.PunishmentGroupIndex)
				{
					BattleCreaturePreLoader.CollectDungeonData(new EnterDungeonData(karmaDungeonData.ChapterIndex, karmaDungeonData.DungeonIndex, karmaDungeonData.DungeonIndex), this, base.Preloader);
				}
			}
		}
		else
		{
			BattleCreaturePreLoader.CollectDungeonData(EnterDungeon, this, base.Preloader);
		}
	}
}
