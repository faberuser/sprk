namespace NGameAsset.NCreature;

public interface IPivotData
{
	string Name { get; set; }

	string From { get; set; }

	string Path { get; set; }

	bool CanHide { get; set; }
}
