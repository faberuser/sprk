using System;

namespace NGame2.NFunctionality;

public class BooleanValueAttribute : FunctionalityAttribute
{
	private static Func<IFunctionalityValue> _creator = () => new GenericValue<bool>(bool.Parse, EqualsAttributeValue);

	public override Func<IFunctionalityValue> ValueCreator => _creator;

	public override string DefaultValue => Default.ToString();

	public bool Default { get; set; }

	public static bool EqualsAttributeValue(bool lhs, bool rhs)
	{
		return lhs == rhs;
	}
}
