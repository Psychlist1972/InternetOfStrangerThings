using Microsoft.Maker.RemoteWiring;
using Microsoft.Maker.Serial;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409


using PeteBrown.Devices.Midi;
using Windows.Media.SpeechRecognition;
using Windows.UI;
using Windows.Devices.Midi;


// This code assumes the following:
// 1. You have an Elektron Analog 4 with a Stranger Things music sequence ready to play. Won't crash or fail if this is not present.
// 2. A bot set up as per instructions on GitHub
// 3. A Big-ass wall with Christmas lights set up as per the blog series. :)


namespace AlphabetRemoteArduino
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            Loaded += MainPage_Loaded;
        }

        RemoteDevice _arduino;
        UsbSerial _serial;

        private const string _vid = "VID_2341";
        private const string _pid = "PID_0043";

        private const int _latchPin = 7;            // LE
        private const int _outputEnablePin = 8;     // OE
        private const int _sdiPin = 3;              // SDI
        private const int _clockPin = 4;            // CLK

        private SolidColorBrush _micListeningBrush = new SolidColorBrush(Colors.SkyBlue);
        private SolidColorBrush _micIdleBrush = new SolidColorBrush(Colors.White);



        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeWiring();

            SetupSpeechRecognizer();

            InitializeMidi();

            InitializeBot();
        }

        // This is there the Arduino connection is set up via
        // Windows Remote Wiring. The Library is via NuGet
        private void InitializeWiring()
        {
            _serial = new UsbSerial(_vid, _pid);
            _arduino = new RemoteDevice(_serial);

            _serial.ConnectionEstablished += OnSerialConnectionEstablished;

            _serial.begin(57600, SerialConfig.SERIAL_8N1);

            System.Diagnostics.Debug.WriteLine("Wiring initialized");
        }

        // Once connection has been established with the Arduino, set up the pins
        private void OnSerialConnectionEstablished()
        {

            //_arduino.pinMode(_sdiPin, PinMode.I2C);
            _arduino.pinMode(_sdiPin, PinMode.OUTPUT);
            _arduino.pinMode(_clockPin, PinMode.OUTPUT);
            _arduino.pinMode(_latchPin, PinMode.OUTPUT);
            _arduino.pinMode(_outputEnablePin, PinMode.OUTPUT);

            _arduino.digitalWrite(_outputEnablePin, PinState.HIGH); // turn off all LEDs

            ClearBoard();

            System.Diagnostics.Debug.WriteLine("Wiring connection established");
        }


        // This table maps the shift register values to the individual letter lights
        private UInt32[] _letterTable = new UInt32[]
        {
            0x80000000, // A
            0x40000000, // B
            0x20000000, // C
            0x10000000, // D
            0x08000000, // E
            0x04000000, // F
            0x02000000, // G
            0x01000000, // H
            0x00800000, // I
            0x00400000, // J
            0x00200000, // K
            0x00100000, // L
            0x00080000, // M
            0x00040000, // N
            0x00020000, // O
            0x00010000, // P
            0x00008000, // Q
            0x00004000, // R
            0x00002000, // S
            0x00001000, // T
            0x00000800, // U
            0x00000400, // V
            0x00000200, // W
            0x00000100, // X
            0x00000080, // Y
            0x00000040, // Z
        };

        // clear all the lights
        private const UInt32 _clearValue = 0x0;        
        private async void ClearBoard()
        {
            // clear it out
            await SendUInt32Async(_clearValue, 0);

        }

        // send 32 bits out by bit-banging them with a software clock
        private async Task SendUInt32Async(UInt32 bitmap, int outputDurationMs)
        {
            for (int i = 0; i < 32; i++)
            {
                // clock low
                _arduino.digitalWrite(_clockPin, PinState.LOW);

                // get the next bit to send
                var b = bitmap & 0x01;

                if (b > 0)
                {
                    // send 1 value

                    _arduino.digitalWrite(_sdiPin, PinState.HIGH);
                }
                else
                {
                    // send 0 value
                    _arduino.digitalWrite(_sdiPin, PinState.LOW);
                }

                // clock high
                _arduino.digitalWrite(_clockPin, PinState.HIGH);

                await Task.Delay(1);    // this is an enormous amount of time, of course. There are faster timers/delays you can use.

                // shift the bitmap to prep for getting the next bit
                bitmap >>= 1;
            }

            // latch
            _arduino.digitalWrite(_latchPin, PinState.HIGH);
            await Task.Delay(1);
            _arduino.digitalWrite(_latchPin, PinState.LOW);
            
            // turn on LEDs
            _arduino.digitalWrite(_outputEnablePin, PinState.LOW);

            // keep the LEDs on for the specified duration
            if (outputDurationMs > 0)
                await Task.Delay(outputDurationMs);

            // turn the LEDs off
            _arduino.digitalWrite(_outputEnablePin, PinState.HIGH);

        }

        // Render on the LEDs any given text string
        public async Task RenderTextAsync(string message, int onDurationMs = 500, int delayMs = 0, int whitespacePauseMs = 500)
        {
            message = message.ToUpper().Trim();

            byte[] asciiValues = Encoding.ASCII.GetBytes(message);

            int asciiA = Encoding.ASCII.GetBytes("A")[0];

            for (int i = 0; i < message.Length; i++)
            {
                char ch = message[i];

                if (char.IsWhiteSpace(ch))
                {
                    // pause
                    if (whitespacePauseMs > 0)
                        await Task.Delay(whitespacePauseMs);
                }
                else if (char.IsLetter(ch))
                {
                    byte val = asciiValues[i];
                    int ledIndex = val - asciiA;

                    UInt32 bitmap = _letterTable[ledIndex];

                    System.Diagnostics.Debug.WriteLine("Processing Character: {0}, val {1}, ledIndex {2}", ch, val, ledIndex);

                    // send the letter
                    await SendUInt32Async(bitmap, onDurationMs);

                    // clear it out
                    await SendUInt32Async(_clearValue, 0);

                    if (delayMs > 0)
                        await Task.Delay(delayMs);

                }
                else
                {
                    // unsupported character. Ignore
                }
            }

            System.Diagnostics.Debug.WriteLine("Message render complete.");

        }




        // This is for testing your wall
        private async void TestSendAlphabetButton_Click(object sender, RoutedEventArgs e)
        {
            await RenderTextAsync("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
        }

        private async void TestNameButton_Click(object sender, RoutedEventArgs e)
        {
            await RenderTextAsync("PETE");
        }

        private async void TestRunButton_Click(object sender, RoutedEventArgs e)
        {
            await RenderTextAsync("RUN");
        }










        // Use two speech recognizers: one for the simple echo, the other for sending
        // to the bot. The echo one is used in cases where network access is down.
        SpeechRecognizer _echoSpeechRecognizer;
        SpeechRecognizer _questionSpeechRecognizer;

        private async void SetupSpeechRecognizer()
        {
            _echoSpeechRecognizer = new SpeechRecognizer();
            _questionSpeechRecognizer = new SpeechRecognizer();

            await _echoSpeechRecognizer.CompileConstraintsAsync();
            await _questionSpeechRecognizer.CompileConstraintsAsync();

            _echoSpeechRecognizer.HypothesisGenerated += OnEchoSpeechRecognizerHypothesisGenerated;
            _echoSpeechRecognizer.StateChanged += OnEchoSpeechRecognizerStateChanged;

            _questionSpeechRecognizer.HypothesisGenerated += OnQuestionSpeechRecognizerHypothesisGenerated;
            _questionSpeechRecognizer.StateChanged += OnQuestionSpeechRecognizerStateChanged;

        }








        private async void DictateEcho_Click(object sender, RoutedEventArgs e)
        {
            var result = await _echoSpeechRecognizer.RecognizeAsync();

            EchoText.Text = result.Text;
        }

        private async void OnEchoSpeechRecognizerStateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                switch (args.State)
                {
                    case SpeechRecognizerState.Idle:
                        //  DictateEcho.IsEnabled = true;
                        EchoText.Background = _micIdleBrush;
                        break;

                    default:
                        // DictateEcho.IsEnabled = false;
                        EchoText.Background = _micListeningBrush;
                        break;
                }
            });
        }

        private async void OnEchoSpeechRecognizerHypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                EchoText.Text = args.Hypothesis.Text;
            });
        }

        private async void SendEcho_Click(object sender, RoutedEventArgs e)
        {
            await RenderTextAsync(EchoText.Text.Trim());
        }







        private async void DictateQuestion_Click(object sender, RoutedEventArgs e)
        {
            var result = await _questionSpeechRecognizer.RecognizeAsync();

            QuestionText.Text = FormatQuestion(result.Text);

        }

        private string FormatQuestion(string questionText)
        {
            if (questionText.Length >= 2)
            {
                return char.ToUpper(questionText[0]) + questionText.Substring(1) + "?";
            }
            else
            {
                return questionText;
            }
        }

        private async void OnQuestionSpeechRecognizerHypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                QuestionText.Text = FormatQuestion(args.Hypothesis.Text);
            });
        }

        private async void OnQuestionSpeechRecognizerStateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                switch (args.State)
                {
                    case SpeechRecognizerState.Idle:
                        QuestionText.Background = _micIdleBrush;
                        break;

                    default:
                        QuestionText.Background = _micListeningBrush;
                        break;
                }
            });
        }


        // this is where the fun happens.
        // Start the MIDI sequence
        // Send the question to the bot, and wait for a response
        // Render the response to the wall
        // Stop the MIDI sequence
        private async void SendQuestion_Click(object sender, RoutedEventArgs e)
        {
            // start music
            StartMidiClock();

            // send question to service
            var response = await _botInterface.TalkToTheUpsideDownAsync(QuestionText.Text);

            // display answer
            await RenderTextAsync(response);

            // stop music
            StopMidiClock();
        }








        private BotInterface _botInterface;

        private async void InitializeBot()
        {
            _botInterface = new BotInterface();

            await _botInterface.ConnectAsync();
        }









        private MidiClockGenerator _midiClock;
        private MidiDeviceWatcher _midiWatcher;

        private void InitializeMidi()
        {
            _midiClock = new MidiClockGenerator();
            _midiWatcher = new MidiDeviceWatcher();

            _midiClock.SendMidiStartMessage = true;
            _midiClock.SendMidiStopMessage = true;

            _midiClock.Tempo = 84;


            _midiWatcher.IgnoreBuiltInWavetableSynth = true;

            _midiWatcher.EnumerateOutputPorts();

        }

        private void StartMidiClock()
        {
            // I do this every time rather than listen for device add/remove
            // becuase my library didn't raise the add/remove event in this version
            SelectMidiOutputDevices();

            _midiClock.Start();

            System.Diagnostics.Debug.WriteLine("MIDI started");
        }

        private void StopMidiClock()
        {
            _midiClock.Stop();

            System.Diagnostics.Debug.WriteLine("MIDI stopped");
        }

        // Change this if there's a different device name to look for in your case.
        // To find the device names, look at the descriptor name when enumerated below
        private const string _midiDeviceName = "Analog Four";
        private async void SelectMidiOutputDevices()
        {
            _midiClock.OutputPorts.Clear();

            IMidiOutPort port = null;

            foreach (var descriptor in _midiWatcher.OutputPortDescriptors)
            {
                System.Diagnostics.Debug.WriteLine(descriptor.Name);

                if (descriptor.Name.Contains(_midiDeviceName))
                {
                    port = await MidiOutPort.FromIdAsync(descriptor.Id);

                    System.Diagnostics.Debug.WriteLine("Found " + _midiDeviceName);

                    break;
                }
            }

            if (port != null)
            {
                _midiClock.OutputPorts.Add(port);
                System.Diagnostics.Debug.WriteLine("Added " + _midiDeviceName);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Cound not open " + _midiDeviceName);
            }

        }

        // for testing MIDI

        private void ToggleMidi_Checked(object sender, RoutedEventArgs e)
        {
            StartMidiClock();
        }

        private void ToggleMidi_Unchecked(object sender, RoutedEventArgs e)
        {
            StopMidiClock();
        }
    }
}
