namespace NShared;

[JsonSerializable(Javascript = true)]
public class StepUpEquipGachaInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => GachaIndex;

	public int GachaIndex { get; set; }

	public int CompletedStep { get; set; }

	public int CompletedCount { get; set; }

	public string ProgressTime { get; set; }
}
