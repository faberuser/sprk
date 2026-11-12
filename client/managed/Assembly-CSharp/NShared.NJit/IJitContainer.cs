using NShared.NJit.NCommon;

namespace NShared.NJit;

public interface IJitContainer
{
	void Bind(JitPacker packer);

	void Reset();

	void Deserialize(ByteSpan span);

	bool IsPackerValid();
}
