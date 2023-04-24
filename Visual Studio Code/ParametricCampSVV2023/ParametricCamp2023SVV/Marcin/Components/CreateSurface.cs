using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ParametricCamp2023SVV.Marcin
{
    public class CreateSurface : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CreateSurface class.
        /// </summary>
        public CreateSurface()
          : base("CreateSurface", "Nickname",
              "Description",
              "Parametric Camp SVV", "Tools")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("sizeX", "sx", "size in X dir", GH_ParamAccess.item, 10); //0
            pManager.AddNumberParameter("sizeY", "sy", "size in Y dir", GH_ParamAccess.item, 20); //1
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("surface","srf","surface of x and y size",GH_ParamAccess.item); //0
            pManager.AddPointParameter("controlPoints", "cPts", "Points for surface", GH_ParamAccess.list); //1
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double sx = 0;
            double sy = 0;
            DA.GetData(0, ref sx);
            DA.GetData(1, ref sy);

            List<Point3d> pts = new List<Point3d>();
            int n = 10;
            int m = 12;
            var random = new Random(); //random class

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    double x = i*sx;
                    double y = j*sy;
                    double z = createNumber(random,5,17);
                    pts.Add(new Point3d(x, y, z));
                }
            }

            var ns = NurbsSurface.CreateFromPoints(pts, n, m, 2, 2).ToBrep();

            DA.SetData(0, ns);
            DA.SetDataList(1, pts);
        }
        double createNumber(Random random, double min, double max)
        {
            var rDouble = random.NextDouble(); //random number from 0.00 to 1.00
            var rRangeDouble = rDouble * (max - min) + min; //apply range
            return rRangeDouble;
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
            get { return new Guid("351442A9-633F-4AA7-9006-15594C89BECA"); }
        }
    }
}