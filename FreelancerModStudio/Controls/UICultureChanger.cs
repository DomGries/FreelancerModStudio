using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

/// <summary>
/// Enables changes of the UI culture of a collection of <see cref="Form"/> objects at runtime.
/// </summary>
public class UICultureChanger
{
    #region ChangeInfo class

    /// <summary>
    /// Encapsulates all information needed to apply localized resources to a form or field.
    /// </summary>
    class ChangeInfo
    {
        #region instance fields

        /// <summary>
        /// Gets the name of the form or field.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Stores the name of the form or field.
        /// </summary>
        readonly string _name;

        /// <summary>
        /// Gets the instance of the form or field.
        /// </summary>
        public object Value
        {
            get
            {
                return _value;
            }
        }

        /// <summary>
        /// Stores the instance of the form or field.
        /// </summary>
        readonly object _value;

        /// <summary>
        /// Gets the <see cref="Type"/> object of the form or field.
        /// </summary>
        public Type Type
        {
            get
            {
                return _type;
            }
        }

        /// <summary>
        /// Stores the <see cref="Type"/> object of the form or field.
        /// </summary>
        readonly Type _type;

        #endregion

        #region construction

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeInfo"/> class.
        /// </summary>
        /// <param name="name">The name of the form or field.</param>
        /// <param name="value">The instance of the form or field.</param>
        /// <param name="type">The <see cref="Type"/> object of the form or field.</param>
        public ChangeInfo(string name, object value, Type type)
        {
            _name = name;
            _value = value;
            _type = type;
        }

        #endregion
    }

    #endregion

    #region instance fields

