/******************************************************************************
* Filename    = BinarySemaphore.cs
*
* Author      = Amithabh A
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = Semaphore class to enforce mutual exclusion among client-server communications
*****************************************************************************/

namespace Updater;

/// <summary>
/// BinarySemaphore class
/// </summary>
public class BinarySemaphore
{
    private readonly SemaphoreSlim _semaphore;

    /// <summary>
    /// Constructor
    /// </summary>
    public BinarySemaphore()
    {
        // Create a semaphore with an initial count of 1 and a maximum count of 1
        _semaphore = new SemaphoreSlim(1, 1);
    }

    /// <summary>
    /// Wait method that blocks the current thread until it can enter
    public void Wait()
    {
        _semaphore.Wait();
    }

    /// <summary>
    /// Release semaphore
    /// </summary>
    public void Signal()
    {
        _semaphore.Release();
    }
}
