namespace WindowsHelper
{
    public class WindowSizeViewModel : BaseViewModel
    {
        private ushort _width;
        private ushort _height;

        public ushort Width
        {
            get => _width;
            set
            {
                _width = value; 
                OnPropertyChanged();
            }
        }

        public ushort Height
        {
            get => _height;
            set
            {
                _height = value; 
                OnPropertyChanged();
            }
        }
    }
}