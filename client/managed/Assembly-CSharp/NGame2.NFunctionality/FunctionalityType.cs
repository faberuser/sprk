namespace NGame2.NFunctionality;

public enum FunctionalityType
{
	None,
	[IntegerValue(Default = 1)]
	Test,
	[BooleanValue(Default = true)]
	EntryMenuNpcWelcomAction,
	[BooleanValue(Default = false)]
	WaitOnCloseLoadingWindow,
	[BooleanValue(Default = false)]
	WaitOnWaveReadyEvent,
	[BooleanValue(Default = false)]
	WaitOnWaveEndEvent,
	[BooleanValue(Default = false)]
	WaitOnCampaignEndEvent,
	[BooleanValue(Default = false)]
	SkipBattleStartEffect,
	[BooleanValue(Default = false)]
	DisableAutoBattle
}
