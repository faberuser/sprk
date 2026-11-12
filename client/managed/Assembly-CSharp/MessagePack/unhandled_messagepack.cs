using System;
using NVespa;

namespace MessagePack;

public static class unhandled_messagepack
{
	public static void Dummy()
	{
		NotifyError();
		throw new Exception("DISABLE_MESSAGEPACK is Defined");
	}

	public static void NotifyError()
	{
		VespaDebug.LogError("##########################################################");
		VespaDebug.LogError("#");
		VespaDebug.LogError("# 메세지팩을 사용하지 않도록 세팅되어 있습니다");
		VespaDebug.LogError("# not define Using_MessagePack");
		VespaDebug.LogError("#");
		VespaDebug.LogError("##########################################################");
	}

	public static bool Is_DISABLE_MESSAGEPACK()
	{
		return false;
	}
}
