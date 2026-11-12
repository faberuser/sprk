namespace NVespa.NDataAsset;

public class BattleConstantDataContainer : IndexableDataAsset<string, BattleConstantData>
{
	protected override string GetKey(BattleConstantData d)
	{
		return d.Key;
	}
}
