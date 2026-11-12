namespace NShared.GetEquipGacha;

public enum ResultType
{
	Fail,
	Success,
	[Comment("소환 정보 게임데이터를 찾을 수 없습니다.")]
	GachaDataNotFound
}
