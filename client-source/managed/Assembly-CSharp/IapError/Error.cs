using System;
using System.Text;

namespace IapError;

[Serializable]
public class Error
{
	public string requestId;

	public string errorCode;

	public string errorMessage;

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder("[Error]\n");
		stringBuilder.Append("requestId: " + requestId + "\n");
		stringBuilder.Append("errorCode: " + errorCode + "\n");
		stringBuilder.Append("errorMessage: " + errorMessage + "\n");
		return stringBuilder.ToString();
	}
}
