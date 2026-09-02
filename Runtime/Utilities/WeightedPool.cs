using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ulys.Runtime.Utilities
{

[Serializable]
public class WeightedPool<T> : IEnumerable<T>, ISerializationCallbackReceiver
{
	[SerializeField] private List<WeightedEntry> pool;
	[NonSerialized] private Dictionary<T, int> _lookup;
	[NonSerialized] private int[] _cumulativeWeights;

	public WeightedPool()
	{
		pool = new();
		_lookup = new();
	}

	public WeightedPool(int capacity)
	{
		pool = new(capacity);
		_lookup = new(capacity);
	}
	
	public WeightedPool(IEnumerable<(T item, int weight)> weightedEntries)
	{
		pool = new();
		_lookup = new();

		AddRange(weightedEntries);
	}
	
	private const string NullItemMessage = "Item must be non-null.";
	private const string InvalidWeightMessage = "Weight must be greater than 0.";
	
	/// <summary> Gets the number of items contained in the pool. </summary>
	public int Count => pool.Count;
	
	/// <summary> Gets the sum of the weights of all items in the pool. </summary>
	public int TotalWeight { get; private set; }

	/// <summary> Removes all items from the pool and resets its total weight. </summary>
	public void Clear()
	{
		pool.Clear();
		_lookup.Clear();
		TotalWeight = 0;
	}
	
	/// <summary>Determines whether the specified item is contained in the pool.</summary>
	/// <param name="item">The item to search for.</param>
	/// <returns><c>true</c> if the item is contained in the pool; otherwise, <c>false</c>.</returns>
	public bool Contains(T item)
	{
		return item != null && _lookup.ContainsKey(item);
	}
	
	/// <summary>Attempts to retrieve the weight associated with the specified item.</summary>
	/// <param name="item">The item whose weight to retrieve.</param>
	/// <param name="weight">When this method returns, contains the item's weight if found; otherwise, 0.</param>
	/// <returns><c>true</c> if the item was found; otherwise, <c>false</c>.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="item"/> is <c>null</c>.</exception>
	public bool TryGetWeight(T item, out int weight)
	{
		weight = 0;
		
		if (item == null)
			throw new ArgumentNullException(nameof(item), NullItemMessage);
		
		if (_lookup.TryGetValue(item, out int index))
		{
			weight = pool[index].weight;
			return true;
		}

		return false;
	}

	/// <summary>Sets the weight of an existing item.</summary>
	/// <param name="item">The item whose weight to change.</param>
	/// <param name="weight">The new weight. Must be greater than 0.</param>
	/// <returns><c>true</c> if the item was found and its weight was updated; otherwise, <c>false</c>.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="item"/> is <c>null</c>.</exception>
	/// <exception cref="ArgumentException">Thrown if <paramref name="weight"/> is less than or equal to 0.</exception>
	public bool SetWeight(T item, int weight)
	{
		if (item == null)
			throw new ArgumentNullException(nameof(item), NullItemMessage);
		
		if (weight <= 0)
			throw new ArgumentException(InvalidWeightMessage, nameof(weight));
		
		if (_lookup.TryGetValue(item, out int index))
		{
			WeightedEntry weightedEntry = pool[index];
			weightedEntry.weight = weight;
			pool[index] = weightedEntry;
			Rebuild();
			return true;
		}
		
		return false;
	}

	/// <summary>Attempts to retrieve the probability of an item being selected.</summary>
	/// <param name="item">The item whose probability to retrieve.</param>
	/// <param name="probability">When this method returns, contains the item's probability as a value between 0 and 1 if found; otherwise, 0.</param>
	/// <returns><c>true</c> if the item was found; otherwise, <c>false</c>.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="item"/> is <c>null</c>.</exception>
	public bool TryGetProbability(T item, out float probability)
	{
		probability = 0f;
		
		if (item == null)
			throw new ArgumentNullException(nameof(item), NullItemMessage);
		
		if (TotalWeight == 0)
			return false;
		
		if (_lookup.TryGetValue(item, out int index))
		{
			probability = (float)pool[index].weight / TotalWeight;
			return true;
		}

		return false;
	}

	/// <summary>Adds an item to the pool with the specified weight.</summary>
	/// <param name="item">The item to add.</param>
	/// <param name="weight">The item's weight. Defaults to 1.</param>
	/// <returns><c>true</c> if the item was added; <c>false</c> if the item was already contained in the pool.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="item"/> is <c>null</c>.</exception>
	/// <exception cref="ArgumentException">Thrown if <paramref name="weight"/> is less than or equal to 0.</exception>
	public bool Add(T item, int weight = 1)
	{
		if (item == null)
			throw new ArgumentNullException(nameof(item), NullItemMessage);
		
		if (weight <= 0)
			throw new ArgumentException(InvalidWeightMessage, nameof(weight));

		if (_lookup.TryAdd(item, pool.Count))
		{
			pool.Add(new(item, weight));
			Rebuild();
			return true;
		}
		
		return false;
	}
	
	/// <summary>Adds an item to the pool or updates its weight if it already exists.</summary>
	/// <param name="item">The item to add or update.</param>
	/// <param name="weight">The item's weight. Defaults to 1.</param>
	/// <returns><c>true</c> after successfully adding or updating the item.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="item"/> is <c>null</c>.</exception>
	/// <exception cref="ArgumentException">Thrown if <paramref name="weight"/> is less than or equal to 0.</exception>
	public bool AddOrUpdate(T item, int weight = 1)
	{
		if (item == null)
			throw new ArgumentNullException(nameof(item), NullItemMessage);
		
		if (weight <= 0)
			throw new ArgumentException(InvalidWeightMessage, nameof(weight));

		if (_lookup.TryGetValue(item, out int index))
		{
			WeightedEntry weightedEntry = pool[index];
			weightedEntry.weight = weight;
			pool[index] = weightedEntry;
		}
		else
		{
			_lookup.Add(item, pool.Count);
			pool.Add(new(item, weight));
		}
		
		Rebuild();
		return true;
	}
	
	/// <summary>
	/// Adds multiple weighted items to the pool.
	/// Duplicate items are ignored. The pool's internal weight data is rebuilt once after all items have been added.
	/// </summary>
	/// <param name="weightedEntries">The items and their corresponding weights to add.</param>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="weightedEntries"/> is <c>null</c> /// or if an entry contains a <c>null</c> item.</exception>
	/// <exception cref="ArgumentException">Thrown if an entry has a weight less than or equal to 0.</exception>
	public void AddRange(IEnumerable<(T item, int weight)> weightedEntries)
	{
		if (weightedEntries == null)
			throw new ArgumentNullException(nameof(weightedEntries));

		if (weightedEntries is IReadOnlyCollection<(T item, int weight)> collection)
		{
			pool.Capacity = Mathf.Max(pool.Capacity, pool.Count + collection.Count);
			_lookup.EnsureCapacity(_lookup.Count + collection.Count);
		}

		bool needRebuild = false;

		foreach ((T item, int weight) in weightedEntries)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item), NullItemMessage);

			if (weight <= 0)
				throw new ArgumentException(InvalidWeightMessage, nameof(weight));

			if (!_lookup.TryAdd(item, pool.Count))
				continue;

			pool.Add(new(item, weight));
			needRebuild = true;
		}

		if (needRebuild)
			Rebuild();
	}

	/// <summary>Removes an item from the pool.</summary>
	/// <param name="item">The item to remove.</param>
	/// <returns><c>true</c> if the item was removed; otherwise, <c>false</c>.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="item"/> is <c>null</c>.</exception>
	public bool Remove(T item)
	{
		if (item == null)
			throw new ArgumentNullException(nameof(item), NullItemMessage);
		
		if (_lookup.TryGetValue(item, out int index))
		{
			int lastIndex = pool.Count - 1;
			if (index != lastIndex)
			{
				T itemToSwap = pool[lastIndex].item;
				_lookup[itemToSwap] = index;
				
				(pool[index], pool[lastIndex]) = (pool[lastIndex], pool[index]);
			}
			
			pool.RemoveAt(lastIndex);
			_lookup.Remove(item);
			
			Rebuild();
			return true;
		}

		return false;
	}

	/// <summary>Returns a randomly selected item using the item's weight as its selection probability.</summary>
	/// <returns>A randomly selected item from the pool.</returns>
	/// <exception cref="InvalidOperationException">Thrown if the pool is empty.</exception>
	public T GetRandomItem()
	{
		if (pool.Count == 0)
			throw new InvalidOperationException("Cannot get random item from an empty pool.");
		
		int random = UnityEngine.Random.Range(0, TotalWeight);
		
		return pool[FindWeightedIndex(random)].item;
	}

	/// <summary>Returns the item corresponding to a position in the pool's weighted range.</summary>
	/// <param name="weightIndex"> A value between 0 inclusive and <see cref="TotalWeight"/> exclusive.
	/// Each item occupies a number of positions equal to its weight.</param>
	/// <returns>The item corresponding to the specified position in the weighted range.</returns>
	/// <exception cref="InvalidOperationException">Thrown if the pool is empty.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="weightIndex"/> is outside the range from 0 inclusive to <see cref="TotalWeight"/> exclusive.</exception>
	public T GetRandomItem(int weightIndex)
	{
		if (pool.Count == 0)
			throw new InvalidOperationException("Cannot get random item from an empty pool.");

		if (weightIndex < 0 || weightIndex >= TotalWeight)
			throw new ArgumentOutOfRangeException(nameof(weightIndex), weightIndex, "Weight index is out of range.");
		
		return pool[FindWeightedIndex(weightIndex)].item;
	}

	private int FindWeightedIndex(int value)
	{
		int low = 0;
		int high = _cumulativeWeights.Length - 1;

		while (low < high)
		{
			int mid = low + (high - low) / 2;

			if (value < _cumulativeWeights[mid])
				high = mid;
			else
				low = mid + 1;
		}

		return low;
	}
	
	/// <summary>Returns an enumerator that iterates through the items in the pool.</summary>
	/// <returns>An enumerator that can be used to iterate through the items in the pool.</returns>
	public IEnumerator<T> GetEnumerator()
	{
		foreach (WeightedEntry entry in pool)
			yield return entry.item;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	private void Rebuild()
	{
		if (_cumulativeWeights == null || _cumulativeWeights.Length != pool.Count)
			_cumulativeWeights = new int[pool.Count];
		
		TotalWeight = 0;
		for (int i = 0; i < pool.Count; i++)
		{
			int weight = pool[i].weight;
			
			if (TotalWeight > int.MaxValue - weight)
				throw new OverflowException("The total weight exceeds Int32.MaxValue.");
			
			TotalWeight += weight;
			_cumulativeWeights[i] = TotalWeight;
		}
	}

	private void RebuildLookup()
	{
		_lookup ??= new(pool.Capacity);
		_lookup.Clear();

		for (int i = 0; i < pool.Count; i++)
		{
			T item = pool[i].item;
			
			if (item == null)
				continue;

			_lookup.TryAdd(item, i);
		}
	}
	
	public void OnBeforeSerialize()
	{
		//Debug.Log("OnBeforeSerialize !");
	}

	public void OnAfterDeserialize()
	{
		//Debug.Log("OnAfterDeserialize !");
		pool ??= new();
		Rebuild();
		RebuildLookup();
	}
	
	[Serializable]
	private struct WeightedEntry
	{
		public T item;
		public int weight;

		public WeightedEntry(T item, int weight)
		{
			this.item = item;
			this.weight = weight;
		}
	}
}


}
