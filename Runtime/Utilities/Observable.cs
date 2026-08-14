using System;

namespace Ulys.Runtime.Utilities
{

public class Observable<T>
{
	private T _value;
	
	public event Action<T> OnValueSet;
	public event Action<T> OnValueGet;

	public Observable(T value)
	{
		_value = value;
	}
	
	public T value
	{
		get
		{
			OnValueGet?.Invoke(_value);
			return _value;
		}
		set
		{
			OnValueSet?.Invoke(value);
			_value = value;
		}
	}
}

}
