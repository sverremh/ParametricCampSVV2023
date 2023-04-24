using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParametricCamp2023SVV.Marcin.Classes
{
    internal class Column
    {
        public string name;
        public int id;
        public Brep geometry;
        public Curve axis;  //this is needed for IFC

        //constructor 1
        public Column()
        {
            //empty constructor
        }

        //constructor 2
        public Column(string _name, int _id, Curve _axis, double radius)
        {
            name = _name;
            id = _id;
            geometry = new Brep();
            var pipes = Brep.CreatePipe(_axis, radius, false, PipeCapMode.Flat, true, 0.0001, 0.0001);
            geometry = pipes[0];
            axis = _axis; //this is needed for IFC

        }
    }
}
