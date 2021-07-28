namespace WindowsHelper
{
    public class WindowPositionViewModel : BaseViewModel
    {
        private ushort _x;
        private ushort _y;

        public ushort X
        {
            get => _x;
            set
            {
                _x = value; 
                OnPropertyChanged();
            }
        }

        public ushort Y
        {
            get => _y;
            set
            {
                _y = value; 
                OnPropertyChanged();
            }
        }
    }
}