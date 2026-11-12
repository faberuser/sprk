namespace NGame2.NAccount.NNotice;

public class SoulWeaponNoticeInfo : ICheckerNoticeInfo
{
	public ESoulWeaponNoticeType NoticeType;

	public int SlotIndex;

	public bool IsOn;

	public override string ToString()
	{
		return string.Format("Type : " + NoticeType.ToString() + " IsOn : " + IsOn + " SlotIndex : " + SlotIndex);
	}
}
