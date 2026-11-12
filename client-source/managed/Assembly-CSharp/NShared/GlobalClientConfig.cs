namespace NShared;

[JsonSerializable]
public class GlobalClientConfig
{
	public string AdjustAppToken { get; set; }

	public bool UseAdjustAppSecret { get; set; }

	public int AndroidAdjustAppSecretId { get; set; }

	public long[] AndroidAdjustAppSecretInfo { get; set; }

	public int IosAdjustAppSecretId { get; set; }

	public long[] IosAdjustAppSecretInfo { get; set; }

	public int WindowsAdjustAppSecretId { get; set; }

	public long[] WindowsAdjustAppSecretInfo { get; set; }
}
