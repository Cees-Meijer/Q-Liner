using System.Collections.ObjectModel;
using MauiApp1.Services;

namespace MauiApp1
{
    public class MainPageViewModel
    {
        public IDrawable ChartDrawable { get; }
        public ObservableCollection<(float x, float y)> DataPoints { get; }
        public ObservableCollection<string> AvailableSerialPorts { get; }
        
        private readonly IBluetoothSerialService _bluetoothService;
        private string _selectedSerialPort="NONE";
        private string _connectionStatus = "Disconnected";

        public string SelectedSerialPort
        {
            get => _selectedSerialPort;
            set => _selectedSerialPort = value;
        }

        public string ConnectionStatus
        {
            get => _connectionStatus;
            set => _connectionStatus = value;
        }

        public MainPageViewModel()
        {
            DataPoints = new ObservableCollection<(float, float)>();
            AvailableSerialPorts = new ObservableCollection<string>();
            _bluetoothService = new BluetoothSerialService();
            GenerateRandomData();
            ChartDrawable = new LineChartDrawable(DataPoints);
            RefreshSerialPorts();
        }

        public void RefreshSerialPorts()
        {
            AvailableSerialPorts.Clear();
            
            try
            {
                var portNames = GetSerialPorts();
                if (portNames.Length == 0)
                {
                    AvailableSerialPorts.Add("No ports available");
                }
                else
                {
                    foreach (string port in portNames)
                    {
                        AvailableSerialPorts.Add(port);
                    }
                }
            }
            catch (Exception ex)
            {
                AvailableSerialPorts.Add($"Error: {ex.Message}");
            }
        }

        public async Task<bool> ConnectBluetoothAsync()
        {
            if (string.IsNullOrEmpty(_selectedSerialPort) || _selectedSerialPort.Contains("No") || _selectedSerialPort.Contains("not"))
                return false;

            try
            {
                // Extract MAC address from the port string (format: "DeviceName (MAC_ADDRESS)")
                var parts = _selectedSerialPort.Split('(');
                if (parts.Length < 2)
                    return false;

                var macAddress = parts[1].TrimEnd(')');
                
                var success = await _bluetoothService.ConnectAsync(macAddress);
                ConnectionStatus = success ? $"Connected to {_selectedSerialPort}" : "Connection failed";
                return success;
            }
            catch (Exception ex)
            {
                ConnectionStatus = $"Error: {ex.Message}";
                return false;
            }
        }

        public async Task<bool> DisconnectBluetoothAsync()
        {
            var success = await _bluetoothService.DisconnectAsync();
            ConnectionStatus = success ? "Disconnected" : "Disconnection failed";
            return success;
        }

        public async Task<bool> SendDataAsync(string data)
        {
            return await _bluetoothService.SendDataAsync(data);
        }

        public async Task<string> ReceiveDataAsync()
        {
            return await _bluetoothService.ReceiveDataAsync();
        }

        private string[] GetSerialPorts()
        {
#if WINDOWS
            try
            {
                var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"HARDWARE\DEVICEMAP\SERIALCOMM");
                if (key == null)
                    return Array.Empty<string>();

                var portNames = key.GetValueNames();
                var ports = new List<string>();
                foreach (var portName in portNames)
                {
                    ports.Add(key.GetValue(portName)?.ToString() ?? "");
                }
                return ports.Where(p => !string.IsNullOrEmpty(p)).ToArray();
            }
            catch
            {
                return Array.Empty<string>();
            }
#elif ANDROID
            try
            {
                return GetAndroidBluetoothDevices();
            }
            catch
            {
                return new[] { "No Bluetooth devices found" };
            }
#else
            return new[] { "Serial ports not supported on this platform" };
#endif
        }

#if ANDROID
        private string[] GetAndroidBluetoothDevices()
        {
            try
            {
                var context = Android.App.Application.Context;
                Android.Bluetooth.BluetoothAdapter? bluetoothAdapter = null;

                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.S)
                {
                    var bluetoothManagerObj = context.GetSystemService(Android.Content.Context.BluetoothService);
                    var bluetoothManager = bluetoothManagerObj as Android.Bluetooth.BluetoothManager;
                    bluetoothAdapter = bluetoothManager?.Adapter;
                }
                else
                {
#pragma warning disable CA1422
                    bluetoothAdapter = Android.Bluetooth.BluetoothAdapter.DefaultAdapter;
#pragma warning restore CA1422
                }

                if (bluetoothAdapter == null)
                    return new[] { "Bluetooth not available" };

                if (!bluetoothAdapter.IsEnabled)
                    return new[] { "Bluetooth is disabled" };

                var devices = bluetoothAdapter.BondedDevices;
                if (devices == null || devices.Count == 0)
                    return new[] { "No paired Bluetooth devices" };

                var ports = new List<string>();
                foreach (var device in devices)
                {
                    ports.Add($"{device.Name} ({device.Address})");
                }
                return ports.ToArray();
            }
            catch (Exception ex)
            {
                return new[] { $"Error: {ex.Message}" };
            }
        }
#endif

        private void GenerateRandomData()
        {
            Random random = new Random();
            for (int i = 0; i < 20; i++)
            {
                DataPoints.Add((i, random.Next(10, 100)));
            }
        }
    }
}