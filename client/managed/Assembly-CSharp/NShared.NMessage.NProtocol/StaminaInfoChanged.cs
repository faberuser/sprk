namespace NShared.NMessage.NProtocol;

[JsonSerializable]
public class StaminaInfoChanged
{
	public StaminaType StaminaType { get; set; }

	public long StaminaCount { get; set; }
}
