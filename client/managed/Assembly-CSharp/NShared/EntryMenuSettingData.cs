using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EntryMenuSettingData : IDataKeySelector<string>
{
	string IDataKeySelector<string>.Key => MenuType;

	[Key(0)]
	public string MenuType { get; set; }

	[Key(1)]
	public string EventBgmCode { get; set; }

	[Key(2)]
	public string AssetNguiCode { get; set; }

	[Key(3)]
	public float PopupInterval { get; set; }

	[Key(4)]
	public float CameraZoomScale { get; set; }

	[Key(5)]
	public float CameraOffsetX { get; set; }

	[Key(6)]
	public float CameraOffsetY { get; set; }

	[Key(7)]
	public float CameraOffsetZ { get; set; }

	[Key(8)]
	public float CameraDirectionX { get; set; }

	[Key(9)]
	public float CameraDirectionY { get; set; }

	[Key(10)]
	public float CameraDirectionZ { get; set; }

	[Key(11)]
	public float AvatarDirAngle { get; set; }

	[Key(12)]
	public string NpcCode { get; set; }

	[Key(13)]
	public int NpcStar { get; set; }

	[Key(14)]
	public string NpcPrefix { get; set; }

	[Key(15)]
	public float NpcPositionX { get; set; }

	[Key(16)]
	public float NpcPositionY { get; set; }

	[Key(17)]
	public float NpcPositionZ { get; set; }

	[Key(18)]
	public float NpcDirectionX { get; set; }

	[Key(19)]
	public float NpcDirectionY { get; set; }

	[Key(20)]
	public float NpcDirectionZ { get; set; }

	[Key(21)]
	public float NpcScale { get; set; }

	[Key(22)]
	public int NpcSlideOffset { get; set; }

	[Key(23)]
	public float NpcSlideTimeLength { get; set; }

	[Key(24)]
	public string NpcCode2 { get; set; }

	[Key(25)]
	public int NpcStar2 { get; set; }

	[Key(26)]
	public string NpcPrefix2 { get; set; }

	[Key(27)]
	public float NpcPosition2X { get; set; }

	[Key(28)]
	public float NpcPosition2Y { get; set; }

	[Key(29)]
	public float NpcPosition2Z { get; set; }

	[Key(30)]
	public float NpcDirection2X { get; set; }

	[Key(31)]
	public float NpcDirection2Y { get; set; }

	[Key(32)]
	public float NpcDirection2Z { get; set; }

	[Key(33)]
	public float NpcScale2 { get; set; }

	[Key(34)]
	public int NpcSlideOffset2 { get; set; }

	[Key(35)]
	public float NpcSlideTimeLength2 { get; set; }

	[Key(36)]
	public bool UseNodeTree { get; set; }

	[Key(37)]
	public string CinemaWindowName { get; set; }

	[Key(38)]
	public string LobbyWindowName { get; set; }

	public override string ToString()
	{
		return "Menutype=" + MenuType;
	}

	public string GetNpcCode(int npcIndex = 1)
	{
		if (!string.IsNullOrEmpty(NpcCode) && npcIndex == 1)
		{
			return NpcCode;
		}
		if (!string.IsNullOrEmpty(NpcCode2) && npcIndex == 2)
		{
			return NpcCode2;
		}
		return "";
	}

	public int GetNpcStar(int npcIndex = 1)
	{
		if (!string.IsNullOrEmpty(NpcCode) && npcIndex == 1)
		{
			return NpcStar;
		}
		if (!string.IsNullOrEmpty(NpcCode2) && npcIndex == 2)
		{
			return NpcStar2;
		}
		return 0;
	}

	public string GetNpcPrefix(int npcIndex = 1)
	{
		if (!string.IsNullOrEmpty(NpcCode) && npcIndex == 1)
		{
			return NpcPrefix;
		}
		if (!string.IsNullOrEmpty(NpcCode2) && npcIndex == 2)
		{
			return NpcPrefix2;
		}
		return "";
	}

	public float[] GetNpcPosition(int npcIndex = 1)
	{
		if (!string.IsNullOrEmpty(NpcCode) && npcIndex == 1)
		{
			return new float[3] { NpcPositionX, NpcPositionY, NpcPositionZ };
		}
		if (!string.IsNullOrEmpty(NpcCode2) && npcIndex == 2)
		{
			return new float[3] { NpcPosition2X, NpcPosition2Y, NpcPosition2Z };
		}
		return new float[3] { -140f, -150f, 0f };
	}

	public float[] GetNpcDirection(int npcIndex = 1)
	{
		if (!string.IsNullOrEmpty(NpcCode) && npcIndex == 1)
		{
			return new float[3] { NpcDirectionX, NpcDirectionY, NpcDirectionZ };
		}
		if (!string.IsNullOrEmpty(NpcCode2) && npcIndex == 2)
		{
			return new float[3] { NpcDirection2X, NpcDirection2Y, NpcDirection2Z };
		}
		return new float[3] { 1f, 0f, 1f };
	}

	public float GetNpcScale(int npcIndex = 1)
	{
		if (!string.IsNullOrEmpty(NpcCode) && npcIndex == 1)
		{
			return NpcScale;
		}
		if (!string.IsNullOrEmpty(NpcCode2) && npcIndex == 2)
		{
			return NpcScale2;
		}
		return 140f;
	}

	public float GetNpcSlideOffset(int npcIndex = 1)
	{
		if (!string.IsNullOrEmpty(NpcCode) && npcIndex == 1)
		{
			return NpcSlideOffset;
		}
		if (!string.IsNullOrEmpty(NpcCode2) && npcIndex == 2)
		{
			return NpcSlideOffset2;
		}
		return -10f;
	}

	public float GetNpcSlideTimeLength(int npcIndex = 1)
	{
		if (!string.IsNullOrEmpty(NpcCode) && npcIndex == 1)
		{
			return NpcSlideTimeLength;
		}
		if (!string.IsNullOrEmpty(NpcCode2) && npcIndex == 2)
		{
			return NpcSlideTimeLength2;
		}
		return 0.3f;
	}
}
