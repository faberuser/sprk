using System;
using System.Collections.Generic;
using System.Text;

namespace IapResponse;

[Serializable]
public class Result
{
	public string message;

	public string code;

	public string txid;

	public string receipt;

	public int count;

	public List<Product> product;

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder("[Result]\n");
		stringBuilder.Append("message: " + message + "\n");
		stringBuilder.Append("code: " + code + "\n");
		stringBuilder.Append("txid: " + txid + "\n");
		stringBuilder.Append("receipt: " + receipt + "\n");
		stringBuilder.Append("count: " + count + "\n");
		if (product != null)
		{
			foreach (Product item in product)
			{
				stringBuilder.Append(item.ToString());
			}
		}
		return stringBuilder.ToString();
	}
}
