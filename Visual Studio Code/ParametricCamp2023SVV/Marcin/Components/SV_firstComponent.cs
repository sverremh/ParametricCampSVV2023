using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

namespace ParametricCamp2023SVV.Marcin
{
    public class SV_firstComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public SV_firstComponent()
          : base("SV_firstComponent", "Nickname",
            "Description",
            "Parametric Camp SVV", "Tools")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("input1","i","this is input to my code", GH_ParamAccess.item); //0
            pManager.AddTextParameter("input2", "i", "this is input to my code", GH_ParamAccess.item); //1
            pManager.AddNumberParameter("input3", "i", "this is input to my code", GH_ParamAccess.item); //2
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("output", "o", "this is output to my code", GH_ParamAccess.item); //0
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string text1 = "some text";
            string text2 = "some text";
            double text3 = 1;
            DA.GetData(0, ref text1);
            DA.GetData(1, ref text2);
            DA.GetData(2, ref text3);

            text1 = text1 + " some modification";

            DA.SetData(0, text1);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("A3A56AB4-4AD1-4DA5-9F3B-FD5E61AF7AA2");
    }
}