namespace NGameAsset.NEffect;

public interface ITrailData
{
	string GetName();

	CommonData GetCommonData();

	int GetSequenceCount();

	TrailSequenceData GetSequenceData(int index);
}
