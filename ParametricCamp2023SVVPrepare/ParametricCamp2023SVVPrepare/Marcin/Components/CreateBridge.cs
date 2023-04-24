using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Grasshopper.Kernel;
using Rhino.Geometry;
using ParametricCamp2023SVV.Marcin.Classes;

namespace ParametricCamp2023SVV.Marcin
{
    public class CreateBridge : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreateBridge class.
        /// </summary>
        public CreateBridge()
          : base("CreateBridge", "Nickname",
              "Description",
              "Parametric Camp SVV", "Bridge")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("axis","a","axis of the bridge",GH_ParamAccess.item) ;
            pManager.AddBrepParameter("brepGround", "bG", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("bridge","b","our own bridge class",GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve a = null;
            Brep b = null;
            DA.GetData(0, ref a);
            DA.GetData(1, ref b);


            Bridge bridge = new Bridge();
            bridge.id = 0;
            bridge.name = "first bridge";
            bridge.axis = a;

            Point3d stPt = a.PointAtStart;
            Point3d enPt = a.PointAtEnd;

            Line vstL = new Line(stPt, new Vector3d(0, 0, -100000)); //create long vertical line
            Line venL = new Line(enPt, new Vector3d(0, 0, -100000)); //create long vertical line
            Curve[] ocrvs1; // empty variables
            Point3d[] opts1; // empty variables
            //find intersection betwwen line and surface
            Rhino.Geometry.Intersect.Intersection.CurveBrep(vstL.ToNurbsCurve(), b, 0.0001, out ocrvs1, out opts1) ;
            Point3d columnSt1 = opts1[0]; //result of intersecting line with ground surface
            Line axColumn1 = new Line(columnSt1, stPt); //axis of the column1

            Curve[] ocrvs2; // empty variables
            Point3d[] opts2; // empty variables
            //find intersection betwwen line and surface
            Rhino.Geometry.Intersect.Intersection.CurveBrep(venL.ToNurbsCurve(), b, 0.0001, out ocrvs2, out opts2);
            Point3d columnSt2 = opts2[0]; //result of intersecting line with ground surface
            Line axColumn2 = new Line(columnSt2, enPt); //axis of the column1

            Column c1 = new Column("first column", 0, axColumn1.ToNurbsCurve() , 1.4);
            Column c2 = new Column("second column", 1, axColumn2.ToNurbsCurve(), 1.4);

            List<Column> listColumns = new List<Column>() { c1 , c2 };

            bridge.columns = listColumns;

            DA.SetData(0, bridge);
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
            get { return new Guid("BDCDD220-A2C3-4C53-A81E-434FB2F00A7A"); }
        }
    }
}