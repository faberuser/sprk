using System;

namespace NVespa;

[Flags]
public enum EAudioMixerFlag
{
	None = 0,
	DarknessCreature = 1,
	Bgm = 2,
	Voice = 4,
	Player = 8,
	Skill = 0x10
}
