using System;

namespace FreelancerModStudio.SystemPresenter
{
    [Flags]
    public enum ManipulationMode
    {
        None,
        Translate,
        Rotate,
        Scale,
    }
}