using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

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
            pManager.AddPlaneParameter("Plane", "p", "Reference plane for section curves. World XY by default", GH_ParamAccess.item, Plane.WorldXY); 
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
            Plane sectionPlane = new Plane();

            if (!DA.GetData(0, ref centerCurve)) return;
            if (!DA.GetData(1, ref leftCurve)) return;
            if (!DA.GetData(2, ref rightCurve)) return;
            if (!DA.GetData(3, ref div)) return;
            if (!DA.GetDataList(4, sections)) return;
            if (!DA.GetData(5, ref sectionPlane)) return;


            // -- method --

            // 1: Divide the guide curve and create planes along the length
            Plane[] guidePlanes = PlanesAlongCurve(centerCurve, div); // get the frames along the curve. 
            
            // create xy tangents
            Vector3d[] xyTangents = guidePlanes.Select(p => (new Vector3d(p.Normal.X, p.Normal.Y, 0))).ToArray(); // Remove the global Z part of the tangents
            foreach (Vector3d tangent in xyTangents)
            {
                _ = tangent.Unitize(); // Unitize all the vectors.
            }


            // use the xy-tangent as plane normals
            guidePlanes = guidePlanes.Zip(xyTangents, (plane, vector) => new Plane(plane.Origin, vector)).ToArray(); //

            Vector3d[] crossProduct = guidePlanes.Select(p => Vector3d.CrossProduct(p.Normal, Vector3d.ZAxis)).ToArray(); // get the cross product between global z- and tangent vectors

            Plane[] testPlanes = guidePlanes.Zip(crossProduct, (plane, x) => new Plane(plane.Origin, x, Vector3d.ZAxis))
                .ToArray();

            // 2: Extend the curves with x percent of original length
            double curveExtension = 0.05;
            leftCurve = leftCurve.Extend(CurveEnd.Both, leftCurve.GetLength() * curveExtension, CurveExtensionStyle.Smooth);
            rightCurve = rightCurve.Extend(CurveEnd.Both, rightCurve.GetLength() * curveExtension, CurveExtensionStyle.Smooth);

            // 3: Intersect left and right curves with middle planes
            List<Plane> leftPlanes = IntersectionPlanes(leftCurve, guidePlanes); // base planes for left cross section curves
            List<Plane> rightPlanes = IntersectionPlanes(rightCurve, guidePlanes); // base planes for right cross section curves

            // 4 move the section curves along the guides.
            List<Curve> leftSections = ReorientedSectionCurves(sectionPlane, leftPlanes, sections[2]); // the third item in the list should be the second curve
            List<Curve> rightSections = ReorientedSectionCurves(sectionPlane, rightPlanes, sections[3]); // the fourth item is right curve
            List<Curve> middleTopSections = ReorientedSectionCurves(sectionPlane, guidePlanes.ToList(), sections[0]); // the first item is top middle curve
            List<Curve> middleBottomSections = ReorientedSectionCurves(sectionPlane, guidePlanes.ToList(), sections[1]); // the second item is bottom middle curve

            // 5 Connect the curves at each section. 

            // 6 Loft an Cap the curves




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

        public List<Plane> IntersectionPlanes(Curve crv, Plane[] pArray)
        {
            List<Plane> interPlanes = new List<Plane>();
            
            foreach (var p in pArray)
            {
                CurveIntersections intersection = Intersection.CurvePlane(crv, p, 0.001);
                if (intersection[0].IsPoint)
                {
                    Point3d pt = intersection[0].PointA;
                    Plane movedPlane = new Plane(pt, p.Normal);
                    interPlanes.Add(movedPlane);
                }
            }
            

            return interPlanes;
        }

        public List<Curve> ReorientedSectionCurves(Plane refPlane, List<Plane> newPlanes, Curve sectionCurve)
        {
            List<Curve> orientedCurves = new List<Curve>(); // initiate empty list for the oriented curves
            // https://discourse.mcneel.com/t/orient-function-in-rhinocommon/48914/9
            for (int i = 0; i < newPlanes.Count; i++)
            {
                
                // Choose Target points. 
                // I use two points along the X-axis 
                // since I wanted my shape to align with the X-axis
                Plane toPlane = newPlanes[i];

                Transform xFormMove = Transform.Translation(toPlane.Origin - refPlane.Origin); // Moving from origin to target on guide curve
                Transform xFormScale = Transform.Identity; // we do not want to scale the geometry
                
                var v0 = refPlane.Normal;
                var v1 = toPlane.Normal;
                Transform xFormRotate = Transform.Rotation(v0, v1, sectionCurve.PointAtStart);
                Transform xFormFinal = xFormMove * xFormScale * xFormRotate;
                
                Transform.ChangeBasis(refPlane.XAxis, refPlane.YAxis, refPlane.ZAxis, newPlanes[i].XAxis,
                    newPlanes[i].YAxis, newPlanes[i].ZAxis);
                //var trans = Transform.PlaneToPlane(refPlane, newPlanes[i]);
                
                orientedCurves.Add(sectionCurve.DuplicateCurve());
                orientedCurves[i].Transform(xFormFinal);
            } 

            return orientedCurves;

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