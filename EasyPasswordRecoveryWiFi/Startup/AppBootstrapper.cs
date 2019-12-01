using Autofac;
using EasyPasswordRecoveryWiFi.Controllers;
using EasyPasswordRecoveryWiFi.Helpers;
using EasyPasswordRecoveryWiFi.Interfaces;
using EasyPasswordRecoveryWiFi.Models.Wlan;
using EasyPasswordRecoveryWiFi.Services;
using EasyPasswordRecoveryWiFi.ViewModels;
using System.Windows;
using System.Windows.Threading;

namespace EasyPasswordRecoveryWiFi.Startup
{
	public class AppBootstrapper : AutofacBootstrapper<IShell>
	{
		public AppBootstrapper()
		{
			Initialize();
		}

		/// <summary>
		/// Override to provide configuration prior to the Autofac configuration. You must call the base version BEFORE any 
		/// other statement or the behaviour is undefined.
		/// Current Defaults:
		///   EnforceNamespaceConvention = true
		///   ViewModelBaseType = <see cref="System.ComponentModel.INotifyPropertyChanged"/> 
		///   CreateWindowManager = <see cref="Caliburn.Micro.WindowManager"/> 
		///   CreateEventAggregator = <see cref="Caliburn.Micro.EventAggregator"/>
		protected override void ConfigureBootstrapper()
		{
			//  you must call the base version first!
			base.ConfigureBootstrapper();

			//  change our view model base type
			ViewModelBaseType = typeof(IShell);
		}

		protected override void Configure()
		{
			base.Configure();
		}

		/// <summary>
		/// Override to include your own Autofac configuration after the framework has finished its configuration, but 
		/// before the container is created.
		/// </summary>
		/// <param name="builder">The Autofac configuration builder.</param>
		protected override void ConfigureContainer(ContainerBuilder builder)
		{
			base.ConfigureContainer(builder);
			builder.RegisterType<ShellViewModel>().As<IShell>();
			builder.RegisterType<ErrorHandler>().As<IErrorHandler>();
			builder.RegisterType<MainController>().AsSelf().SingleInstance();
			builder.RegisterType<ProfileService>().As<IProfileService>();
			builder.RegisterType<ProfileFactory>().As<IProfileFactory>();
			builder.RegisterType<StorageService>().As<IStorageService>();
			builder.RegisterType<RegExService>().As<IRegExService>().SingleInstance();
			builder.RegisterType<ConfigurationProvider>().As<IConfigurationProvider>().SingleInstance();
			builder.RegisterType<BusyStateManager>().As<IBusyStateManager>().SingleInstance();
#if MOCK_DATA
            builder.RegisterType<MockWiFiAdapter>().As<IWiFiService>();
#else
            builder.RegisterType<NativeWiFiAdapter>().As<IWiFiService>();
#endif
		}

		protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs ex)
		{
			base.OnUnhandledException(sender, ex);
			MessageBox.Show($"Unhandled exception: {ex.Exception.Message}");
		}
	}
}
