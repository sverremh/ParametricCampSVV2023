using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace BridgeGenerator
{
    public class BridgeGeneratorComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public BridgeGeneratorComponent()
          : base("BridgePlateFromCurves", "plateFromCurves",
            "Create bridge plate from guide curves",
            "BridgeGenerator", "Geometry")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Center", "ctr", "", GH_ParamAccess.item); // 0
            pManager.AddCurveParameter("Left", "lft", "", GH_ParamAccess.item); //1
            pManager.AddCurveParameter("Right", "rght", "", GH_ParamAccess.item); // 2

            pManager.AddIntegerParameter("Division", "div", "", GH_ParamAccess.item, 50); // 3

            pManager.AddCurveParameter("SectionCurves", "sections", "", GH_ParamAccess.list); // 4
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // Bridge plate as brep
            pManager.AddBrepParameter("BridgePlate", "brep", "", GH_ParamAccess.item); // 0
            pManager.AddGenericParameter("DebugOut", "", "", GH_ParamAccess.list); // 1 Output to test while developing
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // -- input --

            Curve centreCurve = null; // 0 centre curve
            Curve leftCurve = null; // 1 left curve
            Curve rightCurve = null; // 2 right

            int curveDivision = 0; // 3 number of curve divisions
            List<Curve> sectionCurves = new List<Curve>(); // 4


            if (!DA.GetData(0, ref centreCurve))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Make sure to input a curve");
            }
            if (!DA.GetData(1, ref leftCurve))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Make sure to input a curve");
            }
            DA.GetData(2, ref rightCurve); // 2
            DA.GetData(3, ref curveDivision); // 3
            if (curveDivision < 2)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Division should be minimum 2");
            }
            DA.GetDataList(4, sectionCurves); // 4 Section curves for bridge plate

            // -- method ---

            // 1: Get Section on centre line
            List<Plane> centrePlanes = GetSectionPlanesFromCurves(centreCurve, curveDivision);

            // 2: Find intersecting planes on left and right curve


            // 3: Orient section curves to planes on guide curves

            // 4: Join section curves at each plane/section

            // 5: Loft and cap sections 


            // -- output -- 
            DA.SetDataList(1, centrePlanes);


        }
        List<Plane> GetSectionPlanesFromCurves(Curve crv, int div)
        {
            // divide curve 
            Point3d[] divPts; // division points on curve
            double[] parameterList = crv.DivideByCount(div, true, out divPts);


            List<Plane> sectionPlanes = new List<Plane>();

            for (int i = 0; i < divPts.Length; i++)
            {
                // get tangents
                Vector3d tangent = crv.TangentAt(parameterList[i]);

                Point3d localOrigin = divPts[i];
                Vector3d localX = Vector3d.CrossProduct(tangent, Vector3d.ZAxis);
                Vector3d localY = Vector3d.ZAxis;
                // create new plane
                Plane localSectionPlane = new Plane(localOrigin, localX, localY);

                sectionPlanes.Add(localSectionPlane); // add to list
            }        
                        
                       
            return sectionPlanes;
        }

        

        /// <summary>
        /// The Exposure property controls where in the panel a component icon 
        /// will appear. There are seven possible locations (primary to septenary), 
        /// each of which can be combined with the GH_Exposure.obscure flag, which 
        /// ensures the component will only be visible on panel dropdowns.
        /// </summary>
        public override GH_Exposure Exposure => GH_Exposure.primary;

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
        public override Guid ComponentGuid => new Guid("9e2b5cee-d58a-4d16-8900-be1ad49af2c2");
    }
}