using System.Collections.Generic;
using NShared.NUtil;
using NVespa;

namespace NShared;

public class KarmaManager
{
	public class GroupData
	{
		private List<CardData> _cardList = new List<CardData>();

		public int GroupIndex { get; private set; }

		public int GroupChance { get; private set; }

		public int TotalChance_IncludeCard { get; private set; }

		public List<CardData> CardList => _cardList;

		public GroupData(int groupIndex)
		{
			GroupIndex = groupIndex;
		}

		public void UpdateChance()
		{
			GroupChance = 0;
			TotalChance_IncludeCard = 0;
			foreach (CardData card in _cardList)
			{
				if (card != null)
				{
					GroupChance += card.GroupChance;
					TotalChance_IncludeCard += card.Chance;
				}
			}
		}
	}

	public class CardData
	{
		public int GroupIndex => Data.GroupIndex;

		public int Index => Data.Index;

		public int Chance => Data.UniqueChance;

		public int GroupChance => Data.GroupChance;

		public int Stack { get; private set; }

		public bool IsMaxStack
		{
			get
			{
				if (Data.MaxStack <= 0)
				{
					return false;
				}
				return Stack >= Data.MaxStack;
			}
		}

		public CreatureTagType[] CreatureTagTypes => Data.ClassTypes;

		public KarmaCardData Data { get; private set; }

		public CardData(KarmaCardData data)
		{
			Stack = 0;
			Data = data;
		}

		public void Selected()
		{
			Stack++;
		}
	}

	private readonly BattleInstance _battleInstance;

	private RandomTable randomTableHolder;

	private KarmaDungeonData karmaData;

	private Dictionary<int, GroupData> selectableCardGroupDic = new Dictionary<int, GroupData>();

	private HashSet<int> selectedGroupList = new HashSet<int>();

	private List<CardData> selectedCardList = new List<CardData>();

	private HashSet<CreatureTagType> selectableCreatureTagSet = new HashSet<CreatureTagType>();

	private int totalGroupChance;

	private bool selectableState;

	private CardData selectedCard;

	private int selectSlot;

	private int randomSeed;

	private int waveInterval;

	private CardData defaultCard;

	public bool CanUseCardSkill { get; private set; }

	public bool Enabled { get; private set; }

	public KarmaManager(BattleInstance battleInstance)
	{
		_battleInstance = battleInstance;
	}

	public void Init()
	{
		int chapterIndex = _battleInstance.ChapterIndex;
		int dungeonIndex = _battleInstance.DungeonIndex;
		karmaData = GameRoot.Instance.KarmaDungeonDataContainer.GetDataByChapterDungeon(chapterIndex, dungeonIndex);
		if (karmaData != null)
		{
			InitInterval();
			Enabled = true;
		}
	}

	private void InitInterval()
	{
		int punishmentGroupIndex = karmaData.PunishmentGroupIndex;
		PunishmentGroupData data = GameRoot.Instance.PunishmentGroupDataContainer.GetData(punishmentGroupIndex, EPunishmentDungeonType.Karma);
		if (data == null)
		{
			Enabled = false;
		}
		else
		{
			waveInterval = data.KarmaCardPickInterval;
		}
	}

	public void InitRandomTable(int randomSeed)
	{
		this.randomSeed = randomSeed;
		RandomTable randomTable = new RandomTable();
		randomTable.Init(randomSeed);
		randomTableHolder = randomTable;
	}

