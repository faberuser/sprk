using System;
using System.Collections.Generic;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NWorldMap.NMapLayout;

public class AvatarRearranger
{
	public struct NodeKey
	{
		public int DungeonIndex;

		public int CellX;

		public int CellY;
	}

	public class NodeAvatarContainer
	{
		private IncreaseDic<NodeKey, List<Avatar>> _dic = new IncreaseDic<NodeKey, List<Avatar>>();

		public int Count => _dic.Count;

		public void Clear()
		{
			for (int i = 0; i < _dic.Count; i++)
			{
				ReservePoolUtil.ReleaseList(_dic.GetValueByIndex(i));
			}
			_dic.Clear();
		}

		public IList<Avatar> GetValidList(NodeKey key)
		{
			if (!_dic.TryGetValue(key, out var o_value))
			{
				o_value = ReservePoolUtil.CreateList<Avatar>();
				_dic.Add(key, o_value);
			}
			return o_value;
		}

		public void Sort()
		{
			for (int i = 0; i < _dic.Count; i++)
			{
				_dic.GetValueByIndex(i).Sort(delegate(Avatar lhs, Avatar rhs)
				{
					Type type = lhs.GetType();
					Type type2 = rhs.GetType();
					if (type != type2)
					{
						if (type == typeof(Avatar))
						{
							return -1;
						}
						if (type2 == typeof(Avatar))
						{
							return 1;
						}
					}
					float num = NVespa.Math.Angle(lhs.CurrentDirection);
					float value = NVespa.Math.Angle(rhs.CurrentDirection);
					return num.CompareTo(value);
				});
			}
		}

		public IList<Avatar> GetListByIndex(int index)
		{
			return _dic.GetValueByIndex(index);
		}
	}

	private MapLayout.AvatarOptionInfo _option;

	private List<IAvatarContainer> _avatarContainerList = new List<IAvatarContainer>();

	private NodeAvatarContainer _avatarByNode = new NodeAvatarContainer();

	public void Open(MapLayout.AvatarOptionInfo option)
	{
		_option = option;
	}

	public void AddAvatarContainer(IAvatarContainer container)
	{
		if (container == null)
		{
			VespaDebug.LogError("Null avatar container.");
		}
		else if (!_avatarContainerList.Contains(container))
		{
			_avatarContainerList.Add(container);
		}
	}

	public void RemoveAvatarContainer(IAvatarContainer container)
	{
		if (container == null)
		{
			VespaDebug.LogError("Null avatar container.");
		}
		else if (!_avatarContainerList.Contains(container))
		{
			_avatarContainerList.Remove(container);
		}
	}

	public void ChainUpdate()
	{
		RearrangeAvatar();
	}

	private void RearrangeAvatar()
	{
		if (_option == null)
		{
			return;
		}
		_avatarByNode.Clear();
		float cellSize = _option.RearrangeRadius * 2f;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_avatarContainerList); i++)
		{
			_avatarContainerList[i].ForEach(delegate(Avatar avatar2)
			{
				if (avatar2 != null && avatar2.CanBeRearranged())
				{
					if (!avatar2.IsMoving(out var destinationIndex))
					{
						destinationIndex = avatar2.GetLastDungeonIndex();
					}
					int cellX = (int)((avatar2.GetLocalPosition().x + cellSize * 0.5f) / cellSize);
					int cellY = (int)((avatar2.GetLocalPosition().y + cellSize * 0.5f) / cellSize);
					NodeKey key = new NodeKey
					{
						DungeonIndex = destinationIndex,
						CellX = cellX,
						CellY = cellY
					};
					_avatarByNode.GetValidList(key).Add(avatar2);
				}
			});
		}
		_avatarByNode.Sort();
		for (int num = 0; num < _avatarByNode.Count; num++)
		{
			IList<Avatar> listByIndex = _avatarByNode.GetListByIndex(num);
			if (__Extension_Method_ICollection__.COUNT(listByIndex) == 0)
			{
				continue;
			}
			if (__Extension_Method_ICollection__.COUNT(listByIndex) == 1)
			{
				listByIndex[0].RearrangeOffset = Vector3.zero;
				continue;
			}
			Vector2 i_v = -listByIndex[0].CurrentDirection;
			for (int num2 = 0; num2 < __Extension_Method_ICollection__.COUNT(listByIndex); num2++)
			{
				Avatar avatar = listByIndex[num2];
				if (avatar != null && avatar is NpcAvatar)
				{
					int num3 = 360 * num2 / __Extension_Method_ICollection__.COUNT(listByIndex);
					Vector2 vector = NVespa.Math.Rotate(i_v, num3) * cellSize;
					vector.y *= _option.RearrangeXyAspect;
					avatar.RearrangeOffset = vector;
				}
			}
		}
	}
}
