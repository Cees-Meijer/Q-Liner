using System.Collections.ObjectModel;

namespace MauiApp1.Services
{
    public interface IBluetoothSerialService
    {
        Task<bool> ConnectAsync(string deviceAddress);
        Task<bool> DisconnectAsync();
        Task<bool> SendDataAsync(string data);
        Task<string> ReceiveDataAsync();
        bool IsConnected { get; }
    }

#if ANDROID
    public class BluetoothSerialService : IBluetoothSerialService
    {
        private Android.Bluetooth.BluetoothSocket? _socket;
        private Android.Bluetooth.BluetoothDevice? _device;
        private System.IO.Stream? _inputStream;
        private System.IO.Stream? _outputStream;
        private bool _isConnected;
        private const string SerialPortUUID = "00001101-0000-1000-8000-00805F9B34FB";

        public bool IsConnected => _isConnected;

        public async Task<bool> ConnectAsync(string deviceAddress)
        {
            try
            {
                if (_isConnected)
                    await DisconnectAsync();

                // Request runtime permissions for Android 12+
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.S)
                {
                    var scanStatus = await Permissions.RequestAsync<Permissions.Bluetooth>();
                    if (scanStatus != PermissionStatus.Granted)
                    {
                        System.Diagnostics.Debug.WriteLine("Bluetooth permissions not granted");
                        return false;
                    }
                }

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
                    return false;

                _device = bluetoothAdapter.GetRemoteDevice(deviceAddress);
                if (_device == null)
                    return false;

                var uuid = Java.Util.UUID.FromString(SerialPortUUID);
                _socket = _device.CreateRfcommSocketToServiceRecord(uuid);

                if (_socket == null)
                    return false;

                // Cancel discovery before connecting
                bluetoothAdapter.CancelDiscovery();

                // Connect
                await _socket.ConnectAsync();

                _inputStream = _socket.InputStream;
                _outputStream = _socket.OutputStream;
                _isConnected = true;

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Bluetooth connection error: {ex.Message}");
                _isConnected = false;
                return false;
            }
        }

        public async Task<bool> DisconnectAsync()
        {
            try
            {
                _inputStream?.Close();
                _outputStream?.Close();
                _socket?.Close();
                _isConnected = false;
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Bluetooth disconnection error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendDataAsync(string data)
        {
            try
            {
                if (!_isConnected || _outputStream == null)
                    return false;

                var bytes = System.Text.Encoding.UTF8.GetBytes(data);
                _outputStream.Write(bytes, 0, bytes.Length);
                await _outputStream.FlushAsync();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Bluetooth send error: {ex.Message}");
                return false;
            }
        }

        public async Task<string> ReceiveDataAsync()
        {
            try
            {
                if (!_isConnected || _inputStream == null)
                    return string.Empty;

                var buffer = new byte[1024];
                var bytesRead = await _inputStream.ReadAsync(buffer, 0, buffer.Length);
                return System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Bluetooth receive error: {ex.Message}");
                return string.Empty;
            }
        }
    }
#else
    public class BluetoothSerialService : IBluetoothSerialService
    {
        public bool IsConnected => false;

        public async Task<bool> ConnectAsync(string deviceAddress)
        {
            return false;
        }

        public async Task<bool> DisconnectAsync()
        {
            return false;
        }

        public async Task<bool> SendDataAsync(string data)
        {
            return false;
        }

        public async Task<string> ReceiveDataAsync()
        {
            return string.Empty;
        }
    }
#endif
}