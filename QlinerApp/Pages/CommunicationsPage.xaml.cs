using System.Collections.ObjectModel;
using System.IO.Ports;

namespace MauiApp1;

public partial class CommunicationsPage : ContentPage
{
    public ObservableCollection<string> AvailableSerialPorts { get; }
    private string _selectedSerialPort = "NONE";
    private SerialPort? _serialPort;
    private CancellationTokenSource? _readCancellationTokenSource;
    private bool _isReading = false;

    public string SelectedSerialPort
    {
        get => _selectedSerialPort;
        set => _selectedSerialPort = value;
    }

    public CommunicationsPage()
    {
        InitializeComponent();
        AvailableSerialPorts = new ObservableCollection<string>();

        // Set the binding context to this page
        BindingContext = this;

        // Initialize with some sample text

        communicationLogLabel.Text = "Waiting for communication...\n";

        RefreshSerialPorts();
    }

    private void OnRefreshClicked(object sender, EventArgs e)
    {
        AppendLog("Refreshing serial ports...");
        RefreshSerialPorts();
    }

    private void OnOpenClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_selectedSerialPort) ||
            _selectedSerialPort.Contains("No") ||
            _selectedSerialPort.Contains("not") ||
            _selectedSerialPort.Contains("Error"))
        {
            AppendLog("Error: Please select a valid port");
            return;
        }

        if (baudRatePicker.SelectedIndex < 0)
        {
            AppendLog("Error: Please select a baud rate");
            return;
        }

        // Get the selected baud rate
        string baudRateString = baudRatePicker.SelectedItem.ToString();
        int baudRate = int.Parse(baudRateString);

        AppendLog($"Opening port: {_selectedSerialPort} at {baudRate} baud");

        try
        {
            _serialPort = new SerialPort(_selectedSerialPort)
            {
                BaudRate = baudRate,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                ReadTimeout = 500,
                WriteTimeout = 500
            };

            _serialPort.Open();

            AppendLog($"Successfully opened {_selectedSerialPort} at {baudRate} baud");
            openButton.IsEnabled = false;
            closeButton.IsEnabled = true;
            refreshButton.IsEnabled = false;
            serialPortPicker.IsEnabled = false;
            baudRatePicker.IsEnabled = false;

            // Start reading data
            StartReadingData();
        }
        catch (Exception ex)
        {
            AppendLog($"Failed to open {_selectedSerialPort}: {ex.Message}");
            _serialPort?.Dispose();
            _serialPort = null;
        }
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        AppendLog($"Closing port: {_selectedSerialPort}");

        // Stop reading data
        StopReadingData();

        try
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
                _serialPort.Dispose();
                _serialPort = null;
                AppendLog($"Successfully closed {_selectedSerialPort}");
            }
        }
        catch (Exception ex)
        {
            AppendLog($"Error closing port: {ex.Message}");
        }

        openButton.IsEnabled = true;
        closeButton.IsEnabled = false;
        refreshButton.IsEnabled = true;
        serialPortPicker.IsEnabled = true;
    }

    private void StartReadingData()
    {
        if (_isReading || _serialPort == null)
            return;

        _isReading = true;
        _readCancellationTokenSource = new CancellationTokenSource();

        Task.Run(async () =>
        {
            try
            {
                while (!_readCancellationTokenSource.Token.IsCancellationRequested &&
                       _serialPort != null &&
                       _serialPort.IsOpen)
                {
                    try
                    {
                        if (_serialPort.BytesToRead > 0)
                        {
                            string data = _serialPort.ReadExisting();

                            if (!string.IsNullOrEmpty(data))
                            {
                                // Update UI - use Dispatcher instead of MainThread
                                AppendLog($"{data}");
                            }
                        }

                        // Small delay to prevent tight loop
                        await Task.Delay(50, _readCancellationTokenSource.Token);
                    }
                    catch (TimeoutException)
                    {
                        // Normal timeout, continue
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        AppendLog($"Read error: {ex.Message}");
                        await Task.Delay(1000, _readCancellationTokenSource.Token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation, do nothing
            }
            finally
            {
                _isReading = false;
            }
        }, _readCancellationTokenSource.Token);
    }

    private void StopReadingData()
    {
        _readCancellationTokenSource?.Cancel();
        _readCancellationTokenSource?.Dispose();
        _readCancellationTokenSource = null;
        _isReading = false;
    }

    public void RefreshSerialPorts()
    {
        AvailableSerialPorts.Clear();

        try
        {
            var portNames = SerialPort.GetPortNames();
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

    public async Task<bool> SendDataAsync(string data)
    {
        if (_serialPort == null || !_serialPort.IsOpen)
            return false;

        try
        {
            await Task.Run(() => _serialPort.Write(data));
            AppendLog($"TX: {data}");
            return true;
        }
        catch (Exception ex)
        {
            AppendLog($"Send error: {ex.Message}");
            return false;
        }
    }

    // Helper method to append text to the log
    public async void AppendLog(string message)
    {
        // Use Dispatcher to ensure UI updates work even when page is not visible
        Dispatcher.Dispatch(async () =>
        {
            communicationLogLabel.Text += $"{message}\n";

            // Only auto-scroll if the checkbox is checked
            if (autoScrollCheckBox.IsChecked)
            {
                // Wait a brief moment for the label to update its size
                await Task.Delay(10);

                // Scroll to the bottom
                await logScrollView.ScrollToAsync(0, double.MaxValue, false);
            }
        });
    }

    
}