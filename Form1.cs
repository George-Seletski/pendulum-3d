using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tao.FreeGlut;
using Tao.OpenGl;
using Tao.Platform.Windows;

namespace _5
{
    public partial class Form1 : Form
    {
        double px = 0; double py = 0; double z = 0;
        double angle = 0;
        double zoom = 1;

        double ScreenW, ScreenH;

        private float devX;
        private float devY;

        private float[,] arr_v, arr_v_buf, arr_f, arr_f_buf;

        private int cnt = 0;
        private double XS = 1;  //Масштаб графика

        private float rad = 1;   //Радиус шара
        float A = 0;    //тангенциальное ускорение
        float L = 11;    //длина нити
        float M = 3;    //масса
        float FI = 0;   //Угол отклонения маятника
        float W0 = 0;   //Угловая частота


        float X = 0; // смещение
        float H = 0; // высота
        float V = 0; // скорость
        float Ep = 0; // потениальная энергия
        float Ek = 0; // кинетическая энергия

        float M_Ep = 0; //максимальная потенциальная э
        float M_Ek = 0; //максимальная кинетическая э

        int graphic = 0;

        //Начальные условия
        float K = 0.24f; // Коэффициент сопротивления среды
        //float K = 0f;
        float Ws = 0;
        float B = 0;
        float E = (float)Math.E;
        float G = 9.81f;//сила тяжести
        float FI0 = 20;



        float time = 0;

        float XG = 0, YG_V = 0, YG_F;

        float lineX, lineY;

        float Mcoord_X = 0, Mcoord_Y = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void Graph_Load()
        {
            Graph.InitializeContexts();
            Graph.MakeCurrent();
            // установка порта вывода 
            Gl.glViewport(0, 0, Graph.Width, Graph.Height);
            // активация проекционной матрицы 
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();

            ScreenH = 20;
            ScreenW = 40;
            // сохранение коэффициентов, которые нам необходимы для перевода координат указателя в оконной системе в координаты, 
            // принятые в нашей OpenGL сцене 
            devX = (float)ScreenW / (float)Graph.Width;
            devY = (float)ScreenH / (float)Graph.Height;

            Glu.gluOrtho2D(0.0, ScreenW, 0.0, ScreenH);

            // установка объектно-видовой матрицы 
            Gl.glMatrixMode(Gl.GL_MODELVIEW); Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glClearColor(255, 255, 255, 1);
        }

        private void Diagram_Load()
        {
            Diagram.InitializeContexts();
            Diagram.MakeCurrent();
            // установка порта вывода 
            Gl.glViewport(0, 0, Diagram.Width, Diagram.Height);
            // активация проекционной матрицы 
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();

            Glu.gluOrtho2D(0.0, 20, 0.0, 40);

            // установка объектно-видовой матрицы 
            Gl.glMatrixMode(Gl.GL_MODELVIEW); Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glClearColor(255, 255, 255, 1);
        }

        private void AnT_Load()
        {
            AnT.InitializeContexts();
            AnT.MakeCurrent();

            Gl.glViewport(0, 0, AnT.Width, AnT.Height);
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();

            Glu.gluPerspective(45, (float)AnT.Width / (float)AnT.Height, 0.1, 200);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();

            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glEnable(Gl.GL_LIGHTING);
            Gl.glEnable(Gl.GL_LIGHT0);

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            FI = ToRad(180 + FI0);
            W0 = -(float)Math.Sqrt(G / L);
            if (K == 0) {
                B = 0;
            }
            else
            {
                B = K / (2 * M);
            }
            //B = K / (2*M);
           // B = K;
            Ws = -(float)Math.Sqrt((Math.Pow(W0,2) - Math.Pow(B, 2)));


            X = (float)Math.Sin(FI) * L;
            V = (float)Math.Sin(FI) * G;

        

            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_RGB | Glut.GLUT_DOUBLE);
            Gl.glClearColor(255, 255, 255, 1);

            AnT_Load();
            Graph_Load();
            Diagram_Load();

            textBox1.Text = L.ToString();
            textBox2.Text = M.ToString();
            textBox3.Text = FI0.ToString();
            textBox4.Text = XS.ToString();
            textBox5.Text = K.ToString();
        
            //M_Ep = M * G * (L - (float)Math.Cos(FI) * L);

           
            comboBox1.SelectedIndex = 0;
        }


        

