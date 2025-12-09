using System.Threading.Tasks;

namespace FieldScanNew.Services
{
    public interface IRobotArm
    {
        string DeviceName { get; }
        bool IsConnected { get; }
        Task ConnectAsync();
        void Disconnect();

        Task MoveJogAsync(float stepX, float stepY, float stepZ, float stepR);
        Task MoveToAsync(float x, float y, float z, float r);
        Task<RobotPosition> GetPositionAsync();

        // **核心修正：新增拖动模式控制接口**
        Task SetDragModeAsync(bool enable);
    }
}