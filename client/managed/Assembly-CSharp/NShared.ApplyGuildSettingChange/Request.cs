namespace NShared.ApplyGuildSettingChange;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "guild/apply_guild_setting_change";

	public byte Logo { get; set; }

	public byte Back { get; set; }

	public byte Flag { get; set; }

	public byte JoinWay { get; set; }

	public int TeamLevel { get; set; }

	public string Introduce { get; set; }

	public string Notice { get; set; }

	public string CountryCode { get; set; }
}
