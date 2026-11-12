namespace NShared.NJit;

public abstract class JitDataContainerAbstract : IJitDataContainer
{
	public abstract bool IsLoaded { get; protected set; }

	public abstract TablePathDefine PathDefine { get; }

	public abstract void Load();
}
