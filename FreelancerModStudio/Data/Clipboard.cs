using System;
using System.Windows.Forms;

namespace FreelancerModStudio
{
    public static class Clipboard
    {
        public static void Copy(object o, Type type)
        {
            //serialize object to clipboard
            DataFormats.Format format = DataFormats.GetFormat(type.FullName);

            IDataObject dataObj = new DataObject();
            dataObj.SetData(format.Name, false, o);
            System.Windows.Forms.Clipboard.SetDataObject(dataObj, false);
        }

        public static bool CanPaste(Type type)
        {
            //check if clipboard contains object type
            IDataObject dataObject = System.Windows.Forms.Clipboard.GetDataObject();
            return dataObject != null && dataObject.GetDataPresent(type.FullName);
        }

        public static object Paste(Type type)
        {
            //deserialize object from clipboard
            IDataObject dataObject = System.Windows.Forms.Clipboard.GetDataObject();
            if (dataObject != null && dataObject.GetDataPresent(type.FullName))
            {
                return dataObject.GetData(type.FullName);
            }

            return null;
        }
    }
}
