using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
namespace BrainFuckInterpreter
{
    
    public class Interpreter
    {
        public delegate void BFEvent(string msg, int codePos, int dataPos, byte dataValue);
        public event BFEvent OnError;
        public event BFEvent OnLog;
        public event BFEvent OnDebug;
        public event BFEvent OnMemoryChanged;
        public event BFEvent OnFinished;
        public const int MemorySize = Int16.MaxValue;
        bool stop = false;
        public Interpreter(string code, string input)
        {
            Thread t = new Thread(() => { InterpretInternal(code, input); });
            t.Start();
        }

        public void Stop()
        {
            stop = true;
        }
        private void InterpretInternal(string code, string input)
        {
            stop = false;
            Dictionary<int, int> loopStarts = new Dictionary<int, int>();
            Dictionary<int, int> loopEnds = new Dictionary<int, int>();
            Stack<int> open = new Stack<int>();
            for(int i = 0; i < code.Length; i++)
            {
                if(code[i] == '[')
                {
                    open.Push(i);
                }
                else if(code[i] == ']')
                {
                    if(open.Count < 1)
                    {
                        if (OnError != null)
                            OnError("Parenthesis mismatch", i, 0, 0);
                        return;
                    }
                    else
                    {
                        int start = open.Pop();
                        loopStarts.Add(start, i);
                        loopEnds.Add(i, start);
                    }
                }
            }
            if(open.Count > 0)
            {
                if (OnError != null)
                    OnError("Parenthesis mismatch", open.Pop(), 0, 0);
                return;
            }
            int c = 0;
            int d = 0;
            int inpAt = 0;
            byte[] data = new byte[MemorySize];
            if (OnMemoryChanged != null)
                OnMemoryChanged("Memory", c, d, data[d]);
            while (c < code.Length && !stop)
            {
                switch (code[c])
                {
                    case '+':
                        data[d]++;
                        if(OnMemoryChanged != null)
                            OnMemoryChanged("Memory", c, d, data[d]);
                        break;
                    case '-':
                        data[d]--;
                        if(OnMemoryChanged != null)
                            OnMemoryChanged("Memory", c, d, data[d]);
                        break;
                    case '<':
                        d = Dec(d);
                        break;
                    case '>':
                        d = Inc(d);
                        break;
                    case '[':
                        if (data[d] == 0)
                            c = loopStarts[c];
                        
                        break;
                    case ']':
                        c = loopEnds[c] - 1;
                        break;
                    case '.':
                        if (OnLog != null)
                            OnLog("Log", c, d, data[d]);
                        break;
                    case ',':
                        if (inpAt < input.Length)
                            data[d] = (byte)input[inpAt++];
                        else
                            data[d] = 0;
                        if (OnMemoryChanged != null)
                            OnMemoryChanged("Memory", c, d, data[d]);
                        break;
                    case '#':
                        if (OnDebug != null)
                            OnDebug("Debug", c, d, data[d]);
                        break;
                    default:
                        break;
                }
                c++;
            }
            if (OnFinished != null)
                OnFinished("Finished", c, d, data[d]);
        }
        private static int Inc(int at)
        {
            return (at+1) % MemorySize;
        }
        private static int Dec(int at)
        {
            at -= 1;
            if (at < 0)
                at = MemorySize + at;
            return at;
        }
    }
}
