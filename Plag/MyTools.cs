
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace Plag
{
    [Transaction(TransactionMode.Manual)]
    public class MyToolsCommand : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication UIAp = commandData.Application;
            Application App = UIAp.Application;
            UIDocument UIDoc = UIAp.ActiveUIDocument;
            Document Doc = UIDoc.Document;

            try
            {
                IList<Reference> reference = UIDoc.Selection.PickObjects(ObjectType.Element, AllFIlter.CreateFilter(EnumFilter.FamilyInstanceFilter),
           "Выберите элементы");

                Dictionary<string, int> familyInstance = new Dictionary<string, int>();

                foreach (var r in reference)
                {
                    Element elem = Doc.GetElement(r);
                    var category = elem.Category.Name;
                    if (familyInstance.TryGetValue(category, out int coint)) //тут я подсмотрел у других, за что мне очень стыдно
                    {
                        familyInstance[category] = coint + 1;
                    }
                    else
                    {
                        familyInstance.Add(category, 1);
                    }
                }
                string mess = "Количество элементов по категориям:\n"; //тут я подсмотрел у ии, за что мне не стыдно 
                foreach (var r in familyInstance)
                {
                    mess += $"{r.Key}: {r.Value}\n";
                }
                mess += $"Количество выделенных категорий: {familyInstance.Count}\n" +
                    $"Количество элементов: {familyInstance.Values.Sum()}";
                TaskDialog.Show("Оповещение о завершении работы", $"{mess}");
                return Result.Succeeded;
            }
            catch
            {
                TaskDialog.Show("Оповешение о завершении работы", "Операция прервана");
                return Result.Failed;
            }
        }
    }



    [Transaction(TransactionMode.Manual)]
    public class MyTools : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            application.CreateRibbonTab("Организация");
            var panel = application.CreateRibbonPanel("Организация", "Общее");
            var buttom = new PushButtonData("Button", "Кнопка",
                "C:\\ProgramData\\Autodesk\\Revit\\Addins\\2021\\Plag\\Plag.dll",
                "MyToolsCommand.Command");

            panel.AddItem(buttom);
            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}