    /// <summary>
    /// Gets or sets a value indicating whether localized Text values are applied when changing the UI culture.
    /// </summary>
    public bool ApplyText { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether localized Size values are applied when changing the UI culture.
    /// </summary>
    public bool ApplySize { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether localized Location values are applied when changing the UI culture.
    /// </summary>
    public bool ApplyLocation { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether localized RightToLeft values are applied when changing the UI culture.
    /// </summary>
    public bool ApplyRightToLeft { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether localized RightToLeftLayout values are applied when changing the UI culture.
    /// </summary>
    public bool ApplyRightToLeftLayout { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether localized ToolTip values are applied when changing the UI culture.
    /// </summary>
    public bool ApplyToolTip { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether localized Help values are applied when changing the UI culture.
    /// </summary>
    public bool ApplyHelp { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the Size values of forms remain unchanged when changing the UI culture.
    /// </summary>
    /// <remarks>
    /// This property has no effect unless <see cref="ApplySize"/> is <see langword="true"/>.
    /// </remarks>
    public bool PreserveFormSize { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the Location values of forms remain unchanged when changing the UI culture.
    /// </summary>
    /// <remarks>
    /// This property has no effect unless <see cref="ApplyLocation"/> is <see langword="true"/>.
    /// </remarks>
    public bool PreserveFormLocation { get; set; }

    #endregion

    #region construction, deconstruction

    /// <summary>
    /// Initializes a new instance of the <see cref="UICultureChanger"/> class.
    /// </summary>
    public UICultureChanger()
    {
        ApplyText = true;
        ApplySize = false;
        ApplyLocation = false;
        ApplyRightToLeft = false;
        ApplyRightToLeftLayout = false;
        ApplyToolTip = false;
        ApplyHelp = false;
        PreserveFormSize = true;
        PreserveFormLocation = true;
    }

    #endregion

    #region instance methods

    /// <summary>
    /// Applies the specified <see cref="CultureInfo"/> object to the <see cref="Thread.CurrentUICulture"/> field.
    /// </summary>
    /// <param name="cultureInfo">A <see cref="CultureInfo"/> object representing the wanted UI culture.</param>
    public void ApplyCulture(CultureInfo cultureInfo)
    {
        // Applies culture to current Thread.
        Thread.CurrentThread.CurrentUICulture = cultureInfo;
    }

    /// <summary>
    /// Applies localized resources to the specified <see cref="Form"/> object according to the 
    ///   <see cref="Thread.CurrentUICulture"/>.
    /// </summary>
    /// <param name="form">The <see cref="Form"/> object to which changed UI culture should be applied.</param>
    public void ApplyCultureToForm(Form form)
    {
        // Create a resource manager for the form and determine its fields via reflection.
        // Create and fill a collection, containing all infos needed to apply localized resources.
        ComponentResourceManager resources = new ComponentResourceManager(form.GetType());
        FieldInfo[] fields = form.GetType().GetFields(BindingFlags.Instance | BindingFlags.DeclaredOnly |
                                                      BindingFlags.NonPublic);
        List<ChangeInfo> changeInfos = new List<ChangeInfo>(fields.Length + 1)
            {
                new ChangeInfo("$this", form, form.GetType())
            };
        for (int index = 0; index < fields.Length; index++)
        {
            object value = fields[index].GetValue(form);
            if (value != null)
            {
                changeInfos.Add(new ChangeInfo(fields[index].Name, value, fields[index].FieldType));
            }
        }
        changeInfos.TrimExcess();

        // Call SuspendLayout for Form and all fields derived from Control, so assignment of 
        //   localized resources doesn't change layout immediately.
        for (int index = 0; index < changeInfos.Count; index++)
        {
            if (changeInfos[index].Type.IsSubclassOf(typeof(Control)))
            {
                changeInfos[index].Type.InvokeMember("SuspendLayout", BindingFlags.InvokeMethod, null,
                    changeInfos[index].Value, null);
            }
        }

        if (ApplyText)
        {
            // If available, assign localized text to Form and fields.
            for (int index = 0; index < changeInfos.Count; index++)
            {
                if (changeInfos[index].Type.GetProperty("Text", typeof(String)) != null &&
                    changeInfos[index].Type.GetProperty("Text", typeof(String)).CanWrite)
                {
                    String text = resources.GetString(changeInfos[index].Name + ".Text");
                    if (!string.IsNullOrEmpty(text))
                    {
                        changeInfos[index].Type.InvokeMember("Text", BindingFlags.SetProperty, null,
                            changeInfos[index].Value, new object[] { text });
                    }
                }
            }
        }

        if (ApplySize)
        {
            // If available, assign localized sizes to Form and fields.
            int index = 0;
            if (PreserveFormSize)
            {
                // Skip the form entry in changeInfos collection.
                index = 1;
            }
            for (; index < changeInfos.Count; index++)
            {
                object size;
                Control control;
                if (changeInfos[index].Type.GetProperty("Size", typeof(Size)) != null &&
                    changeInfos[index].Type.GetProperty("Size", typeof(Size)).CanWrite)
                {
                    size = resources.GetObject(changeInfos[index].Name + ".Size");
                    if (size != null && size is Size)
                    {
                        if (changeInfos[index].Type.IsSubclassOf(typeof(Control)))
                        {
                            Size newSize = (Size)size;

                            // In case of an inheritor of Control take into account the Anchor property.
                            control = (Control)changeInfos[index].Value;
                            if ((control.Anchor & (AnchorStyles.Left | AnchorStyles.Right)) == (AnchorStyles.Left | AnchorStyles.Right))
                            {
                                // Control is bound to the left and right edge, so preserve its width.
                                newSize.Width = control.Width;
                            }
                            if ((control.Anchor & (AnchorStyles.Top | AnchorStyles.Bottom)) == (AnchorStyles.Top | AnchorStyles.Bottom))
                            {
                                // Control is bound to the top and bottom edge, so preserve its height.
                                newSize.Height = control.Height;
                            }
                            control.Size = newSize;
                        }
                        else
                        {
                            changeInfos[index].Type.InvokeMember("Size", BindingFlags.SetProperty, null,
                                changeInfos[index].Value, new[] { size });
                        }
                    }
                }

                if (changeInfos[index].Type.GetProperty("ClientSize", typeof(Size)) != null &&
                    changeInfos[index].Type.GetProperty("ClientSize", typeof(Size)).CanWrite)
                {
                    size = resources.GetObject(changeInfos[index].Name + ".ClientSize");
                    if (size != null && size is Size)
                    {
                        if (changeInfos[index].Type.IsSubclassOf(typeof(Control)))
                        {
                            Size newSize = (Size)size;

                            // In case of an inheritor of Control take into account the Anchor property.
                            control = (Control)changeInfos[index].Value;
                            if ((control.Anchor & (AnchorStyles.Left | AnchorStyles.Right)) == (AnchorStyles.Left | AnchorStyles.Right))
                            {
                                // Control is bound to the left and right edge, so preserve the width of its client area.
                                newSize.Width = control.ClientSize.Width;
                            }
                            if ((control.Anchor & (AnchorStyles.Top | AnchorStyles.Bottom)) == (AnchorStyles.Top | AnchorStyles.Bottom))
                            {
                                // Control is bound to the top and bottom edge, so preserve the height of its client area.
                                newSize.Height = control.ClientSize.Height;
                            }
                            control.ClientSize = newSize;
                        }
                        else
                        {
                            changeInfos[index].Type.InvokeMember("ClientSize", BindingFlags.SetProperty, null,
                                changeInfos[index].Value, new[] { size });
                        }
                    }
                }
            }
        }

        if (ApplyLocation)
        {
            // If available, assign localized locations to Form and fields.
            int index = 0;
            if (PreserveFormLocation)
            {
                // Skip the form entry in changeInfos collection.
                index = 1;
            }
            for (; index < changeInfos.Count; index++)
            {
                if (changeInfos[index].Type.GetProperty("Location", typeof(Point)) != null &&
                    changeInfos[index].Type.GetProperty("Location", typeof(Point)).CanWrite)
                {
                    object location = resources.GetObject(changeInfos[index].Name + ".Location");
                    if (location != null && location is Point)
                    {
                        if (changeInfos[index].Type.IsSubclassOf(typeof(Control)))
                        {
                            // In case of an inheritor of Control take into account the Anchor property.
                            Control control = (Control)changeInfos[index].Value;
                            if ((control.Anchor & (AnchorStyles.Left | AnchorStyles.Right)) == AnchorStyles.Right)
                            {
                                // Control is bound to the right but not the left edge, so preserve its x-coordinate.
                                location = new Point(control.Left, ((Point)location).Y);
                            }
                            if ((control.Anchor & (AnchorStyles.Top | AnchorStyles.Bottom)) == AnchorStyles.Bottom)
                            {
                                // Control is bound to the bottom but not the top edge, so preserve its y-coordinate.
                                location = new Point(((Point)location).X, control.Top);
                            }
                            control.Location = (Point)location;
                        }
                        else
                        {
                            changeInfos[index].Type.InvokeMember("Location", BindingFlags.SetProperty, null,
                                changeInfos[index].Value, new[] { location });
                        }
                    }
                }
            }
        }

        if (ApplyRightToLeft)
        {
            // If available, assign localized RightToLeft values to Form and fields.
            for (int index = 0; index < changeInfos.Count; index++)
            {
                if (changeInfos[index].Type.GetProperty("RightToLeft", typeof(RightToLeft)) != null &&
                    changeInfos[index].Type.GetProperty("RightToLeft", typeof(RightToLeft)).CanWrite)
                {
                    object rightToLeft = resources.GetObject(changeInfos[index].Name + ".RightToLeft");
                    if (rightToLeft != null && rightToLeft.GetType() == typeof(RightToLeft))
                    {
                        changeInfos[index].Type.InvokeMember("RightToLeft", BindingFlags.SetProperty, null,
                            changeInfos[index].Value, new[] { rightToLeft });
                    }
                }
            }
        }

        if (ApplyRightToLeftLayout)
        {
            // If available, assign localized RightToLeftLayout values to Form and fields.
            for (int index = 0; index < changeInfos.Count; index++)
            {
                if (changeInfos[index].Type.GetProperty("RightToLeftLayout", typeof(bool)) != null &&
                    changeInfos[index].Type.GetProperty("RightToLeftLayout", typeof(bool)).CanWrite)
                {
                    object rightToLeftLayout = resources.GetObject(changeInfos[index].Name + ".RightToLeftLayout");
                    if (rightToLeftLayout != null && rightToLeftLayout is bool)
                    {
                        changeInfos[index].Type.InvokeMember("RightToLeftLayout", BindingFlags.SetProperty, null,
                            changeInfos[index].Value, new[] { rightToLeftLayout });
                    }
                }
            }
        }

        if (ApplyToolTip)
        {
            // If available, assign localized ToolTipText to fields.
            // Also search for a ToolTip component in the current form.
            ToolTip toolTip = null;
            for (int index = 1; index < changeInfos.Count; index++)
            {
                if (changeInfos[index].Type == typeof(ToolTip))
                {
                    toolTip = (ToolTip)changeInfos[index].Value;
                    resources.ApplyResources(toolTip, changeInfos[index].Name);
                    changeInfos.Remove(changeInfos[index]);
                }
                if (changeInfos[index].Type.GetProperty("ToolTipText", typeof(String)) != null &&
                    changeInfos[index].Type.GetProperty("ToolTipText", typeof(String)).CanWrite)
                {
                    String text = resources.GetString(changeInfos[index].Name + ".ToolTipText");
                    if (text != null)
                    {
                        changeInfos[index].Type.InvokeMember("ToolTipText", BindingFlags.SetProperty, null,
                            changeInfos[index].Value, new object[] { text });
                    }
                }
            }

            if (toolTip != null)
            {
                // Form contains a ToolTip component.
                // If available, assign localized tooltips to Form and fields.
                for (int index = 0; index < changeInfos.Count; index++)
                {
                    if (changeInfos[index].Type.IsSubclassOf(typeof(Control)))
                    {
                        String text = resources.GetString(changeInfos[index].Name + ".ToolTip");
                        if (text != null)
                        {
                            toolTip.SetToolTip((Control)changeInfos[index].Value, text);
                        }
                    }
                }
            }
        }

        if (ApplyHelp)
        {
            // Search for a HelpProvider component in the current form.
            HelpProvider helpProvider = null;
            for (int index = 1; index < changeInfos.Count; index++)
            {
                if (changeInfos[index].Type == typeof(HelpProvider))
                {
                    helpProvider = (HelpProvider)changeInfos[index].Value;
                    resources.ApplyResources(helpProvider, changeInfos[index].Name);
                    changeInfos.Remove(changeInfos[index]);
                    break;
                }
            }

            if (helpProvider != null)
            {
                // If available, assign localized help to Form and fields.
                for (int index = 0; index < changeInfos.Count; index++)
                {
                    if (changeInfos[index].Type.IsSubclassOf(typeof(Control)))
                    {
                        String text = resources.GetString(changeInfos[index].Name + ".HelpKeyword");
                        if (text != null)
                        {
                            helpProvider.SetHelpKeyword((Control)changeInfos[index].Value, text);
                        }

                        object help = resources.GetObject(changeInfos[index].Name + ".HelpNavigator");
                        if (help != null && help.GetType() == typeof(HelpNavigator))
                        {
                            helpProvider.SetHelpNavigator((Control)changeInfos[index].Value, (HelpNavigator)help);
                        }

                        text = resources.GetString(changeInfos[index].Name + ".HelpString");
                        if (text != null)
                        {
                            helpProvider.SetHelpString((Control)changeInfos[index].Value, text);
                        }

                        help = resources.GetObject(changeInfos[index].Name + ".ShowHelp");
                        if (help != null && help is bool)
                        {
                            helpProvider.SetShowHelp((Control)changeInfos[index].Value, (bool)help);
                        }
                    }
                }
            }
        }

        // Call ResumeLayout for Form and all fields derived from Control to resume layout logic.
        for (int index = changeInfos.Count - 1; index >= 0; index--)
        {
            if (changeInfos[index].Type.IsSubclassOf(typeof(Control)))
            {
                changeInfos[index].Type.InvokeMember("ResumeLayout", BindingFlags.InvokeMethod, null,
                    changeInfos[index].Value, new object[] { true });
            }
        }
    }

    #endregion
}
