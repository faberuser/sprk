using System.Text.RegularExpressions;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class DeviceModelData : IDataKeySelector<int>
{
	[IgnoreMember]
	public Regex RegExpress;

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string DeviceModel { get; set; }

	[Key(2)]
	public bool UseRegularExpression { get; set; }

	[Key(3)]
	public string ForceMemorySpec { get; set; }

	[Key(4)]
	public DeviceStyleType DeviceStyle { get; set; }

	[Key(5)]
	public float[] ScreenRate { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "DeviceModelData Model=" + Index + " " + DeviceModel;
	}

	public bool CompareDeviceModel(string deviceModel)
	{
		if (UseRegularExpression)
		{
			if (RegExpress == null)
			{
				RegExpress = new Regex(DeviceModel);
			}
			return RegExpress.IsMatch(deviceModel);
		}
		return DeviceModel.CompareTo(deviceModel) == 0;
	}
}
