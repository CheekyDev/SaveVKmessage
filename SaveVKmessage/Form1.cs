using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Leaf.xNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SaveVKmessage.Properties;
using VkNet;
using VkNet.Model;
using VkNet.Model.RequestParams;
using System.IO;
using System.Runtime;
using System.Net;
using System.Threading;

namespace SaveVKmessage
{
    public partial class Form1 : Form
    {
        string access_token;
        string n = "\n";
        string path = "Save Danni";
        string MyTime = DateTime.Now.Year + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Day + "." + DateTime.Now.Hour + "." + DateTime.Now.Minute + "." + DateTime.Now.Millisecond;
        Form2 form = new Form2();
        Thread[] Potok = new Thread[10];

        //this.Invoke((MethodInvoker)delegate () { });

        public Form1()
        {
            InitializeComponent();
            Directory.CreateDirectory(path);
        }

        void login(bool response_login)
        {
            if(response_login)
            {
                HttpRequest request = new HttpRequest();
                try
                {
                    access_token = textBox4.Text;
                    if (textBox4.Text.Length > 251)
                    {
                        richTextBox1.AppendText(MyTime + "Авторизация прошла успешно" + n);
                        richTextBox1.AppendText(MyTime + "Ваш токен: " + access_token + n);
                    }
                    else
                    {
                        //richTextBox1.AppendText(MyTime + "Проверьте введенные данные" + n);
                    }
                }
                catch
                {
                    richTextBox1.AppendText(MyTime + "Проверьте введенные данные" + n);
                }
            }
        } //авторизация

        private void button5_Click(object sender, EventArgs e)
        {
            if (textBox2.Text.Length < 1)
            {
                richTextBox1.AppendText(MyTime + "Поажалуйста введите токен в поле ниже и нажмите кнопку \"Авторизоваться\"" + n);
            }
            else if (textBox3.Text.Length < 1)
            {
                richTextBox1.AppendText(MyTime + "Пожалуйста ведите токен в поле ниже и нажмите кнопку \"Авторизоваться\"" + n);
            }
            else
            {
                //login(true);
                richTextBox1.AppendText(MyTime + "Пожалуйста ведите токен в поле ниже и нажмите кнопку \"Авторизоваться\"" + n);
            }
        } //кнопка авторизации

        void Download()
        {
            string put = "";
            string peer_id = "";
            int combo_num = 0;

            this.Invoke((MethodInvoker)delegate() 
                {
                peer_id = textBox1.Text;
                combo_num = comboBox1.SelectedIndex;
                });

            var danni = new HttpRequest();

            var response = danni.Get("https://api.vk.com/method/" +
                "account.getProfileInfo" + "?"
                + "&" + "v=5.62" + "&" + "access_token=" + access_token).ToString();
            JObject json = JObject.Parse(response);
            string[] Danni_polzovatelya =
            {
                json["response"].SelectToken("first_name").ToString(),
                json["response"].SelectToken("last_name").ToString(),
            };
            put = path + "\\" + "Пользователь - " + Danni_polzovatelya[0] + " " + Danni_polzovatelya[1] + " .Диалог с id" + peer_id ;
            Directory.CreateDirectory(put);

            this.Invoke((MethodInvoker)delegate () { richTextBox1.AppendText(MyTime + "Создаю папку ' " + put + "' для сохранения данных" + n); });

            if (combo_num == 0) //фото
            {
                Rabota("photo", put, peer_id);
            }
            if (combo_num == 1) //аудио
            {
                Rabota("audio", put, peer_id);
            }
            if (combo_num == 2) //документы
            {
                Rabota("doc", put, peer_id);
            }

            //richTextBox1.AppendText(put + n + comboBox1.SelectedIndex.ToString());
        }

