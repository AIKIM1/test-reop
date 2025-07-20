using System;
using System.Collections.Generic;
using System.Threading;

namespace LGC.GMES.MES.Common.Commands
{
    /// <summary>
    /// </summary>
    public static class WeakEventHandlerManager
    {
        private readonly static SynchronizationContext syncContext;

        static WeakEventHandlerManager()
        {
            WeakEventHandlerManager.syncContext = SynchronizationContext.Current;
        }

        /// <summary>
        /// </summary>
        /// <param name="handlers"></param>
        /// <param name="handler"></param>
        /// <param name="defaultListSize"></param>
        public static void AddWeakReferenceHandler(ref List<WeakReference> handlers, EventHandler handler, int defaultListSize)
        {
            if (handlers == null)
                handlers = (defaultListSize > 0 ? new List<WeakReference>(defaultListSize) : new List<WeakReference>());

            handlers.Add(new WeakReference(handler));
        }

        private static void CallHandler(object sender, EventHandler eventHandler)
        {
            if (eventHandler != null)
            {
                if (WeakEventHandlerManager.syncContext != null)
                {
                    WeakEventHandlerManager.syncContext.Post((object o) => eventHandler(sender, EventArgs.Empty), null);
                    return;
                }

                eventHandler(sender, EventArgs.Empty);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="handlers"></param>
        public static void CallWeakReferenceHandlers(object sender, List<WeakReference> handlers)
        {
            if (handlers != null)
            {
                EventHandler[] eventHandlerArray = new EventHandler[handlers.Count];
                int num = 0;
                num = WeakEventHandlerManager.CleanupOldHandlers(handlers, eventHandlerArray, num);

                for (int i = 0; i < num; i++)
                    WeakEventHandlerManager.CallHandler(sender, eventHandlerArray[i]);
            }
        }

        private static int CleanupOldHandlers(List<WeakReference> handlers, EventHandler[] callees, int count)
        {
            for (int i = handlers.Count - 1; i >= 0; i--)
            {
                EventHandler target = handlers[i].Target as EventHandler;

                if (target != null)
                {
                    callees[count] = target;
                    count++;
                }
                else
                {
                    handlers.RemoveAt(i);
                }
            }

            return count;
        }

        /// <summary>
        /// </summary>
        /// <param name="handlers"></param>
        /// <param name="handler"></param>
        public static void RemoveWeakReferenceHandler(List<WeakReference> handlers, EventHandler handler)
        {
            if (handlers != null)
            {
                for (int i = handlers.Count - 1; i >= 0; i--)
                {
                    EventHandler target = handlers[i].Target as EventHandler;

                    if (target == null || target == handler)
                        handlers.RemoveAt(i);
                }
            }
        }
    }
}