using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace BridgeGenerator
{
    public class BridgeGeneratorInfo : GH_AssemblyInfo
    {
        public override string Name => "BridgeGenerator";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("4cff7a75-8ab1-4152-b041-3457b2554f6f");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}