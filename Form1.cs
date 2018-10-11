using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;


namespace WindowsFormsApp1
{

    public partial class Form1 : Form
    {


        string pathOpen, pathSave;
        bool openFlag = false, saveFlag = false;
        Image imOriginal;
        Bitmap bmOriginal;
        Graphics g;
        Bitmap bmBuffer;

        public Form1()
        {
            InitializeComponent();
        }

        private void buttonBeginTransfer_Click(object sender, EventArgs e)
        {
            if (!openFlag || !saveFlag)
            {
                MessageBox.Show("恁这池沼，先选取好路径可否？", "我谔谔", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }



            BinaryWriter bw = new BinaryWriter(new FileStream(pathSave, FileMode.Create, FileAccess.Write, FileShare.ReadWrite));

            try
            {
                bw.Write(0x64684C46);
                bw.Write(0x00000006);
                bw.Write(0x00010010);
                bw.Write(0x4C460060);
                bw.Write(0x00117464);
                bw.Write(0x0AC70000);
                bw.Write(0x352E3231);
                bw.Write(0x352E302E);
                bw.Write(0x031C0038);
                bw.Write((short)0x0041);
                bw.Write((byte)0x00);
                bw.Write((byte)0xE0);

                //test use
                int xm = Convert.ToInt32(numericUpDownW.Value);
                int ym = Convert.ToInt32(numericUpDownH.Value);
                int sizeM = xm * ym;
                //MessageBox.Show(sizeM.ToString());
                // check how much should checking number be
                int chk = 0;
                int tt = 256;
                int chkCount = 1;
                for (int i = 0; i < sizeM; i++)
                {
                    chk += 24;
                    int tBorder = 1;
                    for (int j = 1; j <= chkCount; j++)
                    { 
                        tBorder *= 256;
                        if (j < chkCount)
                        {
                            if (chk % tBorder < (tBorder / 2))
                            {
                                chk += tBorder / 2;
                            }
                        }
                    }
                    if (chk >= tBorder / 2)
                    {
                        chkCount += 1;
                        chk += tBorder;
                    }
                }
                //MessageBox.Show(chk.ToString());
                for (int i = 0; i < chkCount; i++)
                {
                    bw.Write((byte)(chk % 256));
                    chk /= 256;
                }

                //Do Color Picking
                bmBuffer = new Bitmap(xm, ym);
                g = Graphics.FromImage(bmBuffer);
                g.DrawImage(bmOriginal, 0,0, bmBuffer.Width,bmBuffer.Height);

                //Draw
                if (comboBox1.SelectedIndex == 0)
                {
                    for (int i = 0; i < xm; i++)
                    {
                        for (int j = 0; j < ym; j++)
                        {
                            Color pixel = bmBuffer.GetPixel(i, j);
                            //if (i == 10 && j == 10) MessageBox.Show(pixel.ToString());
                            byte vel = (byte)((pixel.R + pixel.G + pixel.B) / 3 * 128 / 255);
                            WritePixelNote(bw, 0, vel, i * 24, 120 - j);
                        }
                    }
                }
                else
                {

                    for (int i = 0; i < xm; i++)
                    {
                        for (int j = 0; j < ym; j++)
                        {
                            Color pixel = bmBuffer.GetPixel(i, j);
                            //if (i == 10 && j == 10) MessageBox.Show(pixel.ToString());
                            byte channel = 0;
                            byte vel = (byte)((pixel.R + pixel.G + pixel.B) / 3 * 128 / 255);
                            RGBData rgbd = new RGBData(pixel.R, pixel.G, pixel.B);
                            HSVData hsvd = getHSV(rgbd);

                            int hc = hsvd.h;
                            if (hc < 85) hc = hc - 85 + 256;
                            else hc -= 85;
                            channel = (byte)(hc / 16);


                            WritePixelNote(bw, channel, vel, i * 24, 120 - j);


                        }
                    }


                    /*
                    bw.Close();
                    bw.Dispose();
                    bw = null;
                    MessageBox.Show("16色正在开发中，创歉，，，", "我谔谔", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;*/
                }


            }
            catch (Exception)
            {
                bw.Close();
                bw.Dispose();
                bw = null;
                MessageBox.Show("哎呀，出了点问题，创歉，，，", "我谔谔", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            bw.Close();
            bw.Dispose();
            bw = null;
            MessageBox.Show("你的大粪完成了！快导入FL姐贵的小窗窗然后去批站投稿罢！", "好时代，来临了！", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;


        }

        public void WritePixelNote(BinaryWriter bw, byte channel, byte vel, int x, int y)
        {
            int xt = x;
            int xb = 0;
            /*for (int i = 0; i < 8; i++)
            {
                if (i % 2 == 0)
                {
                    xb *= 256;
                    xb += xt % 16;
                }
                else
                {
                    xb += (xt % 16) * 16;
                }
                xt = xt / 16;
            }*/
            bw.Write(xt); //4
            bw.Write(0x00004000); //4
            bw.Write(0x00000018); //4
            bw.Write((byte)y); //1
            bw.Write(0x78000000); //4
            bw.Write((short)0x4000); //2
            bw.Write(channel); //1
            bw.Write((byte)0x40); //1
            bw.Write(vel); // 1
            bw.Write((byte)0x80);
            bw.Write((byte)0x80);
        }




        private void buttonOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.Filter = "All files(*.*)|*.*";
            try
            {

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    pathOpen = ofd.FileName;
                    imOriginal = Image.FromFile(pathOpen);
                    pictureBox1.Image = imOriginal;
                    if(bmOriginal!= null)
                    {
                        bmOriginal.Dispose();
                        bmOriginal = null;
                    }
                    bmOriginal = new Bitmap(imOriginal);
                    textBoxOpen.Text = pathOpen;
                    openFlag = true;
                    //MessageBox.Show(bmOriginal.GetPixel(30, 30).ToString());
                }
            }
            catch
            {
                MessageBox.Show("恁到底知道什么叫图片文件🐎？", "我谔谔", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ofd.Dispose();
                ofd = null;
                return;
            }


            ofd.Dispose();
            ofd = null;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "FL Studio Score(*.fsc)|*.fsc";
            
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                pathSave = sfd.FileName;
                textBoxSave.Text = pathSave;
                saveFlag = true;
            }
            sfd.Dispose();
            sfd = null;
        }



        public HSVData getHSV(RGBData rgb)
        {
            byte h, s, v;
            float hf, sf, vf;
            float rf = (float)rgb.r / 255;
            float gf = (float)rgb.g / 255;
            float bf = (float)rgb.b / 255;
            float max, min;
            max = Math.Max(rf, gf);
            max = Math.Max(max, bf);
            min = Math.Min(rf, gf);
            min = Math.Min(min, bf);
            float d = max - min;

            if(d==0)
            {
                hf = 0;
            }
            else if(max==rf)
            {
                hf = ((gf-bf)/d)/ 6;
            }
            else if (max == gf)
            {
                hf= ((bf - rf) / d + 2) / 6;
            }
            else
            {
                hf =((rf - gf) / d + 4) / 6;
            }

            if (max == 0) sf = 0;
            else sf = d / max;

            vf = max;

            h = (byte)(hf * 255);
            s = (byte)(sf * 255);
            v = (byte)(vf * 255);

            return new HSVData(h,s,v);

        }



    }


    public class RGBData
    {
        public byte r, g, b;
        public RGBData(byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }
    }

    public class HSVData
    {
        public byte h, s, v;
        public HSVData(byte h, byte s, byte v)
        {
            this.h = h;
            this.s = s;
            this.v = v;
        }

    }
}
