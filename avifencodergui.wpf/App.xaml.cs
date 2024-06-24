using System.Windows;
using avifencodergui.wpf.Messenger;
using avifencodergui.wpf.Windows;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace avifencodergui.wpf
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 创建主窗口
            MainWindow mainWindow = new MainWindow();

            // 获取屏幕尺寸
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            // 计算窗口应该显示的位置
            double windowWidth = mainWindow.Width;
            double windowHeight = mainWindow.Height;
            double left = (screenWidth - windowWidth) / 2;
            double top = (screenHeight - windowHeight) / 2;

            // 设置窗口位置
            mainWindow.Left = left;
            mainWindow.Top = top;

            // 显示窗口
            mainWindow.Show();

            WeakReferenceMessenger.Default.Register<WindowMessage>(this, (r, m) =>
            {
                if (m.Value == WindowEnum.SettingsWindows)
                {
                    var settingWindow = new SettingsWindow();

                    double settingWindowWidth = settingWindow.Width;
                    double settingWindowHeight = settingWindow.Height;
                    double settingWindowLeft = (screenWidth - settingWindowWidth) / 2;
                    double settingWindowTop = (screenHeight - settingWindowHeight) / 2;

                    settingWindow.Left = settingWindowLeft;
                    settingWindow.Top = settingWindowTop;

                    settingWindow.ShowDialog();
                }
            });
        }
    }
}