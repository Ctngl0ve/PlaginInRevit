
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Plag
{
    //Проверка работы 
    //[Transaction(TransactionMode.Manual)]
    //public class MyToolsCommand : IExternalCommand
    //{
    //    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    //    {
    //        TaskDialog.Show("Начало работы", "Здравствуйте");
    //        return Result.Succeeded;
    //    }
    }


    [Transaction(TransactionMode.Manual)]
    public class MyTools : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            application.CreateRibbonTab("Организация");
            var panel = application.CreateRibbonPanel("Организация", "Общее");
            var buttom = new PushButtonData("Button", "Кнопка",
                "D:\\Progect_VS\\Software Development Kit\\Samples\\DeleteDimensions\\CS\\obj\\Debug\\DeleteDimesions.dll",
                "DeleteDimesions.Command");

            panel.AddItem(buttom);
            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
