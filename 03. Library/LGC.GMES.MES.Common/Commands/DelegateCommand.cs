using LGC.GMES.MES.Common.Mvvm.Properties;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace LGC.GMES.MES.Common.Commands
{
    public class DelegateCommand : DelegateCommandBase
    {
        public DelegateCommand(Action executeMethod) : this(executeMethod, () => true)
        {
        }

        public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod) : base((object o) => executeMethod(), (object o) => canExecuteMethod())
        {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException("executeMethod", Resources.DelegateCommandDelegatesCannotBeNull);
        }

        private DelegateCommand(Func<Task> executeMethod) : this(executeMethod, () => true)
        {
        }

        private DelegateCommand(Func<Task> executeMethod, Func<bool> canExecuteMethod) : base((object o) => executeMethod(), (object o) => canExecuteMethod())
        {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException("executeMethod", Resources.DelegateCommandDelegatesCannotBeNull);
        }

        public virtual bool CanExecute()
        {
            return base.CanExecute(null);
        }

        public virtual async Task Execute()
        {
            await base.Execute(null);
        }

        public static DelegateCommand FromAsyncHandler(Func<Task> executeMethod)
        {
            return new DelegateCommand(executeMethod);
        }

        public static DelegateCommand FromAsyncHandler(Func<Task> executeMethod, Func<bool> canExecuteMethod)
        {
            return new DelegateCommand(executeMethod, canExecuteMethod);
        }
    }

    public class DelegateCommand<T> : DelegateCommandBase
    {
        public DelegateCommand(Action<T> executeMethod) : this(executeMethod, (T o) => true)
        {
        }

        public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod) : base((object o) => executeMethod((T)o), (object o) => canExecuteMethod((T)o))
        {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException("executeMethod", Resources.DelegateCommandDelegatesCannotBeNull);

            TypeInfo typeInfo = typeof(T).GetTypeInfo();

            if (typeInfo.IsValueType && (!typeInfo.IsGenericType || !typeof(Nullable<>).GetTypeInfo().IsAssignableFrom(typeInfo.GetGenericTypeDefinition().GetTypeInfo())))
                throw new InvalidCastException(Resources.DelegateCommandInvalidGenericPayloadType);
        }

        private DelegateCommand(Func<T, Task> executeMethod) : this(executeMethod, (T o) => true)
        {
        }

        private DelegateCommand(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod) : base((object o) => executeMethod((T)o), (object o) => canExecuteMethod((T)o))
        {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException("executeMethod", Resources.DelegateCommandDelegatesCannotBeNull);
        }

        public virtual bool CanExecute(T parameter)
        {
            return base.CanExecute(parameter);
        }

        public virtual async Task Execute(T parameter)
        {
            await base.Execute(parameter);
        }

        public static DelegateCommand<T> FromAsyncHandler(Func<T, Task> executeMethod)
        {
            return new DelegateCommand<T>(executeMethod);
        }

        public static DelegateCommand<T> FromAsyncHandler(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod)
        {
            return new DelegateCommand<T>(executeMethod, canExecuteMethod);
        }
    }
}