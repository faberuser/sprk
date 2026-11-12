using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CampaignDungeonSkillData : IDataKeySelector<int>
{
	[Key(0)]
	public int BaseSkillIndex { get; set; }

	[Key(1)]
	public int EnhancedSkillIndex { get; set; }

	[Key(2)]
	public int MaxUseCount { get; set; }

	[Key(3)]
	public TeamType TeamType { get; set; }

	[Key(4)]
	public string BaseCheckState { get; set; }

	[Key(5)]
	public string EnhancedCheckState { get; set; }

	int IDataKeySelector<int>.Key => BaseSkillIndex;
}
