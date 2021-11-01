// Object pool: allows creation of an object pool for memory management when creating/disposing large numbers of objects.
// Needed in C# as otherwise the garbage collector can slow the game down esp when handling large numbers of objects.
// Use this when we expect many of the same object, e.g. used for many powerups in Pong game (although not strictly needed 
// as still few objects).

// When we release an object into the pool, if the pool is not large enough, instead of Freeing the object we add it to the pool and hide it.
// When we get an object from the pool, if the pool contains an object we just show the object and take it out of the pool. Otherwise,
// we create a new object (hence the ObjectScn member variable) and add it to the parent node (specified as arg1).

// Example usage (e.g. from a World class):
// 
// Create the powerUpPool object pool:
// private ObjectPool powerUpPool = new ObjectPool(powerUpScn, typeof(PowerUp), 20);
// 
// Get a powerUp object from the pool:
// PowerUp powerUp = (PowerUp)powerUpPool.Get(this);
//
// Release a powerUp object into the pool:
// powerUpPool.Release(powerUp);

// References:
// https://www.infoworld.com/article/3221392/c-sharp/how-to-use-the-object-pool-design-pattern-in-c.html

using System;
using System.Collections.Concurrent;
using Godot;

public class ObjectPool
{

	private ConcurrentBag<IPoolable> items = new ConcurrentBag<IPoolable>();
	private PackedScene ObjectScn;

	private int Max;

	public ObjectPool(PackedScene objectScn, Type type, int max = 10)
	{
		if (! (typeof(IPoolable).IsAssignableFrom(type)))
		{
			// GD.Print(string.Format("Cannot use {0} in ObjectPool", type));
			throw new Exception();
		}
		this.ObjectScn = objectScn;
		this.Max = max;
	}

	public IPoolable Get(Node parent)
	{
		// GD.Print("Items in pool: ", items.Count);
		IPoolable item;
		if (items.TryTake(out item))
		{   
			item.Show();
			// GD.Print("Getting from pool");
		}
		else
		{
			// GD.Print("Making new object");
			item = (IPoolable)ObjectScn.Instance();
			parent.AddChild((Node)item);
		}
		item.Init();
		return item;
	}

	public void Release(IPoolable item)
	{
		item.Term();
		if (items.Count >= Max)
		{
			// GD.Print(string.Format("Object pool saturated, deleting {0}", item));
			item.QueueFree();
			return;
		}
		// GD.Print("Releasing to pool");
		items.Add(item);
		item.Hide();
	}
}
