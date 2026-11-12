using System;
using System.Text;

namespace IapVerifyReceipt;

[Serializable]
public class Product
{
	public string log_time;

	public string appid;

	public string product_id;

	public double charge_amount;

	public string tid;

	public string detail_pname;

	public string bp_info;

	public string tcash_flag;

	public new string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder("[Product]\n");
		stringBuilder.Append("log_time: " + log_time + "\n");
		stringBuilder.Append("appid: " + appid + "\n");
		stringBuilder.Append("product_id: " + product_id + "\n");
		stringBuilder.Append("charge_amount: " + charge_amount + "\n");
		stringBuilder.Append("tid: " + tid + "\n");
		stringBuilder.Append("detail_pname: " + detail_pname + "\n");
		stringBuilder.Append("bp_info: " + bp_info + "\n");
		stringBuilder.Append("tcash_flag: " + tcash_flag + "\n");
		return stringBuilder.ToString();
	}
}
