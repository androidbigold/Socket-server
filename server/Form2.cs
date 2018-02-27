using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace server
{
    public partial class Form2 : Form
    {
        //客户端和服务器之间的连接状态
        private bool bConnected = false;
        //监听线程
        private Thread tAcceptMsg = null;
        //用于socket通信的ip地址和端口
        private IPEndPoint IPP = null;
        //socket通信
        private Socket socket = null;
        private Socket clientSocket = null;
        //网络访问的基础数据流；
        private NetworkStream nStream = null;
        //创建读取器
        private TextReader tReader = null;
        //创建编写器
        private TextWriter wReader = null;

        private SqlConnection ct = null;
        private SqlCommand command = null;

        public Form2()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void panel1_Paint(object sender,PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, panel1.ClientRectangle,
                Color.White, 1, ButtonBorderStyle.Solid,
                Color.White, 1, ButtonBorderStyle.Solid,
                Color.White, 1, ButtonBorderStyle.Solid,
                Color.White, 1, ButtonBorderStyle.Solid);

        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, panel2.ClientRectangle,
                Color.White, 1, ButtonBorderStyle.Solid,
                Color.White, 1, ButtonBorderStyle.Solid,
                Color.White, 1, ButtonBorderStyle.Solid,
                Color.White, 1, ButtonBorderStyle.Solid);

        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, panel3.ClientRectangle,
                Color.White, 1, ButtonBorderStyle.Solid,
                Color.White, 1, ButtonBorderStyle.Solid,
                Color.White, 1, ButtonBorderStyle.Solid,
                Color.White, 1, ButtonBorderStyle.Solid);

        }

        private void Form2_FormClosing(object sender,FormClosingEventArgs e)
        {
            if (DialogResult.Yes == MessageBox.Show("确定要退出程序吗？", "提示", MessageBoxButtons.YesNo))
            {
                if (clientSocket != null)
                {
                    if (clientSocket.Connected)
                    {
                        MessageBox.Show("请先关闭连接");
                        e.Cancel = true;
                    }
                    else
                    {
                        if (ct != null)
                            ct.Close();
                        e.Cancel = false;
                        System.Environment.Exit(0);
                    }
                }
                else
                {
                    e.Cancel = false;
                    System.Environment.Exit(0);
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        public void AcceptMessage()
        {
            //接受客户端的请求
            clientSocket = socket.Accept();
            if (clientSocket != null)
            {
                bConnected = true;
                try
                {
                    textBox7.Text =DateTime.Now.ToString() + " : 与客户" + 
                        clientSocket.RemoteEndPoint.ToString() + "连接成功.\r\n" + textBox7.Text;
                    textBox3.Text = ((IPEndPoint)clientSocket.RemoteEndPoint).Address.ToString();
                    textBox4.Text = ((IPEndPoint)clientSocket.RemoteEndPoint).Port.ToString();
                    FileStream fs = new FileStream("log_network.txt", FileMode.Append);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.Write(DateTime.Now.ToString() + " : 与客户" +
                        clientSocket.RemoteEndPoint.ToString() + "连接成功.\r\n");
                    sw.Flush();
                    sw.Close();
                    fs.Close();
                }
                catch { }
            }


            nStream = new NetworkStream(clientSocket);
            tReader = new StreamReader(nStream);
            wReader = new StreamWriter(nStream);
            string sTemp;
            while (bConnected)
            {
                try
                {
                    sTemp = tReader.ReadLine();

                    if (sTemp.Length != 0)
                    {
                        lock (this)
                        {
                            textBox6.Text = "客户机:" + sTemp + "\r\n" + textBox6.Text;
                            try
                            {
                                command = ct.CreateCommand();
                                command.CommandText = "insert into communication values('" + 
                                                     DateTime.Now.ToString() + "','" +
                                                     clientSocket.RemoteEndPoint.ToString() + "','" +
                                                     IPP.ToString() + "','" +
                                                     sTemp + "')";
                                command.ExecuteNonQuery();
                            }
                            catch (SqlException se)
                            {
                                MessageBox.Show("错误信息: " + se.Message, "警告", MessageBoxButtons.OKCancel,
                                  MessageBoxIcon.Warning);
                            }
                        }
                    }
                }
                catch
                {
                    textBox7.Text = DateTime.Now.ToString() + " : 与客户" +
                        clientSocket.RemoteEndPoint.ToString() + "断开连接.\r\n" + textBox7.Text;
                    textBox3.Text = "";
                    textBox4.Text = "";
                    FileStream fs = new FileStream("log_network.txt", FileMode.Append);
                    StreamWriter sw = new StreamWriter(fs);
                    sw.Write(DateTime.Now.ToString() + " : 与客户" +
                        clientSocket.RemoteEndPoint.ToString() + "断开连接.\r\n");
                    sw.Flush();
                    sw.Close();
                    fs.Close();
                    bConnected = false;
                }
            } 
            //禁止当前SOCKET上的发送和接受
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
            AcceptMessage();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IPP = new IPEndPoint(IPAddress.Parse("192.168.1.103"), 65535);//服务器IP、端口
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            socket.Bind(IPP);
            socket.Listen(0);

            tAcceptMsg = new Thread(new ThreadStart(this.AcceptMessage));
            tAcceptMsg.Start();
            button1.Enabled = false;

            textBox1.Text = IPP.Address.ToString();
            textBox2.Text = IPP.Port.ToString();

            try
            {
                ct = new SqlConnection();
                ct.ConnectionString = "server=.;database=Datas;integrated security=SSPI";
                ct.Open();
                command = ct.CreateCommand();
                command.CommandText = "if object_id('communication') is not null select 1 else select 0";
                if ((int)command.ExecuteScalar() == 0)
                {
                    command.CommandText = @"create table communication(Time varchar(50) primary key,
                                        MessageFrom varchar(50) not null,MessageTo varchar(50) not null,
                                        Message varchar(500))";
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("错误信息: " + ex.Message, "警告", MessageBoxButtons.OKCancel,
                                  MessageBoxIcon.Warning);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (bConnected)
            {
                try
                {
                    lock (this)
                    {
                        textBox6.Text = "服务器: " + textBox5.Text + "\r\n" + textBox6.Text;

                        wReader.WriteLine(textBox5.Text);
                        wReader.Flush();

                        try
                        {
                            command = ct.CreateCommand();
                            command.CommandText = "insert into communication values('" + 
                                                 DateTime.Now.ToString() + "','" +
                                                 IPP.ToString() + "','" +
                                                 clientSocket.RemoteEndPoint.ToString() + "','" +
                                                 textBox5.Text + "')";
                            command.ExecuteNonQuery();
                        }
                        catch (SqlException se)
                        {
                            MessageBox.Show("错误信息: " + se.Message, "警告", MessageBoxButtons.OKCancel,
                              MessageBoxIcon.Warning);
                        }

                        textBox5.Text = "";
                        textBox5.Focus();
                    }
                }
                catch
                {
                    MessageBox.Show("无法与客户机通信");
                }
            }
            else
            {
                MessageBox.Show("未与客户机建立连接，不能通信");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox7.Text = "";
        }

    }
}
