using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParametricCamp2023SVV.Marcin.Classes
{
    internal class Bridge
    {
        public string name;
        public int id;

        public List<string> information;

        public Curve axis;

        public Deck deck;
        public List<Column> columns;
    }
}
