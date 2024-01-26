namespace _RUDP_
{
    public class Signal
    {
        readonly RudpChannel channel;
        static byte id;
        readonly string name;
        bool isSet;
        readonly object locker = new();

        //----------------------------------------------------------------------------------------------------------

        public Signal(in RudpChannel channel)
        {
            this.channel = channel;
            name = $"{{ {channel} }} (id:{++id})";
            Console.WriteLine($"{name} Created");
        }

        //----------------------------------------------------------------------------------------------------------

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
                    Console.WriteLine($"{name} Set to {value}");
                    isSet = value;
                    if (isSet)
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