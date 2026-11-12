namespace NShared;

[JsonSerializable(Javascript = true)]
public class PtsUserInfo
{
	public string Region { get; set; }

	public bool IsEntry { get; set; }

	public int AccountId { get; set; }

	public string NickName { get; set; }

	public int TeamLevel { get; set; }
}
