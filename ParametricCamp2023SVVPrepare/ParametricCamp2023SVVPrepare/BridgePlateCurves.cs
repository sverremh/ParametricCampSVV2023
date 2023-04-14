using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ParametricCamp2023SVVPrepare
{
    public class BridgePlateCurves : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the BridgePlateCurves class.
        /// </summary>
        public BridgePlateCurves()
          : base("2. BridgePlateCurves", "plate",
              "Create bridge plate from Curves",
              "VVS", "BridgePlate")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Center curve", "cCurve", "Middle curve of the road line", GH_ParamAccess.item) ;
            pManager.AddCurveParameter("Left curve", "lCurve", "Left curve of the bridge deck", GH_ParamAccess.item);
            pManager.AddCurveParameter("Right curve", "rCurve", "Right curve of the bridge deck", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Count", "count", "Number of divisions", GH_ParamAccess.item);
            pManager.AddCurveParameter("SectionCurves", "secCrvs", "Curves for the plate section", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Bridge plate", "plate", "Bridge plate", GH_ParamAccess.item); 
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // -- input --
            Curve centerCurve = null;
            Curve leftCurve = null;
            Curve rightCurve = null;
            int div = 0;
            List<Curve> sections= new List<Curve>();

            if (!DA.GetData(0, ref centerCurve)) return;
            if (!DA.GetData(1, ref leftCurve)) return;
            if (!DA.GetData(2, ref rightCurve)) return;
            if (!DA.GetData(3, ref div)) return;
            if (!DA.GetDataList(4, sections)) return;


            // -- method --

            // 1: Divide the guide curve and create planes along the length
            Plane[] guidePlanes = PlanesAlongCurve(centerCurve, div); // get the frames along the curve. 
            
            // create xy tangents
            Vector3d[] xyTangents = guidePlanes.Select(p => (new Vector3d(p.Normal.X, p.Normal.Y, 0))).ToArray(); // Remove the global Z part of the tangents
            foreach (Vector3d tangent in xyTangents)
            {
                _ = tangent.Unitize(); // Unitize all the vectors.
            }
            Vector3d[] crossProduct = guidePlanes.Select(p => Vector3d.CrossProduct(p.Normal, Vector3d.ZAxis)).ToArray(); // get the cross product between global z- and tangent vectors


            // 2: Extend the curves



            // -- output --
        }

        public Plane[] PlanesAlongCurve(Curve crv, int div)
        {
            //List<Plane> result = new List<Plane>();
            var tList = crv.DivideByCount(div, true); // list of parameters

            // Get perp frames along the length
            var result = crv.GetPerpendicularFrames(tList);
            return result;
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
            get { return new Guid("3085D37F-C2ED-4AD4-AA44-D0222C44DE3D"); }
        }
    }
}