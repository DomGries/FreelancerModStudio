using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace FreelancerModStudio
{
    public class Clipboard
    {
        public static void Copy(object o)
        {
            //serialize object to clipboard
            DataFormats.Format format = DataFormats.GetFormat(o.GetType().FullName);

            IDataObject dataObj = new DataObject();
            dataObj.SetData(format.Name, false, o);
            System.Windows.Forms.Clipboard.SetDataObject(dataObj, false);
        }

        public static bool CanPaste(Type type)
        {
            //check if clipboard contains object type
            return System.Windows.Forms.Clipboard.GetDataObject().GetDataPresent(type.FullName);
        }

        public static object Paste(Type type)
        {
            //deserialize object from clipboard
            if (CanPaste(type))
                return System.Windows.Forms.Clipboard.GetDataObject().GetData(type.FullName);

            return null;
        }
    }
}
