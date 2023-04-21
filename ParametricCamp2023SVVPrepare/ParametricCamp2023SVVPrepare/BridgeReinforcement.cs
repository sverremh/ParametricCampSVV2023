using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Eto.Forms;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ParametricCamp2023SVVPrepare
{
    public class BridgeReinforcement : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the BridgeReinforcement class.
        /// </summary>
        public BridgeReinforcement()
          : base("BridgeReinforcement", "Nickname",
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
            pManager.AddBrepParameter("Top of bridge deck.", "brep", "Top of bridge deck to project the curve on",
                GH_ParamAccess.item);
            ; // 3
            pManager.AddNumberParameter("Rebar Offsets", "dx", "List of guide curve offsets in local x-direction",
                GH_ParamAccess.list, new List<double>(){0}); // 4
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Rebar brep", "brep", "Shape of rebar channel", GH_ParamAccess.list); // 0
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
            Brep bridgeDeck = null;
            List<double> curveOffsets= new List<double>(); // 4
            // retrieve the input from Grasshopper
            if (!DA.GetData(0, ref guideCurve) || guideCurve is null) return; // 0. If the component fail to retrieve a curve from the first input we return. 
            if (!DA.GetDataList(1, rebarXYs)) return; // 1. 
            DA.GetData(2, ref radius); // 2
            if (!DA.GetData(3, ref bridgeDeck)) return; // 3
            DA.GetDataList(4, curveOffsets); // 4

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

            // start by project the guide curve to the top of the bridge plate. 

            Curve[] projectedCurve = Curve.ProjectToBrep(guideCurve, bridgeDeck, Vector3d.ZAxis, 0.001);
            if (projectedCurve.GetLength(0) == 0)
            {
                // no curve is found, reverse Z axis
                projectedCurve = Curve.ProjectToBrep(guideCurve, bridgeDeck, - Vector3d.ZAxis, 0.001);
            }
            // Offset the points in local-x direction in the xy-plane. 

            // Project points back up to top deck

            // Offset down according to rebar cover list. 

            // Create surface. 

            List<string[]> decomposedCoords = rebarXYs.Select(coord => coord.Split(',')).ToList(); // split coords into strings and lines
            List<double> evalParams = decomposedCoords.Select(c => Double.Parse(c[0].ToString())).ToList();
            List<double> verticalRebarPositions = decomposedCoords.Select(c => Double.Parse(c[1].ToString())).ToList();

            List<Plane> rebarPlanes = BridgeUtility.CreateSectionPlanesAtParameters(projectedCurve[0], evalParams.ToList());

            List<Brep> rebarBreps = new List<Brep>();
            foreach (double dx in curveOffsets)
            {
                rebarBreps.Add(CreateRebarBrep(rebarPlanes, verticalRebarPositions, dx, radius));

            }


            // output data
            DA.SetDataList(0, rebarBreps);


        }

        Brep CreateRebarBrep(List<Plane> circlePlanes, List<double> dyList, double dx, double r)
        {
            
            List<Plane> movedPlanes = new List<Plane>();
            for (int i = 0; i < circlePlanes.Count; i++)
            {
                Plane p = circlePlanes[i]; // original Plane
                Plane movedPlane = new Plane(new Point3d(p.OriginX + dx, p.OriginY, p.OriginZ + dyList[i]), p.XAxis, p.YAxis);
                movedPlanes.Add(movedPlane);
            }
            List<NurbsCurve> rebarCircles = movedPlanes.Select(p => (new Circle(p, r)).ToNurbsCurve()).ToList(); // using Linq to create circles
            // loft and cap 
            Brep[] rebarBrep = Brep.CreateFromLoft(rebarCircles, Point3d.Unset, Point3d.Unset, LoftType.Tight, false);
            

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