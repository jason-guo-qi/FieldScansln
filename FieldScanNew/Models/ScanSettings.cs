using FieldScanNew.Infrastructure;

namespace FieldScanNew.Models
{
    public class ScanSettings : ViewModelBase
    {
        private float _startX;
        public float StartX { get => _startX; set { _startX = value; OnPropertyChanged(); } }

        private float _startY;
        public float StartY { get => _startY; set { _startY = value; OnPropertyChanged(); } }

        private float _stopX;
        public float StopX { get => _stopX; set { _stopX = value; OnPropertyChanged(); } }

        private float _stopY;
        public float StopY { get => _stopY; set { _stopY = value; OnPropertyChanged(); } }

        private int _numX;
        public int NumX { get => _numX; set { _numX = value; OnPropertyChanged(); } }

        private int _numY;
        public int NumY { get => _numY; set { _numY = value; OnPropertyChanged(); } }

        private float _scanHeightZ;
        public float ScanHeightZ { get => _scanHeightZ; set { _scanHeightZ = value; OnPropertyChanged(); } }

        // **核心修正：新增 R 轴角度属性**
        private float _scanAngleR;
        public float ScanAngleR { get => _scanAngleR; set { _scanAngleR = value; OnPropertyChanged(); } }
    }
}