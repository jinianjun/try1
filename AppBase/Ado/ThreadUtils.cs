using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Threading;


namespace AppBase.Commons.Core
{
    /// <summary>
    /// Basic helper functions for dealing with strings.
    /// </summary>
    public static class ThreadUtils
    {
        public delegate R ThreadRunDelegate<R, T>(T param);

        public static R RunWithTimeout<R, T>(int timeoutInSeconds, T param, ThreadRunDelegate<R, T> del)
        {
            // Command does not support timeout, do a manual timeout here
            Thread thread = null;
            Exception threadException = null;
            try
            {
                R rtnVal = default(R);
                thread = new Thread(delegate()
                {
                    try
                    {
                        rtnVal = del(param);
                    }
                    catch (Exception thEx)
                    {
                        // If the exception is not trapped here, .NET will kill the process since this would
                        // be an unhandled exception, so catch the exception here and pass it along below (outside
                        // of the thread context).
                        threadException = thEx;
                    }
                });
                thread.IsBackground = true;
                thread.Start();
                if (!thread.Join(timeoutInSeconds * 1000))
                {
                    throw new TimeoutException(string.Format("Execution did not finish within {0} seconds",
                                                             timeoutInSeconds.ToString()));
                }
                if (threadException != null)
                {
                    throw threadException;
                }
                return rtnVal;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if ((thread != null) && thread.IsAlive)
                {
                    try
                    {
                        thread.Abort();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
    }
    public class PermitsSemaphore
    {
        private int _initialPermits;
        private AutoResetEvent _event = new AutoResetEvent(false);

        public PermitsSemaphore(int initialPermits)
        {
            Init(initialPermits);
        }
        public void Init(int initialPermits)
        {
            if (initialPermits > 0)
            {
                initialPermits = -initialPermits;
            }
            _initialPermits = initialPermits;
        }
        public void ReleaseAndWaitForZero()
        {
            Interlocked.Increment(ref _initialPermits);
            while (_initialPermits < 0)
            {
                _event.WaitOne();
            }
            _event.Set();
        }
        public void Release()
        {
            Interlocked.Increment(ref _initialPermits);
            _event.Set();
        }
    }

}
