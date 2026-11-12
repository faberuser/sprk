using System;

namespace NVespa;

[Flags]
public enum ETimerEventFlag
{
	[TimerEvent(IsLocal = false, PauseType = TimerEventAttribute.EPauseType.Pause)]
	SkillSoloPause = 1,
	[TimerEvent(IsLocal = true, PauseType = TimerEventAttribute.EPauseType.Playing)]
	SkillSoloPlaying = 2,
	[TimerEvent(IsLocal = false)]
	TacticsMode = 4,
	[TimerEvent(IsLocal = false)]
	TacticsTargeting = 8,
	[TimerEvent(IsLocal = false, PauseType = TimerEventAttribute.EPauseType.Pause)]
	BossIntroPause = 0x20,
	[TimerEvent(IsLocal = true, PauseType = TimerEventAttribute.EPauseType.Playing)]
	BossIntroPlaying = 0x40,
	[TimerEvent(IsLocal = false)]
	LastKill = 0x80
}
