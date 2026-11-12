using NShared;

namespace NGame2.NBattleView;

public static class Extension_Method_Utils
{
	public static string EncodeJson<T>(this T This) where T : BaseBattleCallback
	{
		if (This == null)
		{
			return "(null)";
		}
		return "[" + This.GetType().Name + "] : " + JsonMarshaler.EncodeJsonString(This);
	}
}
