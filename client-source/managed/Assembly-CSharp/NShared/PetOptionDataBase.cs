namespace NShared;

[JsonSerializable(Javascript = true)]
public class PetOptionDataBase
{
	public int Index { get; set; }

	public bool CheckActiveCondition(CampaignDungeonData dungeonData)
	{
		if (dungeonData == null)
		{
			return false;
		}
		PetOptionData data = GameRoot.Instance.PetOptionDataContainer.GetData(Index);
		if (data == null)
		{
			return false;
		}
		bool result = false;
		switch (data.ConditionType)
		{
		case PetOptionConditionType.None:
			result = true;
			break;
		case PetOptionConditionType.BattleType:
			result = IsActiveBattleType(data.ConditionValue, dungeonData.BattleType);
			break;
		case PetOptionConditionType.Campaign:
			result = IsActiveCampaign(data.ConditionValue, dungeonData.ChapterIndex, dungeonData.DungeonIndex);
			break;
		}
		return result;
	}

	private bool IsActiveBattleType(string[] conditionValue, BattleType currentBattleType)
	{
		if (conditionValue == null || conditionValue.Length == 0)
		{
			return false;
		}
		bool result = false;
		string text = currentBattleType.ToString();
		foreach (string text2 in conditionValue)
		{
			if (text2 != null && text2 == text)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	private bool IsActiveCampaign(string[] conditionValue, int currentChapterIndex, int currentDungeonIndex)
	{
		if (conditionValue == null || conditionValue.Length == 0)
		{
			return false;
		}
		bool result = false;
		for (int i = 0; i < conditionValue.Length; i += 2)
		{
			int valueInt = ArgParser.GetValueInt(conditionValue, i);
			int valueInt2 = ArgParser.GetValueInt(conditionValue, i + 1);
			if (valueInt != 0 && valueInt == currentChapterIndex && (valueInt2 == 0 || (valueInt2 > 0 && valueInt2 == currentDungeonIndex)))
			{
				result = true;
				break;
			}
		}
		return result;
	}
}
