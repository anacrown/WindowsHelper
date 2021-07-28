namespace WindowsHelper
{
    public static class WindowPositionExtension
    {
        public static WindowPositionViewModel CreateViewModel(this windowPosition position)
        {
            return new WindowPositionViewModel()
            {
                X = position.X,
                Y = position.Y
            };
        }
    }
}