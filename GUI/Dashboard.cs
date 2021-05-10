using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

using ENTITY;
using BLL;

namespace GUI
{
    public partial class Dashboard : Form
    {
        private RedService _Neurona { get; set; }
        private Red Red { get; set; }
        public int X_Click { get; set; }
        public int Y_Click { get; set; }
        private Generalizador FrmSimulador { get; set; }
        private Graficador Grafica { get; set; }
        public ToolTip ToolTips { get; set; }

        public Dashboard()
        {
            _Neurona = new RedService();
            ToolTips = new ToolTip();
            Red = new Red();
            InitializeComponent();
            BtnPausa.Visible = false;
            BtnIniciar.Visible = true;
            Config();
            Preload();
        }

        private void Preload()
        {
            Abrir(new Home(Red));
        }

        private void Config()
        {
            //FILTROS
            OFD.Filter = "Archivo XML (*.XML)|*.XML";
            SFD.Filter = "Archivo XML (*.XML)|*.XML";

            //CONFIG LINK
            linkLabel1.Links.Add(0, linkLabel1.Text.Length, "https://github.com/bdmtnz");
            linkLabel2.Links.Add(0, linkLabel2.Text.Length, "https://sites.google.com/a/unicesar.edu.co/tonnyjimenezm/");

            //CARGA DE CACHÉ
            var Rd = _Neurona.GetXML(null);
            if (Rd != null)
                Red = Plataforma.Red = Rd;
            else
                MessageBox.Show("El dataset está corrupto o está mal configurado");
            ShowInfo(Red);

            //TOOLTIPS
            ToolTips.SetToolTip(TbNeuronas, "Ej. '2 3 2' es equivalente a 2 capas ocultas\n de 2 y 3 neuronas respectivamente y\n una capa de salida con 2 neuronas");
            ToolTips.SetToolTip(TbFuncion, "Ej. 'S T G L' es quivalente asignar las funciones\n Sigmoide, Tangente hiperbólica, Gaussiana\n y Lineal a 4 capas de la red");
            ToolTips.SetToolTip(BtnReiniciar, "Reinicializa la red");
            ToolTips.SetToolTip(BtnSimulation, "Generaliza con al red entrenada");
            ToolTips.SetToolTip(BtnSave, "Guarda el estado de la red");
            ToolTips.SetToolTip(BtnOpen, "Abre un archivo con el estado de una red");
        }

        private void ShowInfo(Red N)
        {
            LbEntradas.Text = N.Entradas.ToString();
            LbPatrones.Text = N.Patrones.Count.ToString();
            LbSalidas.Text = N.Capas[N.Capas.Count - 1].Neuronas.Count.ToString();
            LbEntrenado.Text = N.Entrenamientos.ToString();
            NbErrorMax.Value = (decimal)N.ErrorMaxPermitido;
            NbIteracion.Value = N.Iteraciones;
            NbRata.Value = (decimal)N.Rata;
            LbPeso.Text = N.Capas.Count.ToString() + " M";
            LbUmbral.Text = N.Capas.Count.ToString() + " V";
            TbNeuronas.Text = "";
            foreach (var item in N.Capas)
            {
                TbNeuronas.Text += item.Neuronas.Count + ";";
            }
            TbNeuronas.Text = TbNeuronas.Text.Remove(TbNeuronas.Text.Length - 1);
            TbFuncion.Text = "";
            foreach (var item in N.Capas)
            {
                TbFuncion.Text += item.Activacion.Funcion.ToString()[0] + ";";
            }
            TbFuncion.Text = TbFuncion.Text.Remove(TbFuncion.Text.Length - 1);
        }

        private void MoveWindow(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                X_Click = e.X; Y_Click = e.Y;
            }
            else
            {
                Left += (e.X - X_Click);
                Top += (e.Y - Y_Click);
            }
        }

