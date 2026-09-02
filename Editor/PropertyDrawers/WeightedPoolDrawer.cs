using System;
using Ulys.Runtime.Utilities;

namespace Ulys.Editor.PropertyDrawers
{

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomPropertyDrawer(typeof(WeightedPool<>))]
public class WeightedPoolDrawer : PropertyDrawer
{
	private const int MinimumWeight = 1;
	private const int DefaultWeight = 1;
	
	private const float WeightWidth = 60f;
	private const float PercentageWidth = 70f;

	private const float Spacing = 5f;
	private const float ColumnSpacing = 8f;
	private const float ChanceBarHeight = 18f;
	private const float HandleWidth = 18f;
	private const float ElementVerticalPadding = 4f;
	private const float ComplexHeaderHeight = 18f;
	private const float ComplexFieldSpacing = 2f;

	private const string PoolName = "pool";
	private const string ItemName = "item";
	private const string WeightName = "weight";

	private ReorderableList _list;

	private static readonly GUIStyle ChanceLabelStyle = new(EditorStyles.label)
	{
		alignment = TextAnchor.MiddleCenter,
		fontStyle = FontStyle.Bold
	};

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		SerializedProperty pool = property.FindPropertyRelative(PoolName);

		EditorGUI.BeginProperty(position, label, property);

		// Foldout
		Rect foldoutRect = new Rect(
			position.x,
			position.y,
			position.width,
			EditorGUIUtility.singleLineHeight);

		property.isExpanded = EditorGUI.Foldout(
			foldoutRect,
			property.isExpanded,
			label,
			true);

		EditorGUI.EndProperty();

		if (!property.isExpanded)
			return;

		float y = foldoutRect.yMax + Spacing;

		ClampWeights(pool);

		int totalWeight = GetTotalWeight(pool);

		// Summary
		float summaryHeight = EditorGUIUtility.singleLineHeight * 3f;
		
		Rect summaryRect = new Rect(
			position.x,
			y,
			position.width,
			summaryHeight);
		
		DrawSummary(summaryRect, pool.arraySize, totalWeight);

		y += summaryHeight + Spacing;

		// List
		ReorderableList list = GetList(pool);

		Rect listRect = new Rect(
			position.x,
			y,
			position.width,
			list.GetHeight());

		list.DoList(listRect);

		y += list.GetHeight();

		// Warning
		string warning = GetWarning(pool);

		if (!string.IsNullOrEmpty(warning))
		{
			y += Spacing;

			float warningHeight = EditorGUIUtility.singleLineHeight * 2f;

			Rect warningRect = new Rect(
				position.x,
				y,
				position.width,
				warningHeight);

			EditorGUI.HelpBox(warningRect, warning, MessageType.Warning);
		}
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		if (!property.isExpanded)
			return EditorGUIUtility.singleLineHeight;

		SerializedProperty pool = property.FindPropertyRelative(PoolName);

		ReorderableList list = GetList(pool);

		float height = (EditorGUIUtility.singleLineHeight + Spacing) * 4f + list.GetHeight();

		string warning = GetWarning(pool);

		if (!string.IsNullOrEmpty(warning))
			height += Spacing + EditorGUIUtility.singleLineHeight * 2f;

