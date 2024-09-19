using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace CollabAPIMEP
{
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class VertexCountCommand : IExternalCommand
    {



        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIApplication uiApp = commandData.Application;
                Document doc = commandData.Application.ActiveUIDocument.Document;


                // Count the vertices in the family geometry
                int edgeCount = CountEdges(doc);

                int detailLineCount = CountDetailLines(doc);

                // Show the vertex count in a message box
                MessageBox.Show($"The number of vertices in the family geometry is: {edgeCount}", "Vertex Count");
                MessageBox.Show($"The number of detail lines: {detailLineCount}", "Detail Lines Count");

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                // Log and show the exception
                string errorMessage = ex.GetType().Name + " " + ex.StackTrace;
                MessageBox.Show(errorMessage);
                return Result.Failed;
            }
        }

        private int CountEdges(Document familyDocument)
        {
            int edgeCount = 0;

            // Collect all elements in the family document
            FilteredElementCollector collector = new FilteredElementCollector(familyDocument).WhereElementIsNotElementType();

            foreach (Element element in collector)
            {
                // Get the geometry of the element
                Options options = new Options();
                GeometryElement geometryElement = element.get_Geometry(options);

                if (geometryElement != null)
                {
                    foreach (GeometryObject geometryObject in geometryElement)
                    {
                        edgeCount += CountEdgesInGeometryObject(geometryObject);
                    }
                }
            }

            return edgeCount;
        }

        private int CountEdgesInGeometryObject(GeometryObject geometryObject)
        {
            int edgeCount = 0;

            if (geometryObject is GeometryInstance instance)
            {
                GeometryElement instanceGeometry = instance.GetInstanceGeometry();
                foreach (GeometryObject obj in instanceGeometry)
                {
                    edgeCount += CountEdgesInGeometryObject(obj);
                }
            }
            else if (geometryObject is Solid solid)
            {
                edgeCount += solid.Edges.Size;
            }

            return edgeCount;
        }

        private int CountDetailLines(Document familyDocument)
        {
            FilteredElementCollector colDetailLines = new FilteredElementCollector(familyDocument).OfCategory(BuiltInCategory.OST_Lines).OfClass(typeof(CurveElement));
        IList<Element> detailLines = colDetailLines.ToElements();

            foreach (Element detailLine in detailLines)
            {

            }

        int detailLineCount = detailLines.Count;
            return detailLineCount; 

        }
    }
}