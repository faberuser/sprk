namespace NGameAsset;

public struct BattleFieldMapStock
{
	private string _code;

	private string _prefix;

	public int Layer;

	public string Code => _code;

	public string Prefix => _prefix;

	public BattleFieldMapStock(string code, string prefix)
	{
		_code = code;
		_prefix = prefix;
		Layer = -1;
	}

	public float CalcBackgroundScale(float depth)
	{
		float num = 25f;
		return (num + depth) / num;
	}
}
