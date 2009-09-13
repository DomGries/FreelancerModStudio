// This source is under LGPL license. Sergei Arhipenko (c) 2006-2007. email: sbs-arhipenko@yandex.ru. This notice may not be removed.
using System;
using System.Collections.Generic;
using System.Text;

namespace FreelancerModStudio
{
    public interface IUndoRedoMember
    {
        void OnCommit(object change);
        void OnUndo(object change);
        void OnRedo(object change);
    }
}
