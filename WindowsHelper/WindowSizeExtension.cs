namespace WindowsHelper
{
    public static class WindowSizeExtension
    {
        public static WindowSizeViewModel CreateViewModel(this windowSize size)
        {
            return new WindowSizeViewModel()
            {
                Width = size.width,
                Height = size.height
            };
        }
    }
}