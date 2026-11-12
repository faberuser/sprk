using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class NPCFriendlyPointItemDataContainer : SubItemDataContainer<NPCFriendlyPointItemData>
{
	public const string TABLENAME = "NPCFriendlyPointItemTable";

	private JitDictionary<int, List<NPCFriendlyPointItemData>> _dicByHeroIndex = new JitDictionary<int, List<NPCFriendlyPointItemData>>();

	public override TablePathDefine PathDefine => new TablePathDefine("NPCFriendlyPointItemTable");

	public NPCFriendlyPointItemDataContainer()
	{
		base.Packer.RegisterAndBind(_dicByHeroIndex);
	}

	public override ItemType[] GetSubTypes()
	{
		return new ItemType[1] { ItemType.NPCFriendlyPoint };
	}

	protected override void OnEndLoadFromJsonData()
	{
		base.OnEndLoadFromJsonData();
		foreach (KeyValuePair<string, NPCFriendlyPointItemData> item in base.BaseDic.JsonLoad_Get())
		{
			if (item.Value.HeroIndices != null && item.Value.HeroIndices.Length != 0)
			{
				for (int i = 0; i < item.Value.HeroIndices.Length; i++)
				{
					int key = item.Value.HeroIndices[i];
					_dicByHeroIndex.JsonLoad_AddToListValue(key, item.Value);
				}
			}
		}
	}

	public IList<NPCFriendlyPointItemData> GetItemListByHeroIndex(int heroIndex)
	{
		return __Extension_Method_NShared_ReadonlyEmpty__.GetEmptyIfNull(_dicByHeroIndex.GetOrDefault(heroIndex));
	}
}