	public void InitSelectableCardList()
	{
		if (selectableCreatureTagSet == null || selectableCreatureTagSet.Count <= 0)
		{
			Enabled = false;
			return;
		}
		IList<KarmaCardData> allData = GameRoot.Instance.KarmaCardDataContainer.GetAllData();
		if (allData == null || allData.Count < 1)
		{
			Enabled = false;
			return;
		}
		foreach (KarmaCardData item2 in allData)
		{
			CreatureTagType[] classTypes = item2.ClassTypes;
			if (CheckMatchTagType(classTypes))
			{
				CardData item = new CardData(item2);
				if (!selectableCardGroupDic.ContainsKey(item2.GroupIndex))
				{
					GroupData value = new GroupData(item2.GroupIndex);
					selectableCardGroupDic.Add(item2.GroupIndex, value);
				}
				selectableCardGroupDic[item2.GroupIndex].CardList.Add(item);
			}
		}
		foreach (KeyValuePair<int, GroupData> item3 in selectableCardGroupDic)
		{
			item3.Value.UpdateChance();
		}
		UpdateTotalGroupChance();
		defaultCard = new CardData(allData[0]);
	}

	private bool CheckMatchTagType(CreatureTagType[] tagTypes)
	{
		foreach (CreatureTagType item in tagTypes)
		{
			if (selectableCreatureTagSet.Contains(item))
			{
				return true;
			}
		}
		return false;
	}

	public void InitFilterClassTag()
	{
		BattleCreatureList allCreaturesOfTeam = _battleInstance.CreatureContainer.GetAllCreaturesOfTeam(0);
		if (allCreaturesOfTeam == null || allCreaturesOfTeam.Count <= 0)
		{
			Enabled = false;
			return;
		}
		allCreaturesOfTeam.ForEach(delegate(BattleCreature creatureInfo)
		{
			if (creatureInfo != null && creatureInfo.CreatureData != null)
			{
				CreatureTagType tagType = creatureInfo.CreatureData.TagType;
				selectableCreatureTagSet.Add(tagType);
			}
		});
	}

	public void WaveEndEvent()
	{
		if (Enabled && (_battleInstance.Snapshot.MaxWaveIndex + 1) % waveInterval == 0)
		{
			PickRandomCard3();
		}
	}

	private void PickRandomCard3()
	{
		if (!Enabled)
		{
			return;
		}
		ClearSelectedCardList();
		int num = 3;
		int num2 = 30;
		for (int i = 0; i < num2; i++)
		{
			CardData cardData = SelectRandomCard();
			if (cardData != null && cardData.Data != null && selectedGroupList.Add(cardData.GroupIndex))
			{
				selectedCardList.Add(cardData);
				num--;
				if (num <= 0)
				{
					break;
				}
			}
		}
		if (num > 0)
		{
			VespaDebug.LogError($"[card] need more pick card");
			for (int num3 = num; num3 > 0; num3--)
			{
				selectedCardList.Add(defaultCard);
			}
		}
		_battleInstance.RaiseBattleEvent(new KarmaCardSuffle
		{
			SuffledCardIndexes = SuffledCardIndexes()
		});
		selectableState = true;
	}

	private int[] SuffledCardIndexes()
	{
		int[] array = new int[selectedCardList.Count];
		for (int i = 0; i < selectedCardList.Count; i++)
		{
			array[i] = selectedCardList[i].Index;
		}
		return array;
	}

	private CardData SelectRandomCard()
	{
		if (selectableCardGroupDic.Count <= 0)
		{
			VespaDebug.LogError($"[card] Selectable deck is empty.");
			return null;
		}
		GroupData groupData = null;
		int num = NextRandom(totalGroupChance);
		foreach (KeyValuePair<int, GroupData> item in selectableCardGroupDic)
		{
			groupData = item.Value;
			num -= item.Value.GroupChance;
			if (num <= 0)
			{
				groupData = item.Value;
				break;
			}
		}
		if (groupData == null || groupData.CardList.Count <= 0)
		{
			VespaDebug.LogError($"[card] Card list is empty. groupIndex = {groupData.GroupIndex}");
			return null;
		}
		CardData cardData = null;
		num = NextRandom(groupData.TotalChance_IncludeCard);
		foreach (CardData card in groupData.CardList)
		{
			cardData = card;
			num -= card.Chance;
			if (num <= 0)
			{
				cardData = card;
				break;
			}
		}
		if (cardData == null || cardData.Data == null)
		{
			VespaDebug.LogError($"[card] selectedCard.data is null. groupIndex = {groupData.GroupIndex}");
			return null;
		}
		return cardData;
	}

