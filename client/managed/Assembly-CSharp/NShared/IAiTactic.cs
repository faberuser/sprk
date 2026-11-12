namespace NShared;

public interface IAiTactic : ISnapshotObject
{
	void Update(AiContext context);
}
