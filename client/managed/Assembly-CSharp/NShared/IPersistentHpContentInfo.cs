namespace NShared;

public interface IPersistentHpContentInfo
{
	int GetChapterIndex();

	int GetDungeonIndex();

	int GetDifficulty();

	long GetSavedMonsterHp(int waveSlotIndex);

	long GetMaxHp(int waveSlotIndex);

	int GetMaxMp(int waveSlotIndex);
}
