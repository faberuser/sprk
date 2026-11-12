using System.Collections.Generic;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class HelpListData : IDataKeySelector<HelpListDataKey>
{
	private List<HelpListPageElement> ElementList;

	HelpListDataKey IDataKeySelector<HelpListDataKey>.Key => new HelpListDataKey(Code, Page);

	[Key(0)]
	public string Code { get; set; }

	[Key(1)]
	public int Page { get; set; }

	[Key(2)]
	public string ScrollMovement { get; set; }

	[Key(3)]
	public string MainTitleKey { get; set; }

	[Key(4)]
	public string BackgroundTexturePath { get; set; }

	[Key(5)]
	public string ComponentName1 { get; set; }

	[Key(6)]
	public string TitleKey1 { get; set; }

	[Key(7)]
	public string DescKey1 { get; set; }

	[Key(8)]
	public string TexturePath1 { get; set; }

	[Key(9)]
	public int TextureWidth1 { get; set; }

	[Key(10)]
	public int TextureHeight1 { get; set; }

	[Key(11)]
	public string ComponentName2 { get; set; }

	[Key(12)]
	public string TitleKey2 { get; set; }

	[Key(13)]
	public string DescKey2 { get; set; }

	[Key(14)]
	public string TexturePath2 { get; set; }

	[Key(15)]
	public int TextureWidth2 { get; set; }

	[Key(16)]
	public int TextureHeight2 { get; set; }

	[Key(17)]
	public string ComponentName3 { get; set; }

	[Key(18)]
	public string TitleKey3 { get; set; }

	[Key(19)]
	public string DescKey3 { get; set; }

	[Key(20)]
	public string TexturePath3 { get; set; }

	[Key(21)]
	public int TextureWidth3 { get; set; }

	[Key(22)]
	public int TextureHeight3 { get; set; }

	[Key(23)]
	public string ComponentName4 { get; set; }

	[Key(24)]
	public string TitleKey4 { get; set; }

	[Key(25)]
	public string DescKey4 { get; set; }

	[Key(26)]
	public string TexturePath4 { get; set; }

	[Key(27)]
	public int TextureWidth4 { get; set; }

	[Key(28)]
	public int TextureHeight4 { get; set; }

	private void InitPages()
	{
		ElementList = new List<HelpListPageElement>();
		AddElement(new HelpListPageElement(ComponentName1, TitleKey1, DescKey1, TexturePath1, TextureWidth1, TextureHeight1));
		AddElement(new HelpListPageElement(ComponentName2, TitleKey2, DescKey2, TexturePath2, TextureWidth2, TextureHeight2));
		AddElement(new HelpListPageElement(ComponentName3, TitleKey3, DescKey3, TexturePath3, TextureWidth3, TextureHeight3));
		AddElement(new HelpListPageElement(ComponentName4, TitleKey4, DescKey4, TexturePath4, TextureWidth4, TextureHeight4));
	}

	private void AddElement(HelpListPageElement element)
	{
		if (element.IsValid)
		{
			ElementList.Add(element);
		}
	}

	public List<HelpListPageElement> GetAllElements()
	{
		if (ElementList == null)
		{
			InitPages();
		}
		return ElementList;
	}

	public int GetLastElementNumber()
	{
		if (ElementList == null)
		{
			InitPages();
		}
		return ElementList.Count;
	}
}
