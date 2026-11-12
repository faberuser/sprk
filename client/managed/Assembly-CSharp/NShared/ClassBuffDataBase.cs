using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class ClassBuffDataBase : IExtraSkillData
{
	int IExtraSkillData.SkillIndex => SkillIndex;

	int IExtraSkillData.SkillLevel => 1;

	[Key(0)]
	public int CreatureType { get; set; }

	[Key(1)]
	public int ClassBuffIndex { get; set; }

	[Key(2)]
	public int SkillIndex { get; set; }

	bool IExtraSkillData.CheckActiveCondition(CampaignDungeonData dungeonData)
	{
		if (dungeonData == null || dungeonData.BattleType == BattleType.None)
		{
			return false;
		}
		string[] valueArray = GameRoot.Instance.ConstantDataContainer.GetValueArray<string>("ClassBuffDungeonType");
		if (valueArray == null || valueArray.Length == 0)
		{
			return false;
		}
		string text = dungeonData.BattleType.ToString();
		for (int i = 0; i < valueArray.Length; i++)
		{
			if (valueArray[i] == text)
			{
				return true;
			}
		}
		return false;
	}
}
