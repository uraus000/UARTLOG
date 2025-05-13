using System.IO.Ports;
using System.Net.Http.Json;

namespace UARTLOG
{
    public partial class Form1 : Form
    {
        private System.Windows.Forms.Timer UIUpdateTimer = new System.Windows.Forms.Timer();

        Main_cls m_Main = Main_cls.Instance;
        private int uart1Idx = 0, uart2Idx = 0;
        private bool Flicker = false;
        private DateTime FlickerTimer = DateTime.Now;
        public Form1()
        {
            InitializeComponent();
            UIInit();
        }
        private void UIInit()
        {
            this.FormClosing += new FormClosingEventHandler(formclosingFunc);
            comboBox2.Items.Clear();
            comboBox4.Items.Clear();
            comboBox2.Items.Add(9600);
            comboBox2.Items.Add(19200);
            comboBox2.Items.Add(38400);
            comboBox2.Items.Add(115200);
            comboBox2.Text = "115200";

            comboBox4.Items.Add(9600);
            comboBox4.Items.Add(19200);
            comboBox4.Items.Add(38400);
            comboBox4.Items.Add(115200);
            comboBox4.Text = "115200";

            comboBox1.DropDown += new EventHandler(SerialPortUpdateFunc);
            comboBox3.DropDown += new EventHandler(SerialPortUpdateFunc);
            SerialPortUpdateFunc(comboBox1,null);
            SerialPortUpdateFunc(comboBox3,null);
            
            button1.Click += new EventHandler(ButtonFunc);
            button2.Click += new EventHandler(ButtonFunc);
            button3.Click += new EventHandler(ButtonFunc);
            button4.Click += new EventHandler(ButtonFunc);
            button7.Click += new EventHandler(ButtonFunc);
            button8.Click += new EventHandler(ButtonFunc);
            button5.Click += new EventHandler(ButtonFunc);
            button6.Click += new EventHandler(ButtonFunc);

            textBox1.Text = "Tx";
            textBox2.Text = "Rx";

            UIUpdateTimer.Interval = 10;
            UIUpdateTimer.Tick += new EventHandler(UIUpdateTimerFunc);
            UIUpdateTimer.Start();
        }
        private void ButtonFunc(object sender, EventArgs e)
        {
            if(sender == button1)
            {
                m_Main.Uart1.Connect(comboBox1.Text, Convert.ToInt32(comboBox2.Text), textBox1.Text);
            }
            else if(sender == button3)
            {
                m_Main.Uart2.Connect(comboBox3.Text, Convert.ToInt32(comboBox4.Text), textBox2.Text);
            }
            else if(sender == button2)
            {
                m_Main.Uart1.Close();
            }
            else if(sender == button4)
            {
                m_Main.Uart2.Close();
            }
            else if(sender == button7)
            {
                m_Main.Uart1.ComReadText.Clear();
                richTextBox1.Clear();
                uart1Idx = 0;

                m_Main.Uart2.ComReadText.Clear();
                richTextBox2.Clear();
                uart2Idx = 0;
            }
            else if(sender == button8)
            {
                m_Main.Uart2.ComReadText.Clear();
                richTextBox2.Clear();
                uart2Idx = 0;
                
                m_Main.Uart1.ComReadText.Clear();
                richTextBox1.Clear();
                uart1Idx = 0;
            }

            else if(sender == button5)
            {
                if(m_Main.LogSaveFlag)
                {
                    if(MessageBox.Show("Log기록을 정지하시겠습니까?","Warning",MessageBoxButtons.YesNo) != DialogResult.Yes) return;
                }
                m_Main.LogSaveStart(!m_Main.LogSaveFlag);
            }
            else if(sender == button6)
            {
                System.Diagnostics.Process.Start("explorer.exe",m_Main.LogPath);
            }
        }
        private void UIUpdateTimerFunc(object sender, EventArgs e)
        {
            if(((DateTime.Now-FlickerTimer).TotalMilliseconds) > 500)
            {
                FlickerTimer = DateTime.Now;
                Flicker = Flicker?false:true;
            }
            button1.Enabled = !m_Main.Uart1.isConnected;
            button1.BackColor = m_Main.Uart1.isConnected?Color.LightGreen:Color.LightGray;
            comboBox1.Enabled = !m_Main.Uart1.isConnected;
            comboBox2.Enabled = !m_Main.Uart1.isConnected;
            button2.Enabled = m_Main.Uart1.isConnected;
            button2.BackColor = !m_Main.Uart1.isConnected?Color.LightPink:Color.LightGray;
            textBox1.Enabled = !m_Main.Uart1.isConnected;

            button3.Enabled = !m_Main.Uart2.isConnected;
            button3.BackColor = m_Main.Uart2.isConnected?Color.LightGreen:Color.LightGray;
            comboBox3.Enabled = !m_Main.Uart2.isConnected;
            comboBox4.Enabled = !m_Main.Uart2.isConnected;
            button4.Enabled = m_Main.Uart2.isConnected;
            button4.BackColor = !m_Main.Uart2.isConnected?Color.LightPink:Color.LightGray;
            textBox2.Enabled = !m_Main.Uart2.isConnected;
          
            button5.Text = m_Main.LogSaveFlag?"Log 기록중":"Log 기록중지";
            button5.BackColor = !m_Main.LogSaveFlag?Color.LightGray:(Flicker?Color.LightGreen:Color.LightGray);

            lable.Text = DateTime.Now.ToString("HH:mm:ss.ffff");
            
            while(m_Main.Uart1.ComReadText.Count > uart1Idx)
            {
                richTextBox1.AppendText(m_Main.Uart1.ComReadText[uart1Idx++] + "\r\n");
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.ScrollToCaret();
            }
            while(m_Main.Uart2.ComReadText.Count > uart2Idx)
            {
                richTextBox2.AppendText(m_Main.Uart2.ComReadText[uart2Idx++] + "\r\n");
                richTextBox2.SelectionStart = richTextBox2.Text.Length;
                richTextBox2.ScrollToCaret();
            }
        }
        private void formclosingFunc(object sender, FormClosingEventArgs e)
        {
            UIUpdateTimer.Stop();
            UIUpdateTimer.Dispose();
            m_Main.Uart1.Close();
            m_Main.Uart2.Close();
            m_Main.MainThreadAbort();
            Application.Exit();
        }
        private void SerialPortUpdateFunc(object sender, EventArgs e)
        {
            ComboBox temp = (ComboBox)sender;
            temp.Items.Clear(); // 기존 목록 삭제

            string[] ports = System.IO.Ports.SerialPort.GetPortNames(); // 사용 가능한 포트 가져오기
            Array.Sort(ports); // 보기 좋게 정렬 (예: COM1, COM2, ...)

                temp.Items.AddRange(ports); // ComboBox에 추가

                if (ports.Length > 0)
                    temp.SelectedIndex = 0; // 첫 번째 포트 선택
        }
    }
}