using NShared.SaveReplay;

namespace NShared.B2M_SaveReplay;

[Request]
public class Request : NShared.SaveReplay.Request
{
	public override string ServiceName => "internal/b2m_save_replay";
}
