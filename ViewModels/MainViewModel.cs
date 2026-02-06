using System.Windows;
using nsquare.Models;
using nsquare.Services;

namespace nsquare.ViewModels;

public class MainViewModel : ViewModelBase, IDisposable
{
    private readonly UdpReceiver _udpReceiver;
    private HandleData? _currentHandleData;

    public HandleData? CurrentHandleData
    {
        get => _currentHandleData;
        private set => SetProperty(ref _currentHandleData, value);
    }

    private double _azimuth;
    public double Azimuth
    {
        get => _azimuth;
        private set => SetProperty(ref _azimuth, value);
    }

    private double _elevation;
    public double Elevation
    {
        get => _elevation;
        private set => SetProperty(ref _elevation, value);
    }

    private bool _isFiring;
    public bool IsFiring
    {
        get => _isFiring;
        private set => SetProperty(ref _isFiring, value);
    }

    private double _voltage;
    public double Voltage
    {
        get => _voltage;
        private set => SetProperty(ref _voltage, value);
    }

    public MainViewModel()
    {
        _udpReceiver = new UdpReceiver(9999);
        _udpReceiver.DataReceived += OnDataReceived;
        _udpReceiver.Start();
    }

    private void OnDataReceived(object? sender, HandleData data)
    {
        // UI 스레드에서 속성 업데이트
        Application.Current.Dispatcher.Invoke(() =>
        {
            CurrentHandleData = data;
            Azimuth = data.Azimuth;
            Elevation = data.Elevation;
            IsFiring = data.IsFiring;
            Voltage = data.Voltage;
        });
    }

    public void Dispose()
    {
        _udpReceiver.DataReceived -= OnDataReceived;
        _udpReceiver.Dispose();
    }
}
