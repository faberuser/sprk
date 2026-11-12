namespace NShared;

public interface IRaidData
{
	int ChapterIndex { get; }

	int DungeonIndex { get; }

	int Difficulty { get; }

	string PreviewBg { get; }

	string Tactic { get; }

	int[] RecommendedHeroIndex { get; }

	int[] ShowInfoSkill { get; }

	int RaidMultiInfoGroupIndex { get; }
}
