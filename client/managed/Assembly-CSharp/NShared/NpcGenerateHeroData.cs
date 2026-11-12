namespace NShared;

[JsonSerializable]
public class NpcGenerateHeroData : IDataKeySelector<int>
{
	int IDataKeySelector<int>.Key => Index;

	public int Index { get; set; }

	public int Level { get; set; }

	public int MinStar { get; set; }

	public int MaxStar { get; set; }

	public int EquipTier { get; set; }

	public int MinEquipGrade { get; set; }

	public int MaxEquipGrade { get; set; }

	public int MinEquipLevel { get; set; }

	public int MaxEquipLevel { get; set; }

	public int MinEquipStar { get; set; }

	public int MaxEquipStar { get; set; }

	public int TierIndex { get; set; }

	public int MinMatchScore { get; set; }

	public int MaxMatchScore { get; set; }
}