        private void Abrir(Form ventana)
        {
            Embebed.Controls.Clear();

            ventana.TopLevel = false;
            Embebed.Controls.Add(ventana);
            Embebed.Tag = true;
            ventana.Dock = DockStyle.Fill;
            ventana.Show();
        }

        private void Close(object sender, EventArgs e)
        {
            Dispose();
        }

        private void Minimize(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void Entrenar(object sender, EventArgs e)
        {
            if(Red.Error <= Red.ErrorMaxPermitido)
            {
                MessageBox.Show("Esta red ya se encuentra entrenada, proceda a simular");
                return;
            }
            else if (Red.Patrones.Count <= 0)
            {
                MessageBox.Show("Esta red está vacia, llenela y entrenela");
                return;
            }
            else if(Red.Iteraciones <= 0)
            {
                MessageBox.Show("Por favor ingrese un número de iteraciones valido");
                return;
            }
            BtnIniciar.Visible = false;
            BtnPausa.Visible = true;
            RunTask();
            Abrir(new Graficador(Red));
        }

        private async void RunTask()
        {
            PbCarga.Visible = true;
            Plataforma.Continuar = true;
            Plataforma.Red = Red;
            var T = new Task(_Neurona.Iterar);
            T.Start();
            await T;
            Red = Plataforma.Red;
            ShowInfo(Red);
            PbCarga.Visible = false;
            BtnPausa.Visible = false;
            BtnIniciar.Visible = true;
        }

        private void Simular(object sender, EventArgs e)
        {
            if(Red.Patrones.Count <= 0)
            {
                MessageBox.Show("Esta red está vacia, llenela y entrenela");
                return;
            }
            else if (Red.Error > Red.ErrorMaxPermitido)
            {
                MessageBox.Show("La red aún no esta entrenada, por favor entrenela");
            }
            else
                Abrir(new Generalizador(Red, _Neurona));
            //FrmSimulador = new Generalizador(Red, _Neurona);
        }

        private void BtnOpen_Click(object sender, EventArgs e)
        {
            var Result = OFD.ShowDialog();
            if (Result == DialogResult.OK)
            {
                try
                {
                    if (File.Exists(OFD.FileName))
                    {
                        var Rd =  _Neurona.GetXML(OFD.FileName);
                        if (Rd != null)
                            Red = Plataforma.Red = Rd;
                        else
                            MessageBox.Show("El dataset está corrupto o está mal configurado");
                        ShowInfo(Red);
                    }
                    else
                        MessageBox.Show("Se ha eliminado o movido el archivo");
                }
                catch (Exception er)
                {
                    MessageBox.Show("Error al abrir el archivo => " + er.Message, "Proceso fallido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else if (Result == DialogResult.Cancel)
            {
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SFD.FileName = $"data.XML";
            if (SFD.ShowDialog() == DialogResult.OK)
            {
                _Neurona.PostXML(Red, SFD.FileName);
            }
        }

        private void BtnPausa_Click(object sender, EventArgs e)
        {
            BtnPausa.Visible = false;
            BtnIniciar.Visible = true;
            Red = Plataforma.Red;
            Plataforma.Continuar = false;
            ShowInfo(Red);
            
        }

        private void ItChange(object sender, EventArgs e)
        {
            Red.Iteraciones = (int)NbIteracion.Value;
            Plataforma.Red = Red;
        }

        private void EmChange(object sender, EventArgs e)
        {
            Red.ErrorMaxPermitido = (double)NbErrorMax.Value;
            Plataforma.Red = Red;
        }

        private void RaChange(object sender, EventArgs e)
        {
            Red.Rata = (double)NbRata.Value;
            Plataforma.Red = Red;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Plataforma.Red.ReiniciarRed();
            Red = Plataforma.Red;
            ShowInfo(Red);
        }

        private void OpenLink(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ProcessStartInfo PInfo = new ProcessStartInfo(e.Link.LinkData.ToString());
            Process.Start(PInfo);

        }
    }
}
