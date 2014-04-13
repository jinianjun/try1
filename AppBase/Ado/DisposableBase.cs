
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Threading;


namespace AppBase.Ado
{
    public interface IIsDisposed
    {
        bool IsDisposed
        {
            get;
        }
    }

    /// <summary>
    /// Basic functionality for an object implementing IDisposable
    /// </summary>
    public abstract class DisposableBase : IDisposable, IIsDisposed
    {
        private int _disposedCallCount;

        /// <summary>
        /// Safely dispose of an object without allowing exceptions to propogate.
        /// </summary>
        /// <param name="inObject"></param>
        public static void SafeDispose(object inObject)
        {
            IDisposable disposable = inObject as IDisposable;
            if (disposable != null)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception)
                {
                }
            }
        }

        /// <summary>
        /// Safely dispose of an object without allowing exceptions to propogate, and set input
        /// reference to null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ioObject"></param>
        public static void SafeDispose<T>(ref T ioObject) where T : class
        {
            object obj = ioObject;
            ioObject = null;
            SafeDispose(obj);
        }

        ~DisposableBase()
        {
#if DEBUG
            if (!IsDisposed)
            {
                Assembly entryAssembly = null;
                try
                {
                    entryAssembly = Assembly.GetEntryAssembly();
                    if (entryAssembly == null)
                    {
                        entryAssembly = Assembly.GetExecutingAssembly();
                    }
                }
                catch (Exception)
                {
                }
                DebugUtils.CheckDebuggerBreak();
            }
#endif // DEBUG

            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        /// <summary>
        /// IsDisposed
        /// </summary>
        public bool IsDisposed
        {
            get
            {
                return (_disposedCallCount < 0);
            }
        }

        /// <summary>
        /// Implement IDisposable. 
        /// Do not make this method virtual.
        /// A derived class should not be able to override this method.
        /// </summary>
        public void Dispose()
        {

            Dispose(true);
            // Take yourself off the Finalization queue 
            // to prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios.
        /// If disposing equals true, the method has been called directly
        /// or indirectly by a user's code. Managed and unmanaged resources
        /// can be disposed.
        /// If disposing equals false, the method has been called by the 
        /// runtime from inside the finalizer and you should not reference 
        /// other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="inIsDisposing"></param>
        protected virtual void Dispose(bool inIsDisposing)
        {

            // Check to see if Dispose has already been called.
            if (Interlocked.Decrement(ref _disposedCallCount) == -1)
            {
                OnDispose(inIsDisposing);
            }
        }

        /// <summary>
        /// This is the method to override in your subclass.
        /// </summary>
        protected abstract void OnDispose(bool inIsDisposing);

    }

}
