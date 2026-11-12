using System;

namespace NGame2.NFunctionality;

public class FunctionalityAttribute : Attribute
{
	public virtual Func<IFunctionalityValue> ValueCreator { get; protected set; }

	public virtual string DefaultValue { get; protected set; }
}
