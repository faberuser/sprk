namespace NShared;

[JsonSerializable(Javascript = true)]
public class PlayRecordInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => RecordIndex;

	public int RecordIndex { get; set; }

	public string RecordValue { get; set; }

	public string UpdatedTime { get; set; }
}
