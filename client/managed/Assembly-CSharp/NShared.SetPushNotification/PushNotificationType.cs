namespace NShared.SetPushNotification;

[JsonSerializable(Javascript = true)]
public enum PushNotificationType
{
	None,
	Event,
	GuildRaid,
	Night,
	End,
	GuildSuppress
}
