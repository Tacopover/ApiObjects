﻿using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CollabAPIMEP.Views;
using Microsoft.Extensions.DependencyInjection;
using CollabAPIMEP;

namespace CollabAPIMEP
{
    [Transaction(TransactionMode.Manual)]
    public class LoginCommand : IExternalCommand, IExternalCommandAvailability
    {


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elementSet)
        {
            
            return Result.Succeeded;
        }

        public bool IsCommandAvailable(UIApplication uiapp, CategorySet selectedCategories)
        {
            return true;
        }
    }
}
