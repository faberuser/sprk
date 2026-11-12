using NShared;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class ArchiveItemInfoBase : MonoBehaviour
{
	public void Enable(bool enable, ItemData itemData)
	{
		base.gameObject.SetActive(enable);
		Reset();
		if (enable)
		{
			Init(itemData);
		}
	}

	public void Change(ItemData itemData)
	{
		Init(itemData);
	}

	protected virtual void Init(ItemData itemData)
	{
	}

	protected virtual void Reset()
	{
	}
}
