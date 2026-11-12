namespace NShared.NUtil;

public class OperatorInfo : TokenInfo
{
	public readonly int Precedence;

	public OperatorInfo(string token, int precedence)
		: base(token)
	{
		Precedence = precedence;
	}
}
