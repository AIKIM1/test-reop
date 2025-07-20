using LGC.GMES.MES.Common.Mvvm.Properties;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LGC.GMES.MES.Common.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class DelegateCommandBase : ICommand, IActiveAware
    {
        private bool _isActive;

        private List<WeakReference> _canExecuteChangedHandlers;
        protected readonly Func<object, Task> _executeMethod;
        protected readonly Func<object, bool> _canExecuteMethod;

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public bool IsActive
        {
            get
            {
                return this._isActive;
            }
            set
            {
                if (this._isActive != value)
                {
                    this._isActive = value;
                    this.OnIsActiveChanged();
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="executeMethod"></param>
        /// <param name="canExecuteMethod"></param>
        protected DelegateCommandBase(Action<object> executeMethod, Func<object, bool> canExecuteMethod)
        {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException("executeMethod", Resources.DelegateCommandDelegatesCannotBeNull);

            this._executeMethod = (object arg) =>
            {
                executeMethod(arg);
                return Task.Delay(0);
            };

            this._canExecuteMethod = canExecuteMethod;
        }

        /// <summary>
        /// </summary>
        /// <param name="executeMethod"></param>
        /// <param name="canExecuteMethod"></param>
        protected DelegateCommandBase(Func<object, Task> executeMethod, Func<object, bool> canExecuteMethod)
        {
            if (executeMethod == null || canExecuteMethod == null)
                throw new ArgumentNullException("executeMethod", Resources.DelegateCommandDelegatesCannotBeNull);

            this._executeMethod = executeMethod;
            this._canExecuteMethod = canExecuteMethod;
        }

        /// <summary>
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        protected bool CanExecute(object parameter)
        {
            if (this._canExecuteMethod == null)
                return true;

            return this._canExecuteMethod(parameter);
        }

        /// <summary>
        /// </summary>
        /// <param name="parameter"></param>
        protected async Task Execute(object parameter)
        {
            await this._executeMethod(parameter);
        }

        /// <summary>
        /// </summary>
        protected virtual void OnCanExecuteChanged()
        {
            WeakEventHandlerManager.CallWeakReferenceHandlers(this, this._canExecuteChangedHandlers);
        }

        /// <summary>
        /// </summary>
        protected virtual void OnIsActiveChanged()
        {
            EventHandler eventHandler = this.IsActiveChanged;

            if (eventHandler != null)
                eventHandler(this, EventArgs.Empty);
        }

        /// <summary>
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            this.OnCanExecuteChanged();
        }

        bool System.Windows.Input.ICommand.CanExecute(object parameter)
        {
            return this.CanExecute(parameter);
        }

        async void System.Windows.Input.ICommand.Execute(object parameter)
        {
            await this.Execute(parameter);
        }

        public virtual event EventHandler CanExecuteChanged
        {
            add { WeakEventHandlerManager.AddWeakReferenceHandler(ref this._canExecuteChangedHandlers, value, 2); }
            remove { WeakEventHandlerManager.RemoveWeakReferenceHandler(this._canExecuteChangedHandlers, value); }
        }

        public virtual event EventHandler IsActiveChanged;
    }
}