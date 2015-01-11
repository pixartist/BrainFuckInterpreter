using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BrainFuckInterpreter;
using System.Diagnostics;
namespace BrainFuck
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string logbuf;
        private int maxMem = 0;
        private uint[] memoryBuf;
        Stopwatch sw;
        Stopwatch swm;
        Interpreter interpreter;
        public MainWindow()
        {
            InitializeComponent();
            sw = new Stopwatch();
            swm = new Stopwatch();
        }

        void Interpreter_OnMemoryChanged(string msg, int codePos, int dataPos, byte dataValue)
        {
            if (dataPos > maxMem)
                maxMem = dataPos;
            memoryBuf[dataPos] = dataValue;
            if (swm.ElapsedMilliseconds > maxMem/4)
            {
                DisplayMemory();
            }
        }

        void Interpreter_OnLog(string msg, int codePos, int dataPos, byte dataValue)
        {
            logbuf += "" + (char)dataValue;
            WriteLog();
        }

        void Interpreter_OnError(string msg, int codePos, int dataPos, byte dataValue)
        {
            logbuf += Environment.NewLine + "Error: " + msg + " at pos: " + codePos + " ,data at " + dataPos + ": " + (char)dataValue + "(" + dataValue.ToString() + ")" + Environment.NewLine;
            SelCode(codePos, 1);
            DisplayMemory();
            WriteOut();
        }

        void Interpreter_OnDebug(string msg, int codePos, int dataPos, byte dataValue)
        {
            logbuf += Environment.NewLine + "Debug: " + msg + " at pos: " + codePos + " ,data at " + dataPos + ": " + (char)dataValue + "(" + dataValue.ToString() + ")" + Environment.NewLine;
            logbuf += WriteMem() + Environment.NewLine;
            WriteLog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (interpreter != null)
                interpreter.Stop();
            string c = code.Text;
            string inp = input.Text;
            logbuf = "";
            output.Text = "";
            sw.Start();
            swm.Start();
            interpreter = new Interpreter(c, inp);
            interpreter.OnDebug += Interpreter_OnDebug;
            interpreter.OnError += Interpreter_OnError;
            interpreter.OnLog += Interpreter_OnLog;
            interpreter.OnFinished += interpreter_OnFinished;
            interpreter.OnMemoryChanged += Interpreter_OnMemoryChanged;
            memoryBuf = new uint[Interpreter.MemorySize];
            maxMem = 16;
        }

        void interpreter_OnFinished(string msg, int codePos, int dataPos, byte dataValue)
        {
            //logbuf += Environment.NewLine + "Done";
            DisplayMemory();
            WriteOut();
        }
        void SelCode(int at, int l)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(() => { SelCode(at, l); }));
            }
            else
            {
                code.Focus();
                code.Select(at, l);
            }
        }
        void WriteLog()
        {
            if (sw.ElapsedMilliseconds > 500)
            {
                WriteOut();
                sw.Restart();
            }
        }
        void WriteOut()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(() => { WriteOut(); }));
            }
            else
            {
                string s = output.Text + logbuf;
                if(autoBreak.IsChecked.Value && s.Length > 5)
                {
                    int w = 60;
                    int b = 0;
                    int k = s.IndexOf(Environment.NewLine, b + 1);
                    while(true)
                    {
                        if ((s.Length - b > w && k < 0) || k-b > w)
                        {
                            b += w;
                            s = s.Insert(b, Environment.NewLine);
                            b += Environment.NewLine.Length;
                            k = s.IndexOf(Environment.NewLine, b);
                        }
                        else if(k > 0)
                        {
                            b = k + Environment.NewLine.Length;
                            k = s.IndexOf(Environment.NewLine, b);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                output.Text = s;
                logbuf = "";
                output.ScrollToEnd();
            }

        }
        void DisplayMemory()
        {


            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(() => { DisplayMemory(); }));
            }
            else
            {

                memory.Text = WriteMem();
            }
            swm.Restart();

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (interpreter != null)
                interpreter.Stop();
        }
        string WriteMem()
        {
            StringBuilder num = new StringBuilder();
            StringBuilder str = new StringBuilder();
            StringBuilder hex = new StringBuilder();
            for (int i = 0; i < maxMem; i++)
            {
                num.Append(i.ToString("D3") + " ");
                char c = (char)memoryBuf[i];
                if (Char.IsLetterOrDigit(c) || Char.IsPunctuation(c) || Char.IsSymbol(c))
                    str.Append("  " + c + " ");
                else
                    str.Append(memoryBuf[i].ToString("D3") + " ");
                hex.Append("X" + memoryBuf[i].ToString("X2") + " ");

            }
            return num.ToString() + Environment.NewLine + str.ToString() + Environment.NewLine + hex.ToString();
        }
    }
}
