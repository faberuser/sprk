using System;
using System.Text;

namespace IapResponse;

[Serializable]
public class Response
{
	public string api_version;

	public string identifier;

	public string method;

	public Result result;

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder("[Response]\n");
		stringBuilder.Append("api_version: " + api_version + "\n");
		stringBuilder.Append("identifier: " + identifier + "\n");
		stringBuilder.Append("method: " + method + "\n");
		if (result != null)
		{
			stringBuilder.Append("\n" + result.ToString());
		}
		return stringBuilder.ToString();
	}
}
