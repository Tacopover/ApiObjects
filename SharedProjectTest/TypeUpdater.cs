using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Text;


namespace CollabAPIMEP
{
    public class TypeUpdater : IUpdater
    {
        static AddInId appId;
        static UpdaterId updaterId;

        public TypeUpdater(AddInId id)
        {
            appId = id;
            updaterId = new UpdaterId(appId, new Guid("05cc8ad9-9b18-4c6c-96ba-e5e3e1947d78"));
        }

        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();
            List<FamilySymbol> familySymbols = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).ToElements() as List<FamilySymbol>;

            List<ElementId> elementIds = data.GetAddedElementIds() as List<ElementId>;
            string result = "";
            foreach (ElementId id in elementIds)
            {
                Element element = doc.GetElement(id);
                result += element.Name + "\n";
            }
            DuplicateTypeWindow duplicateTypeWindow = new DuplicateTypeWindow();
            duplicateTypeWindow.ShowDialog();

        }


        public string GetAdditionalInformation()
        {
            return "This updater will show the names of the elements added to the document";
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.Annotations;
        }

        public UpdaterId GetUpdaterId()
        {
            return updaterId;
        }

        public string GetUpdaterName()
        {
            return "TypeUpdater";
        }
    }
}
