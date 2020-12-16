using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.IO;
using System.Diagnostics;

namespace GeneralClicker
{
    public class ActionThread
    {
        string pathfile;
        bool _gbgEnabled;
        string _gbgStartingPosition;
        string _gbgRepeat;

        public ActionThread(string path, bool gbgEnabled, string gbgStartingPosition, string gbgRepeat)
        {
            pathfile = path;
            _gbgEnabled = gbgEnabled;
            _gbgStartingPosition = gbgStartingPosition;
            _gbgRepeat = gbgRepeat;
        }


        public void MainRun()
        {
            if (string.IsNullOrEmpty(pathfile) || !File.Exists(pathfile))
            {
                return;
            }
            RunScript(pathfile);
        }

        private void RunFile(List<string> lines)
        {
            int repeatSteps = 1;
            // currently, line 1 is special
            if (lines.Count < 1)
            {
                return;
            }
            string firstline = lines[0];
            if (firstline.StartsWith("REPEAT", StringComparison.CurrentCultureIgnoreCase))
            {
                firstline = firstline.Substring(7).Trim();
                repeatSteps = Convert.ToInt32(firstline);
                lines.RemoveAt(0);
            }

            if (_gbgEnabled)
            {
                repeatSteps = Convert.ToInt32(_gbgRepeat);
            }
            for (int i = 0; i < repeatSteps; i++)
            {
                if (_gbgEnabled)
                {
                    Point p = Point.Parse(_gbgStartingPosition);
                    DoMouseClick(p);
                    Thread.Sleep(1000);
                    DoMouseClick(p);
                    Thread.Sleep(1000);
                }
                foreach (string line in lines)
                {
                    // handle the action
                    int ind = line.IndexOf(' ');
                    string action = line.Substring(0, ind).Trim().ToUpper();
                    string value = line.Substring(ind).Trim();
                    switch (action)
                    {
                        case "MOVE":
                            Point m = Point.Parse(value);
                            DoMouseMove(m);
                            break;
                        case "CLICK":
                            Point p = Point.Parse(value);
                            DoMouseClick(p);
                            break;
                        case "DELAY":
                            Thread.Sleep(Convert.ToInt32(value));
                            break;
                        case "SCRIPT":
                            RunScript(value);
                            break;
                        case "SHUTDOWN":
                            Process.Start("shutdown", "/s /t " + value);
                            break;
                        default:
                            throw new Exception("Unrecognized command:  " + action);
                    }
                }
                if (_gbgEnabled)
                {
                    CallEscape();
                    Thread.Sleep(1000);
                }
            }
        }

        private void RunScript(string pathfile)
        {
            List<string> lines = new List<string>();
            using (StreamReader sr = new StreamReader(pathfile))
            {
                string line;
                while ((line = sr.ReadLine()) != null) {
                    lines.Add(line);
                }
            }
            RunFile(lines);
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        public static Point GetMousePositionWindowsForms()
        {
            System.Drawing.Point point = System.Windows.Forms.Control.MousePosition;
            return new Point(point.X, point.Y);
        }
        public void DoMouseMove(Point p)
        {
            uint xpos = (uint)p.X;
            uint ypos = (uint)p.Y;
            SetCursorPos((int)xpos, (int)ypos);
            Thread.Sleep(100);
        }

        public void CallEscape()
        {
            keybd_event((byte)0x1B, 0, 0x0001 | 0, 0);
            Thread.Sleep(500);
            keybd_event((byte)0x1B, 0, 0x0001 | 0, 0);
            Thread.Sleep(500);
            keybd_event((byte)0x1B, 0, 0x0001 | 0, 0);
        }

        public void DoMouseClick(Point p)
        {
            uint xpos = (uint)p.X;
            uint ypos = (uint)p.Y;
            SetCursorPos((int)xpos, (int)ypos);
            mouse_event(MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
            Thread.Sleep(100);
            mouse_event(MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
        }
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);
    }
}
