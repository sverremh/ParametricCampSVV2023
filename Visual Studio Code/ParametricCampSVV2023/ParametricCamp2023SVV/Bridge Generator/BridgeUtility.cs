using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace ParametricCamp2023SVV
{
    /// <summary>
    /// Static class to store methods used for several components.
    /// </summary>
    internal static class BridgeUtility
    {

        internal static List<Plane> CreateSectionPlanesAlongCurve(Curve crv, int division)
        {
            Point3d[] divisionPts;
            double[] curve_parameters  = crv.DivideByCount(division - 1, true, out divisionPts); // subtrect froim 

            List<Plane> sectionPlanes = new List<Plane>(); // initiate empty list
            Vector3d globalZ = Vector3d.ZAxis; // I'm declaring this variable outside the loop to avoid performing an identical operation n-times
            for (int i = 0; i < division; i++)
            {
                // get the tangent
                Vector3d tangent = crv.TangentAt(curve_parameters[0]); // get the tangent vector at index i
                Vector3d localX = Vector3d.CrossProduct(tangent, globalZ);
                // create new section plane and add it to the list
                Plane sectionPlane = new Plane(divisionPts[i], localX, globalZ);
                sectionPlanes.Add(sectionPlane);
            }

            return sectionPlanes;
        }

        internal static List<Plane> CreateSectionPlanesAtParameters(Curve crv, List<double> evalParams)
        {
            List<Point3d> evalPts = evalParams.Select(t => crv.PointAt(t)).ToList(); // Here I use Linq to do a "foreach" on a single line.
            List<Vector3d> tangents = evalParams.Select(t => crv.TangentAt(t)).ToList();
            List<Plane> sectionPlanes = evalPts.Zip(tangents,
                (o, t) => new Plane(o, Vector3d.CrossProduct(t, Vector3d.ZAxis), Vector3d.ZAxis)).ToList();
            return sectionPlanes;
        }
    }
}
