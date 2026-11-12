using System;
using System.Collections.Generic;

namespace NShared.NEvent;

public class EventFactory
{
	public delegate object EventCreator(Type runtimeType, string jsonString);

	public delegate string EventSerializer(object o);

	public class EventTemplate
	{
		public IEvent Event;

		public Func<object, string> Serializer;
	}

	private readonly Dictionary<string, EventTemplate> _templateDictionary;

	private EventCreator _creator;

	private EventSerializer _serializer;

	public EventFactory()
	{
		_templateDictionary = new Dictionary<string, EventTemplate>();
		RegisterEventTemplate();
		UnitTest();
	}

	private void RegisterEventTemplate()
	{
		RegisterEventTemplate<BaseGlobalTime>();
		RegisterEventTemplate<BattleBegin>();
		RegisterEventTemplate<BattleEnd>();
		RegisterEventTemplate<BattleSpeedChange>();
		RegisterEventTemplate<BossHpPercentage>();
		RegisterEventTemplate<ChapterMove>();
		RegisterEventTemplate<CompleteTutorial>();
		RegisterEventTemplate<EnterBattle>();
		RegisterEventTemplate<EnterBattleLoading>();
		RegisterEventTemplate<EnterEntryMenu>();
		RegisterEventTemplate<EnterLobby>();
		RegisterEventTemplate<EnterMenu>();
		RegisterEventTemplate<EnterHeroTranscendSkillListPage>();
		RegisterEventTemplate<EnterHeroStatPage>();
		RegisterEventTemplate<EnterHeroSkillListPage>();
		RegisterEventTemplate<EnterHeroEquipListPage>();
		RegisterEventTemplate<EnterEventDungeonSelect>();
		RegisterEventTemplate<GlobalTimePause>();
		RegisterEventTemplate<GlobalTimeResume>();
		RegisterEventTemplate<LeaveMenu>();
		RegisterEventTemplate<Login>();
		RegisterEventTemplate<NodeFunctionMenu>();
		RegisterEventTemplate<OnClickMoveChapter>();
		RegisterEventTemplate<OnClickNextBattle>();
		RegisterEventTemplate<OnClickRetry>();
		RegisterEventTemplate<OnClickReturnToMap>();
		RegisterEventTemplate<OnClickStartBattle>();
		RegisterEventTemplate<OpenSubPartySetting>();
		RegisterEventTemplate<OpenWorldBossRanking>();
		RegisterEventTemplate<PlayerMoveEnd>();
		RegisterEventTemplate<PlayerVisitNode>();
		RegisterEventTemplate<UsePortal>();
		RegisterEventTemplate<WaveStart>();
		RegisterEventTemplate<WaveEnd>();
		RegisterEventTemplate<WaveReady>();
		RegisterEventTemplate<BattleInitDeck>();
		RegisterEventTemplate<BattleInsertDeck>();
		RegisterEventTemplate<BattleRemoveDeck>();
		RegisterEventTemplate<BattleEndRes>();
		RegisterEventTemplate<LeaveEntryMenu>();
		RegisterEventTemplate<OpenRaidSelect>();
		RegisterEventTemplate<OpenTowerOfOrdeal>();
		RegisterEventTemplate<OnSkillFire>();
		RegisterEventTemplate<OpenMaze>();
		RegisterEventTemplate<OpenGuildConstruction>();
		RegisterEventTemplate<OpenGuildTrainingCenter>();
		RegisterEventTemplate<EnterChapter>();
		RegisterEventTemplate<OpenRenewEquip>();
		RegisterEventTemplate<OpenHeroList>();
		RegisterEventTemplate<EntryMenuLoadCompleted>();
		RegisterEventTemplate<OpenTechnoEnchantRaid>();
	}

	private void RegisterEventTemplate<T>() where T : IEvent, new()
	{
		string name = typeof(T).Name;
		_templateDictionary.Remove(name);
		EventTemplate value = new EventTemplate
		{
			Event = new T(),
			Serializer = (object o) => _serializer((T)o)
		};
		_templateDictionary.Add(name, value);
	}

	public void SetDeserializer(EventCreator creator)
	{
		_creator = creator;
	}

	public void SetSerializer(EventSerializer serializer)
	{
		_serializer = serializer;
	}

	public string Serializer(IEvent iEvent)
	{
		if (iEvent == null)
		{
			return null;
		}
		EventTemplate value = null;
		_templateDictionary.TryGetValue(iEvent.GetType().Name, out value);
		return value?.Serializer(iEvent);
	}

	private IEvent GetTemplate(string name)
	{
		EventTemplate value = null;
		_templateDictionary.TryGetValue(name, out value);
		return value?.Event;
	}

	public IEvent Create(string name, string data)
	{
		if (_creator == null)
		{
			return null;
		}
		IEvent template = GetTemplate(name);
		if (template == null)
		{
			return null;
		}
		return _creator(template.GetType(), data) as IEvent;
	}

	private void UnitTest()
	{
	}
}
