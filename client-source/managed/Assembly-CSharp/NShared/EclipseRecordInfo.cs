namespace NShared;

[JsonSerializable(Javascript = true)]
public class EclipseRecordInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => LastWaveIndex;

	public int LastWaveIndex { get; set; }

	public string UpdatedTime { get; set; }
}
