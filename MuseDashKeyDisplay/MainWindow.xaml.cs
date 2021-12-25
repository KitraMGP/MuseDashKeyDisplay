using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MuseDashKeyDisplay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static bool isReady = false;
        public static MainWindow instance;

        // 存储前 5 次的按键计数
        // 类似于一个队列，但长度固定，新计数为数组第一个元素，其他元素向后移动，最后一个元素被丢弃
        private static int[] hitCountHistory = new int[5];
        private static int hitCount = 0;    // 按键计数
        // private static long counterStartTime = 0;   // 计数开始时间
        private static bool counterLocked = false;  // 实现了一个简单的锁，防止多个线程同时修改计数器状态
        private static StaticData staticData = StaticData.Get();    // 存储 UI 数据绑定的数据

        // 以下字段用于 UI 缩放功能
        private static int originWidth = 506;
        private static double originHeight = 113;
        private static double zoomFactor = 1d;

        public MainWindow()
        {
            InitializeComponent();
            instance = this;

            // 用 -1 标记所有尚未使用的元素
            for (int i = 0; i < hitCountHistory.Length; i++)
                hitCountHistory[i] = -1;

            /*
            Stats.SetBinding(TextBlock.TextProperty, new Binding()
            {
                Source = staticData,
                Path = new PropertyPath("Msg")
            });*/
            isReady = true;
            KeyboardHook.SetHook();
            /*Timer updateTimer = new(300);   // 300ms 运行一次，计算按键速度
            updateTimer.Elapsed += (o, e) =>
            {
                if (!counterLocked)
                {
                    UpdateKeyCounter();
                }  
            };
            Timer counterResetTimer = new(4000);    // 4000ms 运行一次，重置计数器和开始时间
            counterResetTimer.Elapsed += (o, e) =>
            {
                counterLocked = true;
                ResetCounter();
                counterLocked = false;
            };
            ResetCounter();
            updateTimer.Start();
            counterResetTimer.Start();*/
            Timer updateTimer = new(300);
            updateTimer.Elapsed += (o, e) =>
            {
                CalculateHitSpeed();
            };
            // updateTimer.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            KeyboardHook.UnHook();
            isReady = false;
            instance = null;
        }

        private void CalculateHitSpeed()
        {
            /*if (counterStartTime == 0) return;
            double secondsPast = (double)(DateTime.Now.Ticks - counterStartTime) / 10000 / 1000;
            if (secondsPast <= 0.02d) return;
            double keySpeed = (double)keyHitCount / secondsPast;
            staticData.Msg = String.Format("击键速度：{0:F2} /s", keySpeed);*/
            // 除第一个元素外所有元素向右移动
            // 随后将 hitCount 赋给第一个元素
            for (int i = hitCountHistory.Length - 1; i > 0 ; i--)
            {
                hitCountHistory[i] = hitCountHistory[i - 1];
            }
            hitCountHistory[0] = hitCount;
            hitCount = 0;

            // 取最近 3 次测量的数据计算击键速度
            int hitSum = 0;
            int validCount = 0;
            for (int i = 0; i < 3; i++)
            {
                if (hitCountHistory[i] != -1)
                {
                    hitSum += hitCountHistory[i];
                    validCount++;
                }
            }
            // int hitSpeed = (int) Math.Round((double) hitSum / 3d, 1);
            int hitSpeed = (int)(hitSum / (double) validCount / 0.9d);
            staticData.Msg = String.Format("击键速度：{0} /s", hitSpeed);
        }

        private void ResetCounter()
        {
            /*keyHitCount = 0;
            counterStartTime = DateTime.Now.Ticks;
            UpdateKeyCounter();*/
        }

        private void SwitchKeyState(Rectangle rect, TextBlock text, bool keyDown)
        {
            rect.Fill = keyDown ? Brushes.White : Brushes.Transparent;
            text.Foreground = keyDown ? Brushes.Black : Brushes.White;
        }

        private void ProcessMajorKeyEvents(string keyName, bool keyDown)
        {
            switch (keyName)
            {
                case "S":
                    SwitchKeyState(Key1B, Key1T, keyDown);
                    break;
                case "D":
                    SwitchKeyState(Key2B, Key2T, keyDown);
                    break;
                case "F":
                    SwitchKeyState(Key3B, Key3T, keyDown);
                    break;
                case "J":
                    SwitchKeyState(Key4B, Key4T, keyDown);
                    break;
                case "K":
                    SwitchKeyState(Key5B, Key5T, keyDown);
                    break;
                case "L":
                    SwitchKeyState(Key6B, Key6T, keyDown);
                    break;
                default:
                    break;
            }
        }

        private void ProcessOtherKeyEvents(string keyName, bool keyDown)
        {
            /*SwitchKeyState(OtherKeyLastB, OtherKeyLastT, keyDown);
            OtherKeyLastT.Text = ProcessKeyNames(keyName);*/
        }

        private string ProcessKeyNames(string keyName)
        {
            string newName = keyName;
            newName = newName.Replace("LeftShift", "Shift")
                .Replace("LeftCtrl", "Ctrl");
            newName = newName.Replace("Return", "回车")
                .Replace("Left", "←")
                .Replace("Right", "→")
                .Replace("Up", "↑")
                .Replace("Down", "↓")
                .Replace("LWin", "Win")
                .Replace("Space", "空格")
                .Replace("Escape", "Esc");

            return newName;
        }

        private void Zoom()
        {
            Width = originWidth * zoomFactor;
            Height = originHeight * zoomFactor;
        }

        private void ZoomIn()
        {
            zoomFactor += 0.1d;
            Zoom();
        }

        private void ZoomOut()
        {
            if (zoomFactor <= 0.25d)
            {
                zoomFactor = 0.25d;
            }
            else
            {
                zoomFactor -= 0.1d;
            }
            Zoom();
        }

        public void OnKeyDown(Key key)
        {
            string keyName = key.ToString();

            if (keyName == "Add" || keyName == "OemPlus")
            {
                ZoomIn();
            }
            else if (keyName == "Subtract" || keyName == "OemMinus")
            {
                ZoomOut();
            }
            /*else if (keyName == "PageUp")
            {
                Show();
            }
            else if (keyName == "PageDown")
            {
                Hide();
            }*/
            else if (keyName.Length == 1 && "SDFJKL".Contains(keyName))
            {
                ProcessMajorKeyEvents(keyName, true);
            }
            else
            {
                ProcessOtherKeyEvents(keyName, true);
                // MessageBox.Show(keyName);
            }

            //if (keyName == "")
            hitCount++;
        }

        public void OnKeyRelease(Key key)
        {
            string keyName = key.ToString();
            if (keyName.Length == 1 && "SDFJKL".Contains(keyName))
            {
                ProcessMajorKeyEvents(keyName, false);
            }
            else
            {
                ProcessOtherKeyEvents(keyName, false);
            }
        }

        
    }
}
