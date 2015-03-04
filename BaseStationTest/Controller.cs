using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseStationTest
{
    public class Controller
    {
        private View v;
        private Model m;

        public Controller(Model m)
        {
            this.m = m;
            this.v = new View(m, this);
            this.m.InitializeComponents();
            this.v.Run();
        }

        public void HandleResumeEvent()
        {
            m.Resume();
        }

        public void HandleActivationEvent(String byte1, String byte2, String byte3, String byte4,
                                          String port, String dbHost, String dbPort)
        {
            byte m1, m2, m3, a1;
            if (!byte.TryParse(byte1, out m1) ||
                !byte.TryParse(byte2, out m2) ||
                !byte.TryParse(byte3, out m3) ||
                !byte.TryParse(byte4, out a1))
            {
                v.ShowErrorDialog("Invalid Value", "Please input a valid number ranging from 1 to 255.");
                return;
            }

            if (m1 < 1 || m2 < 1 || m3 < 1 || a1 < 1)
            {
                v.ShowErrorDialog("Invalid Value", "Please input a valid number ranging from 1 to 255.");
                return;
            }

            if (dbHost == "")
            {
                v.ShowErrorDialog("Invalid DB Host", "Please input Database host.");
                return;
            }

            if (dbPort == "")
            {
                v.ShowErrorDialog("Invalid DB Port", "Please input Database port.");
                return;
            }

            m.Invoke(m1, m2, m3, a1, port, dbHost, Convert.ToInt32(dbPort));
        }

        public void HandleDeactivationEvent()
        {
            m.Stop();
        }

        public void HandlePauseEvent()
        {
            m.Pause();
        }

        public void HandleClosingEvent()
        {
            m.Exit();
        }
    }
}
