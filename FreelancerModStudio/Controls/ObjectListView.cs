using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace FreelancerModStudio
{
	/// <summary>
	/// An object list displays 'aspects' of a collection of objects in a
	/// multi-column list control.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The intelligence for this control is in the columns. OLVColumns are
	/// extended so they understand how to fetch an 'aspect' from each row
	/// object. They also understand how to sort by their aspect, and
    /// how to group them.
    /// </para>
    /// <para>
    /// Aspects are extracted by giving the name of a method to be called or a
    /// property to be fetched. These names can be simple names or they can be dotted
    /// to chain property access e.g. "Owner.Address.Postcode". 
    /// Aspects can also be extracted by installing a delegate.
    /// </para>
    /// <para>
    /// Sorting by column clicking is handled automatically.
    /// Grouping by column is also handled automatically.
    /// </para>
    /// <para>
    /// This list puts sort indicators in the column headers to show the column sorting direction.
    /// If you wish to replace the standard images with your own images, put entries in the small image list
    /// with the key values "sort-indicator-up" and "sort-indicator-down".
    /// </para>
    /// <para>
    /// For these classes to build correctly, the project must have references to these assemblies:
    /// <list>
    /// <item>System.Data</item>
    /// <item>System.Design</item>
    /// <item>System.Drawing</item>
    /// <item>System.Windows.Forms (obviously)</item>
    /// </list>
    /// </para>
    /// </remarks>
    public class ObjectListView : ListView, ISupportInitialize
	{
        /// <summary>
        /// Create an ObjectListView
        /// </summary>
		public ObjectListView()
			: base()
		{
			this.ColumnClick += new ColumnClickEventHandler(this.HandleColumnClick);

			base.View = View.Details;
			this.DoubleBuffered = true; // kill nasty flickers. hiss... me hates 'em
		    this.AlternateRowBackColor = Color.Empty;
            this.ShowSortIndicators = true;
		}

		#region Public properties

        /// <summary>
        /// If there are no items in this list view, what message should be drawn onto the control?
        /// </summary>
        [Category("Appearance"),
         Description("When the list has no items, show this message in the control"),
         DefaultValue("")]
        public String EmptyListMsg
        {
            get { return emptyListMsg; }
            set { emptyListMsg = value; }
        }
        private String emptyListMsg;

        /// <summary>
        /// What font should the 'list empty' message be drawn in?
        /// </summary>
        [Category("Appearance"),
        Description("What font should the 'list empty' message be drawn in?"),
        DefaultValue(null)]
        public Font EmptyListMsgFont
        {
            get { return emptyListMsgFont; }
            set { emptyListMsgFont = value; }
        }
        private Font emptyListMsgFont;

        /// <summary>
        /// Return the font for the 'list empty' message or a default 
        /// </summary>
        [Browsable(false)]
        public Font EmptyListMsgFontOrDefault
        {
            get {
                if (this.EmptyListMsgFont == null)
                    return new Font("Tahoma", 14);
                else
                    return this.EmptyListMsgFont;
            }
        }
	
        /// <summary>
        /// Does this listview have a message that should be drawn when the list is empty?
        /// </summary>
        [Browsable(false)]
        public bool HasEmptyListMsg
        {
            get { return !String.IsNullOrEmpty(this.EmptyListMsg); }
        }
	
        /// <summary>
        /// Should the list view show images on subitems?
        /// </summary>
        /// <remarks>
        /// <para>Under Windows, this works by sending messages to the underlying
        /// Windows control. To make this work under Mono, we would have to owner drawing the items :-(</para></remarks>
        [Category("Behavior"),
         Description("Should the list view show images on subitems?"),
         DefaultValue(false)]
        public bool ShowImagesOnSubItems
        {
            get { return showImagesOnSubItems; }
            set { showImagesOnSubItems = value; }
        }

		/// <summary>
		/// This property controls whether group labels will be suffixed with a count of items.
		/// </summary>
		/// <remarks>
        /// The format of the suffix is controlled by GroupWithItemCountFormat/GroupWithItemCountSingularFormat properties
		/// </remarks>
		[Category("Behavior"),
		 Description("Will group titles be suffixed with a count of the items in the group?"),
		 DefaultValue(false)]
		public bool ShowItemCountOnGroups {
			get { return showItemCountOnGroups; }
			set { showItemCountOnGroups = value; }
		}

        /// <summary>
        /// When a group title has an item count, how should the lable be formatted?
        /// </summary>
        /// <remarks>
        /// The given format string can/should have two placeholders:
        /// <list type="bullet">
        /// <item>{0} - the original group title</item>
        /// <item>{1} - the number of items in the group</item>
        /// </list>
        /// </remarks>
        /// <example>"{0} [{1} items]"</example>
        [Category("Behavior"),
         Description("The format to use when suffixing item counts to group titles"),
         DefaultValue(null)]
        public string GroupWithItemCountFormat
        {
            get { return groupWithItemCountFormat; }
            set { groupWithItemCountFormat = value; }
        }

        /// <summary>
        /// Return this.GroupWithItemCountFormat or a reasonable default
        /// </summary>
        [Browsable(false)]
        public string GroupWithItemCountFormatOrDefault
        {
            get {
                if (String.IsNullOrEmpty(this.GroupWithItemCountFormat))
                    return "{0} [{1} items]";
                else
                    return this.GroupWithItemCountFormat;
            }
        }

        /// <summary>
        /// When a group title has an item count, how should the lable be formatted if
        /// there is only one item in the group?
        /// </summary>
        /// <remarks>
        /// The given format string can/should have two placeholders:
        /// <list type="bullet">
        /// <item>{0} - the original group title</item>
        /// <item>{1} - the number of items in the group (always 1)</item>
        /// </list>
        /// </remarks>
        /// <example>"{0} [{1} item]"</example>
        [Category("Behavior"),
         Description("The format to use when suffixing item counts to group titles"),
         DefaultValue(null)]
        public string GroupWithItemCountSingularFormat
        {
            get { return groupWithItemCountSingularFormat; }
            set { groupWithItemCountSingularFormat = value; }
        }

        /// <summary>
        /// Return this.GroupWithItemCountSingularFormat or a reasonable default
        /// </summary>
        [Browsable(false)]
        public string GroupWithItemCountSingularFormatOrDefault
        {
            get
            {
                if (String.IsNullOrEmpty(this.GroupWithItemCountSingularFormat))
                    return "{0} [{1} item]";
                else
                    return this.GroupWithItemCountSingularFormat;
            }
        }

		/// <summary>
		/// Should the list give a different background color to every second row?
		/// </summary>
        /// <remarks><para>The color of the alternate rows is given by AlternateRowBackColor.</para>
        /// <para>There is a "feature" in .NET for listviews in non-full-row-select mode, where
        /// selected rows are not drawn with their correct background color.</para></remarks>
		[Category("Appearance"),
		 Description("Should the list view use a different backcolor to alternate rows?"),
		 DefaultValue(false)]
		public bool UseAlternatingBackColors {
			get { return useAlternatingBackColors; }
			set { useAlternatingBackColors = value; }
		}

        /// <summary>
        /// Should the list view show a bitmap in the column header to show the sort direction?
        /// </summary>
        [Category("Behavior"),
         Description("Should the list view show sort indicators in the column headers?"),
         DefaultValue(true)]
        public bool ShowSortIndicators
        {
            get { return showSortIndicators; }
            set { showSortIndicators = value; }
        }

		/// <summary>
		/// If every second row has a background different to the control, what color should it be?
		/// </summary>
		[Category("Appearance"),
		 Description("If using alternate colors, what color should alterate rows be?")
		 //DefaultValue(Color.Empty) // I should be able to do this!
		]
		public Color AlternateRowBackColor {
			get { return alternateRowBackColor; }
			set { alternateRowBackColor = value; }
		}

		/// <summary>
		/// Return the alternate row background color that has been set, or the default color
		/// </summary>
		[Browsable(false)]
		public Color AlternateRowBackColorOrDefault {
			get {
				if (alternateRowBackColor == Color.Empty)
					return Color.LemonChiffon;
//					return Color.AliceBlue; // very slight
				else
					return alternateRowBackColor;
			}
		}

        /// <summary>
        /// This delegate can be used to sort the table in a custom fasion.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SortDelegate CustomSorter
        {
            get { return customSorter; }
            set { customSorter = value; }
        }

        /// <summary>
        /// This delegate can be used to format a OLVListItem before it is added to the control.
        /// </summary>
        /// <remarks>
        /// <para>The model object for the row can be found through the RowObject property of the OLVListItem object.</para>
        /// <para>All subitems normally have the same style as list item, so setting the forecolor on one 
        /// subitem changes the forecolor of all subitems.
        /// To allow subitems to have different attributes, <code>ListViewItem.UseItemStyleForSubItems</code> 
        /// must be set to <code>false</code>.
        /// </para>
        /// <para>If <code>UseAlternatingBackColors</code> is true, the backcolor of the listitem will be calculated
        /// by the control and cannot be controlled by the RowFormatter delegate. In general, trying to use a RowFormatter
        /// when <code>UseAlternatingBackColors</code> is <code>true</code> does not work well.</para></remarks>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RowFormatterDelegate RowFormatter
        {
            get { return rowFormatter; }
            set { rowFormatter = value; }
        }
        private RowFormatterDelegate rowFormatter;
	
        /// <summary>
        /// Get or set whether or not the listview is frozen. When the listview is
        /// frozen, it will not update itself.
        /// </summary>
        /// <remarks><para>The Frozen property is similar to the methods Freeze()/Unfreeze()
        /// except that changes to the Frozen property do not nest.</para></remarks>
        /// <example>objectListView1.Frozen = false; // unfreeze the control regardless of the number of Freeze() calls
        /// </example>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Frozen
        {
            get { return freezeCount > 0; }
            set {
                if (value)
                    Freeze();
                else if (freezeCount > 0) {
                    freezeCount = 1;
                    Unfreeze();
                }
            }
        }
        private int freezeCount;

        /// <summary>
        /// Get/set the list of columns that should be used when the list switches to tile view.
        /// </summary>
        /// <remarks>If no list of columns has been installed, this value will default to the
        /// first column plus any column where IsTileViewColumn is true.</remarks>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<OLVColumn> ColumnsForTileView
        {
            get
            {
                if (this.columnsForTileView.Count == 0) {
                    foreach (ColumnHeader column in this.Columns) {
                        if (column.Index == 0 || ((OLVColumn)column).IsTileViewColumn)
                            this.columnsForTileView.Add((OLVColumn)column);
                    }
                }
                return columnsForTileView;
            }
            set { columnsForTileView = value; }
        }
        private List<OLVColumn> columnsForTileView = new List<OLVColumn>();

        /// <summary>
        /// Get the ListViewItem that is currently selected . If no row is selected, or more than one is selected, return null.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ListViewItem SelectedItem
        {
            get
            {
                if (this.SelectedItems.Count == 1)
                    return this.SelectedItems[0];
                else
                    return null;
            }
            set {
                this.SelectedIndices.Clear();
                if (value != null)
                    this.SelectedIndices.Add(value.Index);
            }
        }

        /// <summary>
        /// Get the model object from the currently selected row. If no row is selected, or more than one is selected, return null.
        /// Select the row that is displaying the given model object. All other rows are deselected.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Object SelectedObject
        {
            get { return this.GetSelectedObject(); }
            set { this.SelectObject(value); }
        }

        /// <summary>
        /// Get the model objects from the currently selected rows. If no row is selected, the returned List will be empty.
        /// Select the rows that is displaying the given model objects. All other rows are deselected.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ArrayList SelectedObjects
        {
            get { return this.GetSelectedObjects(); }
            set { this.SelectObjects(value); }
        }

        /// <summary>
        /// Get/set the style of view that this listview is using
        /// </summary>
        /// <remarks>Switching to tile view installs the columns from ColumnsForTileView property.
        /// Confusingly, in tile view, every column is shown as a row of information.</remarks>
        new public View View
        {
            get
            {
                return base.View;
            }
            set
            {
                if (base.View == value)
                    return;

                if (this.Frozen) {
                    base.View = value;
                    return;
                }

                // If we are leaving a details view, remember the columns we had
                if (base.View == View.Details && this.Columns.Count > 0) {
                    this.columnsToRestore = new List<OLVColumn>();
                    foreach (ColumnHeader column in this.Columns)
                        this.columnsToRestore.Add((OLVColumn)column);
                }

                this.Freeze();

                // If we are switching to tile view, install our tile columns and set a reasonable default tile size
                if (value == View.Tile) {
                    if (this.ColumnsForTileView.Count > 0) {
                        this.Columns.Clear();
                        this.Columns.AddRange(this.ColumnsForTileView.ToArray());
                    }
                    if (this.Columns.Count > 0 && this.TileSize.Width == 0 && this.TileSize.Height == 0) {
                        int imageHeight = (this.LargeImageList == null ? 16 : this.LargeImageList.ImageSize.Height);
                        int dataHeight = (this.Font.Height + 1) * this.Columns.Count;
                        this.TileSize = new Size(200, ((imageHeight > dataHeight) ? imageHeight : dataHeight));
                    }
                    // We changed the columns, so we have to rebuild the list contents
                    this.BuildList();
                }

                // If we're switching back to the details view from the tile view,
                // put the original columns back.
                if (value == View.Details && this.columnsToRestore != null) {
                    this.Columns.Clear();
                    this.Columns.AddRange(this.columnsToRestore.ToArray());
                    this.columnsToRestore = null;

                    // We changed the columns, so we have to rebuild the list contents
                    this.BuildList();
                }

                base.View = value;
                this.Unfreeze();
            }
        }
        private List<OLVColumn> columnsToRestore = null;

        /// <summary>
        /// Specify the height of each row in the control in pixels.
        /// </summary>
        /// <remarks><para>The row height in a listview is normally determined by the font size and the small image list size.
        /// This setting allows that calculation to be overridden (within reason: you still cannot set the line height to be 
        /// less than the line height of the font used in the control). </para>
        /// <para>Setting it to -1 means use the normal calculation method.</para>
        /// <para><bold>This feature is experiemental! Strange things may happen if you use it.</bold></para>
        /// </remarks>
        [Category("Appearance"),
         DefaultValue(-1)]
        public int RowHeight
        {
            get { return rowHeight; }
            set {
                if (value < 1)
                    rowHeight = -1;
                else
                    rowHeight = value;
                this.SetupExternalImageList();
            }
        }
        private int rowHeight = -1;

        /// <summary>
        /// Override the SmallImageList property so we can correctly shadow its operations.
        /// </summary>
        /// <remarks><para>If you use the <code>RowHeight</code> property to specify the row height, the SmallImageList
        /// must be fully initialised before setting/changing the RowHeight. If you add new images to the image
        /// list after setting the RowHeight, you must assign the imagelist to the control again. Something as simple 
        /// as this will work:</para>
        /// <para><code>listView1.SmallImageList = listView1.SmallImageList;</code></para>
        /// </remarks>
        new public ImageList SmallImageList
        {
            get { return this.shadowedImageList; }
            set
            {
                this.shadowedImageList = value;
                this.SetupExternalImageList();
            }
        }
        private ImageList shadowedImageList = null;

        /// <summary>
        /// Give access to the image list that is actually being used by the control
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ImageList BaseSmallImageList
        {
            get { return base.SmallImageList; }
        }
	
        /// <summary>
        /// Get/set the column that will be used to resolve comparisons that are equal when sorting.
        /// </summary>
        /// <remarks>There is no user interface for this setting. It must be set programmatically.
        /// The default is the first column.</remarks>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public OLVColumn SecondarySortColumn
        {
            get { 
        		if (this.secondarySortColumn == null) {
        			if (this.Columns.Count > 0)
        				return this.GetColumn(0);
        			else
        				return null;
        		} else
        			return this.secondarySortColumn;
        	}
            set { 
        		this.secondarySortColumn = value;
        	}
        }
        private OLVColumn secondarySortColumn;
        
        /// <summary>
        /// When the SecondarySortColumn is used, in what order will it compare results?
        /// </summary>
         [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SortOrder SecondarySortOrder
        {
        	get { return this.secondarySortOrder; }
        	set { this.secondarySortOrder = value; }
        }
        private SortOrder secondarySortOrder = SortOrder.Ascending;
        
        /// <summary>
        /// When the listview is grouped, should the items be sorted by the primary column?
        /// If this is false, the items will be sorted by the same column as they are grouped.
        /// </summary>
        [Category("Behavior"),
         Description("When the listview is grouped, should the items be sorted by the primary column? If this is false, the items will be sorted by the same column as they are grouped."),
         DefaultValue(true)]
        public bool SortGroupItemsByPrimaryColumn
        {
        	get { return this.sortGroupItemsByPrimaryColumn; }
        	set { this.sortGroupItemsByPrimaryColumn = value; }
        }
        private bool sortGroupItemsByPrimaryColumn = true;
       
		#endregion

        #region List commands

        private delegate void SetObjectsInvoker(IEnumerable collection);

		/// <summary>
		/// Set the collection of objects that will be shown in this list view.
		/// </summary>
		/// <remarks>The list is updated immediately</remarks>
		/// <param name="collection">The objects to be displayed</param>
		virtual public void SetObjects (IEnumerable collection)
		{
			if (this.InvokeRequired) {
				this.Invoke(new SetObjectsInvoker(this.SetObjects), new object [] {collection});
				return;
			}
			this.objects = collection;
			this.BuildList(false);
        }

        /// <summary>
        /// Build/rebuild all the list view items in the list
        /// </summary>
        virtual public void BuildList()
        {
            this.BuildList(true);
        }

        /// <summary>
        /// Build/rebuild all the list view items in the list
        /// </summary>
        /// <param name="shouldPreserveSelection">If this is true, the control will try to preserve the selection, 
        /// i.e. all objects that were selected before the call will be selected after the rebuild</param>
        /// <remarks>Use this method in situations were the contents of the list is basically the same
        /// as previously.</remarks>
		virtual public void BuildList(bool shouldPreserveSelection)
        {
            if (this.Frozen)
                return;

            ArrayList previousSelection = new ArrayList();
            if (shouldPreserveSelection)
                previousSelection = this.SelectedObjects;

			this.BeginUpdate();
			this.Items.Clear();
			this.ListViewItemSorter = null;

            if (this.objects != null) {
                foreach (object rowObject in this.objects) {
                    OLVListItem lvi = new OLVListItem(rowObject);
                    this.FillInValues(lvi, rowObject);
                    this.Items.Add(lvi);
                }
                this.SetAllSubItemImages();
                this.Sort(this.lastSortColumn);
            }

            if (shouldPreserveSelection)
                this.SelectedObjects = previousSelection;

			this.EndUpdate();
        }

        /// <summary>
        /// Sort the items by the last sort column
        /// </summary>
        new public void Sort()
        {
            this.Sort(this.lastSortColumn);
        }

        /// <summary>
        /// Organise the view items into groups, based on the last sort column or the first column
        /// if there is no last sort column
        /// </summary>
        public void BuildGroups()
        {
            this.BuildGroups(this.lastSortColumn);
        }

        /// <summary>
        /// Organise the view items into groups, based on the given column
        /// </summary>
        /// <param name="column">The column whose values should be used for sorting.</param>
        virtual public void BuildGroups(OLVColumn column)
        {
            if (column == null)
                column = this.GetColumn(0);

            this.Groups.Clear();

            // Getting the Count forces any internal cache of the ListView to be flushed. Without
            // this, iterating over the Items will not work correctly if the ListView handle
            // has not yet been created.
            int dummy = this.Items.Count;

            // Separate the list view items into groups, using the group key as the descrimanent
            Dictionary<object, List<OLVListItem>> map = new Dictionary<object, List<OLVListItem>>();
            foreach (OLVListItem olvi in this.Items) {
                object key = column.GetGroupKey(olvi.RowObject);
                if (key == null)
                    key = "{null}"; // null can't be used as the key for a dictionary
                if (!map.ContainsKey(key))
                    map[key] = new List<OLVListItem>();
                map[key].Add(olvi);
            }

            // Make a list of the required groups
            List<ListViewGroup> groups = new List<ListViewGroup>();
            foreach (object key in map.Keys) {
                ListViewGroup lvg = new ListViewGroup(column.ConvertGroupKeyToTitle(key));
                lvg.Tag = key;
                groups.Add(lvg);
            }

            // Sort the groups
            groups.Sort(new ListViewGroupComparer(this.lastSortOrder));

            // Put each group into the list view, and give each group its member items.
            // The order of statements is important here:
            // - the header must be calculate before the group is added to the list view,
            //   otherwise changing the header causes a nasty redraw (even in the middle of a BeginUpdate...EndUpdate pair)
            // - the group must be added before it is given items, otherwise an exception is thrown (is this documented?)
            string fmt = column.GroupWithItemCountFormatOrDefault;
            string singularFmt = column.GroupWithItemCountSingularFormatOrDefault;
            ColumnComparer itemSorter = new ColumnComparer((this.SortGroupItemsByPrimaryColumn ? this.GetColumn(0) : column), 
                                                           this.lastSortOrder, this.SecondarySortColumn, this.SecondarySortOrder);
            foreach (ListViewGroup group in groups) {
                if (this.ShowItemCountOnGroups) {
                    int count = map[group.Tag].Count;
                    group.Header = String.Format((count == 1 ? singularFmt : fmt), group.Header, count);
                }
                this.Groups.Add(group);
                map[group.Tag].Sort(itemSorter);
                group.Items.AddRange(map[group.Tag].ToArray());
            }
        }

        #endregion

		#region Empty List Msg handling


        /// <summary>
        /// Override the basic message pump for this control
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg) {
                case 0xF: // WM_PAINT
                    this.HandlePrePaint();
                    base.WndProc(ref m);
                    this.HandlePostPaint();
                    break;
                case 0x4E: // WM_NOTIFY
                    if (!this.HandleNotify(ref m))
                        base.WndProc(ref m);
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        private void HandlePrePaint()
        {
            // When we get a WM_PAINT msg, remember the rectangle that is being updated. 
            // We can't get this information later, since the BeginPaint call wipes it out.
            this.lastUpdateRectangle = NativeMethods.GetUpdateRect(this);

            // When the list is empty, we want to handle the drawing of the control by ourselves.
            // Unfortunately, there is no easy way to tell our superclass that we want to do this.
            // So we resort to guile and deception. We validate the list area of the control, which
            // effectively tells our superclass that this area does not need to be painted. 
            // Our superclass will then not paint the control, leaving us free to do so ourselves.
            // Without doing this trickery, the superclass will draw the
            // list as empty, and then moments later, we will draw the empty msg, giving a nasty flicker
            if (this.Items.Count == 0 && this.HasEmptyListMsg) 
                NativeMethods.ValidateRect(this, this.ClientRectangle);
        }

        private void HandlePostPaint()
        {
            // If the list isn't empty or there isn't an emptyList msg, do nothing
            if (this.Items.Count != 0 || !this.HasEmptyListMsg)
                return;

            // Draw the empty list msg centered in the client area of the control
            using (BufferedGraphics buffered = BufferedGraphicsManager.Current.Allocate(this.CreateGraphics(), this.ClientRectangle)) {
                Graphics g = buffered.Graphics;
                g.Clear(this.BackColor);
                Font f = this.EmptyListMsgFontOrDefault;
                SizeF msgDims = g.MeasureString(this.EmptyListMsg, f);
                g.DrawString(this.EmptyListMsg, f, SystemBrushes.ControlDark, (this.Width - msgDims.Width) / 2, ((this.Height - msgDims.Height) / 3));
                buffered.Render();
            }
        }
		
		#endregion

        #region Column resizing handling

        /// <summary>
        /// When the control is created capture the messages for the header. 
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            hdrCtrl = new HeaderControl(this);
        }
        private HeaderControl hdrCtrl = null;

        /// <summary>
        /// Class used to capture window messages for the header of the list view
        /// control.  
        /// </summary>
        private class HeaderControl : NativeWindow
        {
            private ObjectListView parentListView = null;

            public HeaderControl(ObjectListView olv)
            {
                parentListView = olv;
                this.AssignHandle(NativeMethods.GetHeaderControl(olv));
            }

            protected override void WndProc(ref Message message)
            {
                const int WM_SETCURSOR = 0x0020;

                switch (message.Msg) {
                    case WM_SETCURSOR:
                        if (IsCursorOverLockedDivider()) {
                            message.Result = (IntPtr)1;	// Don't change the cursor
                            return;
                        }
                        break;
                }

                base.WndProc(ref message);
            }

            private bool IsCursorOverLockedDivider()
            {
                Point pt = this.parentListView.PointToClient(Cursor.Position);
                int dividerIndex = NativeMethods.GetDividerUnderPoint(this.Handle, pt);
                if (dividerIndex >= 0 && dividerIndex < this.parentListView.Columns.Count) {
                    OLVColumn column = this.parentListView.GetColumn(dividerIndex);
                    return column.IsFixedWidth;
                } else 
                    return false;
            }
        }

        /// <summary>
        /// In the notification messages, we handle attempts to change the width of our columns
        /// </summary>
        /// <param name="m">The msg to be processed</param>
        /// <returns>bool to indicate if the msg has been handled</returns>
        private bool HandleNotify(ref Message m)
        {
            bool isMsgHandled = false;

            const int HDN_FIRST = (0 - 300);
            //const int HDN_DIVIDERDBLCLICKA = (HDN_FIRST - 5);
            //const int HDN_DIVIDERDBLCLICKW = (HDN_FIRST - 25);
            //const int HDN_BEGINTRACKA = (HDN_FIRST - 6);
            //const int HDN_BEGINTRACKW = (HDN_FIRST - 26);
            //const int HDN_ENDTRACKA = (HDN_FIRST - 7);
            //const int HDN_ENDTRACKW = (HDN_FIRST - 27);
            const int HDN_TRACKA = (HDN_FIRST - 8);
            const int HDN_TRACKW = (HDN_FIRST - 28);
            const int HDN_ITEMCHANGINGA = (HDN_FIRST - 0);
            const int HDN_ITEMCHANGINGW = (HDN_FIRST - 20);

            // Handle the notification, remembering to handle both ANSI and Unicode versions
            NativeMethods.NMHDR nmhdr = (NativeMethods.NMHDR)m.GetLParam(typeof(NativeMethods.NMHDR));
            //if (nmhdr.code < HDN_FIRST)
            //    System.Diagnostics.Debug.WriteLine(nmhdr.code);

            // In KB Article #183258, MS states that when a header control has the HDS_FULLDRAG style, it will receive
            // ITEMCHANGING events rather than TRACK events. Under XP SP2 (at least) this is not always true, which may be
            // why MS has withdrawn that particular KB article. It is true that the header is always given the HDS_FULLDRAG
            // style. But even while window style set, the control doesn't always received ITEMCHANGING events.
            // The controlling setting seems to be the Explorer option "Show Window Contents While Dragging"!
            // In the category of "truly bizarre side effects", if the this option is turned on, we will receive
            // ITEMCHANGING events instead of TRACK events. But if it is turned off, we receive lots of TRACK events and
            // only one ITEMCHANGING event at the very end of the process.
            // If we receive HDN_TRACK messages, it's harder to control the resizing process. If we return a result of 1, we
            // cancel the whole drag operation, not just that particular track event, which is clearly not what we want.
            // If we are willing to compile with unsafe code enabled, we can modify the size of the column in place, using the
            // commented out code below. But without unsafe code, the best we can do is allow the user to drag the column to
            // any width, and then spring it back to within bounds once they release the mouse button. UI-wise it's very ugly.
            NativeMethods.NMHEADER nmheader;
            switch (nmhdr.code) {
                case HDN_TRACKA:
                case HDN_TRACKW:
                    nmheader = (NativeMethods.NMHEADER)m.GetLParam(typeof(NativeMethods.NMHEADER));
                    if (nmheader.iItem >= 0 && nmheader.iItem < this.Columns.Count) {
                    //    unsafe {
                    //        NativeMethods.HDITEM *hditem = (NativeMethods.HDITEM*)nmheader.pHDITEM;
                    //        OLVColumn column = this.GetColumn(nmheader.iItem);
                    //        if (hditem->cxy < column.MiniumWidth)
                    //            hditem->cxy = column.MiniumWidth;
                    //        else if (column.MaxiumWidth != -1 && hditem->cxy > column.MaxiumWidth)
                    //            hditem->cxy = column.MaxiumWidth;
                    //    }
                    }
                    break;
                case HDN_ITEMCHANGINGA:
                case HDN_ITEMCHANGINGW:
                    nmheader = (NativeMethods.NMHEADER)m.GetLParam(typeof(NativeMethods.NMHEADER));
                    if (nmheader.iItem >= 0 && nmheader.iItem < this.Columns.Count) {
                        NativeMethods.HDITEM hditem = (NativeMethods.HDITEM)Marshal.PtrToStructure(nmheader.pHDITEM, typeof(NativeMethods.HDITEM));
                        OLVColumn column = this.GetColumn(nmheader.iItem);
                        // Check the mask to see if the width field is valid, and if it is, make sure it's within range
                        if ((hditem.mask & 1) == 1 && (hditem.cxy < column.MinimumWidth || (column.MaximumWidth != -1 && hditem.cxy > column.MaximumWidth))) {
                            m.Result = (IntPtr)1; // prevent the change from happening
                            isMsgHandled = true;
                        }
                    }
                    break;
                default:
                    break;
            }

            return isMsgHandled;
        }        

        #endregion

        #region OLV accessing

        /// <summary>
        /// Return the column at the given index
        /// </summary>
        /// <param name="index">Index of the column to be returned</param>
        /// <returns>An OLVColumn</returns>
        public OLVColumn GetColumn(int index)
        {
            return (OLVColumn)this.Columns[index];
        }

        /// <summary>
        /// Return the column at the given title. 
        /// </summary>
        /// <param name="name">Name of the column to be returned</param>
        /// <returns>An OLVColumn</returns>
        public OLVColumn GetColumn(string name)
        {
            foreach (ColumnHeader column in this.Columns) {
                if (column.Text == name)
                    return (OLVColumn)column;
            }
            return null;
        }

        /// <summary>
        /// Return the item at the given index
        /// </summary>
        /// <param name="name">Index of the item to be returned</param>
        /// <returns>An OLVListItem</returns>
        public OLVListItem GetItem(int index)
        {
            return (OLVListItem)this.Items[index];
        }

        #endregion

        #region Object manipulation

        /// <summary>
        /// Select all rows in the listview
        /// </summary>
        public void SelectAll()
        {
            NativeMethods.SelectAllItems(this);
        }

        /// <summary>
        /// Deselect all rows in the listview
        /// </summary>
        public void DeselectAll()
        {
            NativeMethods.DeselectAllItems(this);
        }

        /// <summary>
		/// Return the model object of the row that is selected or null if there is no selection or more than one selection
		/// </summary>
		/// <returns>Model object or null</returns>
		virtual public object GetSelectedObject()
		{
			if (this.SelectedItems.Count == 1)
				return ((OLVListItem)this.SelectedItems[0]).RowObject;
			else
				return null;
		}

		/// <summary>
		/// Return the model objects of the rows that are selected or an empty collection if there is no selection
		/// </summary>
		/// <returns>ArrayList</returns>
		virtual public ArrayList GetSelectedObjects()
		{
			ArrayList objects = new ArrayList(this.SelectedItems.Count);
			foreach (ListViewItem lvi in this.SelectedItems)
				objects.Add(((OLVListItem)lvi).RowObject);

			return objects;
		}

		/// <summary>
		/// Select the row that is displaying the given model object. All other rows are deselected.
		/// </summary>
		/// <param name="modelObject">The object to be selected or null to deselect all</param>
		virtual public void SelectObject(object modelObject)
		{
            if (this.SelectedItems.Count == 1 && ((OLVListItem)this.SelectedItems[0]).RowObject == modelObject)
                return;

			this.SelectedItems.Clear();

			//TODO: If this is too slow, we could keep a map of model object to ListViewItems
			foreach (ListViewItem lvi in this.Items) {
				if (((OLVListItem)lvi).RowObject == modelObject) {
					lvi.Selected = true;
					break;
				}
			}
		}

		/// <summary>
		/// Select the rows that is displaying any of the given model object. All other rows are deselected.
		/// </summary>
		/// <param name="modelObjects">A collection of model objects</param>
		virtual public void SelectObjects(IList modelObjects)
		{
			this.SelectedItems.Clear();

			//TODO: If this is too slow, we could keep a map of model object to ListViewItems
			foreach (ListViewItem lvi in this.Items) {
				if (modelObjects.Contains(((OLVListItem)lvi).RowObject))
					lvi.Selected = true;
			}
		}

		/// <summary>
		/// Update the ListViewItem with the data from its associated model.
		/// </summary>
		/// <remarks>This method does not resort or regroup the view. It simply updates
		/// the displayed data of the given item</remarks>
		virtual public void RefreshItem(OLVListItem olvi)
		{
			// For some reason, clearing the subitems also wipes out the back color,
			// so we need to store it and then put it back again later
			Color c = olvi.BackColor;
			olvi.SubItems.Clear();
			this.FillInValues(olvi, olvi.RowObject);
			this.SetSubItemImages(olvi.Index, olvi);
			olvi.BackColor = c;
		}

		/// <summary>
		/// Update the rows that are showing the given objects
		/// </summary>
		/// <remarks>This method does not resort or regroup the view.</remarks>
		virtual public void RefreshObject(object modelObject)
		{
			this.RefreshObjects(new object[] {modelObject});
		}

		/// <summary>
		/// Update the rows that are showing the given objects
		/// </summary>
  	    /// <remarks>This method does not resort or regroup the view.</remarks>
		virtual public void RefreshObjects(IList modelObjects)
		{
			foreach (ListViewItem lvi in this.Items) {
				OLVListItem olvi = (OLVListItem)lvi;
				if (modelObjects.Contains(olvi.RowObject))
					this.RefreshItem(olvi);
			}
		}

		/// <summary>
		/// Update the rows that are selected
		/// </summary>
  	    /// <remarks>This method does not resort or regroup the view.</remarks>
		public void RefreshSelectedObjects()
		{
			foreach (ListViewItem lvi in this.SelectedItems) 
                this.RefreshItem((OLVListItem)lvi);
		}

        /// <summary>
        /// Find the given model object within the listview and return its index
        /// </summary>
        /// <param name="modelObject">The model object to be found</param>
        /// <returns>The index of the object. -1 means the object was not present</returns>
        public int IndexOf(Object modelObject)
        {
            for (int i = 0; i < this.Items.Count; i++) {
                if (((OLVListItem)this.Items[i]).RowObject == modelObject)
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Return the ListViewItem that appears immediately after the given item. 
        /// If the given item is null, the first item in the list will be returned.
        /// Return null if the given item is the last item.
        /// </summary>
        /// <param name="item">The item that is before the item that is returned, or null</param>
        /// <returns>A ListViewItem</returns>
        public ListViewItem GetNextItem(ListViewItem itemToFind)
        {
            if (this.ShowGroups) {
                bool isFound = (itemToFind == null);
                foreach (ListViewGroup group in this.Groups) {
                    foreach (ListViewItem lvi in group.Items) {
                        if (isFound)
                            return lvi;
                        isFound = (lvi == itemToFind);
                    }
                }
                return null;
            } else {
                if (this.Items.Count == 0)
                    return null;
                if (itemToFind == null)
                    return this.Items[0];
                if (itemToFind.Index == this.Items.Count - 1)
                    return null;
                return this.Items[itemToFind.Index + 1];
            }
        }

        /// <summary>
        /// Return the ListViewItem that appears immediately before the given item. 
        /// If the given item is null, the last item in the list will be returned.
        /// Return null if the given item is the first item.
        /// </summary>
        /// <param name="item">The item that is before the item that is returned</param>
        /// <returns>A ListViewItem</returns>
        public ListViewItem GetPreviousItem(ListViewItem itemToFind)
        {
            if (this.ShowGroups) {
                ListViewItem previousItem = null;
                foreach (ListViewGroup group in this.Groups) {
                    foreach (ListViewItem lvi in group.Items) {
                        if (lvi == itemToFind)
                            return previousItem;
                        else
                            previousItem = lvi;
                    }
                }
                if (itemToFind == null)
                    return previousItem;
                else
                    return null;
            } else {
                if (this.Items.Count == 0)
                    return null;
                if (itemToFind == null)
                    return this.Items[this.Items.Count - 1];
                if (itemToFind.Index == 0)
                    return null;
                return this.Items[itemToFind.Index - 1];
            }
        }

        #endregion

        #region Freezing

        /// <summary>
        /// Freeze the listview so that it no longer updates itself.
        /// </summary>
        /// <remarks>Freeze()/Unfreeze() calls nest correctly</remarks>
        public void Freeze()
        {
            freezeCount++;
        }

        /// <summary>
        /// Unfreeze the listview. If this call is the outermost Unfreeze(),
        /// the contents of the listview will be rebuilt.
        /// </summary>
        /// <remarks>Freeze()/Unfreeze() calls nest correctly</remarks>
        public void Unfreeze()
        {
            if (freezeCount <= 0)
                return;

            freezeCount--;
            if (freezeCount == 0)
                DoUnfreeze();
        }

        /// <summary>
        /// Do the actual work required when the listview is unfrozen
        /// </summary>
        virtual protected void DoUnfreeze()
        {
            BuildList();
        }

		#endregion

		#region ColumnSorting

		/// <summary>
		/// Event handler for the column click event
		/// </summary>
		protected void HandleColumnClick(object sender, ColumnClickEventArgs e)
		{
			// Toggle the sorting direction on successive clicks on the same column
            if (this.lastSortColumn != null && e.Column == this.lastSortColumn.Index)
                this.lastSortOrder = (this.lastSortOrder == SortOrder.Descending ? SortOrder.Ascending : SortOrder.Descending);
			else 
                this.lastSortOrder = SortOrder.Ascending;

			this.BeginUpdate();
            this.Sort(e.Column);
			this.EndUpdate();
		}

        /// <summary>
        /// Sort the items in the list view by the values in the given column.
        /// If ShowGroups is true, the rows will be grouped by the given column,
        /// otherwise, it will be a straight sort.
        /// </summary>
        /// <param name="columnToSortName">The name of the column whose values will be used for the sorting</param>
        public void Sort(string columnToSortName)
        {
			this.Sort(this.GetColumn(columnToSortName));
        }
        
        /// <summary>
        /// Sort the items in the list view by the values in the given column.
        /// If ShowGroups is true, the rows will be grouped by the given column,
        /// otherwise, it will be a straight sort.
        /// </summary>
        /// <param name="columnToSortIndex">The index of the column whose values will be used for the sorting</param>
        public void Sort(int columnToSortIndex)
        {
            if (columnToSortIndex >= 0 && columnToSortIndex < this.Columns.Count)
            	this.Sort(this.GetColumn(columnToSortIndex));
        }

        delegate void SortInvoker(OLVColumn columnToSort);
        
        /// <summary>
		/// Sort the items in the list view by the values in the given column.
		/// If ShowGroups is true, the rows will be grouped by the given column,
		/// otherwise, it will be a straight sort.
		/// </summary>
		/// <param name="columnToSort">The column whose values will be used for the sorting</param>
        public void Sort(OLVColumn columnToSort)
		{
            if (this.InvokeRequired) {
				this.Invoke(new SortInvoker(this.Sort), new object [] {columnToSort});
				return;
			}

            if (this.Columns.Count < 1)
                return;

            if (columnToSort == null)
            	columnToSort = this.GetColumn(0);

			if (lastSortOrder == SortOrder.None)
				lastSortOrder = this.Sorting;

            if (this.ShowGroups)
                this.BuildGroups(columnToSort);
            else if (this.CustomSorter != null)
                this.CustomSorter(columnToSort, lastSortOrder);
			else 
                this.ListViewItemSorter = new ColumnComparer(columnToSort, lastSortOrder, this.SecondarySortColumn, this.SecondarySortOrder);

            if (this.ShowSortIndicators)
    			this.ShowSortIndicator(columnToSort, lastSortOrder);
			
			if (this.UseAlternatingBackColors && this.View == View.Details)
				PrepareAlternateBackColors();

            this.lastSortColumn = columnToSort;
		}

		/// <summary>
		/// Put a sort indicator next to the text of the given given column
		/// </summary>
		/// <param name="columnToSort">The column to be marked</param>
		/// <param name="sortOrder">The sort order in effect on that column</param>
		protected void ShowSortIndicator(OLVColumn columnToSort, SortOrder sortOrder)
		{
            int imageIndex = -1;

            if (!NativeMethods.HasBuiltinSortIndicators()) {
                // If we can't use built image, we have to make and then locate the index of the 
                // sort indicator we want to use. SortOrder.None doesn't show an image.
                MakeSortIndictorImages();
                if (sortOrder == SortOrder.Ascending)
                    imageIndex = this.SmallImageList.Images.IndexOfKey(SORT_INDICATOR_UP_KEY);
                else if (sortOrder == SortOrder.Descending)
                    imageIndex = this.SmallImageList.Images.IndexOfKey(SORT_INDICATOR_DOWN_KEY);
            }
				
            // Set the image for each column
            for (int i = 0; i < this.Columns.Count; i++) {
                if (i == columnToSort.Index)
                    NativeMethods.SetColumnImage(this, i, sortOrder, imageIndex);
                else
                    NativeMethods.SetColumnImage(this, i, SortOrder.None, -1);
            }
		}
		
		private const string SORT_INDICATOR_UP_KEY = "sort-indicator-up";
		private const string SORT_INDICATOR_DOWN_KEY = "sort-indicator-down";
		
		/// <summary>
		/// If the sort indicator images don't already exist, this method will make and install them
		/// </summary>
		protected void MakeSortIndictorImages()
		{
			ImageList il = this.SmallImageList;
            if (il == null) {
                il = new ImageList();
                il.ImageSize = new Size(16, 16);
            }

			// This arrangement of points works well with (16,16) images, and OK with others
			int midX = il.ImageSize.Width / 2;
			int midY = (il.ImageSize.Height / 2) - 1;
			int deltaX = midX - 2;
			int deltaY = deltaX / 2;

			if (il.Images.IndexOfKey(SORT_INDICATOR_UP_KEY) == -1) {
				Point pt1 = new Point(midX - deltaX, midY + deltaY);
				Point pt2 = new Point(midX,          midY - deltaY - 1);
				Point pt3 = new Point(midX + deltaX, midY + deltaY);
                il.Images.Add(SORT_INDICATOR_UP_KEY, this.MakeTriangleBitmap(il.ImageSize, new Point[] { pt1, pt2, pt3 }));
            } 
			
			if (il.Images.IndexOfKey(SORT_INDICATOR_DOWN_KEY) == -1) {
				Point pt1 = new Point(midX - deltaX, midY - deltaY);
				Point pt2 = new Point(midX,          midY + deltaY);
				Point pt3 = new Point(midX + deltaX, midY - deltaY);
				il.Images.Add(SORT_INDICATOR_DOWN_KEY, this.MakeTriangleBitmap(il.ImageSize, new Point[] { pt1, pt2, pt3 }));
			}

            this.SmallImageList = il;
		}

        private Bitmap MakeTriangleBitmap(Size sz, Point[] pts)
        {
            Bitmap bm = new Bitmap(sz.Width, sz.Height);
            Graphics g = Graphics.FromImage(bm);
            g.FillPolygon(new SolidBrush(Color.Gray), pts);
            return bm;
        }

        #endregion

        #region Utilities

        /// <summary>
		/// Fill in the given OLVListItem with values of the given row
		/// </summary>
        /// <param name="lvi">the OLVListItem that is to be stuff with values</param>
		/// <param name="rowObject">the model object from which values will be taken</param>
		protected void FillInValues(OLVListItem lvi, object rowObject)
		{
			if (this.Columns.Count == 0)
				return;
            
			OLVColumn column = this.GetColumn(0);
			lvi.Text = column.GetStringValue(rowObject);
            lvi.ImageSelector = column.GetImage(rowObject);
           
			for (int i=1; i<this.Columns.Count; i++)
			{
				column = this.GetColumn(i);
				lvi.SubItems.Add(new OLVListSubItem(column.GetStringValue(rowObject),
				                                    column.GetImage(rowObject)));
			}

            // Give the row formatter a chance to mess with the item
            if (this.RowFormatter != null)
                this.RowFormatter(lvi);
		}
		
		/// <summary>
		/// Setup all subitem images on all rows
		/// </summary>
		protected void SetAllSubItemImages()
		{
			if (!this.ShowImagesOnSubItems || this.OwnerDraw)
				return;
			
			this.ForceSubItemImagesExStyle();

			for (int rowIndex=0; rowIndex<this.Items.Count; rowIndex++)
				SetSubItemImages(rowIndex, (OLVListItem)this.Items[rowIndex]);
		}
		
		/// <summary>
		/// Tell the underlying list control which images to show against the subitems
		/// </summary>
		/// <param name="rowIndex">the index at which the item occurs</param>
		/// <param name="item">the item whose subitems are to be set</param>
		/// <remarks>When we are owner drawing, this method is a no-op since
        /// we draw the images ourself.</remarks>
		protected void SetSubItemImages(int rowIndex, OLVListItem item)
		{
			if (!this.ShowImagesOnSubItems || this.OwnerDraw)
				return;
			
			int imageIndex;
			
			for (int i=1; i<item.SubItems.Count; i++)
			{
				imageIndex = this.GetActualImageIndex(((OLVListSubItem)item.SubItems[i]).ImageSelector);
				if (imageIndex != -1)
					this.SetSubItemImage(rowIndex, i, imageIndex);
			}
		}

        /// <summary>
		/// Prepare the listview to show alternate row backcolors
		/// </summary>
		/// <remarks>When groups are shown, the ordering of list items is different.
		/// In a straight list, <code>lvi.Index</code> is the display index, and can be used to determine
		/// whether the row should be colored. But when organised by groups, <code>lvi.Index</code> is not
		/// useable because it still refers to the position in the overall list, not the display order.
		/// So we have to walk each group and then the items in each group, counting as we go, to know
		/// at which row an item will be shown (and therefore how it should be back colored).</remarks>
		virtual protected void PrepareAlternateBackColors ()
		{
			Color rowBackColor = this.AlternateRowBackColorOrDefault;

			if (this.ShowGroups) {
				int i = 0;
				foreach (ListViewGroup group in this.Groups) {
					foreach (ListViewItem lvi in group.Items) {
						if (i % 2 == 0)
							lvi.BackColor = this.BackColor;
						else
							lvi.BackColor = rowBackColor;
						i++;
					}
				}
			} else {
				foreach (ListViewItem lvi in this.Items)
				{
					if (lvi.Index % 2 == 0)
						lvi.BackColor = this.BackColor;
					else
						lvi.BackColor = rowBackColor;
				}
			}
        }
        
        /// <summary>
        /// Convert the given image selector to an index into our image list.
        /// Return -1 if that's not possible
        /// </summary>
        /// <param name="imageSelector"></param>
        /// <returns>Index of the image in the imageList, or -1</returns>
        public int GetActualImageIndex(Object imageSelector)
        {
        	if (imageSelector == null)
        		return -1;
        	
        	if (imageSelector is Int32)
        		return (int)imageSelector;
 
        	if (imageSelector is String && this.SmallImageList != null)
        		return this.SmallImageList.Images.IndexOfKey((String)imageSelector);
        	
        	return -1;
        }		
        
		/// <summary>
		/// Make sure the ListView has the extended style that says to display subitem images.
		/// </summary>
		/// <remarks>This method must be called after any .NET call that update the extended styles
		/// since they seem to erase this setting.</remarks>
		protected void ForceSubItemImagesExStyle ()
		{
			NativeMethods.ForceSubItemImagesExStyle(this);
		}

		/// <summary>
		/// For the given item and subitem, make it display the given image
		/// </summary>
		/// <param name="itemIndex">row number (0 based)</param>
		/// <param name="subItemIndex">subitem (0 is the item itself)</param>
		/// <param name="imageIndex">index into the image list</param>
		protected void SetSubItemImage(int itemIndex, int subItemIndex, int imageIndex)
		{
			NativeMethods.SetSubItemImage(this, itemIndex, subItemIndex, imageIndex);
		}
		
		#endregion

        #region ISupportInitialize Members

        void ISupportInitialize.BeginInit()
        {
            this.Frozen = true;
        }

        void ISupportInitialize.EndInit()
        {
            this.Frozen = false;
        }

        #endregion

        #region Image list manipulation

        /// <summary>
        /// Update our externally visible image list so it holds the same images as our shadow list, but sized correctly
        /// </summary>
        private void SetupExternalImageList()
        {
            // If a row height hasn't been set, or an image list has been give which is the required size, just assign it
            if (rowHeight == -1 || (this.shadowedImageList != null && this.shadowedImageList.ImageSize.Height == rowHeight))
                base.SmallImageList = this.shadowedImageList;
            else
                base.SmallImageList = this.MakeResizedImageList(rowHeight, shadowedImageList);
        }

        /// <summary>
        /// Return a copy of the given source image list, where each image has been resized to be height x height in size.
        /// If source is null, an empty image list of the given size is returned
        /// </summary>
        /// <param name="height">Height and width of the new images</param>
        /// <param name="source">Source of the images (can be null)</param>
        /// <returns>A new image list</returns>
        private ImageList MakeResizedImageList(int height, ImageList source)
        {
            // Return a copy of the source image list, where each image has been resized to the given width and height
            ImageList il = new ImageList();
            il.ImageSize = new Size(height, height);

            // If there's nothing to copy, just return the new list
            if (source == null)
                return il;

            il.TransparentColor = source.TransparentColor;
            il.ColorDepth = source.ColorDepth;

            // Fill the imagelist with resized copies from the source
            for (int i = 0; i < source.Images.Count; i++) {
                Bitmap bm = this.MakeResizedImage(height, source.Images[i], source.TransparentColor);
                il.Images.Add(bm);
            }

            // Give each image the same key it has in the original
            foreach (String key in source.Images.Keys) {
                il.Images.SetKeyName(source.Images.IndexOfKey(key), key);
            }

            return il;
        }

        /// <summary>
        /// Return a bitmap of the given height x height, which shows the given image, centred.
        /// </summary>
        /// <param name="height">Height and width of new bitmap</param>
        /// <param name="image">Image to be centred</param>
        /// <param name="transparent">The background color</param>
        /// <returns>A new bitmap</returns>
        private Bitmap MakeResizedImage(int height, Image image, Color transparent)
        {
            Bitmap bm = new Bitmap(height, height);
            Graphics g = Graphics.FromImage(bm);
            g.Clear(transparent);
            int x = Math.Max(0, (bm.Size.Width - image.Size.Width) / 2);
            int y = Math.Max(0, (bm.Size.Height - image.Size.Height) / 2);
            g.DrawImage(image, x, y, image.Size.Width, image.Size.Height);
            return bm;
        }
        
        #endregion

        #region Owner drawing

        /// <summary>
        /// Owner draw the column header
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDrawColumnHeader(DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
            base.OnDrawColumnHeader(e);
        }

        /// <summary>
        /// Owner draw the item
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDrawItem(DrawListViewItemEventArgs e)
        {
            e.DrawDefault = (this.View != View.Details);   
            base.OnDrawItem(e);
        }

        int[] columnRightEdge = new int[128]; // will anyone ever want more than 128 columns??

        /// <summary>
        /// Owner draw a single subitem
        /// </summary>
        /// <param name="e"></param>
        protected override void OnDrawSubItem(DrawListViewSubItemEventArgs e)
        {
        	// Get the special renderer for this column. 
            // If there isn't one, don't draw anything.
            OLVColumn column = this.GetColumn(e.ColumnIndex);
            if (column.RendererDelegate == null) 
                return;
            
            // Calculate where the subitem should be drawn
            // It should be as simple as 'e.Bounds', but it isn't :-(

            // There seems to be a bug in .NET where the left edge of the bounds of subitem 0
            // is always 0. This is normally what is required, but it's wrong when
            // someone drags column 0 to somewhere else in the listview. We could
            // drop down into Windows-ville and use LVM_GETSUBITEMRECT, but I'm specifically
            // avoiding Windows dependencies -- Mono-ready, I guess.
            // So, we keep track of the right edge of all columns, and when subitem 0
            // isn't being shown at column 0, we make its left edge to be the right
            // edge of the previous column plus a little bit.
            //TODO: Replace with LVM_GETSUBITEMRECT
            Rectangle r = e.Bounds;
            if (e.ColumnIndex == 0 && e.Header.DisplayIndex != 0) {
                r.X = this.columnRightEdge[e.Header.DisplayIndex - 1] + 1;
            } else
                //TODO: Check the size of columnRightEdge and dynamically reallocate?
                this.columnRightEdge[e.Header.DisplayIndex] = e.Bounds.Right;

            // Optimize drawing by only redrawing subitems that touch the area that was damaged
            if (!r.IntersectsWith(this.lastUpdateRectangle)) {
                return;
            }

            // Get a graphics context for the renderer to use. 
            // But we have more complications. Virtual lists have a nasty habit of drawing column 0
            // whenever there is any mouse move events over a row, and doing it in an un-double buffered manner,
            // which results in nasty flickers! There are also some unbuffered draw when a mouse is first
            // hovered over column 0 of a normal row. So, to avoid all complications,
            // we always manually double-buffer the drawing.
            Graphics g;
            BufferedGraphics buffer = null;
            bool avoidFlickerMode = true; // set this to false to see the probems with flicker
            if (avoidFlickerMode) {
                buffer = BufferedGraphicsManager.Current.Allocate(e.Graphics, r);
                g = buffer.Graphics;
            } else
                g = e.Graphics;

            // Finally, give the renderer a chance to draw something
            Object row = ((OLVListItem)e.Item).RowObject;
            column.RendererDelegate(e, g, r, row);

            if (buffer != null) {
                buffer.Render();
                buffer.Dispose();
            }
        }

        #endregion

		#region Design Time

		/// <summary>
		/// This class works in conjunction with the OLVColumns property to allow OLVColumns
		/// to be added to the ObjectListView.
		/// </summary>
		internal class OLVColumnCollectionEditor : System.ComponentModel.Design.CollectionEditor
		{
			public OLVColumnCollectionEditor(Type t)
				: base(t)
			{
			}

			protected override Type CreateCollectionItemType()
			{
				return typeof(OLVColumn);
			}
		}

		/// <summary>
		/// Return Columns for this list. We hide the original so we can associate
		/// a specialised editor with it.
		/// </summary>
		[Editor(typeof(OLVColumnCollectionEditor), typeof(System.Drawing.Design.UITypeEditor))]
		new public ListView.ColumnHeaderCollection Columns {
			get {
				return base.Columns;
			}
		}

		#endregion

		private IEnumerable	objects; // the collection of objects on show
        private OLVColumn lastSortColumn; // which column did we last sort by
        private bool showImagesOnSubItems; // should we try to show images on subitems?
        private bool showSortIndicators; // should we show sort indicators in the column headers?
		private SortOrder lastSortOrder; // which direction did we last sort
		private bool showItemCountOnGroups; // should we show items count in group labels?
        private string groupWithItemCountFormat; // when a group title has an item count, how should the label be formatted?
        private string groupWithItemCountSingularFormat; // when a group title has an item count of 1, how should the label be formatted?
		private bool useAlternatingBackColors; // should we use different colors for alternate lines?
		private Color alternateRowBackColor; // what color background should alternate lines have?
        private SortDelegate customSorter; // callback for handling custom sort by column processing
        private Rectangle lastUpdateRectangle; // remember the update rect from the last WM_PAINT msg
    }

    /// <summary>
    /// Wrapper for all native method calls on ListView controls
    /// </summary>
    internal class NativeMethods {

        private const int LVM_FIRST                    = 0x1000;
        private const int LVM_GETHEADER                = LVM_FIRST + 31; 
        private const int LVM_SETITEMSTATE             = LVM_FIRST + 43; 
        private const int LVM_SETEXTENDEDLISTVIEWSTYLE = LVM_FIRST + 54; 
        private const int LVM_SETITEM                  = LVM_FIRST + 76; 
        private const int LVM_GETCOLUMN                = LVM_FIRST + 95; 
        private const int LVM_SETCOLUMN                = LVM_FIRST + 96;
		
        private const int LVS_EX_SUBITEMIMAGES   = 0x0002;
		
		private const int LVIF_TEXT              = 0x0001;
		private const int LVIF_IMAGE             = 0x0002;
		private const int LVIF_PARAM             = 0x0004;
		private const int LVIF_STATE             = 0x0008;
		private const int LVIF_INDENT            = 0x0010;
		private const int LVIF_NORECOMPUTE       = 0x0800;
		
		private const int LVCF_FMT               = 0x0001;
		private const int LVCF_WIDTH             = 0x0002;
		private const int LVCF_TEXT              = 0x0004;
		private const int LVCF_SUBITEM           = 0x0008;
		private const int LVCF_IMAGE             = 0x0010;
		private const int LVCF_ORDER             = 0x0020;
		private const int LVCFMT_LEFT            = 0x0000;
		private const int LVCFMT_RIGHT           = 0x0001;
		private const int LVCFMT_CENTER          = 0x0002;
		private const int LVCFMT_JUSTIFYMASK     = 0x0003;
		
		private const int LVCFMT_IMAGE           = 0x0800;
		private const int LVCFMT_BITMAP_ON_RIGHT = 0x1000;
		private const int LVCFMT_COL_HAS_IMAGES  = 0x8000;

        private const int HDM_FIRST = 0x1200;
        private const int HDM_HITTEST = HDM_FIRST + 6;
        private const int HDM_GETITEM = HDM_FIRST + 11;
        private const int HDM_SETITEM = HDM_FIRST + 12;

        private const int HDI_WIDTH = 0x0001;
        private const int HDI_TEXT = 0x0002;
        private const int HDI_FORMAT = 0x0004;
        private const int HDI_BITMAP = 0x0010;
        private const int HDI_IMAGE = 0x0020;

        private const int HDF_LEFT = 0x0000;
        private const int HDF_RIGHT = 0x0001;
        private const int HDF_CENTER = 0x0002;
        private const int HDF_JUSTIFYMASK = 0x0003;
        private const int HDF_RTLREADING = 0x0004;
        private const int HDF_STRING = 0x4000;
        private const int HDF_BITMAP = 0x2000;
        private const int HDF_BITMAP_ON_RIGHT = 0x1000;
        private const int HDF_IMAGE = 0x0800;
        private const int HDF_SORTUP = 0x0400;
        private const int HDF_SORTDOWN = 0x0200;

		[StructLayout(LayoutKind.Sequential,CharSet=CharSet.Auto)]
		private struct LVITEM
		{
			public int     mask;
			public int     iItem;
			public int     iSubItem;
			public int     state;
			public int     stateMask;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string  pszText;
			public int     cchTextMax;
			public int     iImage;
			public int     lParam;
			// These are available in Common Controls >= 0x0300
			public int     iIndent;
			// These are available in Common Controls >= 0x056
			public int     iGroupId;
			public int     cColumns;
			public IntPtr  puColumns;
		};
		
		[StructLayout(LayoutKind.Sequential,CharSet=CharSet.Auto)]
		private struct LVCOLUMN 
		{
		    public int mask; 
		    public int fmt; 
		    public int cx; 
			[MarshalAs(UnmanagedType.LPTStr)]
		    public string pszText; 
		    public int cchTextMax; 
		    public int iSubItem; 
			// These are available in Common Controls >= 0x0300
		    public int iImage;
		    public int iOrder;
		};

        /// <summary>
        /// Notify message header structure.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct NMHDR
        {
            public IntPtr hwndFrom;
            public int idFrom;
            public int code;
        } 

        [StructLayout(LayoutKind.Sequential)]
        public struct NMHEADER
        {
            public NMHDR nhdr;
            public int iItem;
            public int iButton;
            public IntPtr pHDITEM;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HDITEM
        {
            public int mask;
            public int cxy;
            public IntPtr pszText;
            public IntPtr hbm;
            public int cchTextMax;
            public int fmt;
            public IntPtr lParam;
            public int iImage;
            public int iOrder;
            //if (_WIN32_IE >= 0x0500)
            public int type;
            public IntPtr pvFilter;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class HDHITTESTINFO
        {
            public int pt_x;
            public int pt_y;
            public int flags;
            public int iItem;
        }

		// Various flavours of SendMessage: plain vanilla,
		// others pass references to various structures
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
		[DllImport("user32.dll", EntryPoint="SendMessage", CharSet=CharSet.Auto)]
		private static extern IntPtr SendMessageLVItem(IntPtr hWnd, int msg, int wParam, ref LVITEM lvi);
        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessageLVColumn(IntPtr hWnd, int msg, int wParam, ref LVCOLUMN lvc);
        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessageHDItem(IntPtr hWnd, int msg, int wParam, ref HDITEM hdi);
        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageHDHITTESTINFO(IntPtr hWnd, int Msg, IntPtr wParam, [In, Out] HDHITTESTINFO lParam);

		/// <summary>
		/// Make sure the ListView has the extended style that says to display subitem images.
		/// </summary>
		/// <remarks>This method must be called after any .NET call that update the extended styles
		/// since they seem to erase this setting.</remarks>
		/// <param name="list">The listview to send a message to</param>
		public static void ForceSubItemImagesExStyle (ListView list)
		{
			SendMessage(list.Handle, LVM_SETEXTENDEDLISTVIEWSTYLE, LVS_EX_SUBITEMIMAGES, LVS_EX_SUBITEMIMAGES);
		}

        /// <summary>
        /// For the given item and subitem, make it display the given image
        /// </summary>
        /// <param name="list">The listview to send a message to</param>
        /// <param name="itemIndex">row number (0 based)</param>
        /// <param name="subItemIndex">subitem (0 is the item itself)</param>
        /// <param name="imageIndex">index into the image list</param>
        public static void SetSubItemImage(ListView list, int itemIndex, int subItemIndex, int imageIndex)
        {
            LVITEM lvItem = new LVITEM();
            lvItem.mask = LVIF_IMAGE;
            lvItem.iItem = itemIndex;
            lvItem.iSubItem = subItemIndex;
            lvItem.iImage = imageIndex;
            SendMessageLVItem(list.Handle, LVM_SETITEM, 0, ref lvItem);
        }

		/// <summary>
		/// Setup the given column of the listview to show the given image to the right of the text.
		/// If the image index is -1, any previous image is cleared
		/// </summary>
		/// <param name="list">The listview to send a message to</param>
		/// <param name="columnIndex">Index of the column to modifiy</param>
		/// <param name="imageIndex">Index into the small image list</param>
		public static void SetColumnImage(ListView list, int columnIndex, SortOrder order, int imageIndex)
		{
            IntPtr hdrCntl = NativeMethods.GetHeaderControl(list);
            if (hdrCntl.ToInt32() == 0)
                return;

            HDITEM item = new HDITEM();
            item.mask = HDI_FORMAT;
            IntPtr result = SendMessageHDItem(hdrCntl, HDM_GETITEM, columnIndex, ref item);

            item.fmt &= ~(HDF_SORTUP | HDF_SORTDOWN | HDF_IMAGE | HDF_BITMAP_ON_RIGHT);

            if (NativeMethods.HasBuiltinSortIndicators()) {
                if (order == SortOrder.Ascending)
                    item.fmt |= HDF_SORTUP;
                if (order == SortOrder.Descending)
                    item.fmt |= HDF_SORTDOWN;
            } else {
                item.mask |= HDI_IMAGE;
                item.fmt |= (HDF_IMAGE | HDF_BITMAP_ON_RIGHT);
                item.iImage = imageIndex;
            }

            result = SendMessageHDItem(hdrCntl, HDM_SETITEM, columnIndex, ref item);
		}

        /// <summary>
        /// Does this version of the operating system have builtin sort indicators?
        /// </summary>
        /// <returns>Are there builtin sort indicators</returns>
        /// <remarks>XP and later have these</remarks>
        public static bool HasBuiltinSortIndicators()
        {
            return OSFeature.Feature.GetVersionPresent(OSFeature.Themes) != null;
        }

        [DllImport("user32.dll", EntryPoint = "GetUpdateRect", CharSet = CharSet.Auto)]
        private static extern IntPtr GetUpdateRectInternal(IntPtr hWnd, ref Rectangle r, bool eraseBackground);

        /// <summary>
        /// Return the bounds of the update region on the given control.
        /// </summary>
        /// <remarks>The BeginPaint() system call validates the update region, effectively wiping out this information.
        /// So this call has to be made before the BeginPaint() call.</remarks>
        /// <param name="cntl">The control whose update region is be calculated</param>
        /// <returns>A rectangle</returns>
        public static Rectangle GetUpdateRect(Control cntl)
        {
            Rectangle r = new Rectangle();
            GetUpdateRectInternal(cntl.Handle, ref r, false);
            return r;
        }

        [DllImport("user32.dll", EntryPoint = "ValidateRect", CharSet = CharSet.Auto)]
        private static extern IntPtr ValidatedRectInternal(IntPtr hWnd, ref Rectangle r);

        /// <summary>
        /// Validate an area of the given control. A validated area will not be repainted at the next redraw.
        /// </summary>
        /// <param name="cntl">The control to be validated</param>
        /// <param name="r">The area of the control to be validated</param>
        public static void ValidateRect(Control cntl, Rectangle r)
        {
            ValidatedRectInternal(cntl.Handle, ref r);
        }

        /// <summary>
        /// Select all rows on the given listview
        /// </summary>
        /// <param name="list">The listview whose items are to be selected</param>
        public static void SelectAllItems(ListView list)
        {
            NativeMethods.SetItemState(list, -1, 2, 2);
        }

        /// <summary>
        /// Deselect all rows on the given listview
        /// </summary>
        /// <param name="list">The listview whose items are to be deselected</param>
        public static void DeselectAllItems(ListView list)
        {
            NativeMethods.SetItemState(list, -1, 2, 0);
        }

        /// <summary>
        /// Set the item state on the given item
        /// </summary>
        /// <param name="list">The listview whose item's state is to be changed</param>
        /// <param name="itemIndex">The index of the item to be changed</param>
        /// <param name="mask">Which bits of the value are to be set?</param>
        /// <param name="value">The value to be set</param>
        public static void SetItemState(ListView list, int itemIndex, int mask, int value)
        {
            LVITEM lvItem = new LVITEM();
            lvItem.stateMask = mask;
            lvItem.state = value;
            SendMessageLVItem(list.Handle, LVM_SETITEMSTATE, itemIndex, ref lvItem);
        }

        /// <summary>
        /// Return the handle to the header control on the given list
        /// </summary>
        /// <param name="list">The listview whose header control is to be returned</param>
        /// <returns>The handle to the header control</returns>
        public static IntPtr GetHeaderControl(ListView list)
        {
            return SendMessage(list.Handle, LVM_GETHEADER, 0, 0);
        }
 
        /// <summary>
        /// Return the index of the divider under the given point. Return -1 if no divider is under the pt
        /// </summary>
        /// <param name="list">The list we are interested in</param>
        /// <param name="pt">The client co-ords</param>
        /// <returns>The index of the divider under the point, or -1 if no divider is under that point</returns>
        public static int GetDividerUnderPoint(IntPtr handle, Point pt)
        {
            const int HHT_ONDIVIDER = 4;

            HDHITTESTINFO testInfo = new HDHITTESTINFO();
            testInfo.pt_x = pt.X;
            testInfo.pt_y = pt.Y;
            IntPtr result = NativeMethods.SendMessageHDHITTESTINFO(handle, HDM_HITTEST, IntPtr.Zero, testInfo);
            if ((testInfo.flags & HHT_ONDIVIDER) != 0)
                return result.ToInt32();
            else
                return -1;
        }
    }
    
	/// <summary>
	/// A virtual object list view operates in virtual mode, that is, it only gets model objects for
	/// a row when it is needed. This gives it the ability to handle very large numbers of rows with
	/// minimal resources.
	/// </summary>
	/// <remarks><para>A listview is not a great user interface for a large number of items. But if you've
	/// ever wanted to have a list with 10 million items, go ahead, knock yourself out.</para>
    /// <para>Virtual lists can never iterate their contents. That would defeat the whole purpose.</para>
    /// <para>Given the above, grouping and sorting are not possible on virtual lists. But if the backing data store has
    /// a sorting mechanism, a CustomSorter can be installed which will be called when the sorting is required.</para>
    /// <para>For the same reason, animate GIFs should not be used in virtual lists. Animated GIFs require some state
    /// information to be stored for each animation, but virtual lists specifically do not keep any state information.
    /// You really do not want to keep state information for 10 million animations!</para>
    /// </remarks>
	public class VirtualObjectListView : ObjectListView
	{
        /// <summary>
        /// Create a VirtualObjectListView
        /// </summary>
		public VirtualObjectListView()
			: base()
		{
			this.VirtualMode = true;
			this.RetrieveVirtualItem += new RetrieveVirtualItemEventHandler(this.HandleRetrieveVirtualItem);

            // Install a null custom sorter to turn off sorting. Who wants to fetch and sort 10 million items?
            this.CustomSorter = delegate(OLVColumn column, SortOrder sortOrder) { };
		}

		#region Public Properties

		/// <summary>
		/// This delegate is used to fetch a rowObject, given it's index within the list
		/// </summary>
		public RowGetterDelegate RowGetter {
			get { return rowGetter; }
			set { rowGetter = value; }
		}

		#endregion

		#region Object manipulation

		/// <summary>
		/// Return the model object of the row that is selected or null if there is no selection or more than one selection
		/// </summary>
		/// <returns>Model object or null</returns>
		override public object GetSelectedObject()
		{
			if (this.SelectedIndices.Count == 1)
				return this.GetRowObjectAt(this.SelectedIndices[0]);
			else
				return null;
		}

		/// <summary>
		/// Return the model objects of the rows that are selected or an empty collection if there is no selection
		/// </summary>
		/// <returns>ArrayList</returns>
        /// <remarks>Be careful with this method! Virtual lists normally have a large number of objects.
        /// If the user selects all of them, this method will try to return all of them -- even if there
        /// are 10 million of them!</remarks>
		override public ArrayList GetSelectedObjects()
		{
			ArrayList objects = new ArrayList(this.SelectedIndices.Count);
			foreach (int index in this.SelectedIndices)
				objects.Add(this.GetRowObjectAt(index));

			return objects;
		}

		/// <summary>
		/// Select the row that is displaying the given model object.
		/// This does nothing in virtual lists.
		/// </summary>
		/// <remarks>This is a no-op for virtual lists, since there is no way to map the model
		/// object back to the ListViewItem that represents it.</remarks>
		/// <param name="modelObject">The object that gave data</param>
		override public void SelectObject(object modelObject)
		{
			// do nothing
		}

		/// <summary>
		/// Select the rows that is displaying any of the given model object.
		/// This does nothing in virtual lists.
		/// </summary>
		/// <remarks>This is a no-op for virtual lists, since there is no way to map the model
		/// objects back to the ListViewItem that represents them.</remarks>
		/// <param name="modelObjects">A collection of model objects</param>
		override public void SelectObjects(IList modelObjects)
		{
			// do nothing
		}

        /// <summary>
        /// Update the rows that are showing the given objects
        /// </summary>
        /// <remarks>This is a no-op for virtual lists, since there is no way to map the model
        /// objects back to the ListViewItem that represents them.</remarks>
        override public void RefreshObjects(IList modelObjects)
        {
            // do nothing
        }

		#endregion

        /// <summary>
        /// Invalidate any cached information when we rebuild the list.
        /// </summary>
        public override void BuildList(bool shouldPreserveSelection)
        {
            this.lastRetrieveVirtualItemIndex = -1;

            // Virtual lists cannot preserve the selection because they cannot map a model
            // object to a row.
            base.BuildList(false);
        }

		/// <summary>
		/// Prepare the listview to show alternate row backcolors
		/// </summary>
        /// <remarks>Alternate colored backrows can't be handle in the same way as our base class.
		/// With virtual lists, they are handled at RetrieveVirtualItem time.</remarks>
        protected override void PrepareAlternateBackColors ()
		{
            // do nothing
		}

		#region Event handlers

        /// <summary>
        /// Handle a RetrieveVirtualItem
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		protected void HandleRetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
		{
			// .NET 2.0 seems to generate a lot of these events. Before drawing *each* sub-item,
			// this event is triggered 4-8 times for the same index. So we save lots of CPU time
			// by caching the last result.
			if (this.lastRetrieveVirtualItemIndex != e.ItemIndex) {
				this.lastRetrieveVirtualItemIndex = e.ItemIndex;
				this.lastRetrieveVirtualItem = this.MakeListViewItem(e.ItemIndex);
			}
			e.Item = this.lastRetrieveVirtualItem;
		}

        /// <summary>
        /// Create a OLVListItem for given row index
        /// </summary>
        /// <param name="itemIndex">The index of the row that is needed</param>
        /// <returns>An OLVListItem</returns>
		protected OLVListItem MakeListViewItem(int itemIndex)
		{
			OLVListItem olvi = new OLVListItem(this.GetRowObjectAt(itemIndex));
			this.FillInValues(olvi, olvi.RowObject);
            if (this.UseAlternatingBackColors) {
                if (this.View == View.Details && itemIndex % 2 == 1)
                    olvi.BackColor = this.AlternateRowBackColorOrDefault;
                else
                    olvi.BackColor = this.BackColor;
            }
			this.SetSubItemImages(itemIndex, olvi);
			return olvi;
		}

        /// <summary>
        /// Return the row object for the given row index
        /// </summary>
        /// <param name="index">index of the row whose object is to be fetched</param>
        /// <returns>A model object or null if no delegate is installed</returns>
        protected object GetRowObjectAt(int index)
        {
            if (this.RowGetter == null)
                return null;
            else
                return this.RowGetter(index);
        }

		#endregion

		#region Variable declaractions

		private RowGetterDelegate rowGetter;
		private int lastRetrieveVirtualItemIndex = -1;
		private OLVListItem lastRetrieveVirtualItem;

		#endregion
	}

	/// <summary>
	/// A DataListView is a ListView that can be bound to a datasource (which would normally be a DataTable or DataView).
	/// </summary>
	/// <remarks>
    /// <para>This listview keeps itself in sync with its source datatable by listening for change events.</para>
	/// <para>If the listview has no columns when given a data source, it will automatically create columns to show all of the datatables columns.
	/// This will be only the simplest view of the world, and would look more interesting with a few delegates installed.</para>
	/// <para>This listview will also automatically generate missing aspect getters to fetch the values from the data view.</para>
	/// <para>Changing data sources is possible, but error prone. Before changing data sources, the programmer is responsible for modifying/resetting
	/// the column collection to be valid for the new data source.</para>
	/// </remarks>
	public class DataListView : ObjectListView
	{
        /// <summary>
        /// Make a DataListView
        /// </summary>
		public DataListView() : base ()
		{
		}

        #region Public Properties

        /// <summary>
        /// Get or set the DataSource that will be displayed in this list view.
        /// </summary>
        /// <remarks>The DataSource should implement either <see cref="IList"/>, <see cref="IBindingList"/>, 
        /// or <see cref="IListSource"/>. Some common examples are the following types of objects:
        /// <list type="unordered">
        /// <item><see cref="DataView"/></item>
        /// <item><see cref="DataTable"/></item>
        /// <item><see cref="DataSet"/></item>
        /// <item><see cref="DataViewManager"/></item>
        /// <item><see cref="BindingSource"/></item>
        /// </list>
        /// <para>When binding to a list container (i.e. one that implements the
        /// <see cref="IListSource"/> interface, such as <see cref="DataSet"/>)
        /// you must also set the <see cref="DataMember"/> property in order
        /// to identify which particular list you would like to display. You
        /// may also set the <see cref="DataMember"/> property even when
        /// DataSource refers to a list, since <see cref="DataMember"/> can
        /// also be used to navigate relations between lists.</para>
        /// </remarks>
        [Category("Data"),
        TypeConverter("System.Windows.Forms.Design.DataSourceConverter, System.Design")]
        public Object DataSource
        {
            get { return dataSource; }
            set {
                //THINK: Should we only assign it if it is changed?
                //if (dataSource != value) {
                    dataSource = value;
                    this.RebindDataSource(true);
                //}
            }
        }
        private Object dataSource;

        /// <summary>
        /// Gets or sets the name of the list or table in the data source for which the DataListView is displaying data.
        /// </summary>
        /// <remarks>If the data source is not a DataSet or DataViewManager, this property has no effect</remarks>
        [Category("Data"),
         Editor("System.Windows.Forms.Design.DataMemberListEditor, System.Design", typeof(UITypeEditor)),
         DefaultValue("")]
        public string DataMember
        {
            get { return dataMember; }
            set {
                if (dataMember != value) {
                    dataMember = value;
                    RebindDataSource();
                }
            }
        }
        private string dataMember = "";

        #endregion

        #region Initialization

        private CurrencyManager currencyManager = null;

        /// <summary>
        /// Our data source has changed. Figure out how to handle the new source
        /// </summary>
        protected void RebindDataSource()
        {
            RebindDataSource(false);
        }

        /// <summary>
        /// Our data source has changed. Figure out how to handle the new source
        /// </summary>
        protected void RebindDataSource(bool forceDataInitialization)
        {
            if (this.BindingContext == null)
                return;

            // Obtain the CurrencyManager for the current data source.
            CurrencyManager tempCurrencyManager = null;

            if (this.DataSource != null) {
                tempCurrencyManager = (CurrencyManager)this.BindingContext[this.DataSource, this.DataMember];
            }

            // Has our currency manager changed?
            if (this.currencyManager != tempCurrencyManager) {

                // Stop listening for events on our old currency manager
                if (this.currencyManager != null) {
                    this.currencyManager.MetaDataChanged -=  new EventHandler(currencyManager_MetaDataChanged);
                    this.currencyManager.PositionChanged -=  new EventHandler(currencyManager_PositionChanged);
                    this.currencyManager.ListChanged -= new ListChangedEventHandler(currencyManager_ListChanged);
                }

                this.currencyManager = tempCurrencyManager;

                // Start listening for events on our new currency manager
                if (this.currencyManager != null) {
                    this.currencyManager.MetaDataChanged += new EventHandler(currencyManager_MetaDataChanged);
                    this.currencyManager.PositionChanged += new EventHandler(currencyManager_PositionChanged);
                    this.currencyManager.ListChanged += new ListChangedEventHandler(currencyManager_ListChanged);
                }

                // Our currency manager has changed so we have to initialize a new data source
                forceDataInitialization = true;
            }

            if (forceDataInitialization)
                InitializeDataSource();
        }

        /// <summary>
        /// The data source for this control has changed. Reconfigure the control for the new source
        /// </summary>
        protected void InitializeDataSource()
        {
            if (this.Frozen || this.currencyManager == null)
                return;

            this.CreateColumnsFromSource();
            this.CreateMissingAspectGetters();
            this.SetObjects(this.currencyManager.List);

            // If we have some data, resize the new columns based on the data available.
            if (this.Items.Count > 0) {
                foreach (ColumnHeader column in this.Columns) {
                    if (column.Width == 0)
                        this.AutoResizeColumn(column.Index, ColumnHeaderAutoResizeStyle.ColumnContent);
                }
            }
        }

        /// <summary>
        /// Create columns for the listview based on what properties are available in the data source
        /// </summary>
        /// <remarks>
        /// <para>This method will not replace existing columns.</para>
        /// </remarks>
        protected void CreateColumnsFromSource()
        {
            if (this.currencyManager == null || this.Columns.Count != 0)
            	return;

            PropertyDescriptorCollection properties = this.currencyManager.GetItemProperties();
            if (properties.Count == 0)
                return; 

            for (int i = 0; i < properties.Count; i++) {
                // Make a stack variable to hold the property so it can be used in the AspectGetter delegate
                PropertyDescriptor property = properties[i];

                // Relationships to other tables turn up as IBindibleLists. Don't make columns to show them.
                // CHECK: Is this always true? What other things could be here? Constraints? Triggers?
                if (property.PropertyType == typeof(IBindingList))
                    continue;

                // Create a column 
                OLVColumn column = new OLVColumn(property.DisplayName, property.Name);
                column.Width = 0; // zero-width since we will resize it once we have some data
                column.AspectGetter = delegate(object row) {
                    return property.GetValue(row);
                };
                // If our column is a BLOB, it could be an image, so assign a renderer to draw it. 
                // CONSIDER: Is this a common enough case to warrant this code?
                if (property.PropertyType == typeof(System.Byte[]))
                    column.Renderer = new ImageRenderer();

                // Add it to our list
                this.Columns.Add(column);
            }
        }

        /// <summary>
        /// Generate aspect getters for any columns that are missing them (and for which we have
        /// enough information to actually generate a getter)
        /// </summary>
        protected void CreateMissingAspectGetters()
        {
            for (int i = 0; i < this.Columns.Count; i++) {
                OLVColumn column = this.GetColumn(i);
                if (column.AspectGetter == null && !String.IsNullOrEmpty(column.AspectName)) {
                    column.AspectGetter = delegate(object row) {
                        try {
                            // In most cases, rows will be DataRowView objects
                            return ((DataRowView)row)[column.AspectName];
                        } catch {
                            return column.GetAspectByName(row);
                        }
                    };
                }
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// What should we do when the list is unfrozen
        /// </summary>
        override protected void DoUnfreeze()
        {
            // Clear any previous currency manager so the rebind will always work from scratch
            this.RebindDataSource(true);
        }

        /// <summary>
        /// Handles binding context changes
        /// </summary>
        /// <param name="e">The EventArgs that will be passed to any handlers
        /// of the BindingContextChanged event.</param>
        protected override void OnBindingContextChanged(EventArgs e)
        {
            base.OnBindingContextChanged(e);

            // If our binding context changes, we must rebind, since we will
            // have a new currency managers, even if we are still bound to the
            // same data source.
            this.RebindDataSource(false);
        }


        /// <summary>
        /// Handles parent binding context changes
        /// </summary>
        /// <param name="e">Unused EventArgs.</param>
        protected override void OnParentBindingContextChanged(EventArgs e)
        {
            base.OnParentBindingContextChanged(e);

            // BindingContext is an ambient property - by default it simply picks
            // up the parent control's context (unless something has explicitly
            // given us our own). So we must respond to changes in our parent's
            // binding context in the same way we would changes to our own
            // binding context.
            this.RebindDataSource(false);
        }

        // CurrencyManager ListChanged event handler. 
        // Deals with fine-grained changes to list items.
        // It's actually difficult to deal with these changes in a fine-grained manner.
        // If our listview is grouped, then any change may make a new group appear or
        // an old group disappear. It is rarely enough to simply update the affected row.
        private void currencyManager_ListChanged(object sender, ListChangedEventArgs e)
        {
            switch (e.ListChangedType) {

                // Well, usually fine-grained... The whole list has changed utterly, so reload it.
                case ListChangedType.Reset:
                    this.InitializeDataSource();
                    break;

                // A single item has changed, so just refresh that.
                // TODO: Even in this simple case, we should probably rebuild the list.
                case ListChangedType.ItemChanged:
                    Object changedRow = this.currencyManager.List[e.NewIndex];
                    this.RefreshObject(changedRow);
                    break;

                // A new item has appeared, so add that.
				// We get this event twice if certain grid controls are used to add a new row to a 
				// datatable: once when the editing of a new row begins, and once again when that 
				// editing commits. (If the user cancels the creation of the new row, we never see 
				// the second creation.) We detect this by seeing if this is a view on a row in a 
				// DataTable, and if it is, testing to see if it's a new row under creation.
                case ListChangedType.ItemAdded:
                    Object newRow = this.currencyManager.List[e.NewIndex];
                    DataRowView drv = newRow as DataRowView;
                    if (drv == null || !drv.IsNew) {
						// Either we're not dealing with a view on a data table, or this is the commit 
						// notification. Either way, this is the final notification, so we want to
						// handle the new row now!
                        this.InitializeDataSource();
                    }
                    break;

                // An item has gone away.
                case ListChangedType.ItemDeleted:
                    this.InitializeDataSource();
                    break;

                // An item has changed its index.
                case ListChangedType.ItemMoved:
                    this.InitializeDataSource();
                    break;

                // Something has changed in the metadata.
                // CHECK: When are these events actually fired?
                case ListChangedType.PropertyDescriptorAdded:
                case ListChangedType.PropertyDescriptorChanged:
                case ListChangedType.PropertyDescriptorDeleted:
                    this.InitializeDataSource();
                    break;
            }
        }


        // The CurrencyManager calls this if the data source looks
        // different. We just reload everything.
        // CHECK: Do we need this if we are handle ListChanged metadata events?
        private void currencyManager_MetaDataChanged(object sender, EventArgs e)
        {
            this.InitializeDataSource();
        }


        // Called by the CurrencyManager when the currently selected item
        // changes. We update the ListView selection so that we stay in sync
        // with any other controls bound to the same source.
        private void currencyManager_PositionChanged(object sender, EventArgs e)
        {
            int index = this.currencyManager.Position;

            // Make sure the index is sane (-1 pops up from time to time)
            if (index < 0 || index >= this.Items.Count) 
                return;

            // Avoid recursion. If we are currently changing the index, don't
            // start the process again. 
            if (this.isChangingIndex) 
                return;

            try {
                this.isChangingIndex = true;

                // We can't use the index directly, since our listview may be sorted
                this.SelectedObject = this.currencyManager.List[index];

                // THINK: Do we always want to bring it into view?
                if (this.SelectedItems.Count > 0)
                    this.SelectedItems[0].EnsureVisible();

            } finally {
                this.isChangingIndex = false;
            }
        }
        private bool isChangingIndex = false;


        // Called by Windows Forms when the currently selected index of the
        // control changes. This usually happens because the user clicked on
        // the control. In this case we want to notify the CurrencyManager so
        // that any other bound controls will remain in sync. This method will
        // also be called when we changed our index as a result of a
        // notification that originated from the CurrencyManager, and in that
        // case we avoid notifying the CurrencyManager back!
        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);

            // Prevent recursion
            if (this.isChangingIndex) 
                return;

            // If we are bound to a datasource, and only one item is selected,
            // tell the currency manager which item is selected.
            if (this.SelectedIndices.Count == 1 && this.currencyManager != null) {
                try {
                    this.isChangingIndex = true;

                    // We can't use the selectedIndex directly, since our listview may be sorted.
                    // So we have to find the index of the selected object within the original list.
                    this.currencyManager.Position = this.currencyManager.List.IndexOf(this.SelectedObject);
                } finally {
                    this.isChangingIndex = false;
                }
            }
        }
 
		#endregion

	}

	#region Delegate declarations

	/// <summary>
	/// These delegates are used to extract an aspect from a row object
	/// </summary>
    public delegate Object AspectGetterDelegate(Object rowObject);

	/// <summary>
	/// These delegates are used to fetch the image selector that should be used
    /// to choose an image for this column.
	/// </summary>
    public delegate Object ImageGetterDelegate(Object rowObject);

	/// <summary>
	/// These delegates can be used to convert an aspect value to a display string,
	/// instead of using the default ToString()
	/// </summary>
    public delegate string AspectToStringConverterDelegate(Object value);

	/// <summary>
	/// These delegates are used to retrieve the object that is the key of the group to which the given row belongs.
	/// </summary>
    public delegate Object GroupKeyGetterDelegate(Object rowObject);

	/// <summary>
	/// These delegates are used to convert a group key into a title for the group
	/// </summary>
    public delegate string GroupKeyToTitleConverterDelegate(Object groupKey);

	/// <summary>
	/// These delegates are used to fetch a row object for virtual lists
	/// </summary>
    public delegate Object RowGetterDelegate(int rowIndex);

    /// <summary>
    /// These delegates are used to sort the listview in some custom fashion
    /// </summary>
    public delegate void SortDelegate(OLVColumn column, SortOrder sortOrder);

    /// <summary>
    /// These delegates are used to format a listviewitem before it is added to the control.
    /// </summary>
    public delegate void RowFormatterDelegate(OLVListItem olvItem);

    /// <summary>
    /// These delegates are used to draw a cell
    /// </summary>
    public delegate void RenderDelegate(DrawListViewSubItemEventArgs e, Graphics g, Rectangle r, Object rowObject);

	#endregion

	#region Column

	/// <summary>
	/// An OLVColumn knows which aspect of an object it should present.
	/// </summary>
	/// <remarks>
	/// The column knows how to:
	/// <list type="bullet">
	///	<item>extract its aspect from the row object</item>
	///	<item>convert an aspect to a string</item>
	///	<item>calculate the image for the row object</item>
	///	<item>extract a group "key" from the row object</item>
	///	<item>convert a group "key" into a title for the group</item>
	/// </list>
	/// <para>For sorting to work correctly, aspects from the same column
	/// must be of the same type, that is, the same aspect cannot sometimes
	/// return strings and other times integers.</para>
	/// </remarks>
	[Browsable(false)]
	public class OLVColumn : ColumnHeader
	{
        /// <summary>
        /// Create an OLVColumn
        /// </summary>
		public OLVColumn()
			: base ()
		{
			this.Renderer = new BaseRenderer();
        }

		/// <summary>
		/// Initialize a column to have the given title, and show the given aspect
		/// </summary>
		/// <param name="title">The title of the column</param>
		/// <param name="aspect">The aspect to be shown in the column</param>
		public OLVColumn(string title, string aspect)
			: this ()
		{
			this.Text = title;
			this.AspectName = aspect;
		}

		#region Public Properties

		/// <summary>
		/// The name of the property or method that should be called to get the value to display in this column.
		/// This is only used if a ValueGetterDelegate has not been given.
		/// </summary>
        /// <remarks>This name can be dotted to chain references to properties or methods.</remarks>
        /// <example>"DateOfBirth"</example>
        /// <example>"Owner.HomeAddress.Postcode"</example>
		[Category("Behavior"),
		 Description("The name of the property or method that should be called to get the aspect to display in this column")]
		public string AspectName {
			get { return aspectName; }
			set { aspectName = value; }
		}

		/// <summary>
		/// This format string will be used to convert an aspect to its string representation.
		/// </summary>
		/// <remarks>
		/// This string is passed as the first parameter to the String.Format() method.
		/// This is only used if ToStringDelegate has not been set.</remarks>
		/// <example>"{0:C}" to convert a number to currency</example>
		[Category("Behavior"),
		 Description("The format string that will be used to convert an aspect to its string representation"),
		 DefaultValue(null)]
		public string AspectToStringFormat {
			get { return aspectToStringFormat; }
			set { aspectToStringFormat = value; }
		}

        /// <summary>
        /// Group objects by the initial letter of the aspect of the column
        /// </summary>
        /// <remarks>
        /// One common pattern is to group column by the initial letter of the value for that group.
        /// The aspect must be a string (obviously).
        /// </remarks>
        [Category("Behavior"),
         Description("The name of the property or method that should be called to get the aspect to display in this column"),
         DefaultValue(false)]
        public bool UseInitialLetterForGroup
        {
            get { return useInitialLetterForGroup; }
            set { useInitialLetterForGroup = value; }
        }

        /// <summary>
        /// Get/set whether this column should be used when the view is switched to tile view.
        /// </summary>
        /// <remarks>Column 0 is always included in tileview regardless of this setting.
        /// Tile views do not work well with many "columns" of information, 2 or 3 works best.</remarks>
        [Category("Behavior"),
        Description("Will this column be used when the view is switched to tile view"),
         DefaultValue(false)]
        public bool IsTileViewColumn
        {
            get { return isTileViewColumn; }
            set { isTileViewColumn = value; }
        }
        private bool isTileViewColumn = false;

		/// <summary>
		/// This delegate will be used to extract a value to be displayed in this column.
		/// </summary>
		/// <remarks>
		/// If this is set, AspectName is ignored.
		/// </remarks>
		[Browsable(false),
		 DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public AspectGetterDelegate AspectGetter {
			get { return aspectGetter; }
			set {
				aspectGetter = value;
			    aspectGetterAutoGenerated = false;
			}
		}

		/// <summary>
		/// The delegate that will be used to translate the aspect to display in this column into a string.
		/// </summary>
		/// <remarks>If this value is set, ValueToStringFormat will be ignored.</remarks>
		[Browsable(false),
		 DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public AspectToStringConverterDelegate AspectToStringConverter {
			get { return aspectToStringConverter; }
			set { aspectToStringConverter = value; }
		}

		/// <summary>
		/// This delegate is called to get the image selector of the image that should be shown in this column.
		/// It can return an int, string, Image or null.
		/// </summary>
        /// <remarks><para>This delegate can use these return value to identify the image:</para>
        /// <list>
        /// <item>null or -1 -- indicates no image</item>
        /// <item>an int -- the int value will be used as an index into the image list</item>
        /// <item>a String -- the string value will be used as a key into the image list</item>
        /// <item>an Image -- the Image will be drawn directly (only in OwnerDrawn mode)</item>
        /// </list>
        /// </remarks>
		[Browsable(false),
		 DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ImageGetterDelegate ImageGetter {
			get { return imageGetter; }
			set { imageGetter = value; }
		}

		/// <summary>
		/// This delegate is called to get the object that is the key for the group
		/// to which the given row belongs.
		/// </summary>
		[Browsable(false),
		 DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public GroupKeyGetterDelegate GroupKeyGetter {
			get { return groupKeyGetter; }
			set { groupKeyGetter = value; }
		}

		/// <summary>
		/// This delegate is called to convert a group key into a title for that group.
		/// </summary>
		[Browsable(false),
		 DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public GroupKeyToTitleConverterDelegate GroupKeyToTitleConverter {
			get { return groupKeyToTitleConverter; }
			set { groupKeyToTitleConverter = value; }
		}

        /// <summary>
        /// This delegate is called when a cell needs to be drawn in OwnerDrawn mode.
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RenderDelegate RendererDelegate
        {
            get { return rendererDelegate; }
            set { rendererDelegate = value; }
        }

        /// <summary>
        /// Get/set the renderer that will be invoked when a cell needs to be redrawn
        /// </summary>
        [Browsable(false),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public BaseRenderer Renderer
        {
            get { return renderer; }
            set { 
                renderer = value;
                if (renderer == null)
                    this.RendererDelegate = null; 
                else {
                    renderer.Column = this;
                    this.RendererDelegate = new RenderDelegate(renderer.HandleRendering);
                }
            }
        }
        private BaseRenderer renderer;

		/// <summary>
		/// Remember if this aspect getter for this column was generated internally, and can therefore
		/// be regenerated at will
		/// </summary>
		[Browsable(false),
		 DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool AspectGetterAutoGenerated {
			get { return aspectGetterAutoGenerated; }
			set { aspectGetterAutoGenerated = value; }
		}
       	private bool aspectGetterAutoGenerated;

        /// <summary>
        /// When the listview is grouped by this column and group title has an item count, 
        /// how should the lable be formatted?
        /// </summary>
        /// <remarks>
        /// The given format string can/should have two placeholders:
        /// <list type="bullet">
        /// <item>{0} - the original group title</item>
        /// <item>{1} - the number of items in the group</item>
        /// </list>
        /// <para>If this value is not set, the values from the list view will be used</para>
        /// </remarks>
        /// <example>"{0} [{1} items]"</example>
        [Category("Behavior"),
         Description("The format to use when suffixing item counts to group titles"),
         DefaultValue(null)]
        public string GroupWithItemCountFormat
        {
            get { return groupWithItemCountFormat; }
            set { groupWithItemCountFormat = value; }
        }
        private string groupWithItemCountFormat;

        /// <summary>
        /// Return this.GroupWithItemCountFormat or a reasonable default
        /// </summary>
        [Browsable(false)]
        public string GroupWithItemCountFormatOrDefault
        {
            get {
                if (String.IsNullOrEmpty(this.GroupWithItemCountFormat))
                    // There is one rare but pathelogically possible case where the ListView can
                    // be null, so we have to provide a workable default for that rare case.
                    if (this.ListView == null)
                        return "{0} [{1} items]";
                    else
                        return ((ObjectListView)this.ListView).GroupWithItemCountFormatOrDefault;
                else
                    return this.GroupWithItemCountFormat;
            }
        }

        /// <summary>
        /// When the listview is grouped by this column and a group title has an item count, 
        /// how should the lable be formatted if there is only one item in the group?
        /// </summary>
        /// <remarks>
        /// The given format string can/should have two placeholders:
        /// <list type="bullet">
        /// <item>{0} - the original group title</item>
        /// <item>{1} - the number of items in the group (always 1)</item>
        /// </list>
        /// <para>If this value is not set, the values from the list view will be used</para>
        /// </remarks>
        /// <example>"{0} [{1} item]"</example>
        [Category("Behavior"),
         Description("The format to use when suffixing item counts to group titles"),
         DefaultValue(null)]
        public string GroupWithItemCountSingularFormat
        {
            get { return groupWithItemCountSingularFormat; }
            set { groupWithItemCountSingularFormat = value; }
        }
        private string groupWithItemCountSingularFormat;

        /// <summary>
        /// Return this.GroupWithItemCountSingularFormat or a reasonable default
        /// </summary>
        [Browsable(false)]
        public string GroupWithItemCountSingularFormatOrDefault
        {
            get {
                if (String.IsNullOrEmpty(this.GroupWithItemCountSingularFormat))
                    // There is one pathelogically rare but still possible case where the ListView can
                    // be null, so we have to provide a workable default for that rare case.
                    if (this.ListView == null)
                        return "{0} [{1} item]";
                    else
                        return ((ObjectListView)this.ListView).GroupWithItemCountSingularFormatOrDefault;
                else
                    return this.GroupWithItemCountSingularFormat;
            }
        }

        /// <summary>
        /// What is the minimum width that the user can give to this column?
        /// </summary>
        /// <remarks>-1 means there is no minimum width. Give this the same value as MaximumWidth to make a fixed width column.</remarks>
        [Category("Misc"),
         Description("What is the minimum width to which the user can resize this column?"),
         DefaultValue(-1)]
        public int MinimumWidth
        {
            get { return minWidth; }
            set { minWidth = value; }
        }
        private int minWidth = -1;

        /// <summary>
        /// What is the maximum width that the user can give to this column?
        /// </summary>
        /// <remarks>-1 means there is no maximum width. Give this the same value as MinimumWidth to make a fixed width column.</remarks>
        [Category("Misc"),
         Description("What is the maximum width to which the user can resize this column?"),
         DefaultValue(-1)]
        public int MaximumWidth
        {
            get { return maxWidth; }
            set { maxWidth = value; }
        }
        private int maxWidth = -1;

        /// <summary>
        /// Is this column a fixed width column?
        /// </summary>
        [Browsable(false)]
        public bool IsFixedWidth
        {
            get
            {
                return (this.MinimumWidth != -1 && this.MaximumWidth != -1 && this.MinimumWidth >= this.MaximumWidth);
            }
        }

		#endregion

		/// <summary>
		/// For a given row object, return the object that is to be displayed in this column.
		/// </summary>
		/// <param name="rowObject">The row object that is being displayed</param>
		/// <returns>An object, which is the aspect to be displayed</returns>
		public object GetValue(object rowObject)
		{
			if (this.aspectGetter == null)
				return this.GetAspectByName(rowObject);
			else
				return this.aspectGetter(rowObject);
		}

		/// <summary>
		/// For a given row object, extract the value indicated by the AspectName property of this column.
		/// </summary>
		/// <param name="rowObject">The row object that is being displayed</param>
		/// <returns>An object, which is the aspect named by AspectName</returns>
		public object GetAspectByName(object rowObject)
		{
			if (string.IsNullOrEmpty(this.aspectName))
				return null;

			//CONSIDER: Should we include NonPublic in this list?
			BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
				BindingFlags.InvokeMethod | BindingFlags.GetProperty | BindingFlags.GetField;
            object source = rowObject;
            foreach (string property in this.aspectName.Split('.')) {
                try {
                    source = source.GetType().InvokeMember(property, flags, null, source, null);
                } catch (System.MissingMethodException) {
                    return String.Format("Cannot invoke '{0}' on a {1}", property, source.GetType());
                }
            }
            return source;
        }

        /// <summary>
		/// For a given row object, return the string representation of the value shown in this column.
		/// </summary>
		/// <remarks>
		/// For aspects that are string (e.g. aPerson.Name), the aspect and its string representation are the same.
		/// For non-strings (e.g. aPerson.DateOfBirth), the string representation is very different.
		/// </remarks>
		/// <param name="rowObject"></param>
		/// <returns></returns>
		public string GetStringValue(object rowObject)
		{
			return this.ValueToString(this.GetValue(rowObject));
		}

		/// <summary>
		/// Convert the aspect object to its string representation.
		/// </summary>
		/// <remarks>
		/// If the column has been given a ToStringDelegate, that will be used to do
		/// the conversion, otherwise just use ToString(). Nulls are always converted
		/// to empty strings.
		/// </remarks>
		/// <param name="value">The value of the aspect that should be displayed</param>
		/// <returns>A string representation of the aspect</returns>
		public string ValueToString(object value)
		{
			// CONSIDER: Should we give aspect-to-string converters a chance to work on a null value?
			if (value == null)
				return "";

			if (this.aspectToStringConverter != null)
				return this.aspectToStringConverter(value);

			string fmt = this.AspectToStringFormat;
            if (String.IsNullOrEmpty(fmt))
                return value.ToString();
            else
    			return String.Format(fmt, value);
		}

		/// <summary>
		/// For a given row object, return the image selector of the image that should displayed in this column.
		/// </summary>
		/// <param name="rowObject">The row object that is being displayed</param>
		/// <returns>int or string or Image. int or string will be used as index into image list. null or -1 means no image</returns>
		public Object GetImage(object rowObject)
		{
			if (this.imageGetter != null)
				return this.imageGetter(rowObject);

            if (!String.IsNullOrEmpty(this.ImageKey))
				return this.ImageKey;
            
            return this.ImageIndex;
		}

		/// <summary>
		/// For a given row object, return the object that is the key of the group that this row belongs to.
		/// </summary>
		/// <param name="rowObject">The row object that is being displayed</param>
		/// <returns>Group key object</returns>
		public object GetGroupKey(object rowObject)
		{
			if (this.groupKeyGetter == null)
			{
				object key = this.GetValue(rowObject);
                if (key is string && this.UseInitialLetterForGroup) {
                    String keyAsString = (String)key;
                    if (keyAsString.Length > 0)
                        key = keyAsString.Substring(0, 1).ToUpper();
                }
				return key;
			}
			else
				return this.groupKeyGetter(rowObject);
		}

		/// <summary>
		/// For a given group value, return the string that should be used as the groups title.
		/// </summary>
		/// <param name="value">The group key that is being converted to a title</param>
		/// <returns>string</returns>
		public string ConvertGroupKeyToTitle(object value)
		{
			if (this.groupKeyToTitleConverter == null)
				return this.ValueToString(value);
			else
				return this.groupKeyToTitleConverter(value);
        }

        #region Utilities

        /// <summary>
        /// Install delegates that will group the columns aspects into progressive partitions.
        /// If an aspect is less than value[n], it will be grouped with description[n]. 
        /// If an aspect has a value greater than the last element in "values", it will be grouped
        /// with the last element in "descriptions".
        /// </summary>
        /// <param name="values">Array of values. Values must be able to be
        /// compared to the aspect (using IComparable)</param>
        /// <param name="descriptions">The description for the matching value. The last element is the default description.
        /// If there are n values, there must be n+1 descriptions.</param>
        /// <example>
        /// this.salaryColumn.MakeGroupies(
        ///     new UInt32[] { 20000, 100000 },
        ///     new string[] { "Lowly worker",  "Middle management", "Rarified elevation"});
        /// </example>
        public void MakeGroupies<T>(T[] values, string[] descriptions) 
        {
            if (values.Length + 1 != descriptions.Length)
                throw new ArgumentException("descriptions must have one more element than values.");

            // Install a delegate that returns the index of the description to be shown
            this.GroupKeyGetter = delegate(object row) {
                Object aspect = this.GetValue(row);
                if (aspect == null || aspect == System.DBNull.Value)
                    return -1;
                IComparable comparable = (IComparable)aspect;
                for (int i = 0; i < values.Length; i++) {
                    if (comparable.CompareTo(values[i]) < 0)
                        return i;
                }

                // Display the last element in the array
                return descriptions.Length - 1;
            };

            // Install a delegate that simply looks up the given index in the descriptions. 
            this.GroupKeyToTitleConverter = delegate(object key) {
                if ((int)key < 0)
                    return "";

                return descriptions[(int)key];
            };
        }

        #endregion

        #region Private Variables

        private string aspectName;
		private string aspectToStringFormat;
		private bool useInitialLetterForGroup;
		private AspectGetterDelegate aspectGetter;
		private AspectToStringConverterDelegate aspectToStringConverter;
		private ImageGetterDelegate imageGetter;
		private GroupKeyGetterDelegate groupKeyGetter;
        private GroupKeyToTitleConverterDelegate groupKeyToTitleConverter;
        private RenderDelegate rendererDelegate;
        

        #endregion

    }

	#endregion

	#region OLVListItem and OLVListSubItem

	/// <summary>
	/// OLVListItems are specialized ListViewItems that know which row object they came from,
	/// and the row index at which they are displayed, even when in group view mode. They
    /// also know the image they should draw against themselves
	/// </summary>
	public class OLVListItem : ListViewItem
	{
        /// <summary>
        /// Create a OLVListItem for the given row object
        /// </summary>
		public OLVListItem(object rowObject)
			: base()
		{
			this.rowObject = rowObject;
		}

        /// <summary>
        /// Create a OLVListItem for the given row object, represented by the given string and image
        /// </summary>
        public OLVListItem(object rowObject, string text, Object image)
			: base(text, -1)
		{
			this.rowObject = rowObject;
            this.imageSelector = image;
		}

		/// <summary>
		/// RowObject is the model object that is source of the data for this list item.
		/// </summary>
		public object RowObject {
			get { return rowObject; }
			set { rowObject = value; }
		}
        private object rowObject;

		/// <summary>
		/// DisplayIndex is the index of the row where this item is displayed. For flat lists,
		/// this is the same as ListViewItem.Index, but for grouped views, it is different.
		/// </summary>
		public int DisplayIndex {
			get { return displayIndex; }
			set { displayIndex = value; }
		}
        private int displayIndex;

        /// <summary>
        /// Get or set the image that should be shown against this item
        /// </summary>
        /// <remarks><para>This can be an Image, a string or an int. A string or an int will
        /// be used as an index into the small image list.</para></remarks>
        public Object ImageSelector
        {
            get { return imageSelector; }
            set { 
                imageSelector = value;
                if (value is Int32)
                    this.ImageIndex = (Int32)value;
                else if (value is String)
                    this.ImageKey = (String)value;
                else
                    this.ImageIndex = -1;
            }
        }
        private Object imageSelector;
	}

	/// <summary>
	/// A ListViewSubItem that knows which image should be drawn against it.
	/// </summary>
    [Browsable(false)]
	public class OLVListSubItem : ListViewItem.ListViewSubItem
	{
        /// <summary>
        /// Create a OLVListSubItem
        /// </summary>
		public OLVListSubItem()
			: base()
		{
		}

        /// <summary>
        /// Create a OLVListSubItem that shows the given string and image
        /// </summary>
        public OLVListSubItem(string text, Object image)
			: base()
		{
			this.Text = text;
            this.ImageSelector = image;
		}

        /// <summary>
        /// Get or set the image that should be shown against this item
        /// </summary>
        /// <remarks><para>This can be an Image, a string or an int. A string or an int will
        /// be used as an index into the small image list.</para></remarks>
        public Object ImageSelector
        {
            get { return imageSelector; }
            set { imageSelector = value;}
        }
        private Object imageSelector;


        /// <summary>
        /// Return the state of the animatation of the image on this subitem. 
        /// Null means there is either no image, or it is not an animation
        /// </summary>
        public ImageRenderer.AnimationState AnimationState
        {
            get { return animationState; }
            set { animationState = value; }
        }
        private ImageRenderer.AnimationState animationState;
	
	}

	#endregion

    #region Comparers

    /// <summary>
	/// This comparer sort list view groups.
	/// It does this on the basis of the values in the Tags, if we can figure out how to compare
	/// objects of that type. Failing that, it uses a case insensitive compare on the group header.
	/// </summary>
	internal class ListViewGroupComparer : IComparer<ListViewGroup>
	{
	    public ListViewGroupComparer(SortOrder order)
	    {
	        this.sortOrder = order;
	    }

		public int Compare(ListViewGroup x, ListViewGroup y)
		{
	    	// If we know how to compare the tags, do that.
	    	// Otherwise do a case insensitive compare on the group header.
            // We have explicitly catch the "almost-null" value of DBNull.Value,
            // since comparing to that value always produces a type exception.
			int result;
            IComparable comparable = x.Tag as IComparable;
            if (comparable != null && y.Tag != System.DBNull.Value)
                result = comparable.CompareTo(y.Tag);
            else
                result = String.Compare(x.Header, y.Header, true);

	    	if (this.sortOrder == SortOrder.Descending)
	    		result = 0 - result;

	    	return result;
		}

	    private SortOrder sortOrder;
	}

	/// <summary>
	/// ColumnComparer is the workhorse for all comparison between two values of a particular column.
	/// If the column has a specific comparer, use that to compare the values. Otherwise, do
	/// a case insensitive string compare of the string representations of the values.
	/// </summary>
    /// <remarks><para>This class inherits from both IComparer and its generic counterpart
    /// so that it can be used on untyped and typed collections.</para></remarks>
	internal class ColumnComparer : IComparer, IComparer<OLVListItem>
	{
		public ColumnComparer(OLVColumn col, SortOrder order)
		{
			this.column = col;
			this.sortOrder = order;
			this.secondComparer = null;
		}

		public ColumnComparer(OLVColumn col, SortOrder order, OLVColumn col2, SortOrder order2) : this(col, order)
		{
			// There is no point in secondary sorting on the same column
			if (col != col2)
				this.secondComparer = new ColumnComparer(col2, order2);
		}

		public int Compare(object x, object y)
		{
			return this.Compare((OLVListItem)x, (OLVListItem)y);
		}

		public int Compare(OLVListItem x, OLVListItem y)
		{
			int result = 0;
			object x1 = this.column.GetValue(x.RowObject);
			object y1 = this.column.GetValue(y.RowObject);

			if (this.sortOrder == SortOrder.None)
				return 0;

			// Handle nulls. Null values come last
            bool xIsNull = (x1 == null || x1 == System.DBNull.Value);
            bool yIsNull = (y1 == null || y1 == System.DBNull.Value);
            if (xIsNull || yIsNull) {
                if (xIsNull && yIsNull)
					result = 0;
				else
                    result = (xIsNull ? -1 : 1);
            } else {
				result = this.CompareValues(x1, y1);
            }
			
			if (this.sortOrder == SortOrder.Descending)
				result = 0 - result;
			
			// If the result was equality, use the secondary comparer to resolve it
			if (result == 0 && this.secondComparer != null)
				result = this.secondComparer.Compare(x, y);

			return result;
		}

		public int CompareValues(object x, object y)
		{
            // Force case insensitive compares on strings
            if (x is String)
                return String.Compare((String)x, (String)y, true);
            else {
                IComparable comparable = x as IComparable;
                if (comparable != null)
                    return comparable.CompareTo(y);
                else
                    return 0;
            }
		}

		private OLVColumn column;
		private SortOrder sortOrder;
		private ColumnComparer secondComparer;
	}

	#endregion

    #region Renderers

    /// <summary>
    /// Renderers are responsible for drawing a single cell within an owner drawn ObjectListView. 
    /// </summary>
    /// <remarks>
    /// <para>Methods on this class are called during the DrawSubItem event.</para>
    /// <para>Subclasses will normally override the Render method, and use the other
    /// methods as helper functions.</para>
    /// </remarks>
    [Browsable(false)]
    public class BaseRenderer
    {
        /// <summary>
        /// Make a simple renderer
        /// </summary>
        public BaseRenderer()
        {
        }

        #region Properties 

        /// <summary>
        /// Get/set the event that caused this renderer to be called
        /// </summary>
        public DrawListViewSubItemEventArgs Event
        {
            get { return eventArgs; }
            set { eventArgs = value; }
        }
        private DrawListViewSubItemEventArgs eventArgs;

        /// <summary>
        /// Get/set the listview for which the drawing is to be done
        /// </summary>
        public ObjectListView ListView
        {
            get { return objectListView; }
            set { objectListView = value; }
        }
        private ObjectListView objectListView;

        /// <summary>
        /// Get or set the OLVColumn that this renderer will draw
        /// </summary>
        public OLVColumn Column
        {
            get { return column; }
            set { column = value; }
        }
        private OLVColumn column;

        /// <summary>
        /// Get or set the model object that this renderer should draw
        /// </summary>
        public Object RowObject
        {
            get { return rowObject; }
            set { rowObject = value; }
        }
        private Object rowObject;

        /// <summary>
        /// Get or set the aspect of the model object that this renderer should draw
        /// </summary>
        public Object Aspect
        {
            get {
                if (aspect == null)
                    aspect = column.GetValue(this.rowObject);
                return aspect; 
            }
            set { aspect = value; }
        }
        private Object aspect;

        /// <summary>
        /// Get or set the listitem that this renderer will be drawing
        /// </summary>
        public OLVListItem ListItem
        {
            get { return listItem; }
            set { listItem = value; }
        }
        private OLVListItem listItem;

        /// <summary>
        /// Get or set the list subitem that this renderer will be drawing
        /// </summary>
        public ListViewItem.ListViewSubItem SubItem
        {
            get { return listSubItem; }
            set { listSubItem = value; }
        }
        private ListViewItem.ListViewSubItem listSubItem;

        /// <summary>
        /// Get the specialized OLVSubItem that this renderer is drawing
        /// </summary>
        /// <remarks>This returns null for column 0.</remarks>
        public OLVListSubItem OLVSubItem
        {
            get { return listSubItem as OLVListSubItem; }
        }

		/// <summary>
		/// Cache whether or not our item is selected
		/// </summary>
        public bool IsItemSelected
        {
            get { return isItemSelected; }
            set { isItemSelected = value; }
        }
        private bool isItemSelected;
	
        /// <summary>
        /// Is this renderer drawing into a graphics double buffer?
        /// </summary>
        /// <remarks>.NET 2.0 has bugs in its handling of double buffered graphics,
        /// so we sometimes need to behave differently if we are drawing into a 
        /// buffered graphics context.</remarks>
        public bool DoubleBuffered
        {
            get { 
                return true; // set this to false if you've changed the DrawSubItem event code
            }
        }

        /// <summary>
        /// Return the font to be used for text in this cell
        /// </summary>
        /// <returns>The font of the subitem</returns>
        public Font Font
        {
            get {
                if (this.font == null) {
                    if (this.ListItem.UseItemStyleForSubItems)
                        return this.ListItem.Font;
                    else
                        return this.SubItem.Font;
                } else
                    return this.font;
            }
            set {
                this.font = value;
            }
        }
        private Font font;

        /// <summary>
        /// The brush that will be used to paint the text
        /// </summary>
        public Brush TextBrush
        {
            get {
                if (textBrush == null)
                    return new SolidBrush(this.GetForegroundColor());
                else
                    return this.textBrush;
            }
            set { textBrush = value; }
        }
        private Brush textBrush;

        /// <summary>
        /// Should this renderer fill in the background before drawing?
        /// </summary>
        public bool IsDrawBackground
        {
            get { return isDrawBackground; }
            set { isDrawBackground = value; }
        }
        private bool isDrawBackground = true;
	
        #endregion

        #region Utilities
        
        /// <summary>
        /// Return the image that should be drawn against this subitem
        /// </summary>
        /// <returns>An Image or null if no image should be drawn.</returns>
        public Image GetImage() {
            if (this.Column.Index == 0)
                return this.GetImage(this.ListItem.ImageSelector);
            else
                return this.GetImage(this.OLVSubItem.ImageSelector);
        }
        
        /// <summary>
        /// Return the actual image that should be drawn when keyed by the given image selector.
        /// An image selector can be: <list>
        /// <item>an int, giving the index into the image list</item>
        /// <item>a string, giving the image key into the image list</item>
        /// <item>an Image, being the image itself</item>
        /// </list>
        /// </summary>
        /// <param name="imageSelector">The value that indicates the image to be used</param>
        /// <returns>An Image or null</returns>
        public Image GetImage(Object imageSelector)
        {
            if (imageSelector == null)
                return null;

            ImageList il = this.ListView.BaseSmallImageList;
            if (il != null) {
                if (imageSelector is Int32) {
                    Int32 index = (Int32)imageSelector;
                    if (index < 0 || index >= il.Images.Count)
                        return null;
                    else
                        return il.Images[index];
                }

                if (imageSelector is String) {
                    if (il.Images.ContainsKey((String)imageSelector))
                        return il.Images[(String)imageSelector];
                    else
                        return null;
                }
            }
            
            return imageSelector as Image;
        }

        /// <summary>
        /// Return the Color that is the background color for this item's cell
        /// </summary>
        /// <returns>The background color of the subitem</returns>
        public Color GetBackgroundColor()
        {
            if (this.IsItemSelected && this.ListView.FullRowSelect) {
                return SystemColors.Highlight;
            } else {
                if (this.ListItem.UseItemStyleForSubItems)
                    return this.ListItem.BackColor;
                else
                    return this.SubItem.BackColor;
            }
        }

        /// <summary>
        /// Return the Color that is the background color for this item's text
        /// </summary>
        /// <returns>The background color of the subitem's text</returns>
        protected Color GetTextBackgroundColor()
        {
            if (this.IsItemSelected && (this.Column.Index == 0 || this.ListView.FullRowSelect))
                return SystemColors.Highlight;
            else
                if (this.ListItem.UseItemStyleForSubItems)
                    return this.ListItem.BackColor;
                else
                    return this.SubItem.BackColor;
        }

        /// <summary>
        /// Return the color to be used for text in this cell
        /// </summary>
        /// <returns>The text color of the subitem</returns>
        protected Color GetForegroundColor()
        {
            if (this.IsItemSelected && (this.Column.Index == 0 || this.ListView.FullRowSelect))
                return SystemColors.HighlightText;
            else
                if (this.ListItem.UseItemStyleForSubItems)
                    return this.ListItem.ForeColor;
                else
                    return this.SubItem.ForeColor;
        }


        /// <summary>
        /// Align the second rectangle with the first rectangle, 
        /// according to the alignment of the column
        /// </summary>
        /// <param name="outer">The cell's bounds</param>
        /// <param name="inner">The rectangle to be aligned within the bounds</param>
        /// <returns>An aligned rectangle</returns>
        protected Rectangle AlignRectangle(Rectangle outer, Rectangle inner)
        {
            Rectangle r = new Rectangle(inner.Location, inner.Size);

            // Centre horizontally depending on the column alignment
            if (inner.Width < outer.Width) {
                switch (this.Column.TextAlign) {
                    case HorizontalAlignment.Left:
                        r.X = outer.Left;
                        break;
                    case HorizontalAlignment.Center:
                        r.X = outer.Left + ((outer.Width - inner.Width) / 2);
                        break;
                    case HorizontalAlignment.Right:
                        r.X = outer.Right - inner.Width - 1;
                        break;
                }
            }
            // Centre vertically too
            if (inner.Height < outer.Height)
                r.Y = outer.Top + ((outer.Height - inner.Height) / 2);

            return r;
        }

        /// <summary>
        /// Draw the given image aligned horizontally within the column.
        /// </summary>
        /// <remarks>
        /// Over tall images are scaled to fit. Over-wide images are 
        /// truncated. This is by design!
        /// </remarks>
        /// <param name="g">Graphics context to use for drawing</param>
        /// <param name="r">Bounds of the cell</param>
        /// <param name="image">The image to be drawn</param>
        protected void DrawAlignedImage(Graphics g, Rectangle r, Image image)
        {
            if (image == null)
                return;

            // By default, the image goes in the top left of the rectangle
            Rectangle imageBounds = new Rectangle(r.Location, image.Size);

            // If the image is too tall to be drawn in the space provided, proportionally scale it down.
            // Too wide images are not scaled.
            if (image.Height > r.Height) {
                float scaleRatio = (float)r.Height / (float)image.Height;
                imageBounds.Width = (int)((float)image.Width * scaleRatio);
                imageBounds.Height = r.Height - 1;
            }

            // Align and draw our (possibly scaled) image
            g.DrawImage(image, this.AlignRectangle(r, imageBounds));
        }

        /// <summary>
        /// Fill in the background of this cell
        /// </summary>
        /// <param name="g">Graphics context to use for drawing</param>
        /// <param name="r">Bounds of the cell</param>
        protected void DrawBackground(Graphics g, Rectangle r)
        {
            if (this.IsDrawBackground)
                g.FillRectangle(new SolidBrush(this.GetBackgroundColor()), r);
        }

        #endregion


        /// <summary>
        /// The delegate that is called from the list view. This is the main entry point, but
        /// subclasses should override Render instead of this method.
        /// </summary>
        /// <param name="e">The event that caused this redraw</param>
        /// <param name="g">The context that our drawing should be done using</param>
        /// <param name="r">The bounds of the cell within which the renderer can draw</param>
        /// <param name="rowObject">The model object for this row</param>
        public void HandleRendering(DrawListViewSubItemEventArgs e, Graphics g, Rectangle r, Object rowObject)
        {
            this.Event = e;
            this.ListView = (ObjectListView)this.Column.ListView;
            this.RowObject = rowObject;
            this.ListItem = e.Item as OLVListItem;
            this.SubItem = e.SubItem;
            this.Aspect = null; // uncache previous result
            this.IsItemSelected = this.ListItem.Selected; // ((e.ItemState & ListViewItemStates.Selected) == ListViewItemStates.Selected);
            this.Render(g, r);
        }

        /// <summary>
        /// Draw our data into the given rectangle using the given graphics context.
        /// </summary>
        /// <remarks>
        /// <para>Subclasses should override this method.</para></remarks>
        /// <param name="g">The graphics context that should be used for drawing</param>
        /// <param name="r">The bounds of the subitem cell</param>
        virtual public void Render(Graphics g, Rectangle r)
        {
            this.DrawBackground(g, r);
            
            // Adjust the rectangle to match the padding used by the native mode of the ListView
            Rectangle r2 = r;
            r2.X += 4;
            r2.Width -= 4;
            this.DrawImageAndText(g, r2);
        }

        /// <summary>
        /// Draw our subitems image and text
        /// </summary>
        /// <param name="g">Graphics context to use for drawing</param>
        /// <param name="r">Bounds of the cell</param>
        protected void DrawImageAndText(Graphics g, Rectangle r)
        {
            DrawImageAndText(g, r, this.SubItem.Text, this.GetImage());
        }

        /// <summary>
        /// Draw the given text and optional image in the "normal" fashion
        /// </summary>
        /// <param name="g">Graphics context to use for drawing</param>
        /// <param name="r">Bounds of the cell</param>
        /// <param name="txt">The string to be drawn</param>
        /// <param name="image">The optional image to be drawn</param>
        protected void DrawImageAndText(Graphics g, Rectangle r, String txt, Image image)
        {
            // Draw the image
            if (image != null) {
                g.DrawImageUnscaled(image, r.X, r.Y);
                r.X += image.Width;
                r.Width -= image.Width;
            }

            StringFormat fmt = new StringFormat();
            fmt.LineAlignment = StringAlignment.Center; 
            fmt.Trimming = StringTrimming.EllipsisCharacter;
            fmt.FormatFlags = StringFormatFlags.NoWrap;
            switch (this.Column.TextAlign) {
                case HorizontalAlignment.Center: fmt.Alignment = StringAlignment.Center; break;
                case HorizontalAlignment.Left: fmt.Alignment = StringAlignment.Near; break;
                case HorizontalAlignment.Right: fmt.Alignment = StringAlignment.Far; break;
            }
            RectangleF rf = r;
            g.DrawString(txt, this.Font, this.TextBrush, rf, fmt);
            //g.FillRectangle(Brushes.Red, rf);
            // We should put a focus rectange around the column 0 text if it's selected --
            // but we don't because:
            // - I really dislike this UI convention
            // - we are using buffered graphics, so the DrawFocusRecatangle method of the event doesn't work

            //if (this.Column.Index == 0) {
            //    Size size = TextRenderer.MeasureText(this.SubItem.Text, this.ListView.ListFont);
            //    if (r.Width > size.Width)
            //        r.Width = size.Width;
            //    this.Event.DrawFocusRectangle(r);
            //}
        }
    }

    /// <summary>
    /// This class maps a data value to an image that should be drawn for that value.
    /// </summary>
    /// <remarks><para>It is useful for drawing data that is represented as an enum or boolean.</para></remarks>
    public class MappedImageRenderer : BaseRenderer
    {
        /// <summary>
        /// Return a renderer that draw boolean values using the given images
        /// </summary>
        /// <param name="trueImage">Draw this when our data value is true</param>
        /// <param name="falseImage">Draw this when our data value is false</param>
        /// <returns>A Renderer</returns>
        static public MappedImageRenderer Boolean(Object trueImage, Object falseImage)
        {
            return new MappedImageRenderer(true, trueImage, false, falseImage);
        }

        /// <summary>
        /// Return a renderer that draw tristate boolean values using the given images
        /// </summary>
        /// <param name="trueImage">Draw this when our data value is true</param>
        /// <param name="falseImage">Draw this when our data value is false</param>
        /// <param name="nullImage">Draw this when our data value is null</param>
        /// <returns>A Renderer</returns>
        static public MappedImageRenderer TriState(Object trueImage, Object falseImage, Object nullImage)
        {
            return new MappedImageRenderer(new Object[] { true, trueImage, false, falseImage, null, nullImage });
        }

        /// <summary>
        /// Make a new empty renderer
        /// </summary>
        public MappedImageRenderer()
            : base()
        {
            map = new System.Collections.Hashtable();
        }

        /// <summary>
        /// Make a new renderer that will show the given image when the given key is the aspect value
        /// </summary>
        /// <param name="key">The data value to be matched</param>
        /// <param name="image">The image to be shown when the key is matched</param>
        public MappedImageRenderer(Object key, Object image)
            : this()
        {
            this.Add(key, image);
        }

        public MappedImageRenderer(Object key1, Object image1, Object key2, Object image2)
            : this()
        {
            this.Add(key1, image1);
            this.Add(key2, image2);
        }

        /// <summary>
        /// Build a renderer from the given array of keys and their matching images
        /// </summary>
        /// <param name="keysAndImages">An array of key/image pairs</param>
        public MappedImageRenderer(Object [] keysAndImages)
            : this()
        {
            if ((keysAndImages.GetLength(0) % 2) != 0)
                throw new ArgumentException("Array must have key/image pairs");

            for (int i = 0; i < keysAndImages.GetLength(0); i += 2)
                this.Add(keysAndImages[i], keysAndImages[i + 1]);
        }

        /// <summary>
        /// Register the image that should be drawn when our Aspect has the data value.
        /// </summary>
        /// <param name="value">Value that the Aspect must match</param>
        /// <param name="image">An ImageSelector -- an int, string or image</param>
        public void Add(Object value, Object image)
        {
            if (value == null)
                this.nullImage = image;
            else
                map[value] = image;
        }

        /// <summary>
        /// Render our value
        /// </summary>
        /// <param name="g"></param>
        /// <param name="r"></param>
        public override void Render(Graphics g, Rectangle r)
        {
            this.DrawBackground(g, r);

            Image image = null;
            if (this.Aspect == null)
                image = this.GetImage(this.nullImage);
            else
                if (map.ContainsKey(this.Aspect))
                	image = this.GetImage(map[this.Aspect]);
            
            this.DrawAlignedImage(g, r, image);
        }

        #region Private variables
        
        private Hashtable map; // Track the association between values and images
        private Object nullImage; // image to be drawn for null values (since null can't be a key)

        #endregion
    }

    /// <summary>
    /// Render an image that comes from our data source.
    /// </summary>
    /// <remarks>The image can be sourced from:
    /// <list>
    /// <item>a byte-array (normally when the image to be shown is
    /// stored as a value in a database)</item>
    /// <item>an int, which is treated as an index into the image list</item>
    /// <item>a string, which is treated first as a file name, and failing that as an index into the image list</item>
    /// </list>
    /// <para>If an image is an animated GIF, it's state is stored in the SubItem object.</para>
    /// <para>By default, the image renderer does not render animations (it begins life with animations paused). 
    /// To enable animations, you must call Unpause().</para>
    /// </remarks>
    public class ImageRenderer : BaseRenderer
    {
        /// <summary>
        /// Make an empty image renderer
        /// </summary>
        public ImageRenderer()
            : base()
        {
            this.tickler = new System.Threading.Timer(new TimerCallback(this.OnTimer), null, Timeout.Infinite, Timeout.Infinite);
            this.stopwatch = new Stopwatch();
        }

        /// <summary>
        /// Make an empty image renderer that begins life ready for animations
        /// </summary>
        public ImageRenderer(bool startAnimations)
            : this()
        {
            this.Paused = !startAnimations;
        }

        /// <summary>
        /// Draw our image
        /// </summary>
        /// <param name="g"></param>
        /// <param name="r"></param>
        public override void Render(Graphics g, Rectangle r)
        {
            this.DrawBackground(g, r);
            this.DrawAlignedImage(g, r, this.GetImageFromAspect());
        }

        /// <summary>
        /// Translate our Aspect into an image.
        /// </summary>
        /// <remarks>The strategy is:<list type="unordered">
        /// <item>If its a byte array, we treat it as an in-memory image</item>
        /// <item>If it's an int, we use that as an index into our image list</item>
        /// <item>If it's a string, we try to load a file by that name. If we can't, we use the string as an index into our image list.</item>
        ///</list></remarks>
        /// <returns>An image</returns>
        protected Image GetImageFromAspect()
        {
            if (this.Aspect == null || this.Aspect == System.DBNull.Value)
                return null;

            // If we've already figured out the image, don't do it again
            if (this.OLVSubItem != null && this.OLVSubItem.ImageSelector is Image) {
                if (this.OLVSubItem.AnimationState == null)
                    return (Image)this.OLVSubItem.ImageSelector;
                else
                    return this.OLVSubItem.AnimationState.image;
            }

            // Try to convert our Aspect into an Image
            // If its a byte array, we treat it as an in-memory image
            // If it's an int, we use that as an index into our image list
            // If it's a string, we try to find a file by that name. 
            //    If we can't, we use the string as an index into our image list.
            Image image = null;
            if (this.Aspect is System.Byte[]) {
                using (MemoryStream stream = new MemoryStream((System.Byte[])this.Aspect)) {
                    try {
                        image = Image.FromStream(stream);
                    } catch (ArgumentException) {
                        // ignore
                    }
                }
            } else if (this.Aspect is Int32) {
                image = this.GetImage(this.Aspect);
            } else if (this.Aspect is String) {
                try {
                    image = Image.FromFile((String)this.Aspect);
                } catch (FileNotFoundException) {
                    image = this.GetImage(this.Aspect);
                } catch (OutOfMemoryException) {
                    image = this.GetImage(this.Aspect);
                }
            }

            // If this image is an animation, initialize the animation process
            if (this.OLVSubItem != null && AnimationState.IsAnimation(image)) {
                this.OLVSubItem.AnimationState = new AnimationState(image);
            }

            // Cache the image so we don't repeat this dreary process
            if (this.OLVSubItem != null)
                this.OLVSubItem.ImageSelector = image;

            return image;
        }

        /// <summary>
        /// Should the animations in this renderer be paused?
        /// </summary>
        public bool Paused
        {
            get { return isPaused; }
            set {
                if (isPaused != value) {
                    isPaused = value;
                    if (isPaused) {
                        this.tickler.Change(Timeout.Infinite, Timeout.Infinite);
                        this.stopwatch.Stop();
                    } else {
                        this.tickler.Change(1, Timeout.Infinite);
                        this.stopwatch.Start();
                    }
                }
            }
        }
        private bool isPaused = true;

        /// <summary>
        /// Pause any animations
        /// </summary>
        public void Pause()
        {
            this.Paused = true;
        }

        /// <summary>
        /// Unpause any animations
        /// </summary>
        public void Unpause()
        {
            this.Paused = false;
        }

        protected delegate void OnTimerCallback(Object state);

        /// <summary>
        /// This is the method that is invoked by the timer. It basically switches control to the listview thread.
        /// </summary>
        /// <param name="state">not used</param>
        public void OnTimer(Object state)
        {
            if (this.ListView == null || this.Paused) {
                this.tickler.Change(1000, Timeout.Infinite);
            } else {
                if (this.ListView.InvokeRequired) {
                    this.ListView.Invoke(new OnTimerCallback(this.OnTimer), new object[] { state });
                } else {
                    this.OnTimerInThread();
                }
            }
        }

        /// <summary>
        /// This is the OnTimer callback, but invoked in the same thread as the creator of the ListView.
        /// This method can use all of ListViews methods without creating a CrossThread exception.
        /// </summary>
        protected void OnTimerInThread()
        {
            // MAINTAINER NOTE: This method must renew the tickler. If it doesn't the animations will stop.

            // If this listview has been destroyed, we can't do anything, so we return without 
            // renewing the tickler, effectively killing all animations on this renderer
            if (this.ListView.IsDisposed)
                return;

            // If we're not in Detail view, we can't do anything at the moment, but we still renew
            // the tickler because the view may change later.
            if (this.ListView.View != System.Windows.Forms.View.Details) {
                this.tickler.Change(1000, Timeout.Infinite);
                return;
            }

            long elapsedMilliseconds = this.stopwatch.ElapsedMilliseconds;
            int subItemIndex = this.Column.Index;
            long nextCheckAt = elapsedMilliseconds + 1000; // wait at most one second before checking again
            Rectangle updateRect = new Rectangle(); // what part of the view must be updated to draw the changed gifs?

            // Run through all the subitems in the view for our column, and for each one that 
            // has an animation attached to it, see if the frame needs updating.
            foreach (ListViewItem lvi in this.ListView.Items) {
                // Get the gif state from the subitem. If there isn't an animation state, skip this row.
                OLVListSubItem lvsi = (OLVListSubItem)lvi.SubItems[subItemIndex];
                AnimationState state = lvsi.AnimationState;
                if (state == null || !state.IsValid)
                    continue;

                // Has this frame of the animation expired?
                if (elapsedMilliseconds >= state.currentFrameExpiresAt) {
                    state.AdvanceFrame(elapsedMilliseconds);

                    // Track the area of the view that needs to be redrawn to show the changed images
                    if (updateRect.IsEmpty)
                        updateRect = lvsi.Bounds;
                    else
                        updateRect = Rectangle.Union(updateRect, lvsi.Bounds);
                }

                // Remember the minimum time at which a frame is next due to change
                nextCheckAt = Math.Min(nextCheckAt, state.currentFrameExpiresAt);
            }

            // Update the part of the listview where frames have changed
            if (!updateRect.IsEmpty)
                this.ListView.Invalidate(updateRect);

            // Renew the tickler in time for the next frame change
            this.tickler.Change(nextCheckAt - elapsedMilliseconds, Timeout.Infinite);
        }

        /// <summary>
        /// Instances of this class kept track of the animation state of a single image.
        /// </summary>
        public class AnimationState
        {
            const int PropertyTagTypeShort = 3;
            const int PropertyTagTypeLong = 4;
            const int PropertyTagFrameDelay = 0x5100;
            const int PropertyTagLoopCount = 0x5101;

            /// <summary>
            /// Is the given image an animation
            /// </summary>
            /// <param name="image">The image to be tested</param>
            /// <returns>Is the image an animation?</returns>
            static public bool IsAnimation(Image image)
            {
                if (image == null)
                    return false;
                else
                    return (new List<Guid>(image.FrameDimensionsList)).Contains(FrameDimension.Time.Guid);
            }

            /// <summary>
            /// Create an AnimationState in a quiet state
            /// </summary>
            public AnimationState()
            {
                this.currentFrame = 0;
                this.frameCount = 0;
                this.imageDuration = new List<int>();
                this.image = null;
            }

            /// <summary>
            /// Create an animation state for the given image, which may or may not
            /// be an animation
            /// </summary>
            /// <param name="image">The image to be rendered</param>
            public AnimationState(Image image)
                : this()
            {
                if (!AnimationState.IsAnimation(image))
                    return;

                // How many frames in the animation?
                this.image = image;
                this.frameCount = this.image.GetFrameCount(FrameDimension.Time);

                // Find the delay between each frame. 
                // The delays are stored an array of 4-byte ints. Each int is the 
                // number of 1/100th of a second that should elapsed before the frame expires
                foreach (PropertyItem pi in this.image.PropertyItems) {
                    if (pi.Id == PropertyTagFrameDelay) {
                        for (int i = 0; i < pi.Len; i += 4) {
                            //TODO: There must be a better way to convert 4-bytes to an int
                            int delay = (pi.Value[i + 3] << 24) + (pi.Value[i + 2] << 16) + (pi.Value[i + 1] << 8) + pi.Value[i];
                            this.imageDuration.Add(delay * 10); // store delays as milliseconds
                        }
                        break;
                    }
                }

                // There should be as many frame durations as frames
                Debug.Assert(this.imageDuration.Count == this.frameCount, "There should be as many frame durations as there are frames.");
            }

            /// <summary>
            /// Does this state represent a valid animation
            /// </summary>
            public bool IsValid {
                get  {
                    return (this.image != null && this.frameCount > 0);
                }
            }

            /// <summary>
            /// Advance our images current frame and calculate when it will expire
            /// </summary>
            public void AdvanceFrame(long millisecondsNow)
            {
                this.currentFrame = (this.currentFrame + 1) % this.frameCount;
                this.currentFrameExpiresAt = millisecondsNow + this.imageDuration[this.currentFrame];
                this.image.SelectActiveFrame(FrameDimension.Time, this.currentFrame);
            }

            public int currentFrame;
            public long currentFrameExpiresAt;
            public Image image;
            public List<int> imageDuration;
            public int frameCount;
        }

        #region Private variables

        private System.Threading.Timer tickler; // timer used to tickle the animations
        private Stopwatch stopwatch; // clock used to time the animation frame changes

        #endregion
    }

    /// <summary>
    /// Render our Aspect as a progress bar
    /// </summary>
    public class BarRenderer : BaseRenderer
    {
        #region Constructors

        /// <summary>
        /// Make a BarRenderer
        /// </summary>
        public BarRenderer()
            : base()
        {
            this.Pen = new Pen(Color.Blue, 1);
            this.Brush = Brushes.Aquamarine;
            this.BackgroundBrush = Brushes.White;
            this.StartColor = Color.Empty;
        }

        /// <summary>
        /// Make a BarRenderer for the given range of data values
        /// </summary>
        public BarRenderer(int minimum, int maximum)
            : this()
        {
            this.MinimumValue = minimum;
            this.MaximumValue = maximum;
        }

        /// <summary>
        /// Make a BarRenderer using a custom bar scheme
        /// </summary>
        public BarRenderer(Pen aPen, Brush aBrush)
            : this()
        {
            this.Pen = aPen;
            this.Brush = aBrush;
            this.UseStandardBar = false;
        }

        /// <summary>
        /// Make a BarRenderer using a custom bar scheme
        /// </summary>
        public BarRenderer(int minimum, int maximum, Pen aPen, Brush aBrush)
            : this(minimum, maximum)
        {
            this.Pen = aPen;
            this.Brush = aBrush;
            this.UseStandardBar = false;
        }

        /// <summary>
        /// Make a BarRenderer that uses a horizontal gradient
        /// </summary>
        public BarRenderer(Pen aPen, Color start, Color end)
            : this()
        {
            this.Pen = aPen;
            this.SetGradient(start, end);
        }

        /// <summary>
        /// Make a BarRenderer that uses a horizontal gradient
        /// </summary>
        public BarRenderer(int minimum, int maximum, Pen aPen, Color start, Color end)
            : this(minimum, maximum)
        {
            this.Pen = aPen;
            this.SetGradient(start, end);
        }

        #endregion

        #region Public variables

        /// <summary>
        /// Should this bar be drawn in the system style
        /// </summary>
        public bool UseStandardBar = true;

        /// <summary>
        /// How many pixels in from our cell border will this bar be drawn
        /// </summary>
        public int Padding = 2;

        /// <summary>
        /// The Pen that will draw the frame surrounding this bar
        /// </summary>
        public Pen Pen;

        /// <summary>
        /// The brush that will be used to fill the bar
        /// </summary>
        public Brush Brush;

        /// <summary>
        /// The brush that will be used to fill the background of the bar
        /// </summary>
        public Brush BackgroundBrush;

        /// <summary>
        /// The first color when a gradient is used to fill the bar
        /// </summary>
        public Color StartColor;

        /// <summary>
        /// The end color when a gradient is used to fill the bar
        /// </summary>
        public Color EndColor;

        /// <summary>
        /// Regardless of how wide the column become the progress bar will never be wider than this
        /// </summary>
        public int MaximumWidth = 100;

        /// <summary>
        /// Regardless of how high the cell is  the progress bar will never be taller than this
        /// </summary>
        public int MaximumHeight = 16;

        /// <summary>
        /// The minimum data value expected. Values less than this will given an empty bar
        /// </summary>
        public int MinimumValue = 0;

        /// <summary>
        /// The maximum value for the range. Values greater than this will give a full bar
        /// </summary>
        public int MaximumValue = 100;
        
        #endregion

        /// <summary>
        /// Draw this progress bar using a gradient
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public void SetGradient(Color start, Color end)
        {
            this.StartColor = start;
            this.EndColor = end;
            this.UseStandardBar = false;
        }

    	/// <summary>
    	/// Draw our aspect
    	/// </summary>
    	/// <param name="g"></param>
    	/// <param name="r"></param>
		public override void Render(Graphics g, Rectangle r)
		{
			this.DrawBackground(g, r);

            Rectangle frameRect = Rectangle.Inflate(r, 0 - this.Padding, 0 - this.Padding);
            frameRect.Width = Math.Min(frameRect.Width, this.MaximumWidth);
            frameRect.Height = Math.Min(frameRect.Width, this.MaximumHeight);
            frameRect = this.AlignRectangle(r, frameRect);

            // Convert our aspect to a numeric value
            // CONSIDER: Is this the best way to do this?
            if (!(this.Aspect is IConvertible))
                return;
            double aspectValue = ((IConvertible)this.Aspect).ToDouble(NumberFormatInfo.InvariantInfo);

            Rectangle fillRect = Rectangle.Inflate(frameRect, -1, -1);
            if (aspectValue <= this.MinimumValue)
                fillRect.Width = 0;
            else if (aspectValue < this.MaximumValue)
                fillRect.Width = (int)(fillRect.Width * (aspectValue - this.MinimumValue) / this.MaximumValue);

            if (this.UseStandardBar && ProgressBarRenderer.IsSupported) {
                ProgressBarRenderer.DrawHorizontalBar(g, frameRect);
                ProgressBarRenderer.DrawHorizontalChunks(g, fillRect);
            } else {
            	g.FillRectangle(this.BackgroundBrush, frameRect);
                if (fillRect.Width > 0) {
                    fillRect.Height++;
                    if (this.StartColor == Color.Empty)
                        g.FillRectangle(this.Brush, fillRect);
                    else {
                        using (LinearGradientBrush gradient = new LinearGradientBrush(frameRect, this.StartColor, this.EndColor, LinearGradientMode.Horizontal)) {
                            g.FillRectangle(gradient, fillRect);
                        }
                    }
                }
                g.DrawRectangle(this.Pen, frameRect);
            }
		}
    }

    /// <summary>
    /// A MultiImageRenderer draws the same image a number of times based on our data value
    /// </summary>
    /// <remarks><para>The stars in the Rating column of iTunes is a good example of this type of renderer.</para></remarks>
    public class MultiImageRenderer : BaseRenderer
    {
    	/// <summary>
    	/// Make a quiet rendererer
    	/// </summary>
        public MultiImageRenderer()
            : base()
        {
        }

        /// <summary>
        /// Make an image renderer that will draw the indicated image, at most maxImages times.
        /// </summary>
        /// <param name="imageSelector"></param>
        /// <param name="maxImages"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        public MultiImageRenderer(Object imageSelector, int maxImages, int minValue, int maxValue)
            : this()
        {
            this.ImageSelector = imageSelector;
            this.MaxNumberImages = maxImages;
            this.MinimumValue = minValue;
            this.MaximumValue = maxValue;
        }

        /// <summary>
        /// The image selector that will give the image to be drawn
        /// </summary>
        public Object ImageSelector;

        /// <summary>
        /// Get or set the number of pixels between each image
        /// </summary>
        public int Spacing = 1;

        /// <summary>
        /// What is the maximum number of images that this renderer should draw?
        /// </summary>
        public int MaxNumberImages = 10;

        /// <summary>
        /// Values less than or equal to this will have 0 images drawn
        /// </summary>
        public int MinimumValue = 0;

        /// <summary>
        /// Values greater than or equal to this will have MaxNumberImages images drawn
        /// </summary>
        public int MaximumValue = 100;

        /// <summary>
        /// Draw our data value
        /// </summary>
        /// <param name="g"></param>
        /// <param name="r"></param>
        public override void Render(Graphics g, Rectangle r)
        {
            this.DrawBackground(g, r);

            Image image = this.GetImage(this.ImageSelector);
            if (image == null)
                return;

            // Convert our aspect to a numeric value
            // CONSIDER: Is this the best way to do this?
            if (!(this.Aspect is IConvertible))
                return;
            double aspectValue = ((IConvertible)this.Aspect).ToDouble(NumberFormatInfo.InvariantInfo);

            // Calculate how many images we need to draw to represent our aspect value
            int numberOfImages;
            if (aspectValue <= this.MinimumValue)
                numberOfImages = 0;
            else if (aspectValue < this.MaximumValue)
                numberOfImages = 1 + (int)(this.MaxNumberImages * (aspectValue - this.MinimumValue) / this.MaximumValue);
            else
                numberOfImages = this.MaxNumberImages;

            // If we need to shrink the image, what will its on-screen dimensions be?
            int imageScaledWidth = image.Width;
            int imageScaledHeight = image.Height;
            if (r.Height < image.Height) {
                imageScaledWidth = (int)((float)image.Width * (float)r.Height / (float)image.Height);
                imageScaledHeight = r.Height;
            }
            // Calculate where the images should be drawn
            Rectangle imageBounds = r;
            imageBounds.Width = (this.MaxNumberImages * (imageScaledWidth + this.Spacing)) - this.Spacing;
            imageBounds.Height = imageScaledHeight;
            imageBounds = this.AlignRectangle(r, imageBounds);

            // Finally, draw the images
            for (int i = 0; i < numberOfImages; i++) {
                g.DrawImage(image, imageBounds.X, imageBounds.Y, imageScaledWidth, imageScaledHeight);
                imageBounds.X += (imageScaledWidth + this.Spacing);
            }
        }
    }
    #endregion

}
