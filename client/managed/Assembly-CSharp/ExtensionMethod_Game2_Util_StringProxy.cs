using NGame2.NUtil;
using NGame2.NUtil.NStringProxy;

public static class ExtensionMethod_Game2_Util_StringProxy
{
	public static StringProxy APPEND(this StringProxy This, IBuilder stringBuilder)
	{
		This?.Append(stringBuilder);
		return This;
	}
}
