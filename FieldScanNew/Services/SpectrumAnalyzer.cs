using FieldScanNew.Models;
using Ivi.Visa;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FieldScanNew.Services
{
    public class SpectrumAnalyzer : IMeasurementDevice
    {
        public string DeviceName => "Spectrum Analyzer (VISA)";
        public bool IsConnected { get; private set; } = false;
        private IMessageBasedSession? _saSession;

        public async Task ConnectAsync(InstrumentSettings settings)
        {
            if (IsConnected) Disconnect();

            await Task.Run(() =>
            {
                try
                {
                    string visaAddress = $"TCPIP0::{settings.IpAddress}::inst0::INSTR";
                    var visaSession = GlobalResourceManager.Open(visaAddress);
                    _saSession = visaSession as IMessageBasedSession;

                    if (_saSession == null) throw new Exception("无法创建VISA会话。");

                    _saSession.TimeoutMilliseconds = 3000;
                    _saSession.TerminationCharacterEnabled = true;
                    _saSession.TerminationCharacter = (byte)'\n';
                    _saSession.SendEndEnabled = true;

                    _saSession.FormattedIO.WriteLine("*IDN?");
                    string idn = _saSession.FormattedIO.ReadLine();

                    // 下发参数
                    _saSession.FormattedIO.WriteLine(string.Format(CultureInfo.InvariantCulture, ":FREQ:CENT {0}", settings.CenterFrequencyHz));
                    _saSession.FormattedIO.WriteLine(string.Format(CultureInfo.InvariantCulture, ":FREQ:SPAN {0}", settings.SpanHz));
                    _saSession.FormattedIO.WriteLine(string.Format(CultureInfo.InvariantCulture, ":DISP:WIND:TRAC:Y:RLEV {0}", settings.ReferenceLevelDb));

                    if (settings.Points > 0)
                        _saSession.FormattedIO.WriteLine($":SWE:POIN {settings.Points}");

                    _saSession.FormattedIO.WriteLine(":DET:TRAC POS");

                    // =========================================================
                    // **核心修正：关闭连续扫描，改为单次触发模式**
                    // 这能确保每次都是全新的、完整的数据
                    // =========================================================
                    _saSession.FormattedIO.WriteLine(":INIT:CONT OFF");

                    _saSession.TimeoutMilliseconds = 30000;
                    IsConnected = true;
                }
                catch (Exception ex)
                {
                    Disconnect();
                    throw new Exception($"连接失败: {ex.Message}");
                }
            });
        }

        public void Disconnect()
        {
            if (_saSession != null) { try { _saSession.Dispose(); } catch { } _saSession = null; }
            IsConnected = false;
        }

        public async Task<double> GetMeasurementValueAsync(int delayMs)
        {
            var traceData = await GetTraceDataAsync(delayMs);
            if (traceData.Length > 0)
                return traceData.Max();
            return -120.0; // 如果没数据，返回底噪水平而不是 -999，避免图表缩放异常
        }

        public async Task<double[]> GetTraceDataAsync(int delayMs)
        {
            if (!IsConnected || _saSession == null) throw new InvalidOperationException("未连接");

            return await Task.Run(() =>
            {
                try
                {
                    var formattedIO = _saSession.FormattedIO;

                    // =========================================================
                    // **核心修正：握手式扫描逻辑**
                    // =========================================================

                    // 1. 设置轨迹类型为写入 (每一次都是新的)
                    formattedIO.WriteLine(":TRAC:TYPE WRIT");

                    // 2. 发起一次扫描，并死等它完成 (*WAI)
                    // 这样程序会自动适应 RBW 造成的扫描时间变化，不用人为设置 Sleep
                    formattedIO.WriteLine(":INIT:IMM; *WAI");

                    // 3. (可选) 如果还需要额外的稳定时间，才使用 delayMs
                    // 通常有了 *WAI 就不需要 delayMs 了，除非有外部放大器稳定时间
                    if (delayMs > 0) Thread.Sleep(delayMs);

                    // 4. 读取数据
                    formattedIO.WriteLine(":TRAC:DATA? TRACE1");

                    string dataStr = formattedIO.ReadLine();

                    if (string.IsNullOrWhiteSpace(dataStr)) return new double[0];

                    var values = dataStr.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(s => double.Parse(s, CultureInfo.InvariantCulture))
                                        .ToArray();
                    return values;
                }
                catch (Exception ex)
                {
                    throw new Exception($"读取Trace失败: {ex.Message}");
                }
            });
        }
    }
}