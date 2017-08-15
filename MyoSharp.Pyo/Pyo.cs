using MyoSharp.Communication;
using MyoSharp.Device;
using MyoSharp.Exceptions;
using MyoSharp.Math;
using MyoSharp.Util;
using MyoSharp.Poses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace MyoSharp.Pyo
{
    public partial class Pyo : Form
    {
        #region Variáveis Privadas
        private readonly IChannel _channel;
        private readonly IHub _hub;
        private DateTime hora = DateTime.Now;
        private DateTime flagHora = DateTime.Now;

        
        private List<double> rollAnterior = new List<double>();
        private List<double> pitchAnterior = new List<double>();
        private List<double> yawAnterior = new List<double>();

        private List<double> listaRoll = new List<double>();
        private List<double> listaPitch = new List<double>();
        private List<double> listaYaw = new List<double>();

        private const int numeroCasasDecimais = 3;
        private const int pontosMovementoCursor = 25;
        private const float PI = (float)System.Math.PI;
        
        private Util.Util Tela = new Util.Util();
        //Graphics g = Graphics.FromImage(bmp);
        private Point ponto = new Point(-1,-1);

        private bool DesenharAtivo = false;
        private bool flagBorracha = false;

        private int ContCor=0;
        private int click_cont=0;

        private int formflag = 0;
        private List<Point> lPoint = new List<Point>();

        private int tam = 2;

        private int btam = 10;//tamanho borracha


        #endregion

        public Pyo()
        {
            InitializeComponent();
            
            //Criação do canal de comunicação com o Myo
            _channel = Channel.Create(ChannelDriver.Create(ChannelBridge.Create(),
                                        MyoErrorHandlerDriver.Create(MyoErrorHandlerBridge.Create())));
            
            //Hub que irá utilizar o canal
            _hub = Hub.Create(_channel);
            
            //Envento de conexão quando o Myo é conectado
            _hub.MyoConnected += (sender, e) =>
            {
                MethodInvoker inv2 = delegate 
                {
                    this.buttonState.Image = Image.FromFile("C:\\Users\\Leandro\\OneDrive\\Documentos\\7 Periodo\\CG\\MyoSharp\\icon\\check-mark-8-16.png");
                    this.btnTam.Text = $"{this.tam}";
                };
                this.Invoke(inv2);
                e.Myo.Unlock(UnlockType.Hold);
                e.Myo.Vibrate(VibrationType.Short);
                e.Myo.PoseChanged += Myo_PoseChanged;
                e.Myo.Locked += Myo_Locked;
                e.Myo.Unlocked += Myo_Unlocked;
                e.Myo.AccelerometerDataAcquired += Myo_AccelerometerDataAcquired;
                e.Myo.GyroscopeDataAcquired += Myo_GyroscopeDataAcquired;
                e.Myo.OrientationDataAcquired += Myo_OrientationDataAcquired;
                InicializaListaOrientacao(e.Myo.Orientation);
            };

            //Envento de conexão quando o Myo é conectado
            _hub.MyoDisconnected += (sender, e) =>
            {
                MethodInvoker inv2 = delegate
                {
                    this.buttonState.Image = Image.FromFile("C:\\Users\\Leandro\\OneDrive\\Documentos\\7 Periodo\\CG\\MyoSharp\\icon\\cross - 25x25.png");
                    this.btnTam.Text = $"{this.tam}";
                };
                this.Invoke(inv2);
                e.Myo.PoseChanged -= Myo_PoseChanged;
                e.Myo.Locked -= Myo_Locked;
                e.Myo.Unlocked -= Myo_Unlocked;
                e.Myo.AccelerometerDataAcquired -= Myo_AccelerometerDataAcquired;
                e.Myo.GyroscopeDataAcquired -= Myo_GyroscopeDataAcquired;
                e.Myo.OrientationDataAcquired -= Myo_OrientationDataAcquired;
            };
        }

        #region Construtor e Desconstrutor
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _channel.StartListening();
        }

        protected override void OnClosed(EventArgs e)
        {
            _channel.Dispose();
            _hub.Dispose();

            base.OnClosed(e);
        }
        #endregion

        #region Métodos Privados
        private void InicializaListaOrientacao(QuaternionF orientacao)
        {
            rollAnterior.Add(0); rollAnterior.Add(0);
            pitchAnterior.Add(0); pitchAnterior.Add(0);
            yawAnterior.Add(0); yawAnterior.Add(0);
            Cursor.Position = new Point(0, 0);
        }

        private void LimpaListaOrientacao()
        {
            rollAnterior.Clear();
            pitchAnterior.Clear();
            yawAnterior.Clear();
        }

        private void PreencheListaOrientacao(double roll, double pitch, double yaw, double precisao)
        {
            LimpaListaOrientacao();

            rollAnterior.Add(((roll + PI) / (PI * 2.0f) * 10) - precisao);
            rollAnterior.Add(((roll + PI) / (PI * 2.0f) * 10) + precisao);
            pitchAnterior.Add(((pitch + PI) / (PI * 2.0f) * 10) - precisao);
            pitchAnterior.Add(((pitch + PI) / (PI * 2.0f) * 10) + precisao);
            yawAnterior.Add(((yaw + PI) / (PI * 2.0f) * 10) - precisao);
            yawAnterior.Add(((yaw + PI) / (PI * 2.0f) * 10) + precisao);
        }
        #endregion

        #region Event Handlers
        private void Myo_OrientationDataAcquired(object sender, OrientationDataEventArgs e)
        {
            var roll = System.Math.Round(((e.Roll + PI) / (PI * 2.0f) * 10), numeroCasasDecimais);
            var pitch = System.Math.Round(((e.Pitch + PI) / (PI * 2.0f) * 10), numeroCasasDecimais);
            var yaw = System.Math.Round(((e.Yaw + PI) / (PI * 2.0f) * 10), numeroCasasDecimais);
            
            if (listaRoll.Count % 2 != 1)
            {
                listaRoll.Add(roll);
                listaPitch.Add(pitch);
                listaYaw.Add(yaw);
                return;
            }
            else
            {
                roll = listaRoll.Sum() / listaRoll.Count();
                pitch = listaPitch.Sum() / listaPitch.Count();
                yaw = listaYaw.Sum() / listaYaw.Count();
                listaRoll.Clear();
                listaPitch.Clear();
                listaYaw.Clear();
            }

            if (roll > rollAnterior[1])
            {
                MethodInvoker inv1 = delegate { };
                this.Invoke(inv1);
            }
            else if (roll < rollAnterior[0])
            {
                MethodInvoker inv1 = delegate { };
                this.Invoke(inv1);
            }
            else
            {
                MethodInvoker inv1 = delegate { };
                this.Invoke(inv1);
            }

            if (yaw > yawAnterior[1])
            {
                MethodInvoker inv1 = delegate { };
                this.Invoke(inv1);
                Cursor.Position = new Point(Cursor.Position.X - pontosMovementoCursor, Cursor.Position.Y);
            }
            else if (yaw < yawAnterior[0])
            {
                MethodInvoker inv1 = delegate { };
                this.Invoke(inv1);
                Cursor.Position = new Point(Cursor.Position.X + pontosMovementoCursor, Cursor.Position.Y);
            }
            else
            {
                MethodInvoker inv1 = delegate { };
                this.Invoke(inv1);
            }

            if (pitch > pitchAnterior[1])
            {
                MethodInvoker inv1 = delegate { };
                this.Invoke(inv1);
                Cursor.Position = new Point(Cursor.Position.X, Cursor.Position.Y - pontosMovementoCursor);
            }
            else if (pitch < pitchAnterior[0])
            {
                MethodInvoker inv1 = delegate { };
                this.Invoke(inv1);
                Cursor.Position = new Point(Cursor.Position.X, Cursor.Position.Y + pontosMovementoCursor);
            }
            else
            {
                MethodInvoker inv1 = delegate { };
                this.Invoke(inv1);
            }


            MethodInvoker inv;

            PreencheListaOrientacao(e.Roll, e.Pitch, e.Yaw, 0.01);
            if (DesenharAtivo && !this.flagBorracha)
            {
                if (e.Myo.Pose == Pose.Fist)
                {
                    inv = delegate
                    {
                        Point relativePoint = this.PointToClient(Cursor.Position);
                        Bitmap bmp = new Bitmap(this.pbTela.Image);
                        Graphics g = Graphics.FromImage(bmp);

                        if (ponto.X == -1 && ponto.Y == -1)
                        {
                            ponto.X = relativePoint.X - 10;
                            ponto.Y = relativePoint.Y - 100;
                        }
                        else
                        {
                            Util.Util.PlotaRetaBresenham(g, ponto, new Point(relativePoint.X - 10, relativePoint.Y - 100), this.tam, this.dCor.BackColor);
                            ponto = new Point(relativePoint.X - 10, relativePoint.Y - 100);
                        }

                        this.pbTela.Image = bmp;
                    };
                    this.Invoke(inv);
                }
            }
            if (this.flagBorracha && !this.DesenharAtivo)
            {
                if (e.Myo.Pose == Pose.Fist)
                {

                    inv = delegate
                    {
                        Point relativePoint = this.PointToClient(Cursor.Position);
                        Bitmap bmp = new Bitmap(this.pbTela.Image);
                        Graphics g = Graphics.FromImage(bmp);



                        if (ponto.X == -1 && ponto.Y == -1)
                        {
                            ponto.X = relativePoint.X - 10;
                            ponto.Y = relativePoint.Y - 100;
                        }
                        else
                        {
                            Util.Util.PlotaRetaBresenham(g, ponto, new Point(relativePoint.X - 10, relativePoint.Y - 100), this.btam, this.pbTela.BackColor);
                            ponto = new Point(relativePoint.X - 10, relativePoint.Y - 100);
                        }

                        this.pbTela.Image = bmp;
                    };
                    this.Invoke(inv);
                }
            }
            if(e.Myo.Pose == Pose.Rest )
            {
                ponto = new Point(-1, -1);
            }
        }
        private void Myo_AccelerometerDataAcquired(object sender, AccelerometerDataEventArgs e)
        {
            MethodInvoker inv = delegate { };
            this.Invoke(inv);
        }
        private void Myo_GyroscopeDataAcquired(object sender, GyroscopeDataEventArgs e)
        {
            MethodInvoker inv = delegate { };
            this.Invoke(inv);
        }
        private void Myo_PoseChanged(object sender, PoseEventArgs e)
        {
            MethodInvoker inv;

            if (e.Myo.Pose == Pose.DoubleTap && this.click_cont == 1)
            {
                inv = delegate
                {
                    Point relativePoint = this.PointToClient(Cursor.Position);
                    Bitmap bmp = new Bitmap(this.pbTela.Image);
                    Graphics g = Graphics.FromImage(bmp);

                    lPoint.Add(new Point(relativePoint.X - 10, relativePoint.Y - 100));
                    Util.Util.DesenhaUmPonto(g, lPoint.Last<Point>().X, lPoint.Last<Point>().Y, this.tam, this.dCor.BackColor);
                    this.pbTela.Image = bmp;
                   
                };
                this.Invoke(inv);                          
                  
            }
            if (e.Myo.Pose == Pose.WaveIn)
            {
                if (this.click_cont == 1)
                {
                    this.formflag++;
                    if (this.formflag > 3)
                    { this.formflag = 0; }
                    inv = delegate
                    {
                       // this.button4.Text = $"{click_cont} | {this.formflag} | {DesenharAtivo} |  {flagBorracha}";
                        switch (this.formflag)
                        {
                            
                            case 1: //quadrado ativo
                                this.button1.BackColor = Color.BlueViolet;
                                this.button2.BackColor = Color.FromArgb(255, 240, 240, 240);
                                this.button3.BackColor = Color.FromArgb(255, 240, 240, 240);
                                this.button6.BackColor = Color.FromArgb(255, 240, 240, 240);
                                break;
                            case 2: //triangulo ativo
                                this.button3.BackColor = Color.BlueViolet;
                                this.button1.BackColor = Color.FromArgb(255, 240, 240, 240);
                                this.button2.BackColor = Color.FromArgb(255, 240, 240, 240);
                                this.button6.BackColor = Color.FromArgb(255, 240, 240, 240);
                                break;
                            case 3://circuferencia ativo
                                this.button6.BackColor = Color.BlueViolet;
                                this.button1.BackColor = Color.FromArgb(255, 240, 240, 240);
                                this.button3.BackColor = Color.FromArgb(255, 240, 240, 240);
                                this.button2.BackColor = Color.FromArgb(255, 240, 240, 240);
                                break;
                            default: // linha ativa
                                this.formflag = 0;
                                this.button2.BackColor = Color.BlueViolet;
                                this.button1.BackColor = Color.FromArgb(255, 240, 240, 240);
                                this.button3.BackColor = Color.FromArgb(255, 240, 240, 240);
                                this.button6.BackColor = Color.FromArgb(255, 240, 240, 240);
                                break;
                        }
                    };
                    this.Invoke(inv);

                }

                if (this.click_cont == 2)
                {

                    this.tam++;
                    if (this.tam < 0)
                    {
                        this.tam = 0;
                    }
                    inv = delegate { this.btnTam.Text = $"{this.tam}"; };
                    this.Invoke(inv);
                }
                if (this.click_cont == 3)
                {

                    this.btam++;
                    if (this.btam < 0)
                    {
                        this.btam = 0;
                    }
                    inv = delegate { this.btnTam.Text = $"{this.btam}"; };
                    this.Invoke(inv);
                }
                if (this.click_cont != 1 && this.click_cont != 2 && this.click_cont != 3)
                {
                    this.ContCor++;
                    this.ChangeColor(this.ContCor);
                    if(this.ContCor > 19 || this.ContCor < 0)
                    {
                        this.ContCor = 0;
                    }
                }

            }
            if (e.Myo.Pose == Pose.WaveOut)
            {
                if (this.click_cont == 1)
                {
                    this.formflag--;
                    if (this.formflag <=0)
                    { this.formflag = 0; }
                    inv = delegate
                    {
                       // this.button4.Text = $"{click_cont} | {this.formflag} | {DesenharAtivo} |  {flagBorracha}";
                        switch (this.formflag)
                        {

                            case 1: //quadrado ativo
                                this.button1.BackColor = Color.LightBlue;
                                this.button2.BackColor = Color.FromArgb(255, 240, 240, 240);
                                this.button3.BackColor = Color.FromArgb(255, 240, 240, 240);
                                this.button6.BackColor = Color.FromArgb(255, 240, 240, 240);
                                break;
                            case 2: //triangulo ativo
                                this.button3.BackColor = Color.LightBlue;
                                this.button1.BackColor = Color.FromArgb(255, 240, 240, 240);
                                this.button2.BackColor = Color.FromArgb(255, 240, 240, 240);
                                this.button6.BackColor = Color.FromArgb(255, 240, 240, 240);
                                break;
                            case 3://circuferencia ativo
                                this.button6.BackColor = Color.LightBlue;
                                this.button1.BackColor = Color.FromArgb(255, 240, 240, 240);
                                this.button3.BackColor = Color.FromArgb(255, 240, 240, 240);
                                this.button2.BackColor = Color.FromArgb(255, 240, 240, 240);
                                break;
                            default: // linha ativa
                                this.formflag = 0;
                                this.button2.BackColor = Color.LightBlue;
                                this.button1.BackColor = Color.FromArgb(255, 240, 240, 240);
                                this.button3.BackColor = Color.FromArgb(255, 240, 240, 240);
                                this.button6.BackColor = Color.FromArgb(255, 240, 240, 240);
                                break;
                        }
                    };
                    this.Invoke(inv);
                }

                if (this.click_cont == 2)
                {
                    
                        this.tam--;
                        if (this.tam < 0)
                        {
                            this.tam = 0;
                        }
                    inv = delegate { this.btnTam.Text = $"{this.tam}"; };
                    this.Invoke(inv);
                }
                if ( this.click_cont == 3)
                {

                    this.btam--;
                    if (this.btam < 0)
                    {
                        this.btam = 0;
                    }
                    inv = delegate { this.btnTam.Text = $"{this.btam}"; };
                    this.Invoke(inv);
                }
                if (this.click_cont!=1 && this.click_cont != 2 && this.click_cont != 3 )
                {
                    this.ContCor--;
                    if (this.ContCor > 19 || this.ContCor < 0)
                    {
                        this.ContCor = 0;
                    }
                    this.ChangeColor(this.ContCor);
                }
            }

            if (e.Myo.Pose == Pose.Fist)
            {
                if (this.click_cont == 0)
                {
                    DesenharAtivo = true;
                    
                }
                if (this.click_cont == 1)
                {

                    inv = delegate
                    {
                        Bitmap bmp = new Bitmap(this.pbTela.Image);
                        Graphics g = Graphics.FromImage(bmp);
                        //Linha
                        if (this.formflag == 0 && lPoint.Count >= 2)
                        {
                            Util.Util.PlotaRetaBresenham(g, lPoint[lPoint.Count - 2], lPoint[lPoint.Count-1], this.tam, this.dCor.BackColor);
                            lPoint.Clear();
                        }
                        //Quadrado
                        if (this.formflag == 1 && lPoint.Count >= 4)
                        {
                            Util.Util.PlotaRetaBresenham(g, lPoint[lPoint.Count - 4], lPoint[lPoint.Count - 3], this.tam, this.dCor.BackColor);
                            Util.Util.PlotaRetaBresenham(g, lPoint[lPoint.Count - 3], lPoint[lPoint.Count - 2], this.tam, this.dCor.BackColor);
                            Util.Util.PlotaRetaBresenham(g, lPoint[lPoint.Count - 2], lPoint[lPoint.Count - 1], this.tam, this.dCor.BackColor);
                            Util.Util.PlotaRetaBresenham(g, lPoint[lPoint.Count - 1], lPoint[lPoint.Count -4], this.tam, this.dCor.BackColor);
                            lPoint.Clear();
                        }
                        //Triangulo
                        if (this.formflag == 2 && lPoint.Count >= 3)
                        {
                            Util.Util.PlotaRetaBresenham(g, lPoint[lPoint.Count - 3], lPoint[lPoint.Count - 2], this.tam, this.dCor.BackColor);
                            Util.Util.PlotaRetaBresenham(g, lPoint[lPoint.Count - 2], lPoint[lPoint.Count - 1], this.tam, this.dCor.BackColor);
                            Util.Util.PlotaRetaBresenham(g, lPoint[lPoint.Count - 1], lPoint[lPoint.Count - 3], this.tam, this.dCor.BackColor);
                            lPoint.Clear();
                        }
                        //Circuferencia
                        if (this.formflag == 3 && lPoint.Count >= 2)
                        {
                            Util.Util.PlotaCircunferenciaBresenham(g, lPoint[lPoint.Count - 1], (int)Util.Util.CalculaRaio(lPoint[lPoint.Count - 2], lPoint[lPoint.Count -1]), this.tam, this.dCor.BackColor);
                            lPoint.Clear();
                        }
                        this.pbTela.Image = bmp;
                    };
                    this.Invoke(inv);
                }
                if (this.click_cont == 4 )
                {
                    inv = delegate
                     {
                         Bitmap bmp = new Bitmap(this.pbTela.Image);
                         bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                         this.pbTela.Image = bmp;
                     };
                    this.Invoke(inv);
                }
                if (this.click_cont == 5)
                {
                    inv = delegate
                    {
                        Bitmap bmp = new Bitmap(this.pbTela.Image);
                        bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        this.pbTela.Image = bmp;
                    };
                    this.Invoke(inv);
                }
            }
              
            
            if (e.Myo.Pose == Pose.FingersSpread)
            {
                this.click_cont++;

                switch (this.click_cont)
                {
                    case 1:
                        this.flagBorracha = false;
                        this.DesenharAtivo = false;
                        this.ponto = new Point(-1, -1);
                        this.formflag = 0;
                        inv = delegate {
                            this.button2.BackColor = Color.LightBlue;
                            this.button1.BackColor = Color.FromArgb(255, 240, 240, 240);
                            this.button3.BackColor = Color.FromArgb(255, 240, 240, 240);
                            this.button6.BackColor = Color.FromArgb(255, 240, 240, 240);
                            this.button4.Text = "Formas"; };
                        this.Invoke(inv);
                        break;
                    case 2:
                        inv = delegate
                        {
                            this.button2.BackColor = Color.FromArgb(255, 240, 240, 240);
                            this.button1.BackColor = Color.FromArgb(255, 240, 240, 240);
                            this.button3.BackColor = Color.FromArgb(255, 240, 240, 240);
                            this.button6.BackColor = Color.FromArgb(255, 240, 240, 240);
                            this.button4.Text = "Tamanho do Pincel";
                            this.btnTam.BackColor = Color.LightBlue;
                        };
                        this.Invoke(inv);
                        this.ponto = new Point(-1, -1);
                        break;
                    case 3:
                        this.ponto = new Point(-1, -1);
                        this.flagBorracha = true;
                        this.DesenharAtivo = false;
                        inv = delegate 
                        {
                            this.button4.Text = "Borracha";
                            this.btnTam.BackColor = Color.FromArgb(255, 240, 240, 240);
                            this.button9.BackColor = Color.LightBlue;
                            this.btnTam.Text = $"{this.btam}";
                        };
                        this.Invoke(inv);
                        this.ponto = new Point(-1, -1);
                        break;
                    case 4:
                        inv = delegate 
                        {
                            this.button4.Text = "Rotação 90";

                            this.button7.BackColor = Color.LightBlue;
                            this.button8.BackColor = Color.FromArgb(255, 240, 240, 240);
                            this.button9.BackColor = Color.FromArgb(255, 240, 240, 240);
                            this.btnTam.Text = $"{this.tam}";
                        };
                        this.Invoke(inv);
                        this.ponto = new Point(-1, -1);
                        break;
                    case 5:
                        inv = delegate
                        {
                            this.button4.Text = "Rotação 180";
                            this.button7.BackColor = Color.FromArgb(255, 240, 240, 240);
                            this.button8.BackColor = Color.LightBlue;
                            this.btnTam.Text = $"{this.tam}";
                        };
                        this.Invoke(inv);
                        this.ponto = new Point(-1, -1);
                        break;
                    default:
                        this.ponto = new Point(-1, -1);
                        this.flagBorracha = false;
                        this.DesenharAtivo = false;
                        this.click_cont = 0;

                        inv = delegate
                        {
                            this.button7.BackColor = Color.FromArgb(255, 240, 240, 240);
                            this.button8.BackColor = Color.FromArgb(255, 240, 240, 240);
                            this.button2.BackColor = Color.FromArgb(255, 240, 240, 240);
                            this.button1.BackColor = Color.FromArgb(255, 240, 240, 240);
                            this.button3.BackColor = Color.FromArgb(255, 240, 240, 240);
                            this.button6.BackColor = Color.FromArgb(255, 240, 240, 240);
                            this.button4.Text = "Desenhar";
                        };
                        this.Invoke(inv);
                        break;
                }
                

            }
           
            //MethodInvoker inv = delegate { };
            //this.Invoke(inv);


        }

        private void Myo_Unlocked(object sender, MyoEventArgs e)
        {
            MethodInvoker inv = delegate { };
            this.Invoke(inv);
        }

        private void Myo_Locked(object sender, MyoEventArgs e)
        {
            MethodInvoker inv = delegate { };
            this.Invoke(inv);
        }
        /**
        Metodo que troca a cor utilizada
            Entrada @int c
            Saida: Void 
         */
        private void ChangeColor(int c)
        {
            MethodInvoker inv;
            switch (c)
            {
                case 0:
                    
                    inv = delegate { this.dCor.BackColor = Color.Black; };
                    this.Invoke(inv);
                    break;
                case 1:
                    
                     inv = delegate { this.dCor.BackColor = Color.DarkRed; };
                    this.Invoke(inv);
                    break;
                case 2:
                    
                     inv = delegate { this.dCor.BackColor = Color.Red; };
                    this.Invoke(inv);
                    break;
                case 3:
                    
                     inv = delegate { this.dCor.BackColor = Color.Orange; };
                    this.Invoke(inv);
                    break;
                case 4:
                    
                     inv = delegate { this.dCor.BackColor = Color.Yellow; };
                    this.Invoke(inv);
                    break;
                case 5:
                    
                     inv = delegate { this.dCor.BackColor = Color.Green; };
                    this.Invoke(inv);
                    break;
                case 6:
                    
                     inv = delegate { this.dCor.BackColor = Color.Turquoise; };
                    this.Invoke(inv);
                    break;
                case 7:
                    
                     inv = delegate { this.dCor.BackColor = Color.Indigo; };
                    this.Invoke(inv);
                    break;
                case 8:
                    
                     inv = delegate { this.dCor.BackColor = Color.Purple; };
                    this.Invoke(inv);
                    break;
                case 9:
                    
                     inv = delegate { this.dCor.BackColor = Color.Brown; };
                    this.Invoke(inv);
                    break;
                case 10:
                    
                     inv = delegate { this.dCor.BackColor = Color.LightGray; };
                    this.Invoke(inv);
                    break;
                case 11:
                    
                     inv = delegate { this.dCor.BackColor = Color.White; };
                    this.Invoke(inv);
                    break;
                case 12:
                   
                     inv = delegate { this.dCor.BackColor = Color.Lavender; };
                    this.Invoke(inv);
                    break;
                case 13:
                    
                     inv = delegate { this.dCor.BackColor = Color.BlueViolet; };
                    this.Invoke(inv);
                    break;
                case 14:
                    
                     inv = delegate { this.dCor.BackColor = Color.PaleTurquoise; };
                    this.Invoke(inv);
                    break;
                case 15:
                    
                     inv = delegate { this.dCor.BackColor = Color.LemonChiffon; };
                    this.Invoke(inv);
                    break;
                case 16:
                    
                     inv = delegate { this.dCor.BackColor = Color.LightYellow; };
                    this.Invoke(inv);
                    break;
                case 17:
                    
                     inv = delegate { this.dCor.BackColor = Color.Gold; };
                    this.Invoke(inv);
                    break;
                case 18:
                    
                    inv = delegate { this.dCor.BackColor = Color.Pink; };
                    this.Invoke(inv);
                    break;
                case 19:
                    
                     inv = delegate { this.dCor.BackColor = Color.DarkRed; };
                    this.Invoke(inv);
                    break;
                default:
                    
                     inv = delegate { this.dCor.BackColor = Color.DarkRed; };
                    this.Invoke(inv);
                    break;
            }
        }


    }

    
        #endregion
        

    }


