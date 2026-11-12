using System;

namespace NShared;

[JsonSerializable(Javascript = true)]
public class CurrencyResultInfo3
{
	public CurrencyType CurrencyType { get; set; }

	public long AddValue { get; set; }

	public long NewValue { get; set; }

	public string Field1 { get; set; }

	public long Value1 { get; set; }

	public string Field2 { get; set; }

	public long Value2 { get; set; }

	public string Field3 { get; set; }

	public long Value3 { get; set; }

	public string Field4 { get; set; }

	public long Value4 { get; set; }

	public long GetFieldValue(string fieldName)
	{
		if (string.IsNullOrEmpty(fieldName))
		{
			return 0L;
		}
		if (string.IsNullOrEmpty(Field1))
		{
			return 0L;
		}
		if (string.Equals(Field1, fieldName, StringComparison.InvariantCultureIgnoreCase))
		{
			return Value1;
		}
		if (string.IsNullOrEmpty(Field2))
		{
			return 0L;
		}
		if (string.Equals(Field2, fieldName, StringComparison.InvariantCultureIgnoreCase))
		{
			return Value2;
		}
		if (string.IsNullOrEmpty(Field3))
		{
			return 0L;
		}
		if (string.Equals(Field3, fieldName, StringComparison.InvariantCultureIgnoreCase))
		{
			return Value3;
		}
		if (string.IsNullOrEmpty(Field4))
		{
			return 0L;
		}
		if (string.Equals(Field4, fieldName, StringComparison.InvariantCultureIgnoreCase))
		{
			return Value4;
		}
		return 0L;
	}
}