        void Rabota(string type, string put, string peer_id)
        {
            string next_from = "";

            try
            {
                for (int j = 0; j < 100; j++)
                {
                    var danni = new HttpRequest();
                    string response = danni.Get("https://api.vk.com/method/" +
                        "messages.getHistoryAttachments" + "?"
                        + "&" + "peer_id=" + peer_id
                        + "&" + "media_type=" + type
                        + "&" + "start_from=" + next_from
                        + "&" + "count=" + 200
                        + "&" + "v=5.62" + "&" + "access_token=" + access_token).ToString();

                    JObject json = JObject.Parse(response);

                    try
                    {
                        for (int i = 1; i < 201; i++)
                        {
                            string St = i.ToString();
                            if (type == "photo")
                            {
                                #region опа
                                string src = "";
                                int date = Convert.ToInt32(json.SelectToken($"response.items[{St}].attachment.photo.date"));
                                #region спрятал
                                try
                                {
                                    src = json.SelectToken($"response.items[{St}].attachment.photo.photo_2560").ToString();
                                }
                                catch
                                {
                                    try
                                    {
                                        src = json.SelectToken($"response.items[{St}].attachment.photo.photo_1280").ToString();
                                        src = json.SelectToken($"response.items[{St}].attachment.photo.photo_807").ToString();
                                    }
                                    catch
                                    {
                                        src = json.SelectToken($"response.items[{St}].attachment.photo.photo_75").ToString();
                                    }
                                }
                                #endregion
                                DateTime created = new DateTime(1970, 1, 1).AddSeconds(date);

                                this.Invoke((MethodInvoker)delegate () { richTextBox1.AppendText(MyTime + "Процесс сохранения начат: " + src + n); });

                                WebClient wc = new WebClient();

                                string test = put + "\\" + created.ToString("yyyy.MM.dd.hh.mm.ss") + ".jpg";
                                wc.DownloadFile(src, test);

                                this.Invoke((MethodInvoker)delegate () { richTextBox1.AppendText(MyTime + "Фото" + src + ", успешно сохранено" + n); });

                                //richTextBox1.Text = created.ToString();
                                #endregion
                            }
                            else if (type == "audio")
                            {
                                #region опа
                                string artist = "";
                                string title = "";
                                string url = "";


                                artist = json.SelectToken($"response.items[{St}].attachment.audio.artist").ToString();
                                title = json.SelectToken($"response.items[{St}].attachment.audio.title").ToString();
                                url = json.SelectToken($"response.items[{St}].attachment.audio.url").ToString();

                                try
                                {
                                    string test = put + "\\" + title + ".mp3";
                                    this.Invoke((MethodInvoker)delegate () { richTextBox1.AppendText(MyTime + "Процесс сохранения начат: " + title + n); });
                                    try
                                    {

                                        this.Invoke((MethodInvoker)delegate () { richTextBox1.AppendText(MyTime + "Аудио " + title + ", успешно сохранено" + n); });

                                        WebClient wc = new WebClient();


                                        wc.DownloadFile(url, test);

                                        this.Invoke((MethodInvoker)delegate () { richTextBox1.AppendText(MyTime + "Аудио " + title + ", было успешно сохранено с новым названием " + "1" + n); });
                                    }
                                    catch
                                    {
                                        WebClient wc = new WebClient();

                                        test = put + "\\" + "1" + ".mp3";
                                        wc.DownloadFile(url, test);
                                    }
                                }
                                catch
                                {

                                }

                                //richTextBox1.Text = response;
                                #endregion
                            }
                            else if (type == "doc")
                            {
                                #region опа
                                string title = "";
                                string ext = "";
                                string url = "";

                                try
                                {
                                    title = json.SelectToken($"response.items[{St}].attachment.doc.title").ToString();
                                    ext = json.SelectToken($"response.items[{St}].attachment.doc.ext").ToString();
                                    url = json.SelectToken($"response.items[{St}].attachment.doc.url").ToString();

                                    this.Invoke((MethodInvoker)delegate () { richTextBox1.AppendText(MyTime + "Процесс сохранения начат: " + title + "." + type + n); });

                                    string test = put + "\\" + title + "." + ext;
                                    try
                                    {
                                        this.Invoke((MethodInvoker)delegate () { richTextBox1.AppendText(MyTime + "Документ " + title + "." + type + ", успешно сохранено" + n); });
                                        WebClient wc = new WebClient();
                                        wc.DownloadFile(url, test);
                                    }
                                    catch
                                    {
                                        this.Invoke((MethodInvoker)delegate () { richTextBox1.AppendText(MyTime + "Документ " + title + "." + type + ", успешно сохранено с новым названием" + "1" + n); });
                                        test = put + "\\" + "1" + "." + ext;
                                        WebClient wc = new WebClient();
                                        wc.DownloadFile(url, test);
                                    }
                                }
                                catch
                                {
                                    this.Invoke((MethodInvoker)delegate () { richTextBox1.AppendText(MyTime + "Невозможно получить ссылку на скачивание" + n); });
                                    break;
                                }

                                //richTextBox1.Text = response;
                                #endregion
                            }
                        }
                    }
                    catch
                    {
                        this.Invoke((MethodInvoker)delegate () { richTextBox1.AppendText(MyTime + "Работа завершена, все возможные данные были успешно сохранены" + n); });
                        break;
                    }

                    next_from = json.SelectToken($"response.next_from").ToString();
                }
            }
            catch
            {
                this.Invoke((MethodInvoker)delegate () { richTextBox1.AppendText(MyTime + "Работа завершена, все возможные данные были успешно сохранены" + n); });
                Potok[0].Abort();
            }
        }

        private void button1_Click(object sender, EventArgs e) //кнопка старт
        {
            Potok[0] = new Thread(Download);
            Potok[0].Start();
        }

        private void button6_Click(object sender, EventArgs e)
        { 
            if(textBox4.Text.Length < 85)
            {
                this.Invoke((MethodInvoker)delegate () { richTextBox1.AppendText(MyTime + "Пожалуйста введите токен" + n); });
            }
            else
            {
                login(true);
                this.Invoke((MethodInvoker)delegate () { richTextBox1.AppendText(MyTime + "Авторизация успешно произведена" + n); });
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            form.ShowDialog();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            this.Invoke((MethodInvoker)delegate () { richTextBox1.AppendText(MyTime + "Лог успешно очищен" + n); });
        }

        private void Time_Tick(object sender, EventArgs e)
        {
            MyTime = DateTime.Now.ToLongTimeString();
            MyTime = "[" + MyTime + "] ";
        }

        private void button2_Click(object sender, EventArgs e) //кнопка стоп
        {
            Potok[0].Abort();
            this.Invoke((MethodInvoker)delegate () { richTextBox1.AppendText(MyTime + "Программа остановленна" + n); });
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            groupBox2.Text = $"Лог [{richTextBox1.Lines.Count()}]";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://vk.com/misha2282014");
        }
    }
}
