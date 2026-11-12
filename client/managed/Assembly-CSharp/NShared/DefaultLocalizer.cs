using System;
using System.Text;

namespace NShared;

public class DefaultLocalizer : ILocalizer
{
	string ILocalizer.GetString(Enum type, params object[] values)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(type);
		if (values != null)
		{
			for (int i = 0; i < values.Length; i++)
			{
				stringBuilder.Append(",");
				stringBuilder.Append(values[i]);
			}
		}
		return stringBuilder.ToString();
	}
}
