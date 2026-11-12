namespace NShared;

[JsonSerializable(Javascript = true)]
public class TutorialInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => TutorialIndex;

	public int TutorialIndex { get; set; }

	public string CompletedTime { get; set; }
}
