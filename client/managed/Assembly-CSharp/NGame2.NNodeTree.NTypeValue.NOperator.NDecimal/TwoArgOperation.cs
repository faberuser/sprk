using System;

namespace NGame2.NNodeTree.NTypeValue.NOperator.NDecimal;

public static class TwoArgOperation
{
	public static decimal Add(decimal a, decimal b)
	{
		return a + b;
	}

	public static decimal Sub(decimal a, decimal b)
	{
		return a - b;
	}

	public static decimal Multiply(decimal a, decimal b)
	{
		return a * b;
	}

	public static decimal Divide(decimal a, decimal b)
	{
		try
		{
			return a / b;
		}
		catch (Exception)
		{
			return a;
		}
	}
}
