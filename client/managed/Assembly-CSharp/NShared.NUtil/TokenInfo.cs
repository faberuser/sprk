namespace NShared.NUtil;

public class TokenInfo
{
	public readonly string Token;

	public TokenInfo(string token)
	{
		Token = token;
	}

	public override string ToString()
	{
		return Token;
	}
}
