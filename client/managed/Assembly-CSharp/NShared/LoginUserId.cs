namespace NShared;

public struct LoginUserId
{
	public string id { get; private set; }

	public string legacyId { get; private set; }

	public long accountId { get; private set; }

	public LoginUserId(string id, string legacyId, long accountId)
	{
		this.id = id;
		this.legacyId = legacyId;
		this.accountId = accountId;
	}
}
