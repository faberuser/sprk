namespace NShared;

[JsonSerializable(Javascript = true)]
public class GuildArenaDeckInfo
{
	public long AccountId { get; set; }

	public int DeckIndex { get; set; }

	public CachedHeroInfo[] CachedHeroInfos { get; set; }

	public int[] HeroIndices { get; set; }

	public int[] SkillSlotIndices { get; set; }

	public int IsBattle { get; set; }

	public int IsLose { get; set; }

	public int IsShow { get; set; }

	public int IsBlind { get; set; }

	public int IsNPC { get; set; }

	public int IsForceAttack { get; set; }
}
