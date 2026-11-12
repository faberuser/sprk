using System.Collections;
using System.Collections.Generic;
using NGame2.NBattleField.NManager;
using NGame2.NNodeTree;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NBattleView;

public class DropItemManager : BattleViewGenericSingleton<DropItemManager>, InitOnCreate
{
	private struct DropOffsetGenerator
	{
		private float _creatureRadius;

		private int _dropIndex;

		private int _dropCount;

		private Vector3 _offset;

		public DropOffsetGenerator(Creature creature, int dropCount)
		{
			this = default(DropOffsetGenerator);
			_creatureRadius = creature.ShapeBounds.extents.x;
			_dropIndex = -1;
			_dropCount = dropCount;
			_offset = Vector3.zero;
		}

		public Vector3 NextPosition()
		{
			if (_dropCount < 2)
			{
				_offset = Vector3.zero;
			}
			else
			{
				_dropIndex++;
				float num = Random.Range(_creatureRadius, _creatureRadius * 2f);
				float num2 = 360f / (float)_dropCount;
				float i_degrees = Random.Range(num2 * (float)_dropIndex, num2 * ((float)_dropIndex + 0.5f));
				_offset = Math.Rotate(VectorXZ.right, i_degrees).ToVec3() * num;
			}
			return _offset;
		}
	}

	public List<DropItem> DropList = new List<DropItem>();

	private PackLoad _packLoad;

	private TreeData _dropItemTree;

	private TreeData _dropGoldTree;

	private Dictionary<string, TreeData> _dropTreeDics = new Dictionary<string, TreeData>();

	private int _coroutineBegin;

	private int _coroutineEnd;

	public bool CompleteDrop => _coroutineBegin == _coroutineEnd;

	void InitOnCreate.InitializeSingleton()
	{
		_packLoad = SingletonAuto<PackContainer>.instance.GetPack("Common/DropItem");
		if (_packLoad.IsDone)
		{
			CompletePackLoad();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ClearAll();
	}

	public void ClearAll()
	{
		for (int i = 0; i < DropList.Count; i++)
		{
			DropItem dropItem = DropList[i];
			dropItem.Close();
			VespaObject.DestroyGo(dropItem);
		}
		DropList.Clear();
	}

	private void CompletePackLoad()
	{
		if (!_packLoad.IsDone)
		{
			return;
		}
		if (!_packLoad.ISVALID())
		{
			_packLoad = null;
			return;
		}
		if (_dropItemTree == null)
		{
			_dropItemTree = _packLoad.Value.GetNodeTree("DropItem");
		}
		if (_dropGoldTree == null)
		{
			_dropGoldTree = _packLoad.Value.GetNodeTree("DropGold");
		}
	}

	public void CreateDrop(Creature creature, NGame2.NBattleField.NManager.DropItemManager.CreatureDrop drop)
	{
		if (_packLoad.ISVALID())
		{
			CompletePackLoad();
			SetDropTreeData(drop);
		}
		VespaCoroutine.Start(this, coCreateDrop(creature, drop));
	}

	private void SetDropTreeData(NGame2.NBattleField.NManager.DropItemManager.CreatureDrop drop)
	{
		if (!_packLoad.IsDone)
		{
			return;
		}
		for (int i = 0; i < drop.ItemList.Count; i++)
		{
			int index = drop.ItemList[i];
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(index);
			if (data == null || String_Extension_Method.IsNullOrEmpty(data.DropItemNodeTreeCode))
			{
				continue;
			}
			string dropItemNodeTreeCode = data.DropItemNodeTreeCode;
			if (!_dropTreeDics.ContainsKey(dropItemNodeTreeCode))
			{
				TreeData nodeTree = _packLoad.Value.GetNodeTree(dropItemNodeTreeCode);
				if (nodeTree != null)
				{
					_dropTreeDics[dropItemNodeTreeCode] = nodeTree;
				}
			}
		}
	}

	private IEnumerator coCreateDrop(Creature creature, NGame2.NBattleField.NManager.DropItemManager.CreatureDrop drop)
	{
		if (_dropItemTree == null)
		{
			yield break;
		}
		_coroutineBegin++;
		Vector3 creaturePos = creature.Position;
		int creatureUid = creature.Uid;
		int creatureLayer = creature.gameObject.layer;
		int creatureGroupIndex = creature.GroupIndex;
		DropOffsetGenerator offsetGen = new DropOffsetGenerator(creature, drop.ItemList.Count + drop.GoldList.Count);
		for (int index = 0; index < drop.ItemList.Count; index++)
		{
			int num = drop.ItemList[index];
			Vector3 position = creaturePos + offsetGen.NextPosition();
			DropItem dropItem = CreateDropItem();
			dropItem.Open(creatureGroupIndex, position, DropItem.EDropType.Item, num);
			ItemData data = GameRoot.Instance.ItemDataContainer.GetData(num);
			if (data == null || String_Extension_Method.IsNullOrEmpty(data.DropItemNodeTreeCode))
			{
				dropItem.ExecuteNodeTree(_dropItemTree, creatureUid, creatureLayer);
			}
			else if (_dropTreeDics.ContainsKey(data.DropItemNodeTreeCode))
			{
				dropItem.ExecuteNodeTree(_dropTreeDics[data.DropItemNodeTreeCode], creatureUid, creatureLayer);
			}
			DropList.Add(dropItem);
			yield return new WaitForSeconds(0.05f);
		}
		for (int index = 0; index < drop.GoldList.Count; index++)
		{
			int dropValue = drop.GoldList[index];
			Vector3 position2 = creaturePos + offsetGen.NextPosition();
			DropItem dropItem2 = CreateDropItem();
			dropItem2.Open(creatureGroupIndex, position2, DropItem.EDropType.Gold, dropValue);
			dropItem2.ExecuteNodeTree(_dropGoldTree, creatureUid, creatureLayer);
			DropList.Add(dropItem2);
			yield return new WaitForSeconds(0.05f);
		}
		_coroutineEnd++;
	}

	private DropItem CreateDropItem()
	{
		GameObject obj = new GameObject();
		obj.transform.parent = base.transform;
		return obj.AddComponent<DropItem>();
	}
}
