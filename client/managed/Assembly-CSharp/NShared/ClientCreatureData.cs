using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ClientCreatureData : IDataKeySelector<ClientCreatureKey>
{
	ClientCreatureKey IDataKeySelector<ClientCreatureKey>.Key => new ClientCreatureKey(Code, CostumeIndex);

	[Key(0)]
	public HashString Code { get; set; }

	[Key(1)]
	public int CostumeIndex { get; set; }

	[Key(2)]
	public float HeroViewZoomInViewSize { get; set; }

	[Key(3)]
	public float[] HeroViewZoomInOffset { get; set; }

	[Key(4)]
	public float HeroViewZoomOutViewSize { get; set; }

	[Key(5)]
	public float[] HeroViewZoomOutOffset { get; set; }

	[Key(6)]
	public float[] DetailViewZoomInOffset { get; set; }

	[Key(7)]
	public float DetailViewZoomInDistance { get; set; }

	[Key(8)]
	public float[] DetailViewZoomOutOffset { get; set; }

	[Key(9)]
	public float DetailViewZoomOutDistance { get; set; }

	[Key(10)]
	public float[] PurifyDarknessOffset { get; set; }

	[Key(11)]
	public float PurifyCameraScale { get; set; }

	[Key(12)]
	public float[] PurifyCameraOffset { get; set; }

	[Key(13)]
	public float[] PayShopOffset { get; set; }

	[Key(14)]
	public float PayShopScale { get; set; }

	[Key(15)]
	public float[] HeroInnOffset { get; set; }

	[Key(16)]
	public float HeroInnScale { get; set; }

	[Key(17)]
	public string HeroInnMotion { get; set; }

	[Key(18)]
	public float[] EntryMenuOffset { get; set; }

	[Key(19)]
	public float EntryMenuScale { get; set; }

	[Key(20)]
	public float[] MatchPickBanOffset { get; set; }

	[Key(21)]
	public float MatchPickBanScale { get; set; }

	[Key(22)]
	public float[] FullViewZoomInOffset { get; set; }

	[Key(23)]
	public float FullViewZoomInDistance { get; set; }

	[Key(24)]
	public float[] FullViewZoomOutOffset { get; set; }

	[Key(25)]
	public float FullViewZoomOutDistance { get; set; }

	[Key(26)]
	public float[] ArchiveViewZoomInOffset { get; set; }

	[Key(27)]
	public float ArchiveViewZoomInDistance { get; set; }

	[Key(28)]
	public float[] ArchiveViewZoomOutOffset { get; set; }

	[Key(29)]
	public float ArchiveViewZoomOutDistance { get; set; }

	[Key(30)]
	public float ArchiveCameraAngleX { get; set; }

	[Key(31)]
	public float ArchiveCameraAngleY { get; set; }

	[Key(32)]
	public string ArchivePreviewBg { get; set; }

	[Key(33)]
	public float[] StepUpGachaOffset { get; set; }

	[Key(34)]
	public float StepUpScale { get; set; }

	[Key(35)]
	public string PayShopAnimationKey { get; set; }

	[Key(36)]
	public float LookRotationX { get; set; }

	[Key(37)]
	public float LookRotationZ { get; set; }

	[Key(38)]
	public float[] PetListOffset { get; set; }

	[Key(39)]
	public float PetListScale { get; set; }
}
