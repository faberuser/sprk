namespace NShared;

[JsonSerializable(Javascript = true)]
public class MailItemInfo
{
	public int ItemIndex { get; set; }

	public int ItemCount { get; set; }
}