		return height;
	}

	private void DrawSummary(Rect rect, int entryCount, int totalWeight)
	{
		GUIStyle valueStyle = new(EditorStyles.label)
		{
			fontStyle = FontStyle.Bold,
			fontSize = EditorStyles.label.fontSize + 1
		};

		const float indent = 12f;
		float lineHeight = EditorGUIUtility.singleLineHeight;

		Rect lineRect = new Rect(
			rect.x + indent,
			rect.y,
			rect.width - indent,
			lineHeight);

		// Type
		Rect valueRect = EditorGUI.PrefixLabel(
			lineRect,
			new GUIContent("Type"));

		EditorGUI.LabelField(
			valueRect,
			GetPoolTypeName(),
			valueStyle);

		// Entries
		lineRect.y += lineHeight;

		valueRect = EditorGUI.PrefixLabel(
			lineRect,
			new GUIContent("Entries"));

		EditorGUI.LabelField(
			valueRect,
			entryCount.ToString(),
			valueStyle);

		// Total Weight
		lineRect.y += lineHeight;

		valueRect = EditorGUI.PrefixLabel(
			lineRect,
			new GUIContent("Total Weight"));

		EditorGUI.LabelField(
			valueRect,
			totalWeight.ToString(),
			valueStyle);
	}

	private ReorderableList GetList(SerializedProperty pool)
	{
		if (_list != null &&
			_list.serializedProperty.serializedObject == pool.serializedObject &&
			_list.serializedProperty.propertyPath == pool.propertyPath)
			return _list;

		_list = new ReorderableList(
			pool.serializedObject,
			pool,
			true,
			true,
			true,
			true)
		{
			drawHeaderCallback = DrawHeader,
			
			drawElementCallback = (rect, index, _, _) => DrawElement(rect, pool, index),
			
			elementHeightCallback = index =>
			{
				SerializedProperty entry = pool.GetArrayElementAtIndex(index);

				SerializedProperty item = entry.FindPropertyRelative(ItemName);

				if (IsComplexProperty(item))
					return GetComplexItemHeight(item);

				return EditorGUI.GetPropertyHeight(item, true) + ElementVerticalPadding;
			},
			
			onAddCallback = _ => AddEntry(pool),
			
			onRemoveCallback = list =>
			{
				if (list.index < 0 || list.index >= pool.arraySize)
					return;

				pool.DeleteArrayElementAtIndex(list.index);

				list.index = Mathf.Clamp(list.index, 0, pool.arraySize - 1);
			}
		};

		return _list;
	}

	private static void DrawHeader(Rect rect)
	{
		float itemWidth = rect.width - WeightWidth - PercentageWidth - ColumnSpacing * 2f;

		EditorGUI.LabelField(
			new Rect(
				rect.x,
				rect.y,
				itemWidth,
				rect.height),
			"Item");

		EditorGUI.LabelField(
			new Rect(
				rect.x + itemWidth + ColumnSpacing,
				rect.y,
				WeightWidth,
				rect.height),
			"Weight");

		EditorGUI.LabelField(
			new Rect(
				rect.x +
				itemWidth +
				ColumnSpacing +
				WeightWidth +
				ColumnSpacing,
				rect.y,
				PercentageWidth,
				rect.height),
			"Chance");
	}

	private static void DrawElement(Rect rect, SerializedProperty pool, int index)
	{
		SerializedProperty entry = pool.GetArrayElementAtIndex(index);

		SerializedProperty item = entry.FindPropertyRelative(ItemName);

		SerializedProperty weight = entry.FindPropertyRelative(WeightName);

		object oldValue = GetValue(item);

		weight.intValue = Mathf.Max(MinimumWeight, weight.intValue);

		int totalWeight = GetTotalWeight(pool);

		float percentage = totalWeight > 0
			? (float)weight.intValue / totalWeight
			: 0f;

		// --------------------------------------------------
		// Item
		// --------------------------------------------------

		float itemWidth = rect.width - WeightWidth - PercentageWidth - ColumnSpacing * 2f;
		float itemHeight = EditorGUI.GetPropertyHeight(item, true);

		float itemY = rect.y + (rect.height - itemHeight) * 0.5f;

		if (IsComplexProperty(item))
		{
			DrawComplexItem(
				rect,
				item,
				index,
				itemWidth,
				weight,
				percentage);
		}
		else
		{
			DrawSimpleItem(
				rect,
				itemY,
				itemWidth,
				itemHeight,
				item,
				weight,
				percentage);
		}


		// Reject duplicate values.
		if (!SerializedPropertyEqual(item, oldValue) && IsDuplicate(pool, item, index))
			RestoreValue(item, oldValue);
	}

	private static void AddEntry(SerializedProperty pool)
	{
		int index = pool.arraySize;

		pool.InsertArrayElementAtIndex(index);

		SerializedProperty entry = pool.GetArrayElementAtIndex(index);

		SerializedProperty item = entry.FindPropertyRelative(ItemName);

		SerializedProperty weight = entry.FindPropertyRelative(WeightName);

		weight.intValue = DefaultWeight;

		// InsertArrayElementAtIndex copies the previous
		// element, so explicitly clear it first.
		ClearValue(item);

		// Find a unique value for supported value types.
		AssignUnusedValue(pool, item, index);
	}

	private static string GetWarning(SerializedProperty pool)
	{
		for (int i = 0; i < pool.arraySize; i++)
		{
			SerializedProperty item = pool.GetArrayElementAtIndex(i).FindPropertyRelative(ItemName);

			if (item.propertyType == SerializedPropertyType.ObjectReference && item.objectReferenceValue == null)
				return "Every entry must have a value. Null entries cannot be used by the weighted pool.";

			if (item.propertyType == SerializedPropertyType.ManagedReference && item.managedReferenceValue == null)
				return "Every entry must have a value. Null entries cannot be used by the weighted pool.";

			if (item.propertyType == SerializedPropertyType.String && string.IsNullOrWhiteSpace(item.stringValue))
				return "Every entry must have a non-empty string.";
		}

		// Duplicate check.
		for (int i = 0; i < pool.arraySize; i++)
		{
			SerializedProperty item = pool.GetArrayElementAtIndex(i).FindPropertyRelative(ItemName);

			if (IsDuplicate(pool, item, i))
				return "The pool contains duplicate entries. Every item must be unique.";
		}

		return null;
	}

	private static void ClearValue(SerializedProperty property)
	{
		switch (property.propertyType)
		{
		case SerializedPropertyType.ObjectReference:
			property.objectReferenceValue = null;
			break;

		case SerializedPropertyType.ManagedReference:
			property.managedReferenceValue = null;
			break;

		case SerializedPropertyType.Integer:
			property.longValue = 0;
			break;

		case SerializedPropertyType.Float:
			property.doubleValue = 0;
			break;

		case SerializedPropertyType.Boolean:
			property.boolValue = false;
			break;

		case SerializedPropertyType.String:
			property.stringValue = "";
			break;

		case SerializedPropertyType.Enum:
			property.enumValueIndex = 0;
			break;

		case SerializedPropertyType.Vector2:
			property.vector2Value = Vector2.zero;
			break;

		case SerializedPropertyType.Vector3:
			property.vector3Value = Vector3.zero;
			break;

		case SerializedPropertyType.Vector4:
			property.vector4Value = Vector4.zero;
			break;

		case SerializedPropertyType.Color:
			property.colorValue = default;
			break;
		}
	}

	private static void AssignUnusedValue(SerializedProperty pool, SerializedProperty item, int index)
	{
		switch (item.propertyType)
		{
		case SerializedPropertyType.Integer:
		{
			long candidate = 0;

			while (ContainsInteger(pool, candidate, index))
				candidate++;

			item.longValue = candidate;
			break;
		}

		case SerializedPropertyType.Float:
		{
			double candidate = 0;

			while (ContainsFloat(pool, candidate, index))
				candidate++;

			item.doubleValue = candidate;
			break;
		}

		case SerializedPropertyType.Enum:
		{
			for (int candidate = 0; candidate < item.enumNames.Length; candidate++)
			{
				if (!ContainsEnum(pool, candidate, index))
				{
					item.enumValueIndex = candidate;
					return;
				}
			}

			// All enum values are already used.
			pool.DeleteArrayElementAtIndex(index);
			break;
		}
		}
	}

	private static bool IsDuplicate(SerializedProperty pool, SerializedProperty item, int ignoredIndex)
	{
		for (int i = 0; i < pool.arraySize; i++)
		{
			if (i == ignoredIndex)
				continue;

			SerializedProperty other = pool.GetArrayElementAtIndex(i).FindPropertyRelative(ItemName);

			if (SerializedProperty.DataEquals(item, other))
				return true;
		}

		return false;
	}

	private static int GetTotalWeight(SerializedProperty pool)
	{
		int total = 0;

		for (int i = 0; i < pool.arraySize; i++)
		{
			SerializedProperty weight = pool.GetArrayElementAtIndex(i).FindPropertyRelative(WeightName);

			total += Mathf.Max(MinimumWeight, weight.intValue);
		}

		return total;
	}

	private static void ClampWeights(SerializedProperty pool)
	{
		for (int i = 0; i < pool.arraySize; i++)
		{
			SerializedProperty weight = pool.GetArrayElementAtIndex(i).FindPropertyRelative(WeightName);

			weight.intValue = Mathf.Max(MinimumWeight, weight.intValue);
		}
	}

	private static object GetValue(SerializedProperty property)
	{
		return property.propertyType switch
		{
			SerializedPropertyType.Integer => property.longValue,
			SerializedPropertyType.Boolean => property.boolValue,
			SerializedPropertyType.Float => property.doubleValue,
			SerializedPropertyType.String => property.stringValue,
			SerializedPropertyType.ObjectReference => property.objectReferenceValue,
			SerializedPropertyType.Enum => property.enumValueIndex,
			SerializedPropertyType.Vector2 => property.vector2Value,
			SerializedPropertyType.Vector3 => property.vector3Value,
			SerializedPropertyType.Vector4 => property.vector4Value,
			SerializedPropertyType.Color => property.colorValue,
			_ => null
		};
	}

	private static bool SerializedPropertyEqual(SerializedProperty property, object oldValue)
	{
		object newValue = GetValue(property);

		if (oldValue == null || newValue == null)
			return oldValue == newValue;

		return oldValue.Equals(newValue);
	}

	private static void RestoreValue(SerializedProperty property, object value)
	{
		switch (property.propertyType)
		{
		case SerializedPropertyType.Integer:
			property.longValue = (long)value;
			break;

		case SerializedPropertyType.Boolean:
			property.boolValue = (bool)value;
			break;

		case SerializedPropertyType.Float:
			property.doubleValue = (double)value;
			break;

		case SerializedPropertyType.String:
			property.stringValue = (string)value;
			break;

		case SerializedPropertyType.ObjectReference:
			property.objectReferenceValue = (Object)value;
			break;

		case SerializedPropertyType.Enum:
			property.enumValueIndex = (int)value;
			break;

		case SerializedPropertyType.Vector2:
			property.vector2Value = (Vector2)value;
			break;

		case SerializedPropertyType.Vector3:
			property.vector3Value = (Vector3)value;
			break;

		case SerializedPropertyType.Vector4:
			property.vector4Value = (Vector4)value;
			break;

		case SerializedPropertyType.Color:
			property.colorValue = (Color)value;
			break;
		}
		
		property.serializedObject.ApplyModifiedProperties();
	}

	private static bool ContainsInteger(SerializedProperty pool, long value, int ignoredIndex)
	{
		for (int i = 0; i < pool.arraySize; i++)
		{
			if (i == ignoredIndex)
				continue;

			SerializedProperty item = pool.GetArrayElementAtIndex(i).FindPropertyRelative(ItemName);

			if (item.propertyType == SerializedPropertyType.Integer && item.longValue == value)
				return true;
		}

		return false;
	}

	private static bool ContainsFloat(SerializedProperty pool, double value, int ignoredIndex)
	{
		for (int i = 0; i < pool.arraySize; i++)
		{
			if (i == ignoredIndex)
				continue;

			SerializedProperty item = pool.GetArrayElementAtIndex(i).FindPropertyRelative(ItemName);

			if (item.propertyType == SerializedPropertyType.Float && Math.Abs(item.doubleValue - value) < Mathf.Epsilon)
				return true;
		}

		return false;
	}

	private static bool ContainsEnum(SerializedProperty pool, int value, int ignoredIndex)
	{
		for (int i = 0; i < pool.arraySize; i++)
		{
			if (i == ignoredIndex)
				continue;

			SerializedProperty item = pool.GetArrayElementAtIndex(i).FindPropertyRelative(ItemName);

			if (item.propertyType == SerializedPropertyType.Enum && item.enumValueIndex == value)
				return true;
		}

		return false;
	}

	private static void DrawChanceBar(Rect rect, float percentage)
	{
		percentage = Mathf.Clamp01(percentage);

		Rect barRect = new Rect(
			rect.x,
			rect.y + (rect.height - ChanceBarHeight) * 0.5f,
			rect.width,
			ChanceBarHeight);

		// Background
		EditorGUI.DrawRect(
			barRect,
			EditorGUIUtility.isProSkin
				? new Color(0.12f, 0.12f, 0.12f)
				: new Color(0.8f, 0.8f, 0.8f));

		// Filled portion
		if (percentage > 0f)
		{
			EditorGUI.DrawRect(
				new Rect(
					barRect.x,
					barRect.y,
					barRect.width * percentage,
					barRect.height),
				new(0.18f, 0.25f, 0.5f));
		}

		// Outline
		Handles.BeginGUI();

		Color previousColor = Handles.color;

		Handles.color = EditorGUIUtility.isProSkin
			? new Color(0.65f, 0.65f, 0.65f)
			: new Color(0.25f, 0.25f, 0.25f);

		Handles.DrawAAPolyLine(
			1f,
			new Vector3(barRect.x, barRect.y),
			new Vector3(barRect.xMax, barRect.y),
			new Vector3(barRect.xMax, barRect.yMax),
			new Vector3(barRect.x, barRect.yMax),
			new Vector3(barRect.x, barRect.y));

		Handles.color = previousColor;

		Handles.EndGUI();

		// Percentage text
		EditorGUI.LabelField(barRect, percentage.ToString("P2"), ChanceLabelStyle);
	}

	private static string GetTypeName(SerializedProperty property)
	{
		if (property.propertyType == SerializedPropertyType.ManagedReference)
		{
			if (property.managedReferenceValue != null)
				return property.managedReferenceValue.GetType().Name;

			return "Null";
		}

		if (property.propertyType == SerializedPropertyType.Generic)
		{
			string typeName = property.type;

			// Unity sometimes gives names such as:
			// "MyNamespace.MyStruct"
			int lastDot = typeName.LastIndexOf('.');

			if (lastDot >= 0)
				typeName = typeName[(lastDot + 1)..];

			return typeName;
		}

		return property.displayName;
	}
	
	private string GetPoolTypeName()
	{
		if (fieldInfo == null)
			return "Unknown";

		Type poolType = fieldInfo.FieldType;

		if (!poolType.IsGenericType)
			return poolType.Name;

		Type itemType = poolType.GetGenericArguments()[0];

		return itemType.Name;
	}

	private static bool IsComplexProperty(SerializedProperty property)
	{
		return property.propertyType switch
		{
			SerializedPropertyType.Generic => true,
			SerializedPropertyType.ManagedReference => true,
			_ => false
		};
	}

	private static float GetComplexItemHeight(SerializedProperty item)
	{
		float height = ComplexHeaderHeight + ElementVerticalPadding;

		if (!item.isExpanded)
			return height;

		height += ComplexFieldSpacing;
		
		SerializedProperty child = item.Copy();
		SerializedProperty end = child.GetEndProperty();

		bool enterChildren = true;

		while (child.NextVisible(enterChildren) && !SerializedProperty.EqualContents(child, end))
		{
			enterChildren = false;

			height += EditorGUI.GetPropertyHeight(child, true);

			height += EditorGUIUtility.standardVerticalSpacing;
		}

		return height + ElementVerticalPadding;
	}
	
	private static void DrawSimpleItem(
		Rect rect,
		float itemY,
		float itemWidth,
		float itemHeight,
		SerializedProperty item,
		SerializedProperty weight,
		float percentage)
	{
		EditorGUI.PropertyField(
			new Rect(
				rect.x,
				itemY,
				itemWidth,
				itemHeight),
			item,
			GUIContent.none,
			true);

		// Weight
		EditorGUI.PropertyField(
			new Rect(
				rect.x +
				itemWidth +
				ColumnSpacing,
				itemY,
				WeightWidth,
				EditorGUIUtility.singleLineHeight),
			weight,
			GUIContent.none);

		// Chance
		DrawChanceBar(
			new Rect(
				rect.x +
				itemWidth +
				ColumnSpacing +
				WeightWidth +
				ColumnSpacing,
				itemY,
				PercentageWidth,
				EditorGUIUtility.singleLineHeight),
			percentage);
	}

	private static void DrawComplexItem(
		Rect rect,
		SerializedProperty item,
		int index,
		float itemWidth,
		SerializedProperty weight,
		float percentage)
	{
		// The handle occupies the leftmost part of the element.
		float contentX = rect.x + HandleWidth;

		// Complex fields can use the entire width of the element.
		float contentWidth = rect.width - HandleWidth;

		// --------------------------------------------------
		// Top row
		// --------------------------------------------------

		Rect headerRect = new Rect(
			contentX,
			rect.y + 1f,
			itemWidth - HandleWidth,
			ComplexHeaderHeight);
		
		item.isExpanded = EditorGUI.Foldout(
			headerRect,
			item.isExpanded,
			$"Element {index} ({GetTypeName(item)})",
			true);

		// Weight
		float weightX = rect.x + itemWidth + ColumnSpacing;

		EditorGUI.PropertyField(
			new Rect(
				weightX,
				rect.y + 1f,
				WeightWidth,
				EditorGUIUtility.singleLineHeight),
			weight,
			GUIContent.none);

		// Chance
		float chanceX = weightX + WeightWidth + ColumnSpacing;

		DrawChanceBar(
			new Rect(
				chanceX,
				rect.y + 1f,
				PercentageWidth,
				EditorGUIUtility.singleLineHeight),
			percentage);

		// --------------------------------------------------
		// Children
		// --------------------------------------------------

		if (!item.isExpanded)
			return;
		
		float y = rect.y + 1f + ComplexHeaderHeight + ComplexFieldSpacing;

		SerializedProperty child = item.Copy();
		SerializedProperty end = child.GetEndProperty();

		bool enterChildren = true;

		while (child.NextVisible(enterChildren) && !SerializedProperty.EqualContents(child, end))
		{
			enterChildren = false;

			float height = EditorGUI.GetPropertyHeight(child, true);

			EditorGUI.PropertyField(
				new Rect(
					contentX,
					y,
					contentWidth,
					height),
				child,
				true);

			y += height + EditorGUIUtility.standardVerticalSpacing;
		}
	}
}

}
