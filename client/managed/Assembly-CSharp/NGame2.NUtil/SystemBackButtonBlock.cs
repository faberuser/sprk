using System;

namespace NGame2.NUtil;

public class SystemBackButtonBlock : IDisposable
{
	public static bool IsRealBackButton;

	public SystemBackButtonBlock()
	{
		IsRealBackButton = true;
	}

	public void Dispose()
	{
		IsRealBackButton = false;
	}
}
