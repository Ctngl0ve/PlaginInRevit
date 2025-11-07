
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Linq;
using System.Reflection;

namespace Plag
{
    [Transaction(TransactionMode.Manual)]
    public class MyToolsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication UIAp = commandData.Application;
            Application Ap = UIAp.Application;
            UIDocument UID = UIAp.ActiveUIDocument;
            Document Doc = UID.Document;

            var walls = new FilteredElementCollector(Doc)
                .OfClass(typeof(Wall))
                .OfType<Wall>()
                .ToList();


            using (Transaction trans = new Transaction(Doc, "Plag"))
            {
                trans.Start();
                int coint = 0;

                double MaxLight = double.MinValue;
                double MinLignt = double.MaxValue;

                double Light = 0;

                Wall wallMax = null;
                Wall wallMin = null;

                Parameter prmtrCmmntrInDocWallMax = null;
                Parameter prmtrCmmntrInDocWallMin = null;


                foreach (Wall wall in walls)
                {
                    var prmtrLightInDoc = wall.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH);

                    var prmtrCommAllWall = wall.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);

                    if (prmtrLightInDoc == null) continue;


                    var prmtLightConvert = prmtrLightInDoc.AsDouble();

                    if (prmtLightConvert > MaxLight)
                    {
                        MaxLight = prmtLightConvert;
                        wallMax = wall;
                    }
                    if (prmtLightConvert < MinLignt)
                    {
                        MinLignt = prmtLightConvert;
                        wallMin = wall;
                    }
                    Light += prmtLightConvert;

                    prmtrCommAllWall.Set("");
                    coint++;
                }
                prmtrCmmntrInDocWallMax = wallMax.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);

                prmtrCmmntrInDocWallMin = wallMin.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);


                prmtrCmmntrInDocWallMax.Set("Самая длинная стена");
                prmtrCmmntrInDocWallMin.Set("Самая короткая стена");

                double MaxLightMm = UnitUtils.ConvertFromInternalUnits(MaxLight, UnitTypeId.Millimeters);
                double MinLigntMm = UnitUtils.ConvertFromInternalUnits(MinLignt, UnitTypeId.Millimeters);
                double LightMm = UnitUtils.ConvertFromInternalUnits(Light, UnitTypeId.Millimeters);
                double betweenLight = coint > 0 ? LightMm / coint : 0;

                trans.Commit();

                TaskDialog.Show("Оповещение о завершении работы",
                    $"Обработано количество стен: {coint} шт." +
                    $"\nСамая длинная стена: {MaxLightMm:F0} мм\n" +
                    $"Самая короткая стена: {MinLigntMm:F0} мм\n" +
                    $"Длина всех стен: {LightMm:F0} мм\n" +
                    $"Среднее значение всех длин: {betweenLight:F0} мм");
            }
            return Result.Succeeded;
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
                "Plag.MyToolsCommand");

            panel.AddItem(buttom);
            return Result.Succeeded;
        }
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
