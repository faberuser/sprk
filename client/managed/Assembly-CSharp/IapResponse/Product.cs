using System;
using System.Text;

namespace IapResponse;

[Serializable]
public class Product
{
	public string appid;

	public string id;

	public string name;

	public string type;

	public string kind;

	public int validity;

	public double price;

	public string startDate;

	public string endDate;

	public bool purchasability;

	public Status status;

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder("[Product]\n");
		stringBuilder.Append("appid: " + appid + "\n");
		stringBuilder.Append("id: " + id + "\n");
		stringBuilder.Append("name: " + name + "\n");
		stringBuilder.Append("type: " + type + "\n");
		stringBuilder.Append("kind: " + kind + "\n");
		stringBuilder.Append("validity: " + validity + "\n");
		stringBuilder.Append("price: " + price + "\n");
		stringBuilder.Append("startDate: " + startDate + "\n");
		stringBuilder.Append("endDate: " + endDate + "\n");
		stringBuilder.Append("purchasability: " + purchasability + "\n");
		if (status != null)
		{
			stringBuilder.Append(status.ToString());
		}
		return stringBuilder.ToString();
	}
}
