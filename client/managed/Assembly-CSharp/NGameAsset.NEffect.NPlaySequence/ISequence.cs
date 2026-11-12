namespace NGameAsset.NEffect.NPlaySequence;

public interface ISequence
{
	PlaySequenceData GetData();

	float Update(float elapsedTime);

	bool IsEnd();

	void ReleaseSelf();
}
