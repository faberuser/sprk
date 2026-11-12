namespace NGameAsset;

public interface ISound
{
	string Code { get; }

	bool ContainsLoop { get; }

	void Update();

	bool IsEnd();
}
