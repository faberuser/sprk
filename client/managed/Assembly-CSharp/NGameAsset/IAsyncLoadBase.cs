namespace NGameAsset;

public interface IAsyncLoadBase
{
	string KeyDesc { get; }

	bool IsDone { get; }

	bool IsValid { get; }

	string LoadMessage { get; }
}
