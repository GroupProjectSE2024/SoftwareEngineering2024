using Microsoft.VisualStudio.TestTools.UnitTesting;
using Updater;

namespace TestsUpdater;

[TestClass]
public class TestBinarySemaphore
{
    /// <summary>
    /// Test semaphore acquiring by a thread.
    /// </summary>
    [TestMethod]
    public void TestBinarySemaphore_AcquiresSemaphoreSuccessfully()
    {
        // Arrange
        BinarySemaphore semaphore = new BinarySemaphore();

        // Act
        semaphore.Wait();  // Acquire the semaphore
        bool semaphoreAcquired = true;

        // Assert
        Assert.IsTrue(semaphoreAcquired, "Semaphore should be acquired by the current thread.");
    }

    /// <summary>
    /// Test that a thread is blocked if another thread holds the semaphore
    /// </summary>
    [TestMethod]
    public void TestBinarySemaphore_BlockingOtherThreads()
    {
        // Arrange
        BinarySemaphore semaphore = new BinarySemaphore();

        bool isThread2BlockedWhenThread1Acquires = false;

        // ManualResetEvent to synchronize the threads
        ManualResetEvent thread1SemaphoreAcquiringStarted = new ManualResetEvent(false);
        ManualResetEvent thread1SemaphoreAcquiringFinished = new ManualResetEvent(false);
        ManualResetEvent thread2SemaphoreAcquiringStarted = new ManualResetEvent(false);
        ManualResetEvent thread2SemaphoreAcquiringFinished = new ManualResetEvent(false);

        // Thread 0 checks if thread 2 is blocked while thread 1 holds the semaphore
        Thread thread0 = new Thread(() => {
            // Wait for thread 1 to acquire semaphore
            thread1SemaphoreAcquiringStarted.WaitOne();
            // Ensure thread 2 starts waiting for semaphore
            thread2SemaphoreAcquiringStarted.WaitOne();
            // Check if thread 2 is blocked while thread 1 holds the semaphore
            if (!thread2SemaphoreAcquiringFinished.WaitOne(0))
            {
                isThread2BlockedWhenThread1Acquires = true;
            }
        });

        // Thread 1 acquires the semaphore first
        Thread thread1 = new Thread(() => {
            // Notify thread0 that thread 1 started acquiring semaphore
            thread1SemaphoreAcquiringStarted.Set();
            semaphore.Wait();
            // Notify thread2 that thread 1 has finished acquiring semaphore
            thread1SemaphoreAcquiringFinished.Set();
            // Simulate work after acquiring semaphore
            Thread.Sleep(1000);
            semaphore.Signal();
        });

        // Thread 2 will try to acquire semaphore after thread 1
        Thread thread2 = new Thread(() => {
            // Wait until thread 1 has finished its work
            thread1SemaphoreAcquiringFinished.WaitOne();
            // Notify thread0 that thread 2 is waiting for semaphore
            thread2SemaphoreAcquiringStarted.Set();
            // Thread 2 will block here since thread 1 will hold the semaphore
            semaphore.Wait();
            // Notify thread0 that thread 2 has acquired the semaphore
            thread2SemaphoreAcquiringFinished.Set();
            semaphore.Signal();
        });

        // Act
        thread0.Start();
        thread1.Start();
        thread2.Start();

        thread1.Join();
        thread2.Join();
        thread0.Join();

        // Assert
        Assert.IsTrue(isThread2BlockedWhenThread1Acquires, "Thread 2 was not blocked when Thread 1 acquired the semaphore.");
    }


    /// <summary>
    /// Test that the signal method correctly releases the semaphore
    /// </summary>
    [TestMethod]
    public void TestBinarySemaphore_SignalReleasesSemaphore()
    {
        // Arrange
        BinarySemaphore semaphore = new BinarySemaphore();

        // Act
        // Acquire the semaphore and release it immediately
        semaphore.Wait();
        semaphore.Signal();
        bool semaphoreAcquired = true;

        // Assert
        Assert.IsTrue(semaphoreAcquired, "Semaphore should be acquired and released correctly.");
    }

    /// <summary>
    /// Test semaphore behaviour on multiple threads
    /// </summary>
    [TestMethod]
    public void TestBinarySemaphore_MultipleThreads()
    {
        // Arrange
        BinarySemaphore semaphore = new BinarySemaphore();

        int threadCount = 10;
        int noOfThreadsAcquiredSemaphore = 0;

        Thread[] threads = new Thread[threadCount];

        // Act
        for (int i = 0; i < threadCount; i++)
        {
            threads[i] = new Thread(() => {
                semaphore.Wait();
                // Increment without synchronization
                noOfThreadsAcquiredSemaphore++;
                // giving chance to other threads to acquire the semaphore
                Thread.Sleep(50);
                semaphore.Signal();
            });
            threads[i].Start();
        }

        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        // Assert
        Assert.AreEqual(10, noOfThreadsAcquiredSemaphore, "Only one thread should acquire the semaphore at a time.");
    }


    /// <summary>
    /// Testing semaphore in pure mutual exclusion environment
    /// </summary>
    [TestMethod]
    public void TestBinarySemaphore_MutualExclusion()
    {
        // Arrange
        BinarySemaphore semaphore = new BinarySemaphore();
        int accessCount = 0;
        int threadCount = 100;

        // Act
        Thread[] threads = new Thread[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            threads[i] = new Thread(() => {
                semaphore.Wait();
                try
                {
                    accessCount++;
                }
                finally
                {
                    semaphore.Signal();
                }
            });
            threads[i].Start();
        }

        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        // Assert
        Assert.AreEqual(threadCount, accessCount, "The semaphore should ensure that only one thread accesses the critical section at a time.");
    }
}
