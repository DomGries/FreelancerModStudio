using System;
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
            var dataObject = System.Windows.Forms.Clipboard.GetDataObject();
            return dataObject != null && dataObject.GetDataPresent(type.FullName);
        }

        public static object Paste(Type type)
        {
            //deserialize object from clipboard
            var dataObject = System.Windows.Forms.Clipboard.GetDataObject();
            if (dataObject != null && dataObject.GetDataPresent(type.FullName))
                return dataObject.GetData(type.FullName);

            return null;
        }
    }
}
