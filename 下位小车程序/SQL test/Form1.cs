using System;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;
using System.IO.Ports;
using System.Threading;

namespace SQL_test
{
    public partial class Form1 : Form
    {
        //定义串口，小车串口固定COM1，设定数据库口名称
        SerialPort sp = new SerialPort("COM11", 9600, Parity.Even, 8);
        SqlConnection conn = new SqlConnection();
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //定义数据库，输入数据库IP地址，默认数据库为高级权限，名字sa，密码123456，可更改，循环开始
            //string ip = textBox1.Text;
            string ip = "192.168.1.101";
            conn.ConnectionString = "Server=" + ip + ";DataBase=test;uid=sa;pwd=123456;";
            timer1.Start();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            //软件启动打开串口
            sp.Open();
        }
        public static void WaitFor(int ms)
        {
            //程序等待
            DateTime now = DateTime.Now;
            while (true)
            {
                TimeSpan span = (TimeSpan)(DateTime.Now - now);
                if (span.TotalMilliseconds > ms)
                {
                    return;
                }
            }
        }
        public static byte[] StrToHex(string str)
        {
            //字符串转字节
            byte[] cbytes = new byte[str.Length];
            for (int i = 0, c = 0; i < cbytes.Length / 2; i++, c += 2)
                cbytes[i] = Convert.ToByte(str.Substring(c, 2), 16);
            return cbytes;
        }
        //循环开始
        private void timer1_Tick(object sender, EventArgs e)
        {
            //读取数据库第一行第一列，第二行第一列，并新建表格
            SqlDataAdapter data = new SqlDataAdapter("select * From WR", conn);
            DataSet ds = new DataSet();
            data.Fill(ds, "TA");
            DataTable datable = ds.Tables["TA"];
                string s1 = datable.Rows[0][0].ToString();
                string s2 = datable.Rows[1][0].ToString();
                string s3 = datable.Rows[0][0].ToString();
                string s4 = datable.Rows[1][0].ToString();
            //防止循环发送字节到PLC，设置该循环判定当字符串改变后才发送
            while (s1 == s3 && s2 == s4)
            {
                WaitFor(200);
                ds = new DataSet();
                data.Fill(ds, "T");
                DataTable databl = ds.Tables["T"];
                s3 = databl.Rows[0][0].ToString();
                s4 = databl.Rows[1][0].ToString();
            }
            //转换并发送字节
            string a = "1002005C5E16";
                byte[] ack = StrToHex(a);
                byte[] tmp = StrToHex(s3);
                byte[] tmp2 = StrToHex(s4);
                sp.Write(tmp, 0, tmp.Length / 2);
                WaitFor(200);
                sp.Write(ack, 0, ack.Length / 2);
                WaitFor(200);
                sp.Write(tmp2, 0, tmp.Length / 2);
                WaitFor(200);
                sp.Write(ack, 0, ack.Length / 2);
                WaitFor(200);
        }
        //循环停止
        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Stop();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
