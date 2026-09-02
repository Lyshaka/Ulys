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

	private const float Spacing = 5f;
	private const float ColumnSpacing = 8f;
	private const float ChanceBarHeight = 18f;
	private const float HandleWidth = 18f;
	private const float ElementVerticalPadding = 4f;

	private ReorderableList _list;

	private static readonly GUIStyle ChanceLabelStyle = new(EditorStyles.label)
	{
		alignment = TextAnchor.MiddleCenter,
		fontStyle = FontStyle.Bold
	};

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		SerializedProperty pool = property.FindPropertyRelative("pool");

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

		EditorGUI.EndProperty(); // TODO

		if (!property.isExpanded)
			return;

		float y = foldoutRect.yMax + Spacing;

		ClampWeights(pool);

		int totalWeight = GetTotalWeight(pool);

		// Summary
		Rect summaryRect = new Rect(
			position.x,
			y,
			position.width,
			EditorGUIUtility.singleLineHeight);

		DrawSummary(summaryRect, pool.arraySize, totalWeight);

		y += EditorGUIUtility.singleLineHeight + Spacing;

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

		SerializedProperty pool = property.FindPropertyRelative("pool");

		ReorderableList list = GetList(pool);

		float height = EditorGUIUtility.singleLineHeight +
					   Spacing +
					   EditorGUIUtility.singleLineHeight +
					   Spacing +
					   list.GetHeight();

		string warning = GetWarning(pool);

		if (!string.IsNullOrEmpty(warning))
			height += Spacing + EditorGUIUtility.singleLineHeight * 2f;

		return height;
	}

	private void DrawSummary(Rect rect, int entryCount, int totalWeight)
	{
		GUIStyle style = new GUIStyle(EditorStyles.label)
		{
			fontStyle = FontStyle.Bold,
			fontSize = EditorStyles.label.fontSize + 1
		};

		const float entriesWidth = 100f;

		EditorGUI.LabelField(
			new Rect(
				rect.x,
				rect.y,
				entriesWidth,
				rect.height),
			$"Entries: {entryCount}",
			style);

		EditorGUI.LabelField(
			new Rect(
				rect.x + entriesWidth,
				rect.y,
				150f,
				rect.height),
			$"Total Weight: {totalWeight}",
			style);
	}

	private ReorderableList GetList(SerializedProperty pool)
	{
		if (_list != null
			&& _list.serializedProperty.serializedObject == pool.serializedObject
			&& _list.serializedProperty.propertyPath == pool.propertyPath)
			return _list;

		_list = new ReorderableList(
			pool.serializedObject,
			pool,
			true,
			true,
			true,
			true);
		
		_list.drawHeaderCallback = DrawHeader;

		_list.drawElementCallback = (rect, index, _, _) => DrawElement(rect, pool, index);

		_list.elementHeightCallback = index =>
		{
			SerializedProperty entry = pool.GetArrayElementAtIndex(index);

			SerializedProperty item = entry.FindPropertyRelative("item");

			return EditorGUI.GetPropertyHeight(item, true) + ElementVerticalPadding;
		};

		_list.onAddCallback = _ => AddEntry(pool);

		_list.onRemoveCallback = list =>
		{
			if (list.index >= 0 && list.index < pool.arraySize)
			{
				pool.DeleteArrayElementAtIndex(list.index);

				list.index = Mathf.Clamp(list.index, 0, pool.arraySize - 1);
			}
		};

		return _list;
	}

	private void DrawHeader(Rect rect)
	{
		const float weightWidth = 60f;
		const float percentageWidth = 70f;

		float itemWidth =
			rect.width -
			weightWidth -
			percentageWidth -
			ColumnSpacing * 2f;

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
				weightWidth,
				rect.height),
			"Weight");

		EditorGUI.LabelField(
			new Rect(
				rect.x +
				itemWidth +
				ColumnSpacing +
				weightWidth +
				ColumnSpacing,
				rect.y,
				percentageWidth,
				rect.height),
			"Chance");
	}

	private void DrawElement(Rect rect, SerializedProperty pool, int index)
	{
		SerializedProperty entry = pool.GetArrayElementAtIndex(index);

		SerializedProperty item = entry.FindPropertyRelative("item");

		SerializedProperty weight = entry.FindPropertyRelative("weight");

		object oldValue = GetValue(item);

		weight.intValue = Mathf.Max(MinimumWeight, weight.intValue);

		int totalWeight = GetTotalWeight(pool);

		float percentage = totalWeight > 0
			? (float)weight.intValue / totalWeight
			: 0f;

		// --------------------------------------------------
		// Item
		// --------------------------------------------------
		
		const float weightWidth = 60f;
		const float percentageWidth = 70f;

		float itemWidth = rect.width - weightWidth - percentageWidth - ColumnSpacing * 2f;
		float itemHeight = EditorGUI.GetPropertyHeight(item, true);

		bool complex = IsComplexProperty(item);

		float itemX = complex
			? rect.x + HandleWidth
			: rect.x;
		
		float itemY = rect.y + (rect.height - itemHeight) * 0.5f;

		float adjustedItemWidth = complex
			? itemWidth - HandleWidth
			: itemWidth;

		EditorGUI.PropertyField(
			new Rect(
				itemX,
				itemY,
				adjustedItemWidth,
				rect.height - 2f),
			item,
			GetItemLabel(item),
			true);

		// Reject duplicate values.
		if (!SerializedPropertyEqual(item, oldValue) && IsDuplicate(pool, item, index))
			RestoreValue(item, oldValue);

		// --------------------------------------------------
		// Weight
		// --------------------------------------------------

		EditorGUI.PropertyField(
			new Rect(
				rect.x +
				itemWidth +
				ColumnSpacing,
				itemY,
				weightWidth,
				EditorGUIUtility.singleLineHeight),
			weight,
			GUIContent.none);

		weight.intValue = Mathf.Max(MinimumWeight, weight.intValue);

		// --------------------------------------------------
		// Chance
		// --------------------------------------------------

		Rect chanceRect = new Rect(
			rect.x +
			itemWidth +
			ColumnSpacing +
			weightWidth +
			ColumnSpacing,
			itemY,
			percentageWidth,
			EditorGUIUtility.singleLineHeight);

		DrawChanceBar(chanceRect, percentage);
	}

	private void AddEntry(SerializedProperty pool)
	{
		// Don't allow adding if an existing entry
		// is invalid.
		if (HasInvalidEntry(pool))
			return;

		int index = pool.arraySize;

		pool.InsertArrayElementAtIndex(index);

		SerializedProperty entry = pool.GetArrayElementAtIndex(index);

		SerializedProperty item = entry.FindPropertyRelative("item");

		SerializedProperty weight = entry.FindPropertyRelative("weight");

		weight.intValue = DefaultWeight;

		// InsertArrayElementAtIndex copies the previous
		// element, so explicitly clear it first.
		ClearValue(item);

		// Find a unique value for supported value types.
		AssignUnusedValue(pool, item, index);
	}

	private static bool HasInvalidEntry(SerializedProperty pool)
	{
		for (int i = 0; i < pool.arraySize; i++)
		{
			SerializedProperty item = pool.GetArrayElementAtIndex(i).FindPropertyRelative("item");

			if (IsInvalid(item))
				return true;
		}

		return false;
	}

	private static bool IsInvalid(SerializedProperty property)
	{
		switch (property.propertyType)
		{
		case SerializedPropertyType.ObjectReference:
			return property.objectReferenceValue == null;

		case SerializedPropertyType.ManagedReference:
			return property.managedReferenceValue == null;

		case SerializedPropertyType.String:
			return string.IsNullOrWhiteSpace(property.stringValue);

		default:
			return false;
		}
	}

	private static string GetWarning(SerializedProperty pool)
	{
		for (int i = 0; i < pool.arraySize; i++)
		{
			SerializedProperty item = pool.GetArrayElementAtIndex(i).FindPropertyRelative("item");

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
			SerializedProperty item = pool.GetArrayElementAtIndex(i).FindPropertyRelative("item");

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

			SerializedProperty other = pool.GetArrayElementAtIndex(i).FindPropertyRelative("item");

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
			SerializedProperty weight = pool.GetArrayElementAtIndex(i).FindPropertyRelative("weight");

			total += Mathf.Max(MinimumWeight, weight.intValue);
		}

		return total;
	}

	private static void ClampWeights(SerializedProperty pool)
	{
		for (int i = 0; i < pool.arraySize; i++)
		{
			SerializedProperty weight = pool.GetArrayElementAtIndex(i).FindPropertyRelative("weight");

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
	}

	private static bool ContainsInteger(SerializedProperty pool, long value, int ignoredIndex)
	{
		for (int i = 0; i < pool.arraySize; i++)
		{
			if (i == ignoredIndex)
				continue;

			SerializedProperty item = pool.GetArrayElementAtIndex(i).FindPropertyRelative("item");

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

			SerializedProperty item = pool.GetArrayElementAtIndex(i).FindPropertyRelative("item");

			if (item.propertyType == SerializedPropertyType.Float && item.doubleValue == value)
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

			SerializedProperty item = pool.GetArrayElementAtIndex(i).FindPropertyRelative("item");

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

	private static GUIContent GetItemLabel(SerializedProperty property)
	{
		if (property.propertyType == SerializedPropertyType.Generic ||
			property.propertyType == SerializedPropertyType.ManagedReference)
		{
			return new(GetTypeName(property));
		}

		return GUIContent.none;
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
}

}
