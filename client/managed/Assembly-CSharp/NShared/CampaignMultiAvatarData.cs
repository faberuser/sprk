using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CampaignMultiAvatarData : IDataKeySelector<CampaignMultiAvatarKey>
{
	[Key(0)]
	public int LineIndex { get; set; }

	[Key(1)]
	public int ChapterIndex { get; set; }

	[Key(2)]
	public bool Opened { get; set; }

	[Key(3)]
	public string[] NextStartCondition { get; set; }

	[Key(4)]
	public int CostumeIndex { get; set; }

	[Key(5)]
	public int StartNode { get; set; }

	[Key(6)]
	public int PairCostumeIndex { get; set; }

	[Key(7)]
	public int PairStartNode { get; set; }

	[Key(8)]
	public string PairStatus { get; set; }

	[Key(9)]
	public string CenterPopupKey { get; set; }

	[Key(10)]
	public PairEffectType PairEffectType { get; set; }

	CampaignMultiAvatarKey IDataKeySelector<CampaignMultiAvatarKey>.Key => new CampaignMultiAvatarKey(ChapterIndex, LineIndex);
}
