using System.IO.Ports;

namespace UARTLOG
{
    public partial class Form1 : Form
    {
        private System.Windows.Forms.Timer UIUpdateTimer = new System.Windows.Forms.Timer();

         Main_cls m_Main = Main_cls.Instance;
         
        public Form1()
        {
            InitializeComponent();
            UIInit();
        }
        private void UIInit()
        {
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

            UIUpdateTimer.Interval = 10;
            UIUpdateTimer.Tick += new EventHandler(UIUpdateTimerFunc);
            UIUpdateTimer.Start();
            
        }
        private void UIUpdateTimerFunc(object sender, EventArgs e)
        {

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