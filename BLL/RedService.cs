using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ENTITY;
using DAL;

namespace BLL
{
    public class RedService
    {
        private RedRepository RedRepo { get; set; }

        public RedService()
        {
            RedRepo = new RedRepository();
        }

        public Red GetXML(string Path)
        {
            return RedRepo.GetXML(Path);
        }

        public void PostXML(Red R, string Path)
        {
            RedRepo.PostXML(R, Path);
        }

        public List<Patron> GetDS(string Path)
        {
            return RedRepo.GetDS(Path);
        }

        public void Iterar()
        {
            var i = Plataforma.Red.Entrenamientos;
            if (Plataforma.Red.Error > Plataforma.Red.ErrorMaxPermitido)
            {
                while (i < Plataforma.Red.Iteraciones && Plataforma.Continuar)
                {
                    var ErrorIteracion = Plataforma.Red.Entrenar();
                    Plataforma.Red.Error = ErrorIteracion;
                    i++;
                    Plataforma.Red.Entrenamientos = i;
                    System.Threading.Thread.Sleep(100);
                    RedRepo.PostXML(Plataforma.Red, null);

                   if (Plataforma.Red.ErrorMaxPermitido >= ErrorIteracion) break;
                }
            }
        }

    }
}
