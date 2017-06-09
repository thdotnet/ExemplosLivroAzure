using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;

namespace Exemplo.DocumentDB
{
    public class Evento : Resource
    {
        public Evento()
        {
            Participantes = new List<Participante>();
        }

        public string Nome { get; set; }

        public DateTime? DataEvento { get; set; }

        public List<Participante> Participantes { get; set; }
    }
}
