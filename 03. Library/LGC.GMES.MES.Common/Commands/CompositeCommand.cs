using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using LGC.GMES.MES.Common.Mvvm.Properties;

namespace LGC.GMES.MES.Common.Commands
{
    /// <summary>
    /// </summary>
    public class CompositeCommand : ICommand
    {
        private readonly List<ICommand> registeredCommands = new List<ICommand>();

        private readonly bool monitorCommandActivity;

        private readonly EventHandler onRegisteredCommandCanExecuteChangedHandler;

        private List<WeakReference> _canExecuteChangedHandlers;

        /// <summary>
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public IList<ICommand> RegisteredCommands
        {
            get
            {
                IList<ICommand> list;

                lock (this.registeredCommands)
                    list = this.registeredCommands.ToList<ICommand>();

                return list;
            }
        }

        /// <summary>
        /// </summary>
        public CompositeCommand()
        {
            this.onRegisteredCommandCanExecuteChangedHandler = new EventHandler(this.OnRegisteredCommandCanExecuteChanged);
        }

        /// <summary>
        /// </summary>
        /// <param name="monitorCommandActivity"></param>
        public CompositeCommand(bool monitorCommandActivity) : this()
        {
            this.monitorCommandActivity = monitorCommandActivity;
        }

        /// <summary>
        /// </summary>
        /// <param name="parameter">
        /// </param>
        /// <returns></returns>
        public virtual bool CanExecute(object parameter)
        {
            ICommand[] array;
            bool flag = false;

            lock (this.registeredCommands)
                array = this.registeredCommands.ToArray();

            ICommand[] commandArray = array;

            for (int i = 0; i < (int)commandArray.Length; i++)
            {
                ICommand command = commandArray[i];

                if (this.ShouldExecute(command))
                {
                    if (!command.CanExecute(parameter))
                        return false;

                    flag = true;
                }
            }

            return flag;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Command_IsActiveChanged(object sender, EventArgs e)
        {
            this.OnCanExecuteChanged();
        }

        /// <summary>
        /// </summary>
        /// <param name="parameter">
        /// </param>
        public virtual void Execute(object parameter)
        {
            Queue<ICommand> commands;

            lock (this.registeredCommands)
            {
                CompositeCommand compositeCommand = this;
                commands = new Queue<ICommand>(this.registeredCommands.Where(new Func<ICommand, bool>(compositeCommand.ShouldExecute)).ToList());
            }

            while (commands.Count > 0)
                commands.Dequeue().Execute(parameter);
        }

        /// <summary>
        /// </summary>
        protected virtual void OnCanExecuteChanged()
        {
            WeakEventHandlerManager.CallWeakReferenceHandlers(this, this._canExecuteChangedHandlers);
        }

        private void OnRegisteredCommandCanExecuteChanged(object sender, EventArgs e)
        {
            this.OnCanExecuteChanged();
        }

        /// <summary>
        /// </summary>
        ///  <remarks>
        /// </remarks>
        /// <param name="command"></param>
        public virtual void RegisterCommand(ICommand command)
        {
            if (command == null)
                throw new ArgumentNullException("command");

            if (command == this)
                throw new ArgumentException(Resources.CannotRegisterCompositeCommandInItself);

            lock (this.registeredCommands)
            {
                if (this.registeredCommands.Contains(command))
                    throw new InvalidOperationException(Resources.CannotRegisterSameCommandTwice);

                this.registeredCommands.Add(command);
            }

            command.CanExecuteChanged += this.onRegisteredCommandCanExecuteChangedHandler;
            this.OnCanExecuteChanged();

            if (this.monitorCommandActivity)
            {
                IActiveAware activeAware = command as IActiveAware;

                if (activeAware != null)
                    activeAware.IsActiveChanged += new EventHandler(this.Command_IsActiveChanged);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        protected virtual bool ShouldExecute(ICommand command)
        {
            IActiveAware activeAware = command as IActiveAware;

            if (!this.monitorCommandActivity || activeAware == null)
                return true;

            return activeAware.IsActive;
        }

        /// <summary>
        /// </summary>
        /// <param name="command"></param>
        public virtual void UnregisterCommand(ICommand command)
        {
            bool flag;

            if (command == null)
                throw new ArgumentNullException("command");

            lock (this.registeredCommands)
                flag = this.registeredCommands.Remove(command);

            if (flag)
            {
                command.CanExecuteChanged -= this.onRegisteredCommandCanExecuteChangedHandler;
                this.OnCanExecuteChanged();

                if (this.monitorCommandActivity)
                {
                    IActiveAware activeAware = command as IActiveAware;

                    if (activeAware != null)
                        activeAware.IsActiveChanged -= new EventHandler(this.Command_IsActiveChanged);
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <example>
        /// <code>
        /// </code>
        /// </example>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                WeakEventHandlerManager.AddWeakReferenceHandler(ref this._canExecuteChangedHandlers, value, 2);
            }
            remove
            {
                WeakEventHandlerManager.RemoveWeakReferenceHandler(this._canExecuteChangedHandlers, value);
            }
        }
    }
}