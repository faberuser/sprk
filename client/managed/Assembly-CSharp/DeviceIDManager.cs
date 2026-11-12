using System.Security.Cryptography;
using System.Text;

public static class DeviceIDManager
{
	public delegate void DeviceIDHandler(string deviceID);

	public const string UnsupportMacAddress = "02:00:00:00:00:00";

	public static event DeviceIDHandler deviceIDHandler;

	public static void GetDeviceID()
	{
		string empty = string.Empty;
		empty = SprkTestPrefs.DeviceId();
		DeviceIDManager.deviceIDHandler(empty);
	}

	private static string calcMd5(string srcStr)
	{
		MD5 mD = MD5.Create();
		byte[] bytes = Encoding.UTF8.GetBytes(srcStr);
		byte[] array = mD.ComputeHash(bytes);
		StringBuilder stringBuilder = new StringBuilder();
		byte[] array2 = array;
		foreach (byte b in array2)
		{
			stringBuilder.Append(b.ToString("x2"));
		}
		return stringBuilder.ToString();
	}
}
