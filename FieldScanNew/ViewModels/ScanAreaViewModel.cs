using FieldScanNew.Infrastructure;
using FieldScanNew.Models;
using System;
using System.Windows; // 引用 Point, Rect
using System.Windows.Media.Imaging;

using MessageBox = System.Windows.MessageBox;

namespace FieldScanNew.ViewModels
{
    public class ScanAreaViewModel : ViewModelBase, IStepViewModel
    {
        // **核心修正：改名为 "5. 扫描区域配置"**
        public string DisplayName => "5. 扫描区域配置";

        private readonly ProjectData _projectData;

        public ScanSettings Settings
        {
            get => _projectData.ScanConfig;
            set
            {
                if (_projectData.ScanConfig != value)
                {
                    _projectData.ScanConfig = value;
                    OnPropertyChanged();
                }
            }
        }

        private BitmapSource? _dutImageSource;
        public BitmapSource? DutImageSource { get => _dutImageSource; set { _dutImageSource = value; OnPropertyChanged(); } }

        private string _statusText = "请在图片上【按住鼠标左键拖拽】以框选扫描区域。";
        public string StatusText { get => _statusText; set { _statusText = value; OnPropertyChanged(); } }

        public ScanAreaViewModel(ProjectData projectData)
        {
            _projectData = projectData;
            ReloadImage();
        }

        public void ReloadImage()
        {
            if (!string.IsNullOrEmpty(_projectData.DutImagePath) && System.IO.File.Exists(_projectData.DutImagePath))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(_projectData.DutImagePath);
                    bitmap.EndInit();
                    bitmap.Freeze();
                    DutImageSource = bitmap;
                }
                catch { }
            }
            else
            {
                DutImageSource = null;
                StatusText = "未找到校准图片，请先在“4. 机械臂校准”中加载或拍摄图片。";
            }
        }

        public void UpdateScanAreaFromSelection(Rect rectPixel)
        {
            if (!_projectData.IsCalibrated)
            {
                MessageBox.Show("系统尚未校准！\n请先完成“4. 机械臂校准”，否则无法自动计算物理坐标。", "警告");
                return;
            }

            // ================================================================
            // **核心修正：使用独立缩放公式计算物理坐标**
            // ================================================================
            double scaleX = _projectData.MatrixM11;
            double scaleY = _projectData.MatrixM22;
            double offX = _projectData.OffsetX;
            double offY = _projectData.OffsetY;

            // 计算矩形对角线两个点的物理坐标 (P1:左上, P2:右下)
            // 注意：因为可能存在翻转，P1转换后不一定还是“左上”，可能是“右下”

            double physX_1 = (scaleX * rectPixel.X) + offX;
            double physY_1 = (scaleY * rectPixel.Y) + offY;

            double physX_2 = (scaleX * (rectPixel.X + rectPixel.Width)) + offX;
            double physY_2 = (scaleY * (rectPixel.Y + rectPixel.Height)) + offY;

            // 自动判断大小，填入 Start/Stop
            // Start 总是放较小值，Stop 总是放较大值 (或者根据扫描习惯)
            // 这里我们遵循：Start < Stop
            Settings.StartX = (float)Math.Min(physX_1, physX_2);
            Settings.StopX = (float)Math.Max(physX_1, physX_2);
            Settings.StartY = (float)Math.Min(physY_1, physY_2);
            Settings.StopY = (float)Math.Max(physY_1, physY_2);

            StatusText = $"区域已更新：X[{Settings.StartX:F1} ~ {Settings.StopX:F1}], Y[{Settings.StartY:F1} ~ {Settings.StopY:F1}]";
        }
    }
}