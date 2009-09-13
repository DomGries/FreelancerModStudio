using System;
using System.Collections.Generic;
using System.Text;

namespace FreelancerModStudio
{
    public interface ContentInterface
    {
        bool CanAdd();
        bool CanDelete();
        bool CanSelectAll();
        
        void Add();
        void Delete();
        void SelectAll();
    }
}
