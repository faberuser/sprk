using Extension_Method_Helper;

public static class __Extension_Method_CSharp_Class_Common__
{
	public static DowncastHelper<T> Downcasting<T>(this T This) where T : class
	{
		return new DowncastHelper<T>(This);
	}

	public static TParent Upcasting<TParent>(this TParent This)
	{
		return This;
	}
}
