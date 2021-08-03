using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;
using WindowsHelper.Annotations;

namespace WindowsHelper
{
    public class WindowVm : INotifyPropertyChanged
    {
        public window Window { get; }

        private readonly Unitor unitor;

        public bool Enabled
        {
            get => Window.enabled;
            set
            {
                Window.enabled = value;
                OnPropertyChanged();

                if (Window.enabled)
                    unitor?.Run();
                else
                    unitor?.Stop();
            }
        }

        public int SelectedSize
        {
            get => Window.selectedsize;
            set
            {
                Window.selectedsize = value;
                OnPropertyChanged();
            }
        }

        public int SelectedPosition
        {
            get => Window.selectedposition;
            set
            {
                Window.selectedposition = value;
                OnPropertyChanged();
            }
        }

        public WindowVm(window window)
        {
            Window = window;
            unitor = new Unitor(window);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}