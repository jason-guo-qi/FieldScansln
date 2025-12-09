using FieldScanNew.Infrastructure;
using FieldScanNew.Services;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using MessageBox = System.Windows.MessageBox;

namespace FieldScanNew.ViewModels
{
    public class ZCalibViewModel : ViewModelBase, IStepViewModel
    {
        // 这里的标题其实已经不重要了，因为这个页面在新流程中不再显示
        public string DisplayName => "4. Z轴校准(旧)";
        private readonly HardwareService _hardwareService;

        private float _stepSize = 1.0f;
        public float StepSize { get => _stepSize; set { _stepSize = value; OnPropertyChanged(); } }

        public ICommand MoveJogCommand { get; }

        public ZCalibViewModel()
        {
            _hardwareService = HardwareService.Instance;
            MoveJogCommand = new RelayCommand(async (param) => await ExecuteMoveJog(param));
        }

        private async Task ExecuteMoveJog(object? parameter)
        {
            if (_hardwareService.ActiveRobot == null || !_hardwareService.ActiveRobot.IsConnected)
            {
                MessageBox.Show("请先在“仪器连接”中连接机器人。", "提示");
                return;
            }

            float x = 0, y = 0, z = 0;
            switch (parameter?.ToString())
            {  
                case "X+": x = StepSize; break;
                case "X-": x = -StepSize; break;
                case "Y+": y = StepSize; break;
                case "Y-": y = -StepSize; break;
                case "Z+": z = StepSize; break;
                case "Z-": z = -StepSize; break;
            }

            try
            {
                // **核心修复：补上第 4 个参数 (0)**
                // 这样编译器就知道 R 轴没有变化，就不会报错了
                await _hardwareService.ActiveRobot.MoveJogAsync(x, y, z, 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show("移动机器人失败: " + ex.Message, "错误");
            }
        }
    }
}