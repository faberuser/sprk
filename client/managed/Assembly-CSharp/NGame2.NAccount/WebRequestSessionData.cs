using System;

namespace NGame2.NAccount;

[Serializable]
public class WebRequestSessionData
{
	public string status;

	public string message;

	public GetSessionData data;
}
