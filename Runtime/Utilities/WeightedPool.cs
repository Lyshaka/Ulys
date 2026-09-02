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
	
	private const string NullItemMessage = "Item must be non-null.";
	private const string InvalidWeightMessage = "Weight must be greater than 0.";
	
	public int Count => pool.Count;
	public int TotalWeight { get; private set; }

	public void Clear()
	{
		pool.Clear();
		_lookup.Clear();
		Rebuild();
	}

	public bool Contains(T item)
	{
		return item != null && _lookup.ContainsKey(item);
	}

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

	/// <summary>
	/// Returns a random weighted item from the pool, using <see cref="UnityEngine.Random"/>.
	/// </summary>
	/// <returns>Returns a random weighted item from the pool.</returns>
	/// <exception cref="InvalidOperationException">Thrown if the pool is empty.</exception>
	public T GetRandomItem()
	{
		if (pool.Count == 0)
			throw new InvalidOperationException("Cannot get random item from an empty pool.");
		
		int random = UnityEngine.Random.Range(0, TotalWeight);
		
		return pool[FindWeightedIndex(random)].item;
	}

	/// <summary>
	/// Returns a random weighted item from the pool given a random index between 0 inclusive and TotalWeight exclusive.
	/// </summary>
	/// <param name="randomIndex">A random index between 0 inclusive and TotalWeight exclusive.</param>
	/// <returns>Returns a random item from the weighted pool.</returns>
	/// <exception cref="InvalidOperationException">Thrown if the pool is empty.</exception>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if the given index is out of range.</exception>
	public T GetRandomItem(int randomIndex)
	{
		if (pool.Count == 0)
			throw new InvalidOperationException("Cannot get random item from an empty pool.");

		if (randomIndex < 0 || randomIndex >= TotalWeight)
			throw new ArgumentOutOfRangeException(nameof(randomIndex), randomIndex, "Random index is out of range.");
		
		return pool[FindWeightedIndex(randomIndex)].item;
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
		_cumulativeWeights = new int[pool.Count];
		for (int i = 0; i < pool.Count; i++)
		{
			TotalWeight += pool[i].weight;
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
	public struct WeightedEntry
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
