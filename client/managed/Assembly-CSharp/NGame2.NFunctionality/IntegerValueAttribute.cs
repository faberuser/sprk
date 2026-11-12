using System;

namespace NGame2.NFunctionality;

public class IntegerValueAttribute : FunctionalityAttribute
{
	private static Func<IFunctionalityValue> _creator = () => new GenericValue<int>(int.Parse, EqualsAttributeValue);

	public override Func<IFunctionalityValue> ValueCreator => _creator;

	public override string DefaultValue => Default.ToString();

	public int Default { get; set; }

	public static bool EqualsAttributeValue(int lhs, int rhs)
	{
		return lhs == rhs;
	}
}
