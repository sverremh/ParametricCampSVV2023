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
              "Parametric Camp SVV", "Bridge Components")
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

            // -- 1: Divide the guide curve and create planes along the length
            
            /*
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

            */
            List<Plane> guidePlanes = BridgeUtility.CreateSectionPlanesAlongCurve(centerCurve, div);
            // -- 2: Extend the curves with x percent of original length
            double curveExtension = 0.05;
            leftCurve = leftCurve.Extend(CurveEnd.Both, leftCurve.GetLength() * curveExtension, CurveExtensionStyle.Smooth);
            rightCurve = rightCurve.Extend(CurveEnd.Both, rightCurve.GetLength() * curveExtension, CurveExtensionStyle.Smooth);

            // -- 3: Intersect left and right curves with middle planes
            List<Plane> leftPlanes = IntersectionPlanes(leftCurve, guidePlanes); // base planes for left cross section curves
            List<Plane> rightPlanes = IntersectionPlanes(rightCurve, guidePlanes); // base planes for right cross section curves

            // -- 4 move the section curves along the guides.
            List<Curve> leftSections = ReorientedSectionCurves(sectionPlane, leftPlanes, sections[2]); // the third item in the list should be the second curve
            List<Curve> rightSections = ReorientedSectionCurves(sectionPlane, rightPlanes, sections[3]); // the fourth item is right curve
            List<Curve> middleTopSections = ReorientedSectionCurves(sectionPlane, guidePlanes.ToList(), sections[0]); // the first item is top middle curve
            List<Curve> middleBottomSections = ReorientedSectionCurves(sectionPlane, guidePlanes.ToList(), sections[1]); // the second item is bottom middle curve

            // -- 5 Connect the curves at each section. 

            // create a nested list of the curves sorted clockwise
            List<List<Curve>> nestedSectionCurves = new List<List<Curve>>() {leftSections, middleTopSections, rightSections, middleBottomSections };
            var organisedSections = FlipNested2DList(nestedSectionCurves);
            // For concvenience, we flip the nested list to have all four curves on one plane in the same subList
            var planeSectionCurves = CreatePlaneSections(organisedSections);

            // -- 6 Loft an Cap the curves
            Brep loftedBrep = Brep.CreateFromLoft(planeSectionCurves, Point3d.Unset,
                    Point3d.Unset, LoftType.Normal, false)[0].CapPlanarHoles(0.001);



            // -- output --
        }


        public Curve[,] FlipNested2DList(List<List<Curve>> nestedList)
        {
            // before we start, we want to ensure that each list has the same lenght
            List<int> subLengths = nestedList.Select(lst => lst.Count).ToList();// List of lengths for each sublist
            // in the below line we ensure that all sublist have the same length and return true if that is the case
            bool testLength = (subLengths.Where(el => el == nestedList[0].Count).Count() == nestedList.Count) ? true : false;
            if (!testLength)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Make sure the edge curves are longer then the guide curve.");
            }

            int rows = nestedList.Count;
            int cols = nestedList[0].Count;
            Curve[,] result = new Curve[cols, rows]; // instantiate an empty array
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    result[j, i] = nestedList[i][j];
                }
            }
            return result;

        }
        public Plane[] PlanesAlongCurve(Curve crv, int div)
        {
            //List<Plane> result = new List<Plane>();
            var tList = crv.DivideByCount(div, true); // list of parameters

            // Get perp frames along the length
            var result = crv.GetPerpendicularFrames(tList);
            return result;
        }

        public List<Plane> IntersectionPlanes(Curve crv, List<Plane> pList)
        {
            List<Plane> interPlanes = new List<Plane>();
            
            foreach (var p in pList)
            {
                CurveIntersections intersection = Intersection.CurvePlane(crv, p, 0.001);
                if (intersection[0].IsPoint)
                {
                    Point3d pt = intersection[0].PointA;
                    //Plane movedPlane = new Plane(pt, p.Normal);
                    Plane movedPlane = new Plane(pt, p.XAxis, p.YAxis);
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

                Plane toPlane = newPlanes[i]; // the plane we want to move to. 

                var trans = Transform.PlaneToPlane(refPlane, toPlane); // create the transformation

                orientedCurves.Add(sectionCurve.DuplicateCurve()); // duplicate the section curve
                orientedCurves[i].Transform(trans); // reorient the copied curve
            } 

            return orientedCurves;

        }

        public List<Curve> CreatePlaneSections(Curve[,] nestedSections)
        {

            List<Curve> planeSections = new List<Curve>();
            int rows = nestedSections.GetLength(0);
            int cols = nestedSections.GetLength(1);

            for (int i = 0; i < rows; i++) // iterate through each plane
            {
                List<Curve> crvs = new List<Curve>(); // initiate new list of control points for polycurve
                for (int j = 0; j < cols; j++)
                {
                    crvs.Add(nestedSections[i, j]); // add the current curve
                    List<Point3d> endPts;
                    if (j != (cols -1))
                    {                   
                        endPts = new List<Point3d>() { nestedSections[i, j].PointAtEnd,
                            nestedSections[i, j + 1].PointAtStart };
                    }
                    else // at the final iteration we add the first point instead
                    {
                        endPts = new List<Point3d>() { nestedSections[i, j].PointAtEnd,
                            nestedSections[i, 0].PointAtStart };
                        
                    }
                    Curve connectingCurve = Curve.CreateControlPointCurve(endPts, 1);
                    crvs.Add(connectingCurve);
                }

                // join all the curves
                Curve planeSection = Curve.JoinCurves(crvs)[0];
                planeSections.Add(planeSection);
            }

            return planeSections;
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