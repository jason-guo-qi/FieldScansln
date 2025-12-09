using FieldScanNew.ViewModels;
using FieldScanNew.Views;
using System.Windows;

namespace FieldScanNew.Services
{
    public class DialogService : IDialogService
    {
        public void ShowDialog(IStepViewModel viewModel)
        {
            var dialog = new DialogWindow
            {
                DataContext = viewModel
            };

            // **核心修正：直接使用 ViewModel 的 DisplayName 作为窗口标题**
            // 这样无论您在 ViewModel 里怎么改名，弹窗标题都会自动同步，
            // 不需要再写一堆 if-else 来手动赋值了。
            if (!string.IsNullOrEmpty(viewModel.DisplayName))
            {
                dialog.Title = viewModel.DisplayName;
            }
            else
            {
                // 保底逻辑（虽然理论上不会用到，因为大家都有名字）
                dialog.Title = "配置窗口";
            }

            dialog.ShowDialog();
        }
    }
}