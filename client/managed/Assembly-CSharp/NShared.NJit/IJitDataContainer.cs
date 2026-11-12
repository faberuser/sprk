namespace NShared.NJit;

public interface IJitDataContainer
{
	bool IsLoaded { get; }

	void Load();
}
