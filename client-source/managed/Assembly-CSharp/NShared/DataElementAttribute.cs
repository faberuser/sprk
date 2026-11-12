using System;

namespace NShared;

public class DataElementAttribute : Attribute
{
	public Type Type { get; set; }

	public int Count { get; set; }
}
