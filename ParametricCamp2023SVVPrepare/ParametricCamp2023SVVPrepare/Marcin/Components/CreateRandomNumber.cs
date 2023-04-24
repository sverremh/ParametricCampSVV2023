using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ParametricCamp2023SVV.Marcin
{
    public class CreateRandomNumber : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreateRandomNumber class.
        /// </summary>
        public CreateRandomNumber()
          : base("CreateRandomNumber", "random",
              "This is a component which create random double",
              "Parametric Camp SVV", "Tools")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("run","r","find me a new random",GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("number", "n", "new random number", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool r = false;
            DA.GetData(0, ref r);
            double number = 0.2;
            if (r)
            {
                double min = 5;
                double max = 10;
                number = createNumber(min,max);
            }
            DA.SetData(0, number);
        }
        double createNumber(double min, double max)
        {
            double n = 0;
            var random = new Random(); //random class
            var rDouble = random.NextDouble(); //random number from 0.00 to 1.00
            var rRangeDouble = rDouble * (max - min) + min; //apply range
            return rRangeDouble;
        }




        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("35E39473-1BF1-468B-ADAA-BA02637DC7D2"); }
        }
    }
}