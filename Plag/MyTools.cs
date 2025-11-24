
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;
using System.Linq;

namespace Plag
{
    //Проверка работы 
    [Transaction(TransactionMode.Manual)]
    public class MyToolsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            UIApplication UIApp = commandData.Application;
            Application App = UIApp.Application;
            UIDocument UIDoc = UIApp.ActiveUIDocument;
            Document Doc = UIDoc.Document;

            int solidesCount = 0;
            double solidesVolue = 0.0;
            double solidesArea = 0.0;
            int facesCount = 0;
            int edgeCount = 0;
            double edgeLenth = 0.0;

            try
            {
                using (Transaction transaction = new Transaction(Doc, "Расчет количетсва граней и ребер"))
                {
                    transaction.Start();
                    var select = Doc.GetElement(UIDoc.Selection.PickObject(ObjectType.Element, new SystemFamily(), "Выберите сиcтемное семейство"));

                    List<Solid> solids = Solids(select);

                    foreach (var solid in solids)
                    {
                        foreach (Face face in solid.Faces)
                        {
                            facesCount++;

                            solidesArea += face.Area;
                        }
                        solidesCount++;
                        solidesVolue += solid.Volume;
                    }

                    List<Edge> edges = Edges(select);

                    foreach (var edge in solids)
                    {
                        foreach (Edge edge1 in edge.Edges)
                        {
                            edgeCount++;
                            Curve curve = edge1.AsCurve();
                            edgeLenth += curve.Length;
                        }

                    }

                    //
                    // Вот тут блок где переводим из ревитовских настроек размеров, в человеческие 
                    solidesVolue = UnitUtils.ConvertFromInternalUnits(solidesVolue, UnitTypeId.CubicMeters);
                    // и тд 

                    TaskDialog.Show("Оповещение о завершении работы", $"Количество solid: {solidesCount}\n" +
               $"Valume: {solidesVolue}\n" +
               $"Area: {solidesArea}\n" +
               $"Количество face: {facesCount} \n" +
               $"Количество edge: {edgeCount} \n" +
               $"Lenght: {edgeLenth} \n");


                    transaction.Commit();
                }

            }
            catch
            {
                TaskDialog.Show("Оповещение о завершении работы", "Выбор отменен");
            }
            return Result.Succeeded;
        }

        private List<Solid> Solids(Element select)
        {
            Options options = new Options();
            var solids = select.get_Geometry(options)
                  .Where(g => g is Solid)
                  .OfType<Solid>()
                  .Where(g => g.Volume > 0.001)
                  .ToList();
            return solids;
        }

        private List<Edge> Edges(Element select)
        {
            Options options = new Options();
            var edges = select.get_Geometry(options)
                  .Where(g => g is Edge)
                  .OfType<Edge>()
                  .ToList();
            return edges;
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
