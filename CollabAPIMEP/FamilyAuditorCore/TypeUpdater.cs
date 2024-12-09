using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace FamilyAuditorCore
{
    public class TypeUpdater : IUpdater
    {
        static UpdaterId updaterId;
        FamilyLoadHandler familyLoadHandler;

        public TypeUpdater(AddInId appId, FamilyLoadHandler familyLoadHandler)
        {
            updaterId = new UpdaterId(appId, new Guid("05cc8ad9-9b18-4c6c-96ba-e5e3e1947d78"));
            this.familyLoadHandler = familyLoadHandler;
        }

        public void Execute(UpdaterData data)
        {
            FamilyLoadHandler.AddedIds = data.GetAddedElementIds() as List<ElementId>;
            familyLoadHandler.HandleUpdater();

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
