using System;

namespace NGame2.NAccount;

[Serializable]
public class GetRefreshData
{
	public string access_token;

	public string refresh_token;

	public string expires_in;
}
