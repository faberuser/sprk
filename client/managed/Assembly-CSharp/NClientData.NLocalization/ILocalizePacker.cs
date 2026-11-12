using NShared.NJit;
using NShared.NJit.NCommon;

namespace NClientData.NLocalization;

public interface ILocalizePacker
{
	JitPacker Packer { get; }

	bool TryGet(string key, out string value);

	void Deserialize(ByteSpan readSpan);
}
