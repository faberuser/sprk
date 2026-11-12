using System;
using System.Collections.Generic;
using System.Text;

namespace IapVerifyReceipt;

[Serializable]
public class VerifyReceipt
{
	public int status;

	public string detail;

	public string message;

	public int count;

	public List<Product> product;

	public new string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder("[VerifyReceipt]\n");
		stringBuilder.Append("status: " + status + "\n");
		stringBuilder.Append("detail: " + detail + "\n");
		stringBuilder.Append("message: " + message + "\n");
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
