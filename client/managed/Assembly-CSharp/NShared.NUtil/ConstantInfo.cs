namespace NShared.NUtil;

public class ConstantInfo : TokenInfo
{
	public long Value { get; private set; }

	public ConstantPool Pool { get; set; }

	public ConstantInfo(long value)
		: base(value.ToString())
	{
		Set(value);
	}

	public void Set(long value)
	{
		Value = value;
	}
}
