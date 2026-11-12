using System.Collections.Generic;
using NGame2.NBattleContext;
using NGame2.NScene.NPreloader;
using NShared;

namespace NGame2.NBattleField.NManager.NPlay;

public interface IDataProvider
{
	void Preload(BattleScenePreloader preloader);

	EnterDungeonData GetEnterDungeon();

	void OnEnterBattle();

	void OnWaveMoveEnd();

	bool CanStartBattle();

	BattleStartEffectData GetBattleStartEffectData();

	bool CanNextDungeon();

	bool IsOnline();

	IPlay GetBattlePlay();

	BattleType GetBattleType();

	EBattleMenu GetBattleMenuType();

	BattleDefineData GetBattleDefine();

	BattleFacade.BattleConditionBase CreateBattleCondition();

	bool IsTowerBattle();

	int GetTowerFloor();

	void OnUpdate();

	List<CachedHeroInfo> GetPreloadBattleLogs();

	string GetPreloadFieldName();

	bool GetPrealodCreateMonster();

	GuildFlagData GetGuildFlagData();
}
