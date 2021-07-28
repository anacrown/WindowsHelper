namespace WindowsHelper
{
    public class WindowViewModel : BaseViewModel
    {
        private string _processName;
        private windowMode _mode;

        public string ProcessName
        {
            get => _processName;
            set
            {
                _processName = value;
                OnPropertyChanged();
            }
        }

        public windowMode Mode
        {
            get => _mode;
            set
            {
                _mode = value; 
                OnPropertyChanged();
            }
        }
    }
}