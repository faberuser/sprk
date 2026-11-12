using System;
using System.Text;

namespace IapResponse;

[Serializable]
public class Status
{
	public string code;

	public string message;

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder("[Status]\n");
		stringBuilder.Append("code: " + code + "\n");
		stringBuilder.Append("message: " + message + "\n");
		return stringBuilder.ToString();
	}
}
