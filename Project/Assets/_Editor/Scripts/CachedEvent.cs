using System;
using System.Collections.Generic;

public static class CachedEvent<T>
{
    private static T lastEvent;
    private static bool hasRaised = false;
    private static readonly List<Action<T>> listeners = new();

    public static void Raise(T e)
    {
        lastEvent = e;
        hasRaised = true;

        foreach (var listener in listeners)
        {
            listener.Invoke(e);
        }
    }

    public static void Subscribe(Action<T> listener, bool invokeWithLast = false)
    {
        if (!listeners.Contains(listener))
            listeners.Add(listener);

        if (invokeWithLast && hasRaised)
            listener.Invoke(lastEvent);
    }

    public static void Unsubscribe(Action<T> listener)
    {
        listeners.Remove(listener);
    }

    public static void ClearCache()
    {
        hasRaised = false;
        lastEvent = default;
    }

    public static void ClearListeners()
    {
        listeners.Clear();
    }
}
