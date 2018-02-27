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

namespace server
{
    public partial class Form1 : Form
    {
        private SqlConnection ct = null;
        private SqlDataReader reader = null;
        private SqlCommand command = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Equals(""))
                MessageBox.Show("用户名不能为空!", "警告", MessageBoxButtons.OKCancel,
                                  MessageBoxIcon.Warning);
            else
            {
                int flag = 0;
                try
                {
                    ct = new SqlConnection();
                    ct.ConnectionString = "server=.;database=Datas;integrated security=SSPI";
                    ct.Open();
                    command = new SqlCommand("select * from ZC", ct);
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        if (textBox1.Text.Equals(reader.GetString(0)))
                        {
                            flag = 1;
                            if (textBox2.Text.Equals(reader.GetString(1)))
                            {

                                this.Hide();
                                Form2 f2 = new Form2();
                                f2.Show();
                                if (reader != null)
                                    reader.Close();
                            }
                            else
                            {
                                MessageBox.Show("密码错误!", "失败", MessageBoxButtons.OKCancel,
                                      MessageBoxIcon.Warning);
                                if (reader != null)
                                    reader.Close();
                            }
                            
                            if (checkBox1.Checked == true)
                            {
                                command = new SqlCommand("Update ZC set isRemembered=1 where name='" + textBox1.Text + "'", ct);
                                command.ExecuteNonQuery();
                            }
                            else if(checkBox1.Checked == false)
                            {
                                command = new SqlCommand("Update ZC set isRemembered=0 where name='" + textBox1.Text + "'", ct);
                                command.ExecuteNonQuery();
                            }

                            break;
                        }
                    }
                    if (flag == 0)
                    {
                        MessageBox.Show("该用户不存在!", "失败", MessageBoxButtons.OKCancel,
                                          MessageBoxIcon.Warning);
                        if (reader != null)
                            reader.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("错误信息: " + ex.Message, "警告", MessageBoxButtons.OKCancel,
                                      MessageBoxIcon.Warning);
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                    if (ct != null)
                        ct.Close();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            try
            {
                ct = new SqlConnection();
                ct.ConnectionString = "server=.;database=Datas;integrated security=SSPI";
                ct.Open();
                SqlCommand command = new SqlCommand("select * from ZC", ct);
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (textBox1.Text.Equals(reader.GetString(0)))
                    {
                        if (reader.GetInt32(2) == 1)
                        {
                            textBox2.Text = reader.GetString(1);
                            checkBox1.Checked = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("错误信息: " + ex.Message, "警告", MessageBoxButtons.OKCancel,
                                  MessageBoxIcon.Warning);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
                if (ct != null)
                    ct.Close();
            }
        }
    }
}
