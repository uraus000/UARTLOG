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

        private Form1 form1 = new Form1();

        public Main_cls()
        {
            m_Main_Instance = this;
            form1.Show();
        }
    }
}