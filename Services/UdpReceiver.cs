using System.Net;
using System.Net.Sockets;
using nsquare.Models;

namespace nsquare.Services;

public class UdpReceiver : IDisposable
{
    private readonly UdpClient _udpClient;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _receiveTask;
    private readonly int _port;

    public event EventHandler<HandleData>? DataReceived;

    public UdpReceiver(int port = 9999)
    {
        _port = port;
        _udpClient = new UdpClient(_port);
    }

    public void Start()
    {
        if (_receiveTask != null)
            return;

        _cancellationTokenSource = new CancellationTokenSource();
        _receiveTask = Task.Run(() => ReceiveDataAsync(_cancellationTokenSource.Token));
    }

    public void Stop()
    {
        _cancellationTokenSource?.Cancel();
        _receiveTask?.Wait();
        _receiveTask = null;
    }

    private async Task ReceiveDataAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = await _udpClient.ReceiveAsync(cancellationToken);
                var data = result.Buffer;

                if (data.Length == 8 && data[0] == 0xAA)
                {
                    var handleData = ParseHandleData(data);
                    DataReceived?.Invoke(this, handleData);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UDP 수신 오류: {ex.Message}");
            }
        }
    }

    private HandleData ParseHandleData(byte[] data)
    {
        // data[0] = 0xAA (헤더)
        // data[1-2] = Azimuth (2 bytes, little-endian, 0~360도를 0~65535로 매핑)
        // data[3-4] = Elevation (2 bytes, little-endian, 0~90도를 0~65535로 매핑)
        // data[5] = IsFiring (1 byte, 0 or 1)
        // data[6-7] = Voltage (2 bytes, little-endian, 0~50V를 0~65535로 매핑)

        ushort azimuthRaw = BitConverter.ToUInt16(data, 1);
        ushort elevationRaw = BitConverter.ToUInt16(data, 3);
        bool isFiring = data[5] != 0;
        ushort voltageRaw = BitConverter.ToUInt16(data, 6);

        double azimuth = azimuthRaw * 360.0 / 65535.0;
        double elevation = elevationRaw * 90.0 / 65535.0;
        double voltage = voltageRaw * 50.0 / 65535.0;

        return new HandleData(azimuth, elevation, isFiring, voltage);
    }

    public void Dispose()
    {
        Stop();
        _udpClient?.Close();
        _udpClient?.Dispose();
        _cancellationTokenSource?.Dispose();
    }
}
