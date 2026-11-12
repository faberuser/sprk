namespace NShared.Login;

[Request]
public class Request : NShared.Request, IServiceRequest
{
	string IServiceRequest.ServiceName => "user/login";

	public LoginMethodType LoginMethod { get; set; }

	public string LoginId { get; set; }

	public string LegacyLoginId { get; set; }

	public long SelectedAccountId { get; set; }

	public long EncryptSalt { get; set; }

	public string DeviceId { get; set; }

	public string Version { get; set; }

	public string BattleVersion { get; set; }

	public int UTCOffsetHour { get; set; }

	public bool ForcedHost { get; set; }

	public string DeviceOs { get; set; }

	public string DevicePlatform { get; set; }

	public string DeviceModel { get; set; }

	public int DeviceMemorySize { get; set; }

	public string DeviceToken { get; set; }

	public string Language { get; set; }

	public string CountryCode { get; set; }

	public int PatchNoteIndex { get; set; }

	public bool EventPush { get; set; }

	public bool GuildRaidPush { get; set; }

	public bool GuildSuppressPush { get; set; }

	public bool NightPush { get; set; }

	public string GraphicsAPI { get; set; }

	public string SupportTextureFormat { get; set; }

	public string Adid { get; set; }

	public string GoogleAdid { get; set; }

	public string Idfa { get; set; }

	public string Idfv { get; set; }

	public string SigningKey { get; set; }

	public string DeviceBuildFingerprint { get; set; }

	public string DeviceBuildModel { get; set; }

	public string DeviceBuildBrand { get; set; }

	public string DeviceBuildDevice { get; set; }

	public string DeviceBuildProduct { get; set; }

	public string DeviceBuildType { get; set; }

	public string DeviceBuildSerial { get; set; }

	public string DeviceBuildBoard { get; set; }

	public string CpuArch { get; set; }

	public bool IsDeviceRooted { get; set; }

	public bool IsModInstalled { get; set; }

	public string LoginVerificationInfo { get; set; }

	public int AppPlayerBuild { get; set; }

	public bool CancelSignOut { get; set; }

	public bool ForceSignOut { get; set; }

	public string PackageName { get; set; }

	public bool IsEmulator { get; set; }

	public bool EnableCharacterVoice { get; set; }

	public int CharacterVoiceLanguage { get; set; }
}
