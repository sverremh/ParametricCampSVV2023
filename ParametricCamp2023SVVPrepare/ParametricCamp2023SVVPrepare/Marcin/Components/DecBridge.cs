using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using ParametricCamp2023SVV.Marcin.Classes;
namespace ParametricCamp2023SVV.Marcin
{
    public class DecBridge : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DecBridge class.
        /// </summary>
        public DecBridge()
          : base("DecBridge", "Nickname",
              "Description",
              "Parametric Camp SVV", "Bridge")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("bridge","b","",GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("id","i","",GH_ParamAccess.item);  //0
            pManager.AddTextParameter("name", "n", "", GH_ParamAccess.item); //1
            pManager.AddBrepParameter("geometry","g","",GH_ParamAccess.list); //2
            pManager.AddCurveParameter("axis", "a", "", GH_ParamAccess.item); //3
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Bridge b = new Bridge();
            DA.GetData(0, ref b);

            string name = b.name;
            int id = b.id;
            Curve a = b.axis;

            List<Brep> allBreps = new List<Brep>();
            foreach (var column in b.columns)
            { 
                allBreps.Add(column.geometry);
            }

            DA.SetData(0, id);
            DA.SetData(1, name);
            DA.SetDataList(2, allBreps);
            DA.SetData(3, a);

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
            get { return new Guid("59C6CDEA-4AA5-4E2D-82CE-AE3EF45A271A"); }
        }
    }
}