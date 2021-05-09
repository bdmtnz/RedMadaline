﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ENTITY
{
    public class Neurona
    {
        public Salida Salida { get; set; }
        public Pesos Pesos { get; set; }
        public Pesos PesosTemp { get; set; }
        public Umbral Umbral { get; set; }
        public Umbral UmbralTemp { get; set; }
        public bool Habilitada { get; set; }
        public bool Usada { get; set; }

        public Neurona()
        {
            Pesos = new Pesos();
            PesosTemp = new Pesos();
            Salida = new Salida();
            Umbral = new Umbral();
            UmbralTemp = new Umbral();
            Habilitada = true;
        }

        //public void CalcularSoma(Pesos Pesos, Patron Patron)
        private double CalcularSoma(List<double> Entradas)
        {
            var Productos = 0.0;
            for (int i = 0; i < Entradas.Count-1; i++)
                Productos += Entradas[i] * Pesos.Valores[i].Valor;
            return Productos - Umbral.Valor;
        }
        private double CalcularSomaTemp(List<double> Entradas)
        {
            //VALIDAR LOS PESOS TEMPORALES
            var Productos = 0.0;
            for (int i = 0; i < Entradas.Count-1; i++)
                Productos += Entradas[i] * PesosTemp.Valores[i].Valor;
            return Productos - UmbralTemp.Valor;
        }

        public void Activar(Activacion Activacion, List<double> Entradas)
        {            
            Salida.YR = Activacion.Activar(CalcularSoma(Entradas));
        }
        public Salida ActivarTemp(Activacion Activacion, List<double> Entradas)
        {
            PesosTemp.Valores = Pesos.Valores.Select(x => new Peso(x.Valor)).ToList();
            var salida = new Salida();
            salida.YR = Activacion.Activar(CalcularSomaTemp(Entradas));
            salida.YD = Salida.YD;
            return salida;
        }

        //private void EntrenarPesos(int Fila, Patron Patron, double Rata)
        //public void Entrenar(double AnteriorValor, double Rata, double ErrorSalida, double Entrada)
        //LAS ENTRADAS SON LAS SALIDAS DE CADA NEURONA DE LA CAPA INMEDIATAMENTE ANTERIOR
        public void EntrenarPesos(List<double> Entradas, double Rata, double ErrorPatron)
        {
            for (int j = 0; j < Pesos.Valores.Count; j++)
            {
                Pesos.Valores[j].Entrenar(
                    Pesos.Valores[j].Valor,
                    Rata,
                    ErrorPatron,
                    Entradas[j]
                );
            }
        }
        public void EntrenarPesosTemp(List<double> Entradas, double Rata, double ErrorPatron)
        {
            PesosTemp.Valores = Pesos.Valores.Select(x=>new Peso(x.Valor)).ToList();
            for (int j = 0; j < PesosTemp.Valores.Count; j++)
            {
                PesosTemp.Valores[j].Entrenar(
                    PesosTemp.Valores[j].Valor,
                    Rata,
                    ErrorPatron,
                    Entradas[j]
                );
            }
        }

        public void CalcularError(List<double> Pesos, List<double> Errores)
        {
            var Error = 0.0;
            for (int i = 0; i < Pesos.Count; i++)
            {
                Error += Pesos[i] * Errores[i];
            }
            Salida.SetError(Error);
        }

        public void AceptarPesos()
        {
            //SOBREESCRIBIR PESOS TEMP EN PESOS
            Pesos.Valores = PesosTemp.Valores.Select(i => new Peso(i.Valor)).ToList();
            //PesosTemp.Valores.Clear();
        }

    }

}
