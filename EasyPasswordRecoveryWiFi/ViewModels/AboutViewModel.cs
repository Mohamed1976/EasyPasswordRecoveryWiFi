using Caliburn.Micro;
using EasyPasswordRecoveryWiFi.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EasyPasswordRecoveryWiFi.ViewModels
{
	public class AboutViewModel : Screen, IShell
	{
		#region [ Constructor ]

		public AboutViewModel()
		{
		}

		#endregion

		#region [ Screen overrides ]

		/// <summary>
		/// Retrieve assembly info and update assembly properties used in view.
		/// A list of referenced assemblies is retrieved and shown in the view.
		/// </summary>
		protected override void OnInitialize()
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			object[] titleAttr = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
			if (titleAttr.Length > 0)
			{
				AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)titleAttr[0];
				if (!string.IsNullOrEmpty(titleAttribute.Title))
				{
					Title = String.Format("About {0}", titleAttribute.Title);
				}
			}

			object[] productAttr = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
			if (productAttr.Length > 0)
			{
				AssemblyProductAttribute productAttribute = (AssemblyProductAttribute)productAttr[0];
				if (!string.IsNullOrEmpty(productAttribute.Product))
				{
					ProductName = productAttribute.Product;
				}
			}

			Version = assembly.GetName().Version.ToString();

			object[] copyrightAttr = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
			if (copyrightAttr.Length > 0)
			{
				AssemblyCopyrightAttribute copyrightAttribute = (AssemblyCopyrightAttribute)copyrightAttr[0];
				if (!string.IsNullOrEmpty(copyrightAttribute.Copyright))
				{
					Copyright = copyrightAttribute.Copyright;
				}
			}

			object[] descriptionAttr = assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
			if (descriptionAttr.Length > 0)
			{
				AssemblyDescriptionAttribute descriptionAttribute = (AssemblyDescriptionAttribute)descriptionAttr[0];
				if (!string.IsNullOrEmpty(descriptionAttribute.Description))
				{
					Description = descriptionAttribute.Description;
				}
			}

			object[] companyAttr = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
			if (companyAttr.Length > 0)
			{
				AssemblyCompanyAttribute companyAttribute = (AssemblyCompanyAttribute)companyAttr[0];
				if (!string.IsNullOrEmpty(companyAttribute.Company))
				{
					CompanyName = companyAttribute.Company;
				}
			}

			ReferencedAssemblies = new List<AssemblyName>(Assembly.GetExecutingAssembly()
				.GetReferencedAssemblies());

			base.OnInitialize();
		}

		protected override void OnActivate()
		{
			base.OnActivate();
		}

		protected override void OnDeactivate(bool close)
		{
			base.OnDeactivate(close);
		}

		#endregion

		#region [ Properties ]

		public string Title { get; set; }

		public string ProductName { get; set; }

		public string Version { get; set; }

		public string Copyright { get; set; }

		public string Description { get; set; }

		public string CompanyName { get; set; }

		public List<AssemblyName> ReferencedAssemblies { get; set; }

		#endregion
	}
}
