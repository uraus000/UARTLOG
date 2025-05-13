using System.IO.Ports;
using System.Text;
using System;
using System.Diagnostics;

namespace UARTLOG
{
    public class Main_cls
    {
#region MainClass Instance
        //--------------------------------------------------------------------------------
        // CVDR_M_CL 클래스 Get으로 캡슐화
        //--------------------------------------------------------------------------------
        static readonly object padlock = new object();
        static Main_cls m_Main_Instance;
        public static Main_cls Instance
        {
            get
            {
                lock (padlock)
                {
                    return m_Main_Instance;
                }
            }
        }
#endregion

        private Form1 form1; 
        public string LogPath = @"C:\Wonik\UART_Log";
        public string LogFileName;
        public Queue<string> LogQueue = new Queue<string>();
        public object LoglockObject = new object();
        public UARTSerial_Cls Uart1;
        public UARTSerial_Cls Uart2;
        public bool ThreadRun = true;
        private Thread LogThread;
        public bool LogSaveFlag;
        public Main_cls()
        {
            m_Main_Instance = this;
            
            Uart1 = new UARTSerial_Cls("Uart1");
            Uart2 = new UARTSerial_Cls("Uart2");
            
            LogThread = new Thread(LogThreadFunc);
            LogThread.Name = "LogThread";
            LogThread.Start();
            form1 = new Form1();
            form1.Show();
        }
        public void MainThreadAbort()
        {
            ThreadRun = false;
            LogThread.Join();
        }
        public void LogSaveStart(bool Flag)
        {
            if(Flag)
            {
                LogQueue.Clear();
                LogQueue.Enqueue("---- Log Start ----");
                LogSaveFlag = true;
            }
            else
            {
                LogSaveFlag = false;
            }
            
        }

        private void LogThreadFunc()
        {
            DateTime TimeStampe = DateTime.Now;
            string temp = "";

            while(ThreadRun)
            {
                if((DateTime.Now - TimeStampe).TotalMicroseconds > 1000)
                {
                    if(LogSaveFlag)
                    {
                        TimeStampe = DateTime.Now;
                        temp = "";
                        while(LogQueue.Count > 0)
                        {
                            temp += LogQueue.Dequeue() + "\r\n";
                        }
                        if(temp != "")
                        {
                            LogFunc(temp);
                        }
                    }
                    else
                    {
                        LogQueue.Clear();
                    }
                    
                }
                Thread.Sleep(1);
            }
        }

        public bool LogFunc(string Description)
        {
            string DirPath = LogPath;
            string filename = DateTime.Now.ToString("yyyy_MM_dd") + "_Uart.log";
            string FullPath = Path.Combine(DirPath, filename);

            if(!Directory.Exists(DirPath)) { Directory.CreateDirectory(DirPath); }

            if (!File.Exists(FullPath))
            {
                FileStream fs = new FileStream(FullPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                StreamWriter ws = new StreamWriter(fs, System.Text.Encoding.Default);
                StringBuilder sp = new StringBuilder();

                sp.Append(Description);
                ws.Write(sp.ToString());

                ws.Flush();
                ws.Close();
                fs.Close();
            }
            else
            {
                FileStream fs = new FileStream(FullPath, FileMode.Append, FileAccess.Write, FileShare.Read);
                StreamWriter ws = new StreamWriter(fs, System.Text.Encoding.Default);
                StringBuilder sp = new StringBuilder();

                sp.Append(Description);
                ws.Write(sp.ToString());

                ws.Flush();
                ws.Close();
                fs.Close();
            }
            return true;
        }
    }
    public class UARTSerial_Cls
    {
        Main_cls m_Main = Main_cls.Instance;
        private string Comport = "COM3";
        private int Baud = 19200;
        private string label;
        private SerialPort serial = new SerialPort();
        public bool isConnected = false;
        public bool ThreadRun = true;
        private Thread SerialThread;
        public List<string> ComReadText = new List<string>();
        private string trheadName;
        public UARTSerial_Cls(string ThreadName)
        {
            trheadName = ThreadName;
        }
        public bool Connect(string Comport, int Baud, string loglabel)
        {
            if(!serial.IsOpen)
            {
                if(SerialThread != null)
                {
                    if(SerialThread.IsAlive)
                    {
                        ThreadRun = false;
                        SerialThread.Join();
                    }
                }
                
                try
                {
                    serial.BaudRate = Baud;
                    serial.PortName = Comport;
                    serial.Open();
                    label = loglabel;
                    ThreadRun = true;
                    SerialThread = new Thread(SerialThreadFunc);
                    SerialThread.Name = trheadName;
                    SerialThread.Start();
                }
                catch(Exception ex)
                {
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        public void Close()
        {
            if(serial.IsOpen)
            {
                ThreadRun = false;
                SerialThread.Join();
                while(SerialThread.IsAlive){    Thread.Sleep(1);    }
                serial.Close();
                isConnected = false;
            }
            
        }

        private void SerialThreadFunc()
        {
            DateTime ReadTimeStamp = DateTime.Now;
            DateTime TimeStamp = DateTime.Now;
            byte []RxBuf = new byte[999];
            byte RxIndex = 0;
            bool NewlineFlag = false;
            while(ThreadRun)
            {
                isConnected = serial.IsOpen;
                try
                {
                    if(serial.IsOpen)
                    {
                         while(serial.BytesToRead > 0)
                         {
                            if(RxIndex == 0) {  TimeStamp = DateTime.Now;   }
                            ReadTimeStamp = DateTime.Now;
                            RxBuf[RxIndex++] = (byte)serial.ReadByte();
                            NewlineFlag = true;
                         }
                         
                         if((RxIndex > 0 ) && ((DateTime.Now - ReadTimeStamp).TotalMilliseconds > 5))
                         {
                            lock(m_Main.LoglockObject)
                            {
                                ComReadText.Add(String.Format("[{0}] [{1}] {2}",label, TimeStamp.ToString("MM-dd HH:mm:ss.ffff"),ByteArrayToHexString(RxBuf, RxIndex)));                                
                                m_Main.LogQueue.Enqueue(String.Format("[{0}] [{1}] {2}",label, TimeStamp.ToString("MM-dd HH:mm:ss.ffff"),ByteArrayToHexString(RxBuf, RxIndex)));
                                RxIndex = 0;
                                
                                serial.DiscardInBuffer();
                            }
                         }
                         if(NewlineFlag && (((DateTime.Now - ReadTimeStamp).TotalSeconds > 5)))
                         {
                            NewlineFlag = false;
                            m_Main.LogQueue.Enqueue(String.Format("[{0}] Mute!!\r\n",label));
                         }
                    }
                }
                catch(Exception ex)
                {

                }
                //SleepMicroseconds(100);
                Thread.Sleep(1);
            }
        }
        public static void SleepMicroseconds(int microseconds)
        {
            long ticks = microseconds * (Stopwatch.Frequency / 1_000_000);
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.ElapsedTicks < ticks) { }
        }
        public string ByteArrayToHexString(byte[] byteArray, byte Length)
        {
            StringBuilder hex = new StringBuilder(Length * 2);
            for(int i =0;i<Length;i++)
            {
                hex.AppendFormat("{0:X2} ", byteArray[i]);
            }
            // foreach (byte b in byteArray)
            // {
            //     hex.AppendFormat("{0:X2}\t", b);
            // }
            return hex.ToString();
        }
    }
}