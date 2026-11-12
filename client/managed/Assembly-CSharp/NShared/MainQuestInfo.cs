namespace NShared;

[JsonSerializable(Javascript = true)]
public class MainQuestInfo
{
	public int Step { get; set; }

	public int Progress { get; set; }
}
