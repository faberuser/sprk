namespace Extension_Method_Helper;

public struct DowncastHelper<TParent> where TParent : class
{
	private TParent _this;

	public DowncastHelper(TParent This)
	{
		_this = This;
	}

	public TChild To<TChild>() where TChild : class, TParent
	{
		return _this as TChild;
	}
}
