namespace NShared.ChangeAvatarHero;

public enum ResultType
{
	Fail,
	Success,
	[Comment("해당 영웅을 보유하고 있지 않습니다.")]
	NotHaveHero,
	[Comment("잘못된 AvatarHeroIndex값을 요청하였습니다.")]
	InvalidAvatarHeroIndex
}
