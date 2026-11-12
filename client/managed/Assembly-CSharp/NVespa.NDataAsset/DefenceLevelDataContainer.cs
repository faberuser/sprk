namespace NVespa.NDataAsset;

public class DefenceLevelDataContainer : IndexableDataAsset<int, DefenceLevelData>
{
	protected override int GetKey(DefenceLevelData d)
	{
		return d.Level;
	}
}
