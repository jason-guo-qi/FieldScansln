using FieldScanNew.Infrastructure;
using FieldScanNew.Models;
using System.Collections.Generic;

namespace FieldScanNew.ViewModels
{
    public class ScanSettingsViewModel : ViewModelBase, IStepViewModel
    {
        public string DisplayName => "3. 高级扫描设置";

        private InstrumentSettings _settings;
        public InstrumentSettings Settings
        {
            get => _settings;
            set
            {
                if (_settings != value)
                {
                    _settings = value;
                    // 当底层设置对象改变时（例如加载新项目），通知所有相关属性刷新
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CenterFreqDisplay));
                    OnPropertyChanged(nameof(SelectedCenterUnit));
                    OnPropertyChanged(nameof(SpanDisplay));
                    OnPropertyChanged(nameof(SelectedSpanUnit));
                }
            }
        }

        // 单位下拉列表数据源
        public List<string> FrequencyUnits { get; } = new List<string> { "Hz", "KHz", "MHz", "GHz" };

        public ScanSettingsViewModel(InstrumentSettings settings)
        {
            _settings = settings;
        }

        // 辅助方法：获取单位对应的乘数
        private double GetMultiplier(string unit)
        {
            return unit switch
            {
                "KHz" => 1e3,
                "MHz" => 1e6,
                "GHz" => 1e9,
                _ => 1.0
            };
        }

        // ============ 中心频率 ============
        public double CenterFreqDisplay
        {
            get => _settings.CenterFrequencyHz / GetMultiplier(_settings.CenterFrequencyUnit);
            set
            {
                // 界面输入值 * 单位 = 实际Hz值
                _settings.CenterFrequencyHz = value * GetMultiplier(_settings.CenterFrequencyUnit);
                OnPropertyChanged();
            }
        }

        public string SelectedCenterUnit
        {
            get => _settings.CenterFrequencyUnit;
            set
            {
                if (_settings.CenterFrequencyUnit != value)
                {
                    _settings.CenterFrequencyUnit = value;
                    OnPropertyChanged();
                    // 单位变了，数值显示也要变 (例如 1000 MHz -> 1 GHz)
                    OnPropertyChanged(nameof(CenterFreqDisplay));
                }
            }
        }

        // ============ 频域宽度 (Span) ============
        public double SpanDisplay
        {
            get => _settings.SpanHz / GetMultiplier(_settings.SpanUnit);
            set
            {
                _settings.SpanHz = value * GetMultiplier(_settings.SpanUnit);
                OnPropertyChanged();
            }
        }

        public string SelectedSpanUnit
        {
            get => _settings.SpanUnit;
            set
            {
                if (_settings.SpanUnit != value)
                {
                    _settings.SpanUnit = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SpanDisplay));
                }
            }
        }
    }
}