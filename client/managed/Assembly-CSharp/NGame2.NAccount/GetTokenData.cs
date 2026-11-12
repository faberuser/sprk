using System;

namespace NGame2.NAccount;

[Serializable]
public class GetTokenData
{
	public string id;

	public string access_token;

	public string expires_in;

	public string refresh_token;
}
