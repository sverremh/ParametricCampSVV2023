using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace ParametricCamp2023SVV
{
    public class ParametricCamp2023SVVInfo : GH_AssemblyInfo
    {
        public override string Name => "ParametricCamp2023SVV";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("fd8ad984-e8b7-449f-b0cc-2c9cd46b3736");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}