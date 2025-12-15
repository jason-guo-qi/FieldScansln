using FieldScanNew.Models;
using Ivi.Visa;
using System;
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
                // 1. 使用 INSTR 协议
                string visaAddress = $"TCPIP0::{settings.IpAddress}::INSTR";

                var visaSession = GlobalResourceManager.Open(visaAddress);
                _saSession = visaSession as IMessageBasedSession;

                if (_saSession == null) throw new Exception("无法创建VISA会话。");

                // =========================================================
                // **核心修正：适配 INSTR 协议的关键配置**
                // =========================================================
                _saSession.TimeoutMilliseconds = 3000; // 连接测试时给3秒足矣

                // INSTR 协议标准配置：开启 EOI (SendEnd)
                _saSession.SendEndEnabled = true; // <--- 必须为 true！

                // 启用终止符 (读数据时遇到 \n 就停止)
                _saSession.TerminationCharacterEnabled = true;
                _saSession.TerminationCharacter = (byte)'\n';
                // =========================================================

                // 握手测试
                _saSession.FormattedIO.WriteLine("*IDN?");
                string idn = _saSession.FormattedIO.ReadLine();

                // 握手成功，恢复长超时
                _saSession.TimeoutMilliseconds = 30000;
                IsConnected = true;
            });
        }

        public void Disconnect()
        {
            if (_saSession != null)
            {
                try { _saSession.Dispose(); } catch { }
                _saSession = null;
            }
            IsConnected = false;
        }

        public async Task<double> GetMeasurementValueAsync(int delayMs)
        {
            if (!IsConnected || _saSession == null) throw new InvalidOperationException("频谱仪未连接");

            return await Task.Run(() =>
            {
                var formattedIO = _saSession.FormattedIO;

                // 发送指令
                formattedIO.WriteLine(":TRAC:TYPE MAXH;");
                Thread.Sleep(delayMs);
                formattedIO.WriteLine(":CALC:MARK:MAX;");
                formattedIO.WriteLine(":CALC:MARK:Y?;");

                // 读取结果 (因为设置了 TerminationCharacter，这里就不会超时了)
                return formattedIO.ReadLineDouble();
            });
        }
    }
}