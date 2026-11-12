using System;
using System.Collections.Generic;
using NGame2.NScenarioQuest.NRuntime;
using NShared;

namespace NGame2.NScenarioQuest;

public class ScenarioQuestFactory
{
	private readonly Dictionary<QuestActionType, Func<IScenarioQuestRuntime>> _runtimeDictionary = new Dictionary<QuestActionType, Func<IScenarioQuestRuntime>>();

	public ScenarioQuestFactory()
	{
		RegisterRuntime<UsePortal>(QuestActionType.UsePortal);
		RegisterRuntime<CompleteTutorial>(QuestActionType.CompleteTutorial);
		RegisterRuntime<PlayerVisitNode>(QuestActionType.PlayerVisitNode);
		RegisterRuntime<EnterBattle>(QuestActionType.EnterBattle);
		RegisterRuntime<BattleEnd>(QuestActionType.BattleEnd);
	}

	private void RegisterRuntime<T>(QuestActionType type) where T : IScenarioQuestRuntime, new()
	{
		_runtimeDictionary.Remove(type);
		_runtimeDictionary.Add(type, () => new T());
	}

	public IScenarioQuestRuntime Create(QuestActionType type)
	{
		Func<IScenarioQuestRuntime> value = null;
		_runtimeDictionary.TryGetValue(type, out value);
		return value?.Invoke();
	}
}
