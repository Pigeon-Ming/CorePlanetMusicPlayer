using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CorePlanetMusicPlayer6.Models
{
    public class ActionCommand : ICommand
    {
        private readonly Action action;

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            action?.Invoke();
        }

        public ActionCommand(Action _action)
        {
            action = _action;
        }
    }
