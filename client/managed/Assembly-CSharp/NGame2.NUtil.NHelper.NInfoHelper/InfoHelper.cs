using System.Collections.Generic;
using System.Text;

namespace NGame2.NUtil.NHelper.NInfoHelper;

public class InfoHelper
{
	public static List<int> ConvToIntListFromServerString(string intArray)
	{
		List<int> list = new List<int>();
		if (String_Extension_Method.IsNullOrEmpty(intArray))
		{
			return list;
		}
		string text = "";
		int num = 0;
		int i = 0;
		for (int length = intArray.Length; i < length; i++)
		{
			char c = intArray[i];
			switch (c)
			{
			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
				text += c;
				break;
			case ',':
				num = 0;
				if (int.TryParse(text, out num))
				{
					list.Add(num);
				}
				text = "";
				break;
			}
		}
		num = 0;
		if (int.TryParse(text, out num))
		{
			list.Add(num);
		}
		return list;
	}

	public static string ConvToStringFromIntList(int[] intList)
	{
		if (intList == null)
		{
			return "";
		}
		StringBuilder stringBuilder = new StringBuilder();
		int i = 0;
		for (int num = intList.Length; i < num; i++)
		{
			int num2 = intList[i];
			stringBuilder.Append(num2.ToString());
			if (i < num - 1)
			{
				stringBuilder.Append(',');
			}
		}
		return stringBuilder.ToString();
	}

	public static string ConvToStringFromIntList(List<int> intList)
	{
		if (intList == null)
		{
			return "";
		}
		return ConvToStringFromIntList(intList.ToArray());
	}
}
