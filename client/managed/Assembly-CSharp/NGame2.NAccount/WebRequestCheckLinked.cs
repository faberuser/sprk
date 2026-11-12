using System;

namespace NGame2.NAccount;

[Serializable]
public class WebRequestCheckLinked
{
	public string status;

	public string code;

	public string message;

	public LinkedData[] data;
}
