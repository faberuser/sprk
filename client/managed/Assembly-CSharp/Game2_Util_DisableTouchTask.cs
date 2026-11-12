using NGame2.NUtil;
using NVespa.NTask;

public static class Game2_Util_DisableTouchTask
{
	public static TouchControlTask WrapperTouchControl(this ITask This)
	{
		return new TouchControlTask(This);
	}
}