	private int NextRandom(int max, string desc = null)
	{
		return randomTableHolder.NextRandom(max, desc);
	}

	public void AdjustSelectCard()
	{
		if (!Enabled || !selectableState || selectedCardList.Count < selectSlot)
		{
			return;
		}
		selectedCard = selectedCardList[selectSlot];
		if (selectedCard == null || selectedCard.IsMaxStack)
		{
			VespaDebug.LogError($"[card] No selected Card)");
			return;
		}
		_ = selectedCard.GroupIndex;
		_ = selectedCard.Index;
		if (!UseCardSkill())
		{
			VespaDebug.LogError($"[Card] Fail to use card skill. cardIndex = {selectedCard.Data.Index}");
		}
		selectedCard.Selected();
		if (!IsSelectable(selectedCard))
		{
			RemoveCard(selectedCard);
		}
		ClearSelectedCardList();
	}

	private bool UseCardSkill()
	{
		KarmaCardData data = selectedCard.Data;
		int[] skillOptions = data.SkillOptions;
		if (skillOptions == null || skillOptions.Length < 1)
		{
			VespaDebug.LogError($"[Karma] No skillData. cardIndex={data.Index}");
			return false;
		}
		CreatureTagType[] classTypes = data.ClassTypes;
		BattleCreatureList creatures = _battleInstance.CreatureContainer.GetCreatures();
		for (int i = 0; i < creatures.Count; i++)
		{
			BattleCreature battleCreature = creatures[i];
			if (battleCreature == null || battleCreature.TeamController.TeamType == TeamType.Right)
			{
				continue;
			}
			CreatureTagType tagType = battleCreature.CreatureData.TagType;
			bool flag = false;
			CreatureTagType[] array = classTypes;
			foreach (CreatureTagType creatureTagType in array)
			{
				if (tagType == creatureTagType)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				continue;
			}
			int[] array2 = skillOptions;
			foreach (int key in array2)
			{
				SkillData data2 = GameRoot.Instance.SkillDataContainer.GetData(key);
				if (data2 != null)
				{
					SkillUseResult skillUseResult = battleCreature.SkillController.UseSkill(data2, null, null);
					if (skillUseResult != SkillUseResult.Success)
					{
						VespaDebug.LogError($"[card] useSkill fail. skillIndex = {data2.Index}, result = {skillUseResult}");
						return false;
					}
				}
			}
		}
		return true;
	}

	private void ClearSelectedCardList()
	{
		selectableState = false;
		selectedCardList.Clear();
		selectedGroupList.Clear();
	}

	private bool IsSelectable(CardData data)
	{
		if (data == null)
		{
			return false;
		}
		if (data.IsMaxStack)
		{
			return false;
		}
		return true;
	}

	private void RemoveCard(CardData cardData)
	{
		if (!selectableCardGroupDic.ContainsKey(cardData.GroupIndex))
		{
			return;
		}
		GroupData groupData = selectableCardGroupDic[cardData.GroupIndex];
		if (groupData != null)
		{
			groupData.CardList.Remove(cardData);
			groupData.UpdateChance();
			if (groupData.CardList.Count <= 0)
			{
				selectableCardGroupDic.Remove(groupData.GroupIndex);
				UpdateTotalGroupChance();
			}
		}
	}

	private void UpdateTotalGroupChance()
	{
		totalGroupChance = 0;
		foreach (KeyValuePair<int, GroupData> item in selectableCardGroupDic)
		{
			totalGroupChance += item.Value.GroupChance;
		}
	}

	public void SelectSlot(int slot)
	{
		selectSlot = slot;
	}
}
