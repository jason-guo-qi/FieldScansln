namespace FieldScanNew.Models
{
    public class ProbePoint
    {
        public double Frequency { get; set; } // 频率 (Hz)
        public double CorrectionFactor { get; set; } // 修正因子 (dB)

        // 方便显示的属性 (例如显示为 MHz)
        public string FrequencyText => $"{Frequency / 1e6:F2} MHz";
    }
}