using System;
using System.Threading;
using System.Threading.Tasks;

namespace LogMonitor.Watcher
{
    abstract class BaseWatcher
    {
        private Thread _thread;
        private AutoResetEvent _autoResetEvent;

        protected BaseWatcher()
        {
            _thread = new Thread(new ThreadStart(this.RunThread));
            _autoResetEvent = new AutoResetEvent(false);
        }

        public abstract void RunThread();

        public void Start() => this._thread.Start();
        public void Join() => this._thread.Join();
        public bool IsAlive => this._thread.IsAlive;
        public void SetSignal() => this._autoResetEvent.Set();
        public void Wait() => this._autoResetEvent.WaitOne();
    }
}
