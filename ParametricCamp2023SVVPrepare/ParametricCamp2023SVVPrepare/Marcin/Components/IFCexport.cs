using System;
using System.Collections.Generic;
using GeometryGym.Ifc;

using Grasshopper.Kernel;
using Rhino.Geometry;
using ParametricCamp2023SVV.Marcin.Classes;
namespace ParametricCamp2023SVV.Marcin
{
    public class IFCexport : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the IFCexport class.
        /// </summary>
        public IFCexport()
          : base("IFCexport", "Nickname",
              "Description",
              "Parametric Camp SVV", "IFC")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("run","r","produce IFC file",GH_ParamAccess.item);
            pManager.AddGenericParameter("bridge", "bridge","bridge",GH_ParamAccess.item) ;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool r = false;
            Bridge b = new Bridge();
            DA.GetData(0, ref r);
            DA.GetData(1, ref b);

            if (r)
            {
                //the place for creating IFC code

                string path = "C:\\Users\\marcinl\\Desktop\\SV_course_17042023\\myIfcFile.ifc";

                DatabaseIfc db = new DatabaseIfc(ModelView.Ifc4DesignTransfer);
                IfcBuilding building = new IfcBuilding(db, "IfcBuilding-SVcourse") { };
                IfcProject project = new IfcProject(building, "IfcProject_SVcourse", IfcUnitAssignment.Length.Metre) { };

                List<IfcExtrudedAreaSolid> bridgeSolids = new List<IfcExtrudedAreaSolid>();


                //save the IFC file
                db.WriteFile(path);





                //end of IFC code    
            }

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
            get { return new Guid("0EBBB3C3-D639-4EBF-BED9-33E1523768B8"); }
        }
    }
}