using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class HeroAwakeData : IDataKeySelector<int>
{
	[Key(0)]
	public int HeroIndex { get; set; }

	[Key(1)]
	public int AwakeChapter { get; set; }

	[Key(2)]
	public int AwakeDungeon1 { get; set; }

	[Key(3)]
	public string AwakeMaterialItemCode1 { get; set; }

	[Key(4)]
	public int AwakeDungeon2 { get; set; }

	[Key(5)]
	public string AwakeMaterialItemCode2 { get; set; }

	[Key(6)]
	public int AwakeDungeon3 { get; set; }

	[Key(7)]
	public string AwakeMaterialItemCode3 { get; set; }

	[Key(8)]
	public int AwakeDungeon4 { get; set; }

	[Key(9)]
	public string AwakeMaterialItemCode4 { get; set; }

	[Key(10)]
	public int TranscendDungeon0 { get; set; }

	[Key(11)]
	public string TranscendMaterialItemCode0 { get; set; }

	[Key(12)]
	public int TranscendDungeon1 { get; set; }

	[Key(13)]
	public string TranscendMaterialItemCode1 { get; set; }

	[Key(14)]
	public int TranscendDungeon2 { get; set; }

	[Key(15)]
	public string TranscendMaterialItemCode2 { get; set; }

	[Key(16)]
	public int TranscendDungeon3 { get; set; }

	[Key(17)]
	public string TranscendMaterialItemCode3 { get; set; }

	[Key(18)]
	public int TranscendDungeon4 { get; set; }

	[Key(19)]
	public string TranscendMaterialItemCode4 { get; set; }

	int IDataKeySelector<int>.Key => HeroIndex;

	public string GetAwakeMaterialItemCode(int HeroStar)
	{
		return HeroStar switch
		{
			1 => AwakeMaterialItemCode1, 
			2 => AwakeMaterialItemCode2, 
			3 => AwakeMaterialItemCode3, 
			4 => AwakeMaterialItemCode4, 
			_ => string.Empty, 
		};
	}

	public string GetTranscendMaterialItemCode(int HeroTranscended)
	{
		return HeroTranscended switch
		{
			0 => TranscendMaterialItemCode0, 
			1 => TranscendMaterialItemCode1, 
			2 => TranscendMaterialItemCode2, 
			3 => TranscendMaterialItemCode3, 
			4 => TranscendMaterialItemCode4, 
			_ => string.Empty, 
		};
	}

	public bool IsAwakeItem(string itemCode)
	{
		for (int i = 1; i <= 4; i++)
		{
			if (GetAwakeMaterialItemCode(i) == itemCode)
			{
				return true;
			}
		}
		for (int j = 0; j <= 4; j++)
		{
			if (GetTranscendMaterialItemCode(j) == itemCode)
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckIndex(int chapterIndex, int dungeonIndex, int[] awakeDungeon)
	{
		if (awakeDungeon != null && awakeDungeon.Length >= 2 && chapterIndex == awakeDungeon[0] && dungeonIndex == awakeDungeon[1])
		{
			return true;
		}
		return false;
	}

	public bool CheckIndices(int chapterIndex, int dungeonIndex)
	{
		if (chapterIndex != AwakeChapter)
		{
			return false;
		}
		if (dungeonIndex == AwakeDungeon1)
		{
			return true;
		}
		if (dungeonIndex == AwakeDungeon2)
		{
			return true;
		}
		if (dungeonIndex == AwakeDungeon3)
		{
			return true;
		}
		if (dungeonIndex == AwakeDungeon4)
		{
			return true;
		}
		if (dungeonIndex == TranscendDungeon0)
		{
			return true;
		}
		if (dungeonIndex == TranscendDungeon1)
		{
			return true;
		}
		if (dungeonIndex == TranscendDungeon2)
		{
			return true;
		}
		if (dungeonIndex == TranscendDungeon3)
		{
			return true;
		}
		if (dungeonIndex == TranscendDungeon4)
		{
			return true;
		}
		return false;
	}

	public int[] GetDungeonIndex(int star, int transcend)
	{
		if (star == GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroMaxStar"))
		{
			return transcend switch
			{
				0 => new int[2] { AwakeChapter, TranscendDungeon0 }, 
				1 => new int[2] { AwakeChapter, TranscendDungeon1 }, 
				2 => new int[2] { AwakeChapter, TranscendDungeon2 }, 
				3 => new int[2] { AwakeChapter, TranscendDungeon3 }, 
				4 => new int[2] { AwakeChapter, TranscendDungeon4 }, 
				_ => null, 
			};
		}
		return star switch
		{
			1 => new int[2] { AwakeChapter, AwakeDungeon1 }, 
			2 => new int[2] { AwakeChapter, AwakeDungeon2 }, 
			3 => new int[2] { AwakeChapter, AwakeDungeon3 }, 
			4 => new int[2] { AwakeChapter, AwakeDungeon4 }, 
			_ => null, 
		};
	}
}
