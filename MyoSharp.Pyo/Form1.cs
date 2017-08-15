using MyoSharp.Communication;
using MyoSharp.Device;
using MyoSharp.Exceptions;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MyoSharp.Math;
using System.Drawing;
using System.Linq;

namespace MyoSharp.Pyo
{
    public partial class Form1 : Form
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
        private const int pontosMovementoCursor = 10;
        private const float PI = (float)System.Math.PI;
        private int click_cont=0;

        
        #endregion

        public Form1()
        {
            InitializeComponent();
            //comentário
            // get set up to listen for Myo events
            _channel = Channel.Create(ChannelDriver.Create(ChannelBridge.Create(),
                                        MyoErrorHandlerDriver.Create(MyoErrorHandlerBridge.Create())));

            _hub = Hub.Create(_channel);

            _hub.MyoConnected += (sender, e) =>
            {
                MethodInvoker inv = delegate { this.label1.Text = $"Myo {e.Myo.Handle} conectado"; };
                this.Invoke(inv);

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

            // listen for when the Myo disconnects
            _hub.MyoDisconnected += (sender, e) =>
            {
                var braco = e.Myo.Arm == Arm.Right ? "Direito" : e.Myo.Arm == Arm.Left ? "Esquerdo" : "Desconhecido";
                MethodInvoker inv = delegate { this.label2.Text = $"Parece que o braco {braco} desconectou"; };
                this.Invoke(inv);
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

            // start listening for Myo data
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

            //rollAnterior.Add(System.Math.Round(roll, numeroCasasDecimais) - precisao);
            //rollAnterior.Add(System.Math.Round(roll, numeroCasasDecimais) + precisao);

            //pitchAnterior.Add(System.Math.Round(pitch, numeroCasasDecimais) - precisao);
            //pitchAnterior.Add(System.Math.Round(pitch, numeroCasasDecimais) + precisao);

            //yawAnterior.Add(System.Math.Round(yaw, numeroCasasDecimais) - precisao);
            //yawAnterior.Add(System.Math.Round(yaw, numeroCasasDecimais) + precisao);



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
            //hora = DateTime.Now;
            //if (hora <= flagHora)
            //    return;
            //else
            //{
            //    hora = DateTime.Now;
            //    flagHora = hora.AddMilliseconds(1);
            //}
            //double roll = System.Math.Round(e.Roll, numeroCasasDecimais);
            //double pitch = System.Math.Round(e.Pitch, numeroCasasDecimais);
            //double yaw = System.Math.Round(e.Yaw, numeroCasasDecimais);

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
                MethodInvoker inv1 = delegate { this.label9.Text = $"gira esquerda"; };
                this.Invoke(inv1);
            }
            else if (roll < rollAnterior[0])
            {
                MethodInvoker inv1 = delegate { this.label9.Text = $"gira direita"; };
                this.Invoke(inv1);
            }
            else
            {
                MethodInvoker inv1 = delegate { this.label9.Text = $"parado"; };
                this.Invoke(inv1);
            }

            if (yaw > yawAnterior[1])
            {
                MethodInvoker inv1 = delegate { this.label10.Text = $"roda esquerda"; };
                this.Invoke(inv1);
                Cursor.Position = new Point(Cursor.Position.X - pontosMovementoCursor, Cursor.Position.Y);
            }
            else if(yaw < yawAnterior[0])
            {
                MethodInvoker inv1 = delegate { this.label10.Text = $"roda direita"; };
                this.Invoke(inv1);
                Cursor.Position = new Point(Cursor.Position.X + pontosMovementoCursor, Cursor.Position.Y);
            }
            else
            {
                MethodInvoker inv1 = delegate { this.label10.Text = $"parado"; };
                this.Invoke(inv1);
            }

            if (pitch > pitchAnterior[1])
            {
                MethodInvoker inv1 = delegate { this.label11.Text = $"sobe"; };
                this.Invoke(inv1);
                Cursor.Position = new Point(Cursor.Position.X, Cursor.Position.Y - pontosMovementoCursor);
            }
            else if(pitch < pitchAnterior[0])
            {
                MethodInvoker inv1 = delegate { this.label11.Text = $"desce"; };
                this.Invoke(inv1);
                Cursor.Position = new Point(Cursor.Position.X, Cursor.Position.Y + pontosMovementoCursor);
            }
            else
            {
                MethodInvoker inv1 = delegate { this.label11.Text = $"parado"; };
                this.Invoke(inv1);
            }

            MethodInvoker inv = delegate { this.label8.Text = $"Row: {roll} | Pitch: {pitch} | Yaw: {yaw}"; };
            this.Invoke(inv);

            MethodInvoker inv2 = delegate { this.label12.Text = $"X: {Cursor.Position.X-190} |  Y: {Cursor.Position.Y-30}"; };
            this.Invoke(inv2);
            PreencheListaOrientacao(e.Roll, e.Pitch, e.Yaw, 0.01);
        }
        private void Myo_AccelerometerDataAcquired(object sender, AccelerometerDataEventArgs e)
        {
            MethodInvoker inv = delegate { this.label6.Text = $"X: {e.Accelerometer.X} | Y: {e.Accelerometer.Y} | Z: {e.Accelerometer.Z}"; };
            this.Invoke(inv);
        }

        private void Myo_GyroscopeDataAcquired(object sender, GyroscopeDataEventArgs e)
        {
            MethodInvoker inv = delegate { this.label7.Text = $"X: {e.Gyroscope.X} | Y: {e.Gyroscope.Y} | Z: {e.Gyroscope.Z}"; };
            this.Invoke(inv);
        }
        
        private void Myo_PoseChanged(object sender, PoseEventArgs e)
        {
            MethodInvoker inv2;
            var braco = e.Myo.Arm == Arm.Right ? "Direito" : e.Myo.Arm == Arm.Left ? "Esquerdo" : "Desconhecido";
            MethodInvoker inv = delegate { this.label3.Text = $"Pose \"{e.Myo.Pose}\" detectada no braco {braco}"; };

            //Evento de clique
            if ($"{e.Myo.Pose}"=="Fist")
            {
                inv2 = delegate { this.label13.Text = "Entro"; };
                this.Invoke(inv2);
                click_cont = 1;
            }
         else  if ($"{e.Myo.Pose}" == "FingersSpread")
            {
                inv2 = delegate { this.label13.Text = "Clico"; };
                this.Invoke(inv2);
                if ( ( (Cursor.Position.X - 190) - this.label12.Location.X) <= 20 && ((Cursor.Position.Y- 30) - this.label12.Location.Y) <= 20)
                {
                    this.label12.Click+= label12_Click ;
                }
            }
            
        }

        private void Myo_Unlocked(object sender, MyoEventArgs e)
        {
            var braco = e.Myo.Arm == Arm.Right ? "Direito" : e.Myo.Arm == Arm.Left ? "Esquerdo" : "Desconhecido";
            MethodInvoker inv = delegate { this.label4.Text = $"Braco {braco} desbloqueado"; };
            this.Invoke(inv);
        }

        private void Myo_Locked(object sender, MyoEventArgs e)
        {
            var braco = e.Myo.Arm == Arm.Right ? "Direito" : e.Myo.Arm == Arm.Left ? "Esquerdo" : "Desconhecido";
            MethodInvoker inv = delegate { this.label5.Text = $"Braco {braco} bloqueado"; };
            this.Invoke(inv);
        }

        #endregion

        private void label12_Click(object sender, EventArgs e)
        {   
            this.label3.Text = "SADSADASd";
        }
    }
}
