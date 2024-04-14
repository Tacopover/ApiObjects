using System;
using System.Collections.Generic;
using System.Text;

namespace CollabAPIMEP.ViewModels
{
    public class DuplicateTypeViewModel : BaseViewModel
    {
        DuplicateTypeWindow duplicateTypeWindow;

        private bool isReplaceChecked;
        public bool IsReplaceChecked
        {
            get { return isReplaceChecked; }
            set
            {
                isReplaceChecked = value;
                IsReplaceEnabled = value;
                if (value)
                {
                    IsRenameChecked = false;
                }
                OnPropertyChanged(nameof(IsReplaceChecked));
            }
        }
        private bool isRenameChecked;
        public bool IsRenameChecked
        {
            get { return isRenameChecked; }
            set
            {
                isRenameChecked = value;
                IsRenameEnabled = value;
                if (value)
                {
                    IsReplaceChecked = false;
                }
                OnPropertyChanged(nameof(IsRenameChecked));
            }
        }

        public bool IsReplaceEnabled { get; set; }
        public bool IsRenameEnabled { get; set; }

        public DuplicateTypeViewModel(DuplicateTypeWindow duplicateTypeWindow)
        {
            this.duplicateTypeWindow = duplicateTypeWindow;
            isRenameChecked = true;
        }
    }
}
