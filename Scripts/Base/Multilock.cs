using System.Collections.Generic;

public class Multilock 
{
    private HashSet<string> Locks = new();

    public void AddLock(string source)
    {
        Locks.Add(source);
    }

    public void RemoveLock(string source)
    {
        Locks.Remove(source);
    }

    public bool IsUnlocked()
    {
        return Locks.Count == 0;
    }
}