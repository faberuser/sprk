namespace NShared.ConfirmSoulStone;

public enum ResultType
{
	Fail,
	Success,
	[Comment("선택 영혼석 정보를 찾을 수 없습니다.")]
	RestoreSoulStoneIndicesNotFound,
	[Comment("잘못 된 아이템인덱스 입니다.")]
	InvalidItemIndex,
	[Comment("아이템 게임데이터를 찾을 수 없습니다.")]
	ItemDataNotFound,
	[Comment("선택한 아이템이 영혼석이 아닙니다.")]
	IsNotSoulStone
}
