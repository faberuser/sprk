namespace NShared;

[JsonSerializable(Javascript = true)]
public class EclipseDeckRecordInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => DeckIndex;

	public int DeckIndex { get; set; }

	public int MaxWaveIndex { get; set; }

	public string UpdatedTime { get; set; }
}
