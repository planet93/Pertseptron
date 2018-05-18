using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Pertseptron
{
    public partial class Form1 : Form
    {
        const int NI = 10000;

        string[] bytes_str = new string[6];

        double[] w = new double[15];
        double w0;

        double[] an = new double[15];
        double[] zn = new double[15];
        double[] yn = new double[15];

        int pattern; //номер распознаваемого эталона
        int iter;

        double[,] xx = new double[3, 15]; //выборка (эталоны)
        double[,] xxn = new double[3, 15]; //зашумлённые

        Random rnd = new Random();
        public Form1()
        {
            InitializeComponent();
        }

        double h(double a)
        {
            return (Math.Exp(a) - Math.Exp(-a)) / (Math.Exp(a) + Math.Exp(-a));
        }
        double s(double a)
        {
            return 1.0 / (1.0 + Math.Exp(-a));
        }

        void Init()
        {
            for (int i = 0; i < 15; i++)
            {
                w[i] = rnd.Next(0, 9) + 1;
            }
            w0 = 1;
        }

        double npfunc (double [,] xx, int n)
        {
            int i;
            double sum=0;

            for (i = 0; i < 15; i++)
            {
                sum = sum + w[i] * xx[n,i];
            }
            sum = sum + w0;
            an[0] = sum;
            zn[0] = h(an[0]);

            return zn[0];
        }

        bool Stop()
        {
            bool flag = true;

            for (int i = 0; i < 3; i++)
            {
                if (((i != pattern) && (npfunc(xx, i)) > -0.9) || ((i == pattern) && (npfunc(xx, i)) < 0.9))
                {
                    flag = false;
                }
            }

            return flag;
        }

        double Mistake()
        {
            double sum = 0;
            for (int i = 0; i < 3; i++)
            {
                if (i == pattern)
                {
                    sum = sum + (npfunc(xx, i) - 1.0) * (npfunc(xx, i) - 1.0);
                }
                else
                {
                    sum = sum + (npfunc(xx, i) + 1.0) * (npfunc(xx, i) + 1.0);
                }
            }

            return sum;
        }

        void SimpleLearning()
        {
            int npattern;
            double step, delta, E, Ed;

            iter = 0;
            step = 1.0;
            delta = 0.001;

            pattern = Convert.ToInt32(comboBox1.Text);

            Init();

            while (!Stop())
            {
                if (iter > NI)
                {
                    iter = 0;
                    Init();
                }

                iter = iter + 1;

                label7.Text = iter.ToString();
                label7.Refresh();

                npattern = rnd.Next(0, 3);

                if (npattern == pattern)
                {
                    if (npfunc(xx, npattern) < 0.9)
                    {
                        for (int i = 0; i < 15; i++)
                        {
                            if (xx[pattern, i] == 1)
                            {
                                if (w[i] < 1000)
                                {
                                    w[i] = w[i] + step;
                                }
                            }
                        }
                    }
                }

                if (npattern != pattern)
                {
                    if (npfunc(xx, npattern) > -0.9)
                    {
                        for (int i = 0; i < 15; i++)
                        {
                            if (xx[npattern, i] == 1)
                            {
                                if (w[i] > -1000)
                                {
                                    w[i] = w[i] - step;
                                }
                            }
                        }
                    }
                }

                E = Mistake();
                w0 = w0 + delta;
                Ed = Mistake();
                w0 = w0 - 10.0 * (Ed - E) / delta;
                label12.Text = "Значение на эталонах: " + (npfunc(xx, 0).ToString()) + ";" + (npfunc(xx, 1).ToString()) + " ;" + (npfunc(xx, 2).ToString());
                label12.Refresh();
                Thread.Sleep(50);

                if(checkBox_new_weight.Checked)
                {
                    checkBox_new_weight.Checked = false;
                    iter = 0;
                    Init();
                }

                if(checkBox_exit_learn.Checked)
                {
                    break;
                }

            }
        }

        private void button_learn_Click(object sender, EventArgs e)
        {
            SimpleLearning();
        }

        private void button_recognition_Click(object sender, EventArgs e)
        {
            pattern = Convert.ToInt32(comboBox1.Text);
            label15.Text = "Результат распознавания:" + Convert.ToDouble(npfunc(xxn, Convert.ToInt32(comboBox2.Text) - 1));
        }

        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {

            //Спрашиваем какой файл открыть
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;

            string fileName = openFileDialog1.FileName;

            
            StreamReader sr = new StreamReader(fileName);
            string text = sr.ReadToEnd();//Читаем весь текст в файле

            //Разделяем весь текст на 6 строк
            string [] raz = text.Split(new char[] { '\n' });

            int i = 0;
            foreach (string r in raz)
            {
                //Далее разобъем по столбцам
                string[] column = r.Split(new char[] { ' ' });

                foreach (string col in column)
                {
                    //Проверим полученный символ, убрав пробелы и проверим не пустое ли значение
                    if (col.Trim() != "")
                    {
                        bytes_str[i] = col;
                        i++;
                    }
                }
            }
            //Вызываем функцию, чтобы заполнить массивы и поля считанными данными
            input_data();

        }

        void input_data()
        {
            int i, j;
            string buf;

            for (i = 1; i < 6; i++)
            {
                buf = "";
                for (j = 1; j < 4; j++)
                {
                    buf = buf + bytes_str[0][((i - 1) * 3 + j) - 1];
                }
                textBox1.Text = textBox1.Text + buf + "\r\n";//Полученный результат записываем в первый текстбокс
            }

            for (i = 0; i < 15; i++)
            {
                xx[0, i] = Convert.ToDouble(char.GetNumericValue(bytes_str[0][i]));//Заполнем значение массива, преобразуя из символа к числу вещественного вида
            }

            for (i = 1; i < 6; i++)
            {
                buf = "";
                for (j = 1; j < 4; j++)
                {
                    buf = buf + bytes_str[1][((i - 1) * 3 + j) - 1];
                }
                textBox2.Text = textBox2.Text + buf + "\r\n";
            }

            for (i = 0; i < 15; i++)
            {
                xx[1, i] = Convert.ToDouble(char.GetNumericValue(bytes_str[1][i]));
            }

            for (i = 1; i < 6; i++)
            {
                buf = "";
                for (j = 1; j < 4; j++)
                {
                    buf = buf + bytes_str[2][((i - 1) * 3 + j) - 1];
                }
                textBox3.Text = textBox3.Text + buf + "\r\n";
            }

            for (i = 0; i < 15; i++)
            {
                xx[2, i] = Convert.ToDouble(char.GetNumericValue(bytes_str[2][i]));
            }

            for (i = 1; i < 6; i++)
            {
                buf = "";
                for (j = 1; j < 4; j++)
                {
                    buf = buf + bytes_str[3][((i - 1) * 3 + j) - 1];
                }
                textBox4.Text = textBox4.Text + buf + "\r\n";
            }

            for (i = 0; i < 15; i++)
            {
                xxn[0, i] = Convert.ToDouble(char.GetNumericValue(bytes_str[3][i]));
            }

            for (i = 1; i < 6; i++)
            {
                buf = "";
                for (j = 1; j < 4; j++)
                {
                    buf = buf + bytes_str[4][((i - 1) * 3 + j) - 1];
                }
                textBox5.Text = textBox5.Text + buf + "\r\n";
            }

            for (i = 0; i < 15; i++)
            {
                xxn[1, i] = Convert.ToDouble(char.GetNumericValue(bytes_str[4][i]));
            }

            for (i = 1; i < 6; i++)
            {
                buf = "";
                for (j = 1; j < 4; j++)
                {
                    buf = buf + bytes_str[5][((i - 1) * 3 + j) - 1];
                }
                textBox6.Text = textBox6.Text + buf + "\r\n";
            }

            for (i = 0; i < 15; i++)
            {
                xxn[2, i] = Convert.ToDouble(char.GetNumericValue(bytes_str[5][i]));
            }

            //Делаем доступными наши кнопки
            button_learn.Enabled = true;
            button_recognition.Enabled = true;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
