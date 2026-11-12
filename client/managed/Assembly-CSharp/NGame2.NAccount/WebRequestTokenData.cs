using System;

namespace NGame2.NAccount;

[Serializable]
public class WebRequestTokenData
{
	public string status;

	public string message;

	public GetTokenData data;
}
