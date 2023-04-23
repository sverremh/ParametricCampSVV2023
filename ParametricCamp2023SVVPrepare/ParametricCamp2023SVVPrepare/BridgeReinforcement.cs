using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Eto.Forms;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.ApplicationSettings;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace ParametricCamp2023SVVPrepare
{
    public class BridgeReinforcement : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the BridgeReinforcement class.
        /// </summary>
        public BridgeReinforcement()
          : base("3. BridgeReinforcement", "bridgeRebar",
              "Create reinforcement cables for bridge plate",
              "Parametric Camp SVV", "Bridge Components")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Guide Curve", "curve", "Centre line of bridge plate", GH_ParamAccess.item); // 0
            pManager.AddTextParameter("RebarPositions", "xy", "XY positions of rebar along guide curve",
                GH_ParamAccess.list); // 1
            pManager.AddNumberParameter("Rebar Radius", "radius", "Radius of rebar channel", GH_ParamAccess.item, 0.5); // 2 Here I use a default value, meaning that the user does not have to specify this for the component to run
            pManager.AddBrepParameter("Top of bridge deck.", "breps", "Top of bridge deck to project the curve on",
                GH_ParamAccess.list);
            ; // 3
            pManager.AddNumberParameter("Rebar Offsets", "dx", "List of guide curve offsets in local x-direction",
                GH_ParamAccess.item, 0.0); // 4
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Rebar brep", "brep", "Shape of rebar channel", GH_ParamAccess.item); // 0
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // -- input --

            // start by creating variables we can assign the input to
            Curve guideCurve = null; // 0 I can use "null" here because the Curve class is nullable, meaning that it has no value at all. https://learn.microsoft.com/en-us/dotnet/api/system.nullable?view=net-7.0
            List<string> rebarXYs = new List<string>(); // 1 Instantiate an empty list of rebar x-y coordinates.    
            double radius = 0; // 2 
            List<Brep> bridgeDeck = new List<Brep>();
            double curveOffset= 0.0 ; // 4
            // retrieve the input from Grasshopper
            if (!DA.GetData(0, ref guideCurve) || guideCurve is null) return; // 0. If the component fail to retrieve a curve from the first input we return. 
            if (!DA.GetDataList(1, rebarXYs)) return; // 1. 
            DA.GetData(2, ref radius); // 2
            if (!DA.GetDataList(3, bridgeDeck)) return; // 3
            DA.GetData(4, ref curveOffset); // 4

            // -- control input --
            // before I start creating the reinforcement, I want to validate the input. There can be several checks. For example, the radius cannot be 0 or negative, 
            // and the x-position (parameters along the guide curve) cannot be outside its domain. 
            if (radius <= 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error,
                    "The radius have to be greater than 0."); // If the radius is not greater than zero we stop the component and add a message. Try to input a negative value to test this}
            }
            // next we start find the maximum parameter values in the rebarXYs
            string minXString = rebarXYs[0].Split(',')[0]; // Take the first element in the csv list of "x,y" and split by "," 
            string maxXString = rebarXYs[rebarXYs.Count - 1].Split(',')[0]; // Note that we use ',' instead of ",". C# distinguis between these. The first indicates a char type, while the latter indicate string

            // convert to number
            double minX = Double.Parse(minXString);
            double maxX = Double.Parse(maxXString);
            if ((guideCurve.Domain[0] > minX) || (guideCurve.Domain[1] < maxX)) // here, I check if the  minX or (||) maxX is outside the curves domain.
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "The x-coordinates of the rebar cannot be outside the centre curves domain.");
            }

            // -- method --

            // Project the curve to xy-plane
            Curve xyCurve = Curve.ProjectToPlane(guideCurve, Plane.WorldXY); // Projected curve

            // Get the rebar offsets at each input parameter
            List<string[]> decomposedCoords = rebarXYs.Select(coord => coord.Split(',')).ToList(); // split coords into strings and lines
            List<double> evalParams = decomposedCoords.Select(c => Double.Parse(c[0].ToString())).ToList();
            List<double> verticalRebarPositions = decomposedCoords.Select(c => Double.Parse(c[1].ToString())).ToList();


            // Create planes at the input parameters
            List<Plane> rebarPlanes = BridgeUtility.CreateSectionPlanesAtParameters(xyCurve, evalParams.ToList());

            // start by project the guide curve to the top of the bridge plate. 

            // create the breps
            List<Brep> rebarBreps = new List<Brep>();
            // iterate through each curve offset in local x-dir

            Brep rebarBrep = CreateRebarBrep(rebarPlanes, verticalRebarPositions, curveOffset, radius, bridgeDeck.ToArray());
            
            // output data
            DA.SetData(0, rebarBrep);


        }

        Brep CreateRebarBrep(List<Plane> circlePlanes, List<double> dyList, double dx, double r, Brep[] intersectBreps)
        {

            List<Plane> movedPlanes = new List<Plane>();
            // Move the planes in local x-dir from the guide curve planes
            for (int i = 0; i < circlePlanes.Count; i++)
            {
                Plane p = circlePlanes[i]; // original Plane
                Plane movedPlane = new Plane(Point3d.Add(p.Origin, p.XAxis * dx), p.XAxis, p.YAxis); // offset the plane in local x-direction
                movedPlanes.Add(movedPlane);
            }
            // Create circles in all the planes
            // Find the intersection point between the new planes and the brep
            
            List<Curve> CirclesOnBrep = new List<Curve>();

            // find the intersection between the new plane points and the deck breps. 
            List<Point3d> planeOrigins = movedPlanes.Select(pl => pl.Origin).ToList();
            
            // the intInds gives the index from the original list where there has been found an intersection. We need this to get the corresponding planes
            Point3d[] intersectPts =  Intersection.ProjectPointsToBrepsEx(intersectBreps, planeOrigins, Vector3d.ZAxis, 0.001, out int[] intInds); // 
            


            for (int i = 0; i < intersectPts.Length; i++)
            {
                Point3d pt = Point3d.Add(intersectPts[i], Vector3d.ZAxis * dyList[intInds[i]]);
                Plane pl = movedPlanes[intInds[i]];
                //Point3d newO = pl.Origin + pt + Vector3d.Multiply(Vector3d.ZAxis, dyList[intInds[i]]);
                Curve circle = new Circle(new Plane(pt, pl.XAxis, pl.YAxis), r).ToNurbsCurve();
                CirclesOnBrep.Add(circle);
                
            }
            
            // loft and cap 
            Brep[] rebarBrep = Brep.CreateFromLoft(CirclesOnBrep, Point3d.Unset, Point3d.Unset, LoftType.Normal, false);
            

            return rebarBrep[0].CapPlanarHoles(0.001);
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
            get { return new Guid("0A545098-A625-420A-B563-E59241EAEFE6"); }
        }
    }
}