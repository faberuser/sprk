using System;
using System.Reflection;
using MessagePack;
using NVespa;

namespace NShared;

[MessagePackObject(false)]
public class EquipFilterOptionGroupData
{
	private Type _dataType;

	private Type _propertyType;

	private PropertyInfo _propertyInfo;

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public string Key { get; set; }

	[Key(2)]
	public int Step { get; set; }

	[Key(3)]
	public string LocalKey { get; set; }

	[Key(4)]
	public bool Show { get; set; }

	[Key(5)]
	public string FieldTypeName { get; set; }

	[Key(6)]
	public string FieldName { get; set; }

	[Key(7)]
	public string DataTypeName { get; set; }

	[IgnoreMember]
	public Type DataType => _dataType;

	[IgnoreMember]
	public Type PropertyType => _propertyType;

	[IgnoreMember]
	public PropertyInfo PropertyInfo => _propertyInfo;

	public override string ToString()
	{
		return $"Index={Index.ToString()}, Group = {Key}";
	}

	public void InitTypeData()
	{
		try
		{
			if (string.IsNullOrEmpty(FieldName))
			{
				return;
			}
			_dataType = Type.GetType(DataTypeName);
			if (!(DataType == null))
			{
				_propertyInfo = DataType.GetProperty(FieldName);
				if (!(PropertyInfo == null))
				{
					_propertyType = _propertyInfo.PropertyType;
				}
			}
		}
		catch (Exception ex)
		{
			VespaDebug.LogError($"EquipFilterOptionGroupData::InitTypeData - (errorMsg:{ex.Message})");
		}
	}

	public static int GetFilterType(EquipFilterOptionData optionData)
	{
		if (optionData == null)
		{
			return -1;
		}
		if (optionData.ReferenceValue == null)
		{
			return -1;
		}
		return GetFilterType(GameRoot.Instance.EquipFilterOptionGroupDataContainer.GetData(optionData.Group), optionData.ReferenceValue);
	}

	public static int GetFilterType(EquipFilterOptionGroupData gData, object data)
	{
		string dataStr = "";
		if (data == null)
		{
			return -1;
		}
		try
		{
			Type type = data.GetType();
			if (gData.PropertyType == typeof(Array))
			{
				type = type.GetElementType();
			}
			if (type.BaseType == typeof(Enum))
			{
				dataStr = Enum.GetName(type, data);
			}
			else
			{
				if (type == typeof(long))
				{
					return Convert.ToInt32(data);
				}
				if (type == typeof(int))
				{
					return Convert.ToInt32(data);
				}
				if (type == typeof(short))
				{
					return Convert.ToInt32(data);
				}
				if (type == typeof(byte))
				{
					return Convert.ToInt32(data);
				}
				dataStr = ((!(type == typeof(bool))) ? data.ToString() : ((bool)data).ToString());
			}
		}
		catch (Exception ex)
		{
			VespaDebug.LogError($"EquipFilterOptionComponent::GetFilterType - (errorMsg:{ex.Message}, _data.ReferenceValue:{data.GetType().ToString()})");
		}
		return GetFilterType(gData, dataStr);
	}

	public static int GetFilterType(EquipFilterOptionGroupData gData, string dataStr)
	{
		if (gData == null)
		{
			return -1;
		}
		if (dataStr == null)
		{
			return -1;
		}
		try
		{
			Type type = gData.PropertyType;
			if (gData.PropertyType.HasElementType)
			{
				type = gData.PropertyType.GetElementType();
			}
			if (type.BaseType == typeof(Enum))
			{
				return (int)Enum.Parse(type, dataStr);
			}
			if (type == typeof(long))
			{
				return int.Parse(dataStr);
			}
			if (type == typeof(int))
			{
				return int.Parse(dataStr);
			}
			if (type == typeof(short))
			{
				return int.Parse(dataStr);
			}
			if (type == typeof(byte))
			{
				return int.Parse(dataStr);
			}
			if (type == typeof(bool))
			{
				return Convert.ToInt32(bool.Parse(dataStr));
			}
			return (int)Convert.ChangeType(dataStr, type);
		}
		catch (Exception ex)
		{
			VespaDebug.LogError($"EquipFilterOptionComponent::GetFilterType - (errorMsg:{ex.Message}, _data.ReferenceValue:{dataStr})");
			return -1;
		}
	}
}
