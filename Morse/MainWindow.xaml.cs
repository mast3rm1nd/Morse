using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Threading;
using System.Diagnostics;

namespace Morse
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static Dictionary<char, String> morseCode = new Dictionary<char, String>()
            {
				//alpha
                {'a' , ".-"},{'b' , "-..."},{'c' , "-.-."}, 
                {'d' , "-.."},{'e' , "."},{'f' , "..-."},
                {'g' , "--."},{'h' , "...."},{'i' , ".."},
                {'j' , ".---"},{'k' , "-.-"},{'l' , ".-.."},
                {'m' , "--"},{'n' , "-."},{'o' , "---"},
                {'p' , ".--."},{'q' , "--.-"},{'r' , ".-."},
                {'s' , "..."},{'t' , "-"},{'u' , "..-"},
                {'v' , "...-"},{'w' , ".--"},{'x' , "-..-"},
                {'y' , "-.--"},{'z' , "--.."},
                //Numbers 
                {'0' , "-----"},{'1' , ".----"},{'2' , "..---"},{'3' , "...--"},
                {'4' , "....-"},{'5' , "....."},{'6' , "-...."},{'7' , "--..."},
                {'8' , "---.."},{'9' , "----."},
            };

        /*The duration of a dash is three times the duration of a dot.
        Each dot or dash is followed by a short silence, equal to the dot duration.
        The letters of a word are separated by a space equal to three dots (one dash),
        and the words are separated by a space equal to seven dots.*/
        const int _DOT_DURATION = 120;
        const int _DASH_DURATION = 3 * _DOT_DURATION;
        const int _SILENCE_BETWEEN_BITS_DURATION = _DOT_DURATION;
        const int _SILENCE_BETWEEN_SYMBOLS_DURATION = 3 * _DOT_DURATION;
        const int _SILENCE_BETWEEN_WORDS_DURATION = 7 * _DOT_DURATION - 3 * _DOT_DURATION; // 'cuz 3 dot dur is between each symbol

        const int _BEEPING_FREQUENCY = 1000;

        const string _PLAY_TEXT = "Play";
        const string _STOP_TEXT = "Stop";


        public MainWindow()
        {
            InitializeComponent();

            //var message = "sos";

            //PlayMessage(message);

            //PlaySymbol('o');

            //Console.Beep(1000, 1000);
        }

        static void PlaySymbol(char symbol)
        {
            string code;
            if(!morseCode.TryGetValue(symbol, out code)) return;

            Debug.WriteLine("playing '" + symbol + "'");

            var index = 0;
            foreach(var bit in code)
            {
                switch(bit)
                {
                    case '.': Console.Beep(_BEEPING_FREQUENCY, _DOT_DURATION); Debug.WriteLine("."); break;
                    case '-': Console.Beep(_BEEPING_FREQUENCY, _DASH_DURATION); Debug.WriteLine("-"); break;
                    default: continue;
                }

                index++;

                if (index != code.Length) // no midbits pause after last bit
                {
                    Thread.Sleep(_SILENCE_BETWEEN_BITS_DURATION);
                    Debug.WriteLine(_SILENCE_BETWEEN_BITS_DURATION + "ms pause");
                }
            }
        }

        static Thread PlayThread;
        static string Message;
        void PlayMessage(string message)
        {
            Message = message.ToLower();

            PlayThread = new Thread(new ThreadStart(PlayThreadMethod));
            PlayThread.IsBackground = true;
            PlayThread.Start();
        }


        
        void PlayThreadMethod()
        {
            foreach(var symbol in Message)
            {
                if (symbol == ' ')
                {
                    Thread.Sleep(_SILENCE_BETWEEN_WORDS_DURATION);
                    Debug.WriteLine(_SILENCE_BETWEEN_WORDS_DURATION + "ms pause");
                }
                else
                {
                    PlaySymbol(symbol);
                    Thread.Sleep(_SILENCE_BETWEEN_SYMBOLS_DURATION);
                    Debug.WriteLine(_SILENCE_BETWEEN_SYMBOLS_DURATION + "ms pause");
                }                    
            }

            Dispatcher.BeginInvoke(new Action(delegate
            {
                PlayStop_Button.Content = _PLAY_TEXT;

                isPlaying = false;
            }));
        }

        static bool isPlaying = false;
        private void PlayStop_Button_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                PlayStop_Button.Content = _PLAY_TEXT;

                PlayThread.Abort();

                isPlaying = !isPlaying;

                return;
            }

            PlayStop_Button.Content = _STOP_TEXT;

            isPlaying = !isPlaying;

            PlayMessage(Message_TextBox.Text);
        }
    }
}
