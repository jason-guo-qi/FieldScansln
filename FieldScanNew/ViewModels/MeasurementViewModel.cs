using FieldScanNew.Infrastructure;
using FieldScanNew.Views; // 需要引用InputDialog
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FieldScanNew.ViewModels
{
    public class MeasurementViewModel : ViewModelBase, IStepViewModel
    {
        public string DisplayName { get; set; }
        public ObservableCollection<IStepViewModel> Steps { get; }

        // **新增**：用于重命名的命令
        public ICommand RenameCommand { get; }
        public MeasurementViewModel(string name)
        {
            DisplayName = name;

            // 初始化重命名命令
            RenameCommand = new RelayCommand(_ => ExecuteRename());

            Steps = new ObservableCollection<IStepViewModel>
            {
                new InstrumentSetupViewModel(),
                new ProbeSetupViewModel(),
                new ScanSettingsViewModel(),
                new ZCalibViewModel(),
                new XYCalibViewModel(),
                new ScanAreaViewModel()
            };
        }
        private void ExecuteRename()
        {
            // 弹出一个输入对话框，让用户输入新名字
            var inputDialog = new InputDialog("请输入新的测量名称:", this.DisplayName);
            if (inputDialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(inputDialog.Answer))
            {
                DisplayName = inputDialog.Answer;
                OnPropertyChanged(nameof(DisplayName)); // 通知UI更新名称
            }
        }
    }
}