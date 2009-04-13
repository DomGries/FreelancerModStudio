using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;

namespace FreelancerModStudio
{
    partial class frmAbout : Form
    {
		public frmAbout()
		{
			InitializeComponent();

			//  Initialisieren Sie AboutBox, um die Produktinformationen aus den Assemblyinformationen anzuzeigen.
			//  Ändern Sie die Einstellungen für Assemblyinformationen für Ihre Anwendung durch eine der folgenden Vorgehensweisen:
			//  - Projekt->Eigenschaften->Anwendung->Assemblyinformationen
			//  - AssemblyInfo.cs
			this.Text = String.Format(Properties.Strings.AboutText, Helper.Assembly.Title);
			this.lblProductName.Text = Helper.Assembly.Product;
			this.lblVersion.Text = String.Format(Properties.Strings.AboutVersion, Helper.Assembly.Version.ToString());
			this.lblCopyright.Text = Helper.Assembly.Copyright;
			this.lblCompanyName.Text = Helper.Assembly.Company;
			this.txtDescription.Text = Helper.Assembly.Description;
		}
    }
}
