using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class PatchDownloadSpecData : IDataKeySelector<int>
{
	[Key(0)]
	public PatchSpecType Spec { get; set; }

	[Key(1)]
	public PatchPlatform Platform { get; set; }

	[Key(2)]
	public int RamMin { get; set; }

	[Key(3)]
	public int RamMax { get; set; }

	[Key(4)]
	public PatchSpecType StandardSpec { get; set; }

	[Key(5)]
	public PatchSpecType[] SelectSpecInPatchDownload { get; set; }

	[Key(6)]
	public PatchSpecType[] SelectSpecInGameOption { get; set; }

	int IDataKeySelector<int>.Key => GetKey(Spec, Platform);

	public static int GetKey(PatchSpecType spec, PatchPlatform platform)
	{
		return ((int)spec << 8) ^ (int)platform;
	}
}
