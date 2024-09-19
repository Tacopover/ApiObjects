using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using CollabAPIMEP.Commands;
using CollabAPIMEP.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace CollabAPIMEP
{
    public class DuplicateTypeHandler
    {
        private Document doc;
        public Family NewFamily { get; set; }
        public Family ExistingFamily { get; set; }

        public bool ReplaceExisting { get; set; }
        public string NewFamilyName { get; set; }
        public string ExistingFamilyName { get; set; }
        public bool IsCustom { get; set; }
        public DuplicateTypeViewModel DuplicateViewModel { get; set; }
        public RelayCommand<object> ShowWindowCommand { get; set; }

        public DuplicateTypeHandler(Family newFamily, Family existingFamily, Document doc)
        {
            this.doc = doc;
            DuplicateViewModel = new DuplicateTypeViewModel();
            DuplicateViewModel.ResolveAction = "Rename";
            DuplicateViewModel.NewFamilyNameOriginal = newFamily.Name;
            DuplicateViewModel.NewFamilyName = newFamily.Name;
            DuplicateViewModel.NewFamilyNameCustom = newFamily.Name;
            DuplicateViewModel.ExistingFamilyName = existingFamily.Name;

            NewFamily = newFamily;
            ExistingFamily = existingFamily;
            ReplaceExisting = false;
            NewFamilyName = NewFamily.Name;
            ExistingFamilyName = ExistingFamily.Name;
            DuplicateViewModel.NewFamilyNameShort = NewFamilyName.Substring(0, NewFamilyName.Length - 1);
            DuplicateViewModel.NewSuffix = NewFamilyName.Substring(NewFamilyName.Length - 1);

            ShowWindowCommand = new RelayCommand<object>(p => true, p => ShowWindow());
            this.doc = doc;
        }

        public void ShowWindow()
        {

            DuplicateViewModel.ExistingFamilyName = ExistingFamilyName;
            DuplicateViewModel.NewFamilyName = NewFamilyName;
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
            DuplicateViewModel.CreateMapping(newFamTypeNames, existingFamTypeNames);
            DuplicateViewModel.ShowWindow();


            if (DuplicateViewModel.NewFamilyName != DuplicateViewModel.NewFamilyNameMulti ||
                DuplicateViewModel.IsReplaceEnabled)
            {
                IsCustom = true;
                DuplicateViewModel.NewFamilyNameCustom = DuplicateViewModel.NewFamilyName;
                NewFamilyName = DuplicateViewModel.NewFamilyName;
                DuplicateViewModel.ResolveColour = new SolidColorBrush(Color.FromArgb(0xFF, 0xC9, 0xE4, 0xFC));
            }
            else
            {
                IsCustom = false;
                DuplicateViewModel.NewFamilyNameCustom = DuplicateViewModel.NewFamilyNameMulti;
                NewFamilyName = DuplicateViewModel.NewFamilyNameMulti;
                DuplicateViewModel.ResolveColour = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x03, 0xFF));
            }

            ReplaceExisting = DuplicateViewModel.ReplaceExistingChecked;
        }

        public void ResolveFamily(Document doc)
        {

            if (!IsCustom)
            {
                //rename with the default suffix
                NewFamily.Name = DuplicateViewModel.NewFamilyNameCustom;
                return;
            }

            if (DuplicateViewModel.IsRenameEnabled)
            {
                //TODO check if the name has changed at all, if not then do not rename
                string newFamName = DuplicateViewModel.NewFamilyNameCustom;
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
                foreach (var mapping in DuplicateViewModel.Mappings.ToList())
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
