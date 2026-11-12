namespace NVespa.NUtil;

public class UniClipboard
{
	private static NVespa.NUtil.IBoard _board;

	private static NVespa.NUtil.IBoard board
	{
		get
		{
			_ = _board;
			return _board;
		}
	}

	public static void SetText(string str)
	{
		board.SetText(str);
	}

	public static string GetText()
	{
		return board.GetText();
	}
}
