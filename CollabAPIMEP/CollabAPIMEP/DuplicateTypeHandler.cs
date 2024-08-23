using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using CollabAPIMEP.Commands;
using CollabAPIMEP.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CollabAPIMEP
{
    public class DuplicateTypeHandler
    {
        private Document doc;
        public Family NewFamily { get; set; }
        public Family ExistingFamily { get; set; }
        private bool _renameEnabled;
        public bool RenameEnabled
        {
            get { return _renameEnabled; }
            set
            {
                ResolveAction = "Rename";
                _renameEnabled = value;
            }
        }
        private bool _replaceEnabled;
        public bool ReplaceEnabled
        {
            get { return _replaceEnabled; }
            set
            {
                ResolveAction = "Replace";
                _replaceEnabled = value;

            }
        }
        public bool ReplaceExisting { get; set; }
        public string NewFamilyName { get; set; }
        public string ExistingFamilyName { get; set; }
        public string ResolveAction { get; set; }

        private DuplicateTypeViewModel duplicateViewModel;
        public RelayCommand<object> ShowWindowCommand { get; set; }

        public DuplicateTypeHandler(Family newFamily, Family existingFamily, Document doc)
        {
            this.doc = doc;
            NewFamily = newFamily;
            ExistingFamily = existingFamily;
            RenameEnabled = true;
            ReplaceEnabled = false;
            ReplaceExisting = false;
            NewFamilyName = NewFamily.Name;
            ExistingFamilyName = ExistingFamily.Name;

            ShowWindowCommand = new RelayCommand<object>(p => true, p => ShowWindow());
            this.doc = doc;
        }

        public void ShowWindow()
        {
            duplicateViewModel = new DuplicateTypeViewModel();
            duplicateViewModel.ExistingFamilyName = ExistingFamilyName;
            duplicateViewModel.NewFamilyName = NewFamilyName;
            List<string> newFamTypeNames = new List<string>();
            List<string> existingFamTypeNames = new List<string>();
            foreach (ElementId symbolId in NewFamily.GetFamilySymbolIds())
            {
                FamilySymbol symbol = doc.GetElement(symbolId) as FamilySymbol;
                newFamTypeNames.Add(symbol.Name);
            }
            foreach (ElementId symbolId in ExistingFamily.GetFamilySymbolIds())
            {
                FamilySymbol symbol = doc.GetElement(symbolId) as FamilySymbol;
                existingFamTypeNames.Add(symbol.Name);
            }
            existingFamTypeNames.Sort();
            newFamTypeNames.Sort();
            duplicateViewModel.CreateMapping(newFamTypeNames, existingFamTypeNames);
            duplicateViewModel.DuplicateTypeWindow.ShowDialog();

            // get the users choices from the window and relay that back into this object
            RenameEnabled = duplicateViewModel.IsRenameChecked;
            ReplaceEnabled = duplicateViewModel.IsReplaceChecked;
            ReplaceExisting = duplicateViewModel.ReplaceExistingChecked;
            NewFamilyName = duplicateViewModel.NewFamilyName;
            ExistingFamilyName = duplicateViewModel.ExistingFamilyName;

        }

        public void ResolveFamily(Document doc)
        {
            //if (duplicateViewModel.IsCanceled)
            //{
            //    return;
            //}

            if (RenameEnabled)
            {
                //TODO check if the name has change at all, if not then do not rename
                string newFamName = NewFamilyName;
                string existingFamNameNew = ExistingFamilyName;
                ExistingFamily.Name = existingFamNameNew;
                NewFamily.Name = newFamName;
            }
            else
            {
                //TODO check if new family and existing family are of the same category. It could be that a new family has been
                // changed to a different category. In that case you cannot replace, but only rename the family
                Dictionary<string, FamilySymbol> typeMap = new Dictionary<string, FamilySymbol>();

                Family famToReplace;
                Family famToRemain;
                if (ReplaceExisting)
                {
                    famToReplace = ExistingFamily;
                    famToRemain = NewFamily;
                }
                else
                {
                    famToReplace = NewFamily;
                    famToRemain = ExistingFamily;
                }

                List<FamilySymbol> typesToRemain = famToRemain.GetFamilySymbolIds().Select(i => doc.GetElement(i) as FamilySymbol).ToList();
                foreach (var mapping in duplicateViewModel.Mappings.ToList())
                {
                    //create a mapping between the string value of column 1 and the family symbol that represents the string in column 2
                    //TODO item2 is column 1, which is confusing, so change this
                    typeMap[mapping.Item1] = typesToRemain.FirstOrDefault(s => s.Name.Equals(mapping.Item2));
                }


                foreach (ElementId symbolId in famToReplace.GetFamilySymbolIds())
                {
                    FamilyInstanceFilter instanceFilter = new FamilyInstanceFilter(doc, symbolId);
                    FilteredElementCollector famInstanceCollector = new FilteredElementCollector(doc)
                        .OfClass(typeof(FamilyInstance))
                        .WherePasses(instanceFilter);
                    IList<Element> instances = famInstanceCollector.ToElements();
                    FamilySymbol symbol = doc.GetElement(symbolId) as FamilySymbol;
                    string typeName = symbol.Name;
                    FamilySymbol remainingType = typeMap.TryGetValue(typeName, out FamilySymbol type) ? type : null;
                    if (remainingType == null)
                    {
                        continue;
                    }
                    foreach (var inst in instances)
                    {
                        FamilyInstance instance = inst as FamilyInstance;
                        instance.ChangeTypeId(remainingType.Id);
                    }
                }
                doc.Delete(famToReplace.Id);

                return;
            }
        }
    }
}
