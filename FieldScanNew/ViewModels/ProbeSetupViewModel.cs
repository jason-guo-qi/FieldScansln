using FieldScanNew.Infrastructure;
using FieldScanNew.Models;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

// ========================================================================
// **核心修正：添加别名，明确指定使用 WPF 版本的控件，消除歧义**
// ========================================================================
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using MessageBox = System.Windows.MessageBox;

namespace FieldScanNew.ViewModels
{
    public class ProbeSetupViewModel : ViewModelBase, IStepViewModel
    {
        public string DisplayName => "2. 探头配置";

        // 探头数据列表
        public ObservableCollection<ProbePoint> ProbePoints { get; }

        // 图表模型
        public PlotModel ProbePlotModel { get; private set; }

        // 当前加载的文件名
        private string _loadedFileName = "未加载";
        public string LoadedFileName { get => _loadedFileName; set { _loadedFileName = value; OnPropertyChanged(); } }

        public ICommand LoadProbeCommand { get; }
        public ICommand ClearProbeCommand { get; }

        public ProbeSetupViewModel()
        {
            ProbePoints = new ObservableCollection<ProbePoint>();
            LoadProbeCommand = new RelayCommand(ExecuteLoadProbe);
            ClearProbeCommand = new RelayCommand(ExecuteClearProbe);

            InitializePlot();
        }

        private void InitializePlot()
        {
            ProbePlotModel = new PlotModel { Title = "探头修正因子曲线" };

            // 设置坐标轴
            ProbePlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Title = "频率 (MHz)", Unit = "MHz" });
            ProbePlotModel.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = "修正因子 (dB)", Unit = "dB" });

            // 添加一条空线
            var lineSeries = new LineSeries
            {
                Title = "Factor",
                Color = OxyColors.Blue,
                StrokeThickness = 2,
                MarkerType = MarkerType.Circle,
                MarkerSize = 3
            };
            ProbePlotModel.Series.Add(lineSeries);
        }

        private void ExecuteLoadProbe(object? parameter)
        {
            // 现在这里使用的是 Microsoft.Win32.OpenFileDialog (WPF版)
            var openFileDialog = new OpenFileDialog
            {
                Filter = "CSV 文件 (*.csv)|*.csv|文本文件 (*.txt)|*.txt|所有文件 (*.*)|*.*",
                Title = "加载探头校准文件"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    ParseProbeFile(openFileDialog.FileName);
                    LoadedFileName = Path.GetFileName(openFileDialog.FileName);
                    UpdateChart();
                }
                catch (Exception ex)
                {
                    // 现在这里使用的是 System.Windows.MessageBox (WPF版)
                    MessageBox.Show("读取文件失败: " + ex.Message, "错误");
                }
            }
        }

        private void ExecuteClearProbe(object? parameter)
        {
            ProbePoints.Clear();
            LoadedFileName = "未加载";
            UpdateChart();
        }

        private void ParseProbeFile(string filePath)
        {
            ProbePoints.Clear();
            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                // 跳过空行或注释行
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.StartsWith("//")) continue;

                var parts = line.Split(new[] { ',', '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    if (double.TryParse(parts[0], out double freq) && double.TryParse(parts[1], out double factor))
                    {
                        // 假设文件里的频率如果是小数值，单位是MHz；如果是极大值，单位是Hz
                        // 这里做一个简单的自适应处理
                        double freqHz = freq < 10000 ? freq * 1e6 : freq;

                        ProbePoints.Add(new ProbePoint { Frequency = freqHz, CorrectionFactor = factor });
                    }
                }
            }

            if (ProbePoints.Count == 0) MessageBox.Show("未从文件中读取到有效数据，请检查格式。\n推荐格式: 频率(MHz), 因子(dB)", "提示");
        }

        private void UpdateChart()
        {
            var series = ProbePlotModel.Series[0] as LineSeries;
            if (series != null)
            {
                series.Points.Clear();
                // 排序，确保画图连线正确
                var sortedPoints = ProbePoints.OrderBy(p => p.Frequency).ToList();

                foreach (var p in sortedPoints)
                {
                    // X轴显示为MHz
                    series.Points.Add(new DataPoint(p.Frequency / 1e6, p.CorrectionFactor));
                }
            }
            ProbePlotModel.InvalidatePlot(true);
        }
    }
}