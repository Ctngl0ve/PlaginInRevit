
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Plag
{

    [Transaction(TransactionMode.Manual)]
    public class MyToolsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            UIApplication UIApp = commandData.Application;
            Application App = UIApp.Application;
            UIDocument UIDoc = UIApp.ActiveUIDocument;
            Document Doc = UIDoc.Document;


            using (Transaction transaction = new Transaction(Doc, "Вычисление расстояния между стенами"))
            {
                transaction.Start();

                try
                {
                    //тут мы стены выбираем, ссылчки получаем
                    IList<Reference> referenceElement = UIDoc.Selection.PickObjects(ObjectType.Element, new FilterElement(), "Выберите элементы: 2 стены");

                    //тут мы всю коллекцию в список составляем
                    List<Wall> walls = referenceElement
                        .Select(r => Doc.GetElement(r) as Wall)
                        .OfType<Wall>()
                        .ToList();

                    //тут проверем, что выбрано 2 стены
                    if (walls.Count != 2)
                    {
                        TaskDialog.Show("Оповещение о завершении работы", "Выберите 2 стены");
                        return Result.Failed;
                    }

                    //тут мы ищем прямые у наших стен (они же не по точкам стоят, а по кривым, тобишь линии)
                    var wall1 = GetWall(walls[0]);
                    var wall2 = GetWall(walls[1]);

                    //середины каждой стены
                    var pointLenghtWall1 = GetWallPoint(walls[0]);
                    var pointLenghtWall2 = GetWallPoint(walls[1]);

                    //вектор между точками
                    var vectorBetweenPointLenghtWall = pointLenghtWall2 - pointLenghtWall1;

                    //то самое скалярное произведение
                    double dotProduct = wall1.DotProduct(wall2);
                    double tolerance = 0.001;

                    //проверка параллельности
                    if (Math.Abs((Math.Abs(dotProduct)) - 1) < tolerance) //вообще как-то не понял прикола с "-1"
                    {
                        TaskDialog.Show("Оповещение о завершении работы", "Спасибо, стены паралелльны");
                    }
                    else { TaskDialog.Show("Оповещение о завершении работы", "Стены не паралелльны"); return Result.Failed; }


                    //тут мы грани стен выбираем, ссылочки получаем (можно вывести в отдельный метод или цикл) 
                    XYZ vec1;
                    vec1 = Vec(UIDoc, Doc);
                    XYZ vec2;
                    vec2 = Vec(UIDoc, Doc);

                    double x1 = Math.Abs(UnitUtils.ConvertFromInternalUnits((vectorBetweenPointLenghtWall.DotProduct(vec1)), UnitTypeId.Millimeters));
                    //double x2 = UnitUtils.ConvertFromInternalUnits((vectorBetweenPointLenghtWall.DotProduct(vec2)), UnitTypeId.Millimeters);
                    //может я что-то не так сделал, куда мне делать второе число?


                    TaskDialog.Show("Оповещение о завершении работы", $"{x1}");
                }
                catch
                {
                    TaskDialog.Show("Оповещение о завершении работы", "Операция прервана");
                }


                transaction.Commit();
            }
            return Result.Succeeded;
        }

        private XYZ Vec(UIDocument UIDoc, Document Doc)
        {
            Reference referenceFace = UIDoc.Selection.PickObject(ObjectType.Face, "Выберите грань стены");
            var elementFace = Doc.GetElement(referenceFace.ElementId);
            var face = elementFace.GetGeometryObjectFromReference(referenceFace) as Face;

            XYZ point1 = face.Evaluate(new UV(0, 0));
            XYZ point2 = face.Evaluate(new UV(1, 0));
            XYZ point3 = face.Evaluate(new UV(0, 2));

            XYZ vector1 = point2 - point1;
            XYZ vector2 = point3 - point1;

            return vector1.CrossProduct(vector2).Normalize();
        }
        private XYZ GetWall(Element wall)
        {
            var location = (wall.Location) as LocationCurve; //почему-то он не дает вызвать "Locаtion" и выдет ошибку в "location.Curve", поэтому ставим var
            if (location == null) return null;
            Curve curve = location.Curve;
            XYZ start = curve.GetEndPoint(0);
            XYZ end = curve.GetEndPoint(1);
            return (end - start).Normalize();
            //ну пока ничего нового не сделал, такой же как и в примере(
        }
        private XYZ GetWallPoint(Element wall)
        {
            var location = (wall.Location) as LocationCurve; //почему-то он не дает вызвать "Locаtion" и выдет ошибку в "location.Curve", поэтому ставим var
            if (location == null) return null;
            Curve curve = location.Curve;
            XYZ start = curve.GetEndPoint(0);
            XYZ end = curve.GetEndPoint(1);
            return (start + end) / 2;
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
