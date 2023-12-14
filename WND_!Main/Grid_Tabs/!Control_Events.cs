namespace WinUtil
{
    public partial class MainWindow
    {
        internal delegate void Sys_Theme_Switch();
        internal static event Sys_Theme_Switch Sys_Theme_Switch_Setter;
        private void GetSetUIThemeSwitch()
        {
            Sys_Theme_Switch_Setter?.Invoke();
        }
    }
}