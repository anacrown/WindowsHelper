namespace WindowsHelper
{
    public static class WindowExtensions
    {
        public static WindowViewModel CreateViewModel(this window window)
        {
            return new WindowViewModel()
            {
                ProcessName = window.process,
                Mode = window.mode
            };
        }
    }
}