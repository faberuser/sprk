using System;

namespace NVespa.NWeb;

[Flags]
public enum WebRequestFlag
{
	None = 0,
	ShowLoadingGauge = 1,
	DisableEncrypt = 2,
	DisableRetry = 4,
	Ping = 8,
	SilenceError = 0x10
}
