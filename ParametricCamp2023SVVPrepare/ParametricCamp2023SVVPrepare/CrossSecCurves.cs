using System;
using System.Collections.Generic;
using System.Net;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;

namespace ParametricCamp2023SVVPrepare
{
    public class CrossSecCurves : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CrossSecCurves class.
        /// </summary>
        public CrossSecCurves()
          : base("1. CrossSecCurves", "cross_sec_curves",
              "Generate base curves for cross sections",
              "Parametric Camp SVV", "Bridge Components")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddVectorParameter("Centre Top Vecs", "cVec", "List of vectors for centre top points", GH_ParamAccess.list); // 0
            pManager.AddVectorParameter("Centre Bottom Vecs", "cVec", "List of vectors for centre bottom points", GH_ParamAccess.list); // 0
            pManager.AddVectorParameter("Left Vec", "lVec", "", GH_ParamAccess.list); // 1
            pManager.AddVectorParameter("Right Vec", "rVec", "", GH_ParamAccess.list); // 2
            pManager.AddPlaneParameter("Preview Plane", "preview plane", "Optional preview plane. XY-plane by default", GH_ParamAccess.item, Plane.WorldXY); // 3 Note the additional input in the method
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("CrossSecCurves", "curves", "List of base curves for bridge plate section", GH_ParamAccess.list); // 0
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // -- input --
            List<Vector3d> centreTopVecs = new List<Vector3d>(); // 0
            List<Vector3d> centreBottomVecs = new List<Vector3d>(); // 1
            List<Vector3d> leftVecs = new List<Vector3d>(); // 2
            List<Vector3d> rightVecs = new List<Vector3d>(); // 3
            Plane basePlane = new Plane(); // 4

            if (!DA.GetDataList(0, centreTopVecs)) // 0
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No center Vecs");// Adding a message in Grasshopper if something is wrong.
                return;
            }            
            if (!DA.GetDataList(1, centreBottomVecs)) return; // 1
            if (!DA.GetDataList(2, leftVecs)) return; // 2
            if (!DA.GetDataList(3, rightVecs)) return; // 3
            DA.GetData(4, ref basePlane); // 4


            // -- method --            

            // before we start, we consider the option to only input one translation vector for the middle top. In that case, we just add another point to the list
            if (centreTopVecs.Count < 2)
            {
                centreTopVecs.Add(centreTopVecs[0]); // the first point again
            }


            // find the maximum translation distance for the curves
            double dxTop = centreTopVecs.Select(v => v.X).Max() - centreTopVecs.Select(v => v.X).Min(); // find the maximum for top curves
            double dxBottom = centreBottomVecs.Select(v => v.X).Max() - centreBottomVecs.Select(v => v.X).Min(); // find the maximum distance for bottom curves
            double dxMid = (dxTop > dxBottom) ? dxTop : dxBottom; // find the largest x-val between the two

            double dxLeft = Math.Abs(leftVecs.Select(v => v.X).Max() - leftVecs.Select(v => v.X).Min()); 
            double dxRight = Math.Abs(rightVecs.Select(v => v.X).Max() - rightVecs.Select(v => v.X).Min());

            // move the left and right points away from the mid points if we want to preview the results. 
            //leftVecs = leftVecs.Select(v => v - new Vector3d(Math.Abs(dxMid)/2 + dxLeft*1.5,0,0)) .ToList() ;
            //rightVecs = rightVecs.Select(v => v + new Vector3d(Math.Abs(dxMid)/2 + dxRight*1.5, 0, 0)).ToList();


           
            // translate all the points to generate the base curves
            List<Point3d> centreTopPts = TranslatedPoints(basePlane, centreTopVecs); // get translated centre points
            List<Point3d> centreBottomPts = TranslatedPoints(basePlane, centreBottomVecs); // get translated centre points
            List<Point3d> leftPts = TranslatedPoints(basePlane, leftVecs); // get translated centre points
            List<Point3d> rightPts = TranslatedPoints(basePlane, rightVecs); // get translated centre points


            // create curves between points
            
            Polyline centreTopCrv = new Polyline(centreTopPts);
            Polyline centreBottomCrv = new Polyline(centreBottomPts);
            Polyline leftCrv = new Polyline(leftPts);
            Polyline rightCrv = new Polyline(rightPts);
            

            // collect all curves in list
            List<Polyline> lines = new List<Polyline>() {centreTopCrv, centreBottomCrv, leftCrv, rightCrv };
            

            // -- output
            DA.SetDataList(0, lines); // 0 the list of base curves
        }

        // Additional Code
        public List<Point3d> TranslatedPoints(Plane basePlane, List<Vector3d> tVecs)
        {
            List<Point3d> newPts = new List<Point3d>();

            foreach (Vector3d v in tVecs)
            {
                var planeX = basePlane.XAxis * v.X;
                var planeY = basePlane.YAxis * v.Y;
                var planeZ = basePlane.ZAxis * v.Z;
                Point3d point = Point3d.Add(basePlane.Origin, (planeX + planeY + planeZ)); // Translate the point in local coordinate system
                newPts.Add(point);
            }

            return newPts; 
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
            get { return new Guid("A1048221-E989-4CD4-A956-EAA7A15CEA46"); }
        }
    }
}