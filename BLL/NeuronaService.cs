﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ENTITY;
using DAL;

namespace BLL
{
    public class NeuronaService
    {
        private NeuronaRepository _Neurona { get; set; }

        public NeuronaService()
        {
            _Neurona = new NeuronaRepository();
        }

        public Red ReadXml(string Path)
        {
            return _Neurona.ReadXml(Path);
        }

        public void WriteXML(Red R, string Path)
        {
            _Neurona.WriteXml(R, Path);
        }

        public void EntrenarPausable()
        {
            var i = Telefono.Red.Entrenamientos;
            if(Telefono.Red.Error > Telefono.Red.ErrorMaxPermitido)
            {
                while (i < Telefono.Red.Iteraciones && Telefono.Continuar)
                {
                    var ErrorIteracion = Telefono.Red.Entrenar();
                    Telefono.Red.Error = ErrorIteracion;
                    i++;
                    Telefono.Red.Entrenamientos = i;
                    System.Threading.Thread.Sleep(200);
                    _Neurona.WriteXml(Telefono.Red, null);
                    if (Telefono.Red.ErrorMaxPermitido >= ErrorIteracion) break;
                }
            }
        }

    }
}
