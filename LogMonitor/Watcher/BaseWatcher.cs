using System;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogMonitor.Watcher
{
    public abstract class BaseWatcher
    {
        protected Thread _thread;
        private AutoResetEvent _autoResetEvent;
        protected StringBuilder _sb;
        protected const string _consolidatedLogFilePath = @"C:\Windows\Temp\consolidated log\consolidatedLog.log";
        protected BaseWatcher()
        {
            _thread = new Thread(new ThreadStart(this.RunThread));
            _autoResetEvent = new AutoResetEvent(false);
            _sb = new StringBuilder();

        }

        public abstract void RunThread();

        public void Start() => this._thread.Start();
        public void Join() => this._thread.Join();
        public bool IsAlive => this._thread.IsAlive;
        public void SetSignal() => this._autoResetEvent.Set();
        public void Wait() => this._autoResetEvent.WaitOne();
        public virtual void Finish()
        {
            if(IsAlive)
            {
                SetSignal();
                Join();
            }
           
        }
    }
}
