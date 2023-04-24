using System;
using System.Collections.Generic;
using GeometryGym.Ifc;
using GeometryGym;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Xml.Linq;
using ParametricCamp2023SVV.Marcin.Classes;

namespace ParametricCamp2023SVV.Marcin
{
    public class CreateIFC : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreateIFC class.
        /// </summary>
        public CreateIFC()
          : base("CreateIFC", "Nickname",
              "Description",
              "Parametric Camp SVV", "IFC")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("r", "r", "r", GH_ParamAccess.item);
            pManager.AddGenericParameter("bridge","br","bridge class",GH_ParamAccess.item);
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
            DA.GetData(0,ref r);
            DA.GetData(1, ref b);

            if (r)
            { 
   

            string path = "C:\\Users\\marcinl\\Desktop\\SV_course_17042023\\myIfcFile.ifc";

                DatabaseIfc db = new DatabaseIfc(ModelView.Ifc4DesignTransfer);
                IfcBuilding building = new IfcBuilding(db, "IfcBuilding") { };
                IfcProject project = new IfcProject(building, "IfcProject", IfcUnitAssignment.Length.Metre) { };

                List<IfcExtrudedAreaSolid> towerIfcsolids = new List<IfcExtrudedAreaSolid>();

                foreach (Column c in b.columns)
                {
                    IfcProfileDef def = new IfcProfileDef(db, "column");
                    Curve crv = c.axis;
                    Line lc = new Line(c.axis.PointAtStart, c.axis.PointAtEnd);
                    IfcExtrudedAreaSolid solid = new IfcExtrudedAreaSolid(
                        def, 
                        new IfcAxis2Placement3D(
                        new IfcCartesianPoint(db, c.axis.PointAtStart.X, c.axis.PointAtStart.Y, c.axis.PointAtStart.Z ), 
                        new IfcDirection(db, lc.Direction.X, lc.Direction.Y, lc.Direction.Z), 
                        new IfcDirection(db, 0, 0, 1)), 
                        c.axis.GetLength()
                        );
                    towerIfcsolids.Add(solid);
                }


                //element.AddElement();
                
                
                db.WriteFile(path);
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
            get { return new Guid("638296AB-A82A-4B13-BF35-7F15AA3D8491"); }
        }
    }
}