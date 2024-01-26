namespace _RUDP_
{
    public class Signal
    {
        private bool isSet;
        private readonly object locker = new object();

        public bool IsSet
        {
            get
            {
                lock (locker)
                    return isSet;
            }
            set
            {
                lock (locker)
                {
                    isSet = value;
                    if (isSet) // Only notify if the signal is set to true.
                        Monitor.PulseAll(locker);
                }
            }
        }

        public void Wait()
        {
            lock (locker)
            {
                while (!isSet)
                    Monitor.Wait(locker);
            }
        }

        public void Set()
        {
            IsSet = true;
        }

        public void Reset()
        {
            IsSet = false;
        }
    }
}