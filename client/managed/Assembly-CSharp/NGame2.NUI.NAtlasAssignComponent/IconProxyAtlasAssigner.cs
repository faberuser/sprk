using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NNguiExtension;
using UnityEngine;

namespace NGame2.NUI.NAtlasAssignComponent;

public class IconProxyAtlasAssigner : AtlasAssignerBase
{
	[Serializable]
	public class AtlasSaveData
	{
		public string AtlasName = string.Empty;

		public string SpriteName = string.Empty;

		public AtlasSaveData(string atlasName, string spriteName)
		{
			AtlasName = atlasName;
			SpriteName = spriteName;
		}

		public bool IsValid()
		{
			return AtlasName != string.Empty;
		}
	}

	public IconProxy Target;

	public List<AtlasSaveData> CurrencyList = new List<AtlasSaveData>();

	public List<AtlasSaveData> CurrencyNoBgList = new List<AtlasSaveData>();

	public List<AtlasSaveData> StaminaList = new List<AtlasSaveData>();

	public List<AtlasSaveData> ClassBuffList = new List<AtlasSaveData>();

	protected override void SetTargetComponent(Component component)
	{
		IconProxy iconProxy = component as IconProxy;
		if (!(iconProxy == null))
		{
			Target = iconProxy;
		}
	}

	protected override void SetPriorityInternal(Component component)
	{
		base.Priorty = 3;
	}

	protected override void SaveAtlas()
	{
		if (!(Target == null))
		{
			SaveList(Target.CurrencyList, CurrencyList);
			SaveList(Target.CurrencyNoBgList, CurrencyNoBgList);
			SaveList(Target.StaminaList, StaminaList);
			SaveList(Target.ClassBuffList, ClassBuffList);
		}
	}

	private void SaveList(List<UISpriteSelector.SelectData> targetList, List<AtlasSaveData> container)
	{
		if (__Extension_Method_ICollection__.COUNT(targetList) == 0 || __Extension_Method_ICollection__.COUNT(targetList) == __Extension_Method_ICollection__.COUNT(container))
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(targetList); i++)
		{
			AtlasSaveData atlasSaveData = null;
			UISpriteSelector.SelectData selectData = targetList[i];
			_ = selectData.Atlas;
			string atlasName = ((selectData.Atlas == null) ? string.Empty : selectData.Atlas.name);
			string spriteName = selectData.SpriteName;
			if (IsIncludeAtlas(atlasName))
			{
				atlasSaveData = new AtlasSaveData(string.Empty, string.Empty);
			}
			else
			{
				selectData.Atlas = null;
				atlasSaveData = new AtlasSaveData(atlasName, spriteName);
			}
			container.Add(atlasSaveData);
		}
	}

	protected override void AssignAtlas()
	{
		if (!(Target == null))
		{
			AssignList(Target.CurrencyList, CurrencyList);
			AssignList(Target.CurrencyNoBgList, CurrencyNoBgList);
			AssignList(Target.StaminaList, StaminaList);
			AssignList(Target.ClassBuffList, ClassBuffList);
		}
	}

	private void AssignList(List<UISpriteSelector.SelectData> targetList, List<AtlasSaveData> container)
	{
		if (targetList == null || __Extension_Method_ICollection__.COUNT(targetList) != __Extension_Method_ICollection__.COUNT(container))
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(container); i++)
		{
			AtlasSaveData atlasSaveData = container[i];
			if (atlasSaveData.IsValid())
			{
				targetList[i].Atlas = GetAtlas(atlasSaveData.AtlasName);
				targetList[i].SpriteName = atlasSaveData.SpriteName;
			}
		}
	}
}
