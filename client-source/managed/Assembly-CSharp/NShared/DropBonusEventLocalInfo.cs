namespace NShared;

[JsonSerializable(Javascript = true)]
public class DropBonusEventLocalInfo
{
	public int Id { get; set; }

	public string Language { get; set; }

	public string Title { get; set; }

	public string Message { get; set; }
}
