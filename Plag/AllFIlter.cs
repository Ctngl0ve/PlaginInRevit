using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plag
{
    internal class AllFIlter
    {
        public static ISelectionFilter CreateFilter(EnumFilter filter)
        {
            switch (filter)
            {
                case EnumFilter.FamilyInstanceFilter: return new FamilyInstanceFilter();
                case EnumFilter.Window: return new WindowFilter();
                case EnumFilter.Door: return new DoorFilter();
                default:
                    return null;
            }

        }
    }
}
