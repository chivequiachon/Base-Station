using System;
using System.Threading;
using System.Timers;

namespace BaseStationTest
{
    public class BreakdownTimer
    {
        private System.Timers.Timer decrementingTimer;
        private System.Timers.Timer hourlyTimer;

        private AutoResetEvent timerSemaphore;
        private String globalKey;
        private int lastHour;

        private Boolean disableSemaphore;

        private double decrementingTime;
        private double tickTime;

        public BreakdownTimer(uint decrementingTime, double tickTime)
        {
            this.decrementingTime = decrementingTime;
            this.tickTime = tickTime;

            timerSemaphore = new AutoResetEvent(false);
            decrementingTimer = new System.Timers.Timer(decrementingTime);
            hourlyTimer = new System.Timers.Timer(tickTime);

            decrementingTimer.Enabled = false;
            hourlyTimer.Enabled = false;

            globalKey = null;
            lastHour = 0;
            disableSemaphore = false;
        }

        public BreakdownTimer() : this(0, 0)
        {
        }

        public Boolean DisableSemaphore
        {
            set { this.disableSemaphore = value; }
            get { return this.disableSemaphore; }
        }

        public ElapsedEventHandler DecrementEvent
        {
            set { decrementingTimer.Elapsed += value; }
        }

        public ElapsedEventHandler TimerEvent
        {
            set { hourlyTimer.Elapsed += value; }
        }

        public double DecrementingTime
        {
            set
            {
                decrementingTime = value;
                decrementingTimer.Interval = decrementingTime;
            }

            get { return decrementingTime; }
        }

        public double TickTime
        {
            set
            {
                tickTime = value;
                hourlyTimer.Interval = tickTime;
            }

            get { return tickTime; }
        }

        public Boolean EnableDecrementEvent
        {
            set { decrementingTimer.Enabled = value; }
            get { return decrementingTimer.Enabled; }
        }

        public Boolean EnableTimerEvent
        {
            set { hourlyTimer.Enabled = value; }
            get { return hourlyTimer.Enabled; }
        }

        public int LastHour
        {
            set { lastHour = value; }
            get { return lastHour; }
        }

        public String GlobalKey
        {
            set { globalKey = value; }
            get { return globalKey; }
        }

        public void Set()
        {
            if(!disableSemaphore)
                timerSemaphore.Set();
        }

        public void Reset()
        {
            if(!disableSemaphore)
                timerSemaphore.Reset();
        }

        public Boolean WaitOne(int time)
        {
            Boolean res = !disableSemaphore ? timerSemaphore.WaitOne(time) : false;
            return res;
        }
    };
}