        private void timer1_Tick(object sender, EventArgs e)
        {
            time += 0.0155f;
            calc();
            DrawGraph();
            DrawFig();
        }


        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            angle = trackBar1.Value;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            zoom = trackBar2.Value / 10;
        }

        //кнопка Старт
        private void button1_Click(object sender, EventArgs e)
        {

            Graph.MakeCurrent();

            timer1.Start();
           
        }
        //кнопка Рестарт
        private void button3_Click(object sender, EventArgs e)
        {
            Graph.MakeCurrent();

            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glClearColor(255, 255, 255, 1);
            Gl.glLoadIdentity();

            timer1.Stop();
            timer1.Dispose();

            time = 0;

            FI = ToRad(180 + FI0);
            W0 = (float)Math.Sqrt(G / L);

            if (K == 0)
            {
                B = 0;
            }
            else
            {
                B = K / (2 * M);
            }

            B = K / (2 * M);
            B = (float)Convert.ToDouble(textBox5.Text);
            Ws = -(float)Math.Sqrt((Math.Pow(W0, 2) - Math.Pow(B, 2)));

            arr_v = new float[0, 2];
            arr_v_buf = new float[0, 2];

            arr_f = new float[0, 2];
            arr_f_buf = new float[0, 2];
            cnt = 0;

            XG = 0;
            YG_F = ToGr(FI0) * devY;
            YG_V = V * 100 * devY;

            timer1.Start();

        }

       
        //кнопка ПАУЗА
        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Stop();
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
         
                if (float.Parse(textBox1.Text) < 1) L = 1;
                else L = float.Parse(textBox1.Text);
        }

        private void textBox2_KeyUp(object sender, KeyEventArgs e)
        {
           
                if (float.Parse(textBox2.Text) < 1) M = 1;
                else M = float.Parse(textBox2.Text);
        }

      

        private void textBox3_KeyUp(object sender, KeyEventArgs e)
        {
            FI0 = float.Parse(textBox3.Text);
        }


        private void textBox4_KeyUp(object sender, KeyEventArgs e)
        {
             XS = (float)Convert.ToDouble(textBox4.Text);
        }

        private void textBox5_KeyUp(object sender, KeyEventArgs e)
        {
            K = (float)Convert.ToDouble(textBox5.Text);

        }


        private void Graph_MouseMove(object sender, MouseEventArgs e)
        {
            // сохраняем координаты мыши 
            Mcoord_X = e.X;
            Mcoord_Y = e.Y;

            // вычисляем параметры для будущей дорисовки линий от указателя мыши к координатным осям. 
            lineX = devX * e.X;
            lineY = (float)(ScreenH - devY * e.Y);
        }


        private void DrawFig()
        {

            AnT.MakeCurrent();
            double      x = L * Math.Cos(ToRad(180 + ToGr(-FI))),
                        y = L * Math.Sin(ToRad(180 + ToGr(-FI)));

            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glClearColor(255, 255, 255, 1);
            Gl.glLoadIdentity();
            Gl.glPushMatrix();

            Gl.glTranslated(0, AnT.Height / 30, -30);

            Gl.glScaled(zoom, zoom, zoom);

            Gl.glRotated(angle, 0, 1, 0);

            Gl.glBegin(Gl.GL_LINES);

            Gl.glVertex2d(0, -AnT.Height);
            Gl.glVertex2d(0, AnT.Height);

            Gl.glEnd();

            Gl.glRotated(180, 0, 1, 0);

            Gl.glRotated(90, 1, 0, 0);
            Gl.glRotated(ToGr(-FI), 0, 1, 0);
            Glut.glutSolidCylinder(0.05, L, 32, 32);
            Gl.glRotated(-90, 1, 0, 0);

            Gl.glBegin(Gl.GL_LINE_STRIP);

            Gl.glVertex2d(0, -L);
            Gl.glVertex2d(0, -L + 3);
            Gl.glVertex2d(0.25, 2.5 - L);
            Gl.glVertex2d(0, 3 - L);
            Gl.glVertex2d(-0.25, 2.5 - L);

            Gl.glEnd();

            Gl.glRotated(90, 1, 0, 0);
            Gl.glRotated(-ToGr(-FI), 0, 1, 0);
            Gl.glRotated(-90, 1, 0, 0);

            Gl.glRotated(90, 0, 0, 1);

            Gl.glTranslated(x, y, 0);


            Gl.glBegin(Gl.GL_LINE_STRIP);

            Gl.glVertex2d(0, 0);
            Gl.glVertex2d(-3, 0);

            Gl.glVertex2d(-2.5, 0.25);
            Gl.glVertex2d(-3, 0);
            Gl.glVertex2d(-2.5, -0.25);
            
            Gl.glEnd();

            Glut.glutSolidSphere(rad, 32, 32);

            Gl.glRotated(-90, 0, 0, 1);
            Gl.glRotated(90, 0, 1, 0);
            PrintText2D(1f, -3, "Fmg");
            Gl.glRotated(-90, 0, 1, 0);
            Gl.glRotated(90, 0, 0, 1);

            Gl.glTranslated(-x, -y, 0);

            //Gl.glTranslated(-3, 0.5, 0);
            //PrintText2D(0, 0, "Fmg");

            Gl.glRotated(-90, 0, 0, 1);
            Gl.glRotated(90, 0, 1, 0);
            Gl.glRotated(-ToGr(-FI), 1, 0, 0);
            PrintText2D(0.5f, 3 - L, "T");
            Gl.glRotated(ToGr(-FI), 1, 0, 0);
            Gl.glRotated(-90, 0, 1, 0);
            Gl.glRotated(90, 0, 0, 1);

            Gl.glPopMatrix();
            Gl.glFlush();
            AnT.Invalidate();
        }



        private float ToRad(float fi)
        {
            fi = fi * ((float)Math.PI / 180);
            return fi;
        }

       

        private float ToGr(float fi)
        {
            fi = fi * (180 / (float)Math.PI);
            return fi;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            graphic = comboBox1.SelectedIndex;
        }

        private void calc()
        {
     

            H = L - (float)Math.Cos(FI) * L;
            A = (float)(ToRad(FI0) * Math.Pow(E, -B * time));
            FI = (float)(A * Math.Cos(Ws * time));

            X = (float)Math.Sin(FI) * L;
            V = (float)(-A * Ws * Math.Sin(Ws * time));
            
            
            Ep = M * G * H;
            Ek = (M * (V * V)) / 2 * 100;

            if (Ep == 0)
            {
                M_Ek  = Ek;

            }
            else
            {
                if (Ek == 0)
                {
                    M_Ep = Ep;
                }

            }



        }

        private void DrawE()
        {
            Diagram.MakeCurrent();
            Glu.gluOrtho2D(0.0, 20, 0.0, 40);
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glClearColor(255, 255, 255, 1);
            Gl.glLoadIdentity();

            Gl.glTranslated(1, 1, 0);

            Gl.glColor4f(0,0.5f,1,1f);
            //РИСУЕМ ФОН ДЛЯ ГРАФИКА Ep
            Gl.glBegin(Gl.GL_QUADS);
                Gl.glVertex2d(2, 0);
                Gl.glVertex2d(2, 38);
                Gl.glVertex2d(9, 38);
                Gl.glVertex2d(9, 0);
            Gl.glEnd();
            
            Gl.glColor4f(1, 0.5f, 0, 1f);
            ////////////////////////
            //РИСУЕМ ФОН ДЛЯ ГРАФИКА Ek
            Gl.glBegin(Gl.GL_QUADS);
                Gl.glVertex2d(10, 0);
                Gl.glVertex2d(10, 38);
                Gl.glVertex2d(17, 38);
                Gl.glVertex2d(17, 0);
            Gl.glEnd();

            Gl.glTranslated(1.5, 0, 0);
            Gl.glColor3f(0, 0, 0);
            //////////////////////////
            
            for (int i = 0; i < 30; i += 1)
            {
                Gl.glBegin(Gl.GL_LINES);
                    Gl.glVertex2d(0, i);
                    Gl.glVertex2d(-0.5, i);

                    Gl.glVertex2d(0, ((i - 2.5f)));
                   //Gl.glVertex2d(-0.3, ((i - 2.5f)));
                Gl.glEnd();

                if (i < 0)
                {
                    PrintText2D((float)(-2.8), (float)((i - 0.45)), "-");
                   

                }

                //PrintText2D((float)(-2), (float)((i - 0.45)), ((float)(Math.Abs(i) / 10f)).ToString());
                PrintText2D((float)(-2), (float)((i - 0.45)), ((float)(Math.Abs(i) )).ToString());
            }
            
            Gl.glBegin(Gl.GL_LINES);
                Gl.glVertex2d(0, -5);
                Gl.glVertex2d(0, 45);
            Gl.glEnd();
            
            Gl.glTranslated(-1.5, 0, 0);

            Gl.glColor4f(1, 1f, 0.5f, 1f);

            if (Ep > 37.8)
            {
                Gl.glBegin(Gl.GL_QUADS);

                Gl.glVertex2d(2.2, 0.2);
                Gl.glVertex2d(2.2, 37.8);
                Gl.glVertex2d(8.8, 37.8);
                Gl.glVertex2d(8.8, 0.2);

                Gl.glEnd();
            }
            else
            { 

            Gl.glBegin(Gl.GL_QUADS);

            Gl.glVertex2d(2.2, 0.2);
            Gl.glVertex2d(2.2, Ep);
            Gl.glVertex2d(8.8, Ep);
            Gl.glVertex2d(8.8, 0.2);

            Gl.glEnd();

            }

            Gl.glColor4f(1, 1f, 0.5f, 1f);

            if (Ek > 37.8)
            {
                Gl.glBegin(Gl.GL_QUADS);
                Gl.glVertex2d(10.2, 0.2);
                Gl.glVertex2d(10.2, 37.8);
                Gl.glVertex2d(16.8, 37.8);
                Gl.glVertex2d(16.8, 0.2);
                Gl.glEnd();
            }
            else
            {
                Gl.glBegin(Gl.GL_QUADS);
                Gl.glVertex2d(10.2, 0.2);
                Gl.glVertex2d(10.2, Ek);
                Gl.glVertex2d(16.8, Ek);
                Gl.glVertex2d(16.8, 0.2);
                Gl.glEnd();
            }


            Gl.glColor3f(0, 0, 0);
            Diagram.Invalidate();
        }

        private void DrawDiag()
        {
            Graph.MakeCurrent();


            XG = time;
            YG_V = V * 100 * devY;
            YG_F = ToGr(FI) * devY;


            arr_v_buf = arr_v;
            arr_v = new float[cnt + 1, 2];

            for (int i = 0; i < cnt; i++)
                for (int j = 0; j < 2; j++)
                    arr_v[i, j] = arr_v_buf[i, j];

            arr_v[cnt, 0] = (float)XG;
            arr_v[cnt, 1] = (float)YG_V;

            arr_f_buf = arr_f;
            arr_f = new float[cnt + 1, 2];

            for (int i = 0; i < cnt; i++)
                for (int j = 0; j < 2; j++)
                    arr_f[i, j] = arr_f_buf[i, j];

            arr_f[cnt, 0] = (float)XG;
            arr_f[cnt, 1] = (float)YG_F;

            cnt++;

            Gl.glScaled(XS, XS, 0);

            Gl.glBegin(Gl.GL_LINE_STRIP);

            
            switch (graphic)
            {
                case 0:
                    {
                        for (int i = 0; i < cnt; i++)
                            Gl.glVertex2d(arr_v[i, 0], arr_v[i, 1]);
                        break;
                    };

                case 1:
                    {
                        for (int i = 0; i < cnt; i++)
                            Gl.glVertex2d(arr_f[i, 0], arr_f[i, 1]);

                        break;
                    }
            }

            Gl.glEnd();
            Gl.glPointSize(5);
            Gl.glColor3f(255, 0, 0);

            switch (graphic)
            {
                case 0:
                    {
                        Gl.glBegin(Gl.GL_POINTS);

                        Gl.glVertex2d(XG, YG_V);

                        Gl.glEnd();
                        break;
                    };

                case 1:
                    {
                        Gl.glBegin(Gl.GL_POINTS);

                        Gl.glVertex2d(XG, YG_F);

                        Gl.glEnd();
                        break;
                    }
            }
            Gl.glPointSize(1);
        }

       

        private void DrawGraph()
        {
            Graph.MakeCurrent();
            Glu.gluOrtho2D(0.0, ScreenW, 0.0, ScreenH);
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glClearColor(255, 255, 255, 1);
            Gl.glLoadIdentity();
            Gl.glColor3f(0, 0, 0);

            Gl.glPushMatrix();

            Gl.glTranslated(0, devY*Graph.Height/2, 0);

            label8.Text = (Math.Round(ToGr(FI))).ToString();
            label12.Text = (Math.Round(time, 2)).ToString();
            label14.Text = (Math.Round(B, 2)).ToString();
            label16.Text = (Math.Round(W0, 2)).ToString();
            label19.Text = (Math.Round(X, 2)).ToString();
            label21.Text = (Math.Round(V,2)).ToString();
            label23.Text = (Math.Round(Ep, 2)).ToString();
            label25.Text = (Math.Round(Ek, 2)).ToString();
            label27.Text = (Math.Round(H, 2)).ToString();
            label34.Text = (Math.Round(M_Ep, 2)).ToString();
            label36.Text = (Math.Round(M_Ek, 2)).ToString();
            if (XG > Graph.Width * devX / 2 / XS)
            {
                Gl.glTranslated(-XG * XS + Graph.Width * devX / 2 , 0, 0);
            }
            Gl.glBegin(Gl.GL_POINTS);

            for (int ax = -30; ax < XG * XS + Graph.Width*devX; ax++)
            {
                for (int bx = -Graph.Height; bx < Graph.Height; bx++)
                {
                    // вывод точки 
                    Gl.glVertex2d(ax, bx);
                }
            }
            Gl.glEnd();
            Gl.glTranslated(3, 0, 0);

            Gl.glBegin(Gl.GL_LINES);

            Gl.glVertex2d(0, -Graph.Height * devY);
            Gl.glVertex2d(0, Graph.Height * devY);

            Gl.glEnd();

            Gl.glColor3f(0, 0, 0);
            DrawDiag();
            DrawE();

            Graph.MakeCurrent();

            Gl.glColor3f(1, 1, 1);

            if (XG > Graph.Width * devX / 2 / XS)
            {
                Gl.glColor3f(0, 0, 0);

                Gl.glBegin(Gl.GL_LINES);

                Gl.glVertex2d(0, 0);
                Gl.glVertex2d(XG + Graph.Width, 0);

                Gl.glEnd();

                Gl.glColor3f(1, 1, 1);

                Gl.glBegin(Gl.GL_QUADS);

                Gl.glVertex2d(XG - 3 - Graph.Width * devX / 2 / XS, -Graph.Height * devY / XS);
                Gl.glVertex2d(XG - 3 - Graph.Width * devX / 2 / XS, Graph.Height * devY / XS);
                Gl.glVertex2d(XG - Graph.Width * devX / 2 / XS, Graph.Height * devY / XS);
                Gl.glVertex2d(XG - Graph.Width * devX / 2 / XS, -Graph.Height * devY / XS);

                Gl.glEnd();
                Gl.glColor3f(0, 0, 0);

                Gl.glBegin(Gl.GL_LINES);

                // ОСИ //////////////////
                Gl.glVertex2d(XG - Graph.Width * devX / 2 / XS, -Graph.Height * devY);
                Gl.glVertex2d(XG - Graph.Width * devX / 2 / XS, Graph.Height * devY);
                /////////////////////////

                // СТРЕЛКИ //////////////
                Gl.glVertex2d(XG - Graph.Width * devX / 2 / XS, (Graph.Height * devY / 2) / XS);
                Gl.glVertex2d(XG - Graph.Width * devX / 2 / XS + 0.3 / XS, (Graph.Height * devY / 2 - 0.5) / XS);
                Gl.glVertex2d(XG - Graph.Width * devX / 2 / XS, (Graph.Height * devY / 2) / XS);
                Gl.glVertex2d(XG - Graph.Width * devX / 2 / XS - 0.3 / XS, (Graph.Height * devY / 2 - 0.5) / XS);

                Gl.glVertex2d((XG + Graph.Width * devX / 2 / XS - 3 / XS), 0);
                Gl.glVertex2d((XG + Graph.Width * devX / 2 / XS - 3.5 / XS), 0.3 / XS);
                Gl.glVertex2d((XG + Graph.Width * devX / 2 / XS - 3 / XS), 0);
                Gl.glVertex2d((XG + Graph.Width * devX / 2 / XS - 3.5 / XS), -0.3 / XS);
                /////////////////////////
                for (int i = -100; i < 100; i += 20)
                {
                    Gl.glBegin(Gl.GL_LINES);

                    Gl.glVertex2d(XG - Graph.Width * devX / 2 / XS, i * devY / XS);
                    Gl.glVertex2d(XG - Graph.Width * devX / 2 / XS - 0.5 / XS, i * devY / XS);

                    Gl.glVertex2d(XG - Graph.Width * devX / 2 / XS, ((i-10) * devY) / XS);
                    Gl.glVertex2d(XG - Graph.Width * devX / 2 / XS - 0.3 / XS, ((i-10) * devY) / XS);

                    Gl.glEnd();

                    if (i < 0)
                    {
                        PrintText2D((float)(XG - Graph.Width * devX / 2 / XS - 2.8 / XS), (float)((i * devY - 0.45) / XS), "-");
                    }
                    if (graphic == 0)
                        PrintText2D((float)(XG - Graph.Width * devX / 2 / XS - 2 / XS), (float)((i * devY - 0.45) / XS), (Math.Round(Math.Abs(i) / (XS * 100), 2)).ToString());
                    else
                        PrintText2D((float)(XG - Graph.Width * devX / 2 / XS - 2 / XS), (float)((i * devY - 0.45) / XS), ((int)(Math.Abs(i) / XS)).ToString());
                }

                Gl.glEnd();
            }
            else
            {
                Gl.glColor3f(0, 0, 0);

                Gl.glBegin(Gl.GL_LINES);

                Gl.glVertex2d(0, 0);
                Gl.glVertex2d(XG + Graph.Width, 0);

                Gl.glEnd();

                Gl.glColor3f(1, 1, 1);

                Gl.glBegin(Gl.GL_QUADS);

                Gl.glVertex2d(-3, -Graph.Height * devY / XS);
                Gl.glVertex2d(-3, Graph.Height * devY / XS);
                Gl.glVertex2d(0, Graph.Height * devY / XS);
                Gl.glVertex2d(0, -Graph.Height * devY / XS);

                Gl.glEnd();

                Gl.glColor3f(0, 0, 0);

                Gl.glBegin(Gl.GL_LINES);

                // ОСИ //////////////////
                Gl.glVertex2d(0, -Graph.Height * devY);
                Gl.glVertex2d(0, Graph.Height * devY);
                /////////////////////////

                // СТРЕЛКИ //////////////
                Gl.glVertex2d(0, (Graph.Height * devY / 2) / XS);
                Gl.glVertex2d(0.3 / XS, (Graph.Height * devY / 2 - 0.5) / XS);
                Gl.glVertex2d(0, (Graph.Height * devY / 2) / XS);
                Gl.glVertex2d(-0.3 / XS, (Graph.Height * devY / 2 - 0.5) / XS);

                Gl.glVertex2d((-3 / XS) + Graph.Width * devX / XS, 0);
                Gl.glVertex2d((-3.5 / XS) + Graph.Width * devX / XS, 0.3 / XS);
                Gl.glVertex2d((-3 / XS) + Graph.Width * devX / XS, 0);
                Gl.glVertex2d((-3.5 / XS) + Graph.Width * devX / XS, -0.3 / XS);
                /////////////////////////
                for (int i = -100; i < 100; i += 20)
                {
                    Gl.glBegin(Gl.GL_LINES);

                    Gl.glVertex2d(0, i * devY / XS);
                    Gl.glVertex2d(-0.5 / XS, i * devY / XS);

                    Gl.glVertex2d(0, ((i - 10) * devY) / XS);
                    Gl.glVertex2d(-0.3 / XS, ((i - 10) * devY) / XS);

                    Gl.glEnd();

                    if (i < 0)
                    {
                        PrintText2D((float)(-2.8 / XS), (float)((i * devY - 0.45) / XS), "-");
                    }

                    if (graphic == 0)
                        PrintText2D((float)(-2 / XS), (float)((i * devY - 0.45) / XS), ((Math.Abs(i) / (XS * 100))).ToString());
                    else
                        PrintText2D((float)(-2 / XS), (float)((i * devY - 0.45) / XS), ((int)(Math.Abs(i) / XS)).ToString());
                }

                Gl.glEnd();
            }
            Gl.glFlush();
            Graph.Invalidate();
        }

        private void PrintText2D(float x, float y, string text)
        {

            // устанавливаем позицию вывода растровых символов 
            // в переданных координатах x и y. 
            Gl.glRasterPos2f(x, y);

            // в цикле foreach перебираем значения из массива text, 
            // который содержит значение строки для визуализации 
            foreach (char char_for_draw in text)
            {
                Gl.glScaled(0.5, 0.5, 0);
                // символ C визуализируем с помощью функции glutBitmapCharacter, используя шрифт GLUT_BITMAP_9_BY_15. 
                Glut.glutBitmapCharacter(Glut.GLUT_BITMAP_HELVETICA_10, char_for_draw);
                Gl.glScaled(2, 2, 0);
            }
        }

    }
}
