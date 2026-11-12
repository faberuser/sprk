namespace NShared.JoinRaidRoom;

public enum ResultType
{
	Fail,
	Success,
	AlreadyJoined,
	RoomNotExist,
	InvalidRoomInfo,
	RoomFull,
	RoomInBattle,
	[Comment("싱글 플레이 레이드방 입니다.")]
	IsSingleRoom,
	DungeonLocked
}
