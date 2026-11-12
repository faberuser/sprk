using NShared;

public sealed class SprkAccessoryEntry
{
	public int Index;

	public AccessoryPartType Part;

	public PositionInfo Position;

	public SprkAccessoryEntry Clone()
	{
		return new SprkAccessoryEntry
		{
			Index = Index,
			Part = Part,
			Position = ((Position == null) ? PositionInfo.Create() : Position.Clone())
		};
	}
}
