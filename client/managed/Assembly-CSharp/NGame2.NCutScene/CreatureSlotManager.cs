using NGame2.NCutScene.NModifier;
using NVespa;
using UnityEngine;

namespace NGame2.NCutScene;

public class CreatureSlotManager : ISlotProvider
{
	private GameObject _rootGo;

	private GameObject _offsetGo;

	private IncreaseDic<int, CreatureModifier> _slotDic = new IncreaseDic<int, CreatureModifier>();

	private bool _show = true;

	public int Count => _slotDic.Count;

	public bool Show
	{
		get
		{
			return _show;
		}
		set
		{
			_show = value;
			UpdateVisible();
		}
	}

	public Transform CreatureRootTm => _offsetGo.transform;

	public void Open(int layer)
	{
		_rootGo = new GameObject("CutScene CreatureScale");
		Vespa_Unity_Transform_Extension_Method.ParentResetTM(_rootGo.transform, GameUIRoot.instance.ContentsGroup.transform);
		if (layer >= 0)
		{
			_rootGo.layer = layer;
		}
		_offsetGo = new GameObject("CutScene CreatureOffset");
		Vespa_Unity_Transform_Extension_Method.ParentResetTM(_offsetGo.transform, _rootGo.transform);
		if (layer >= 0)
		{
			_offsetGo.layer = layer;
		}
		_rootGo.SetActive(value: false);
		_show = true;
	}

	public void SetScene(float zoomScale, Vector3 offset)
	{
		_rootGo.transform.localScale = Vector3.one * zoomScale;
		_offsetGo.transform.localPosition = offset;
	}

	public int GetSlotIndexByIndex(int index)
	{
		return _slotDic.GetKeyByIndex(index);
	}

	public CreatureModifier GetCreatureModByIndex(int index)
	{
		return _slotDic.GetValueByIndex(index);
	}

	public string GetCreatureCodeByIndex(int index)
	{
		if (_slotDic.GetValueByIndex(index) == null)
		{
			return string.Empty;
		}
		return _slotDic.GetValueByIndex(index).Code;
	}

	public void Add(int slotIndex, CreatureModifier creatureMod)
	{
		if (creatureMod != null)
		{
			_slotDic.Add(slotIndex, creatureMod);
			UpdateVisible();
		}
	}

	public CreatureModifier GetCreatureMod(int slotIndex)
	{
		return _slotDic.GETVALUE(slotIndex, null);
	}

	public void Remove(int slotIndex)
	{
		_slotDic.RemoveSlow(slotIndex);
		UpdateVisible();
	}

	private void UpdateVisible()
	{
		if (_slotDic.IsNullOrEmpty())
		{
			_rootGo.SetActive(value: false);
		}
		else
		{
			_rootGo.SetActive(_show);
		}
	}

	public void ChainUpdate(ILocalTimer timer)
	{
		for (int i = 0; i < _slotDic.Count; i++)
		{
			_slotDic.GetValueByIndex(i).ChainUpdate(timer);
		}
	}
}
