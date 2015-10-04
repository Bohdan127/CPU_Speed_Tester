using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Management;
using Microsoft.Win32;
using System.ComponentModel;
using System.Threading;

namespace WpfApplication1
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private const int Incr = 2;

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			//Get proc name 
			var centralProcessor = Registry.LocalMachine.OpenSubKey(@"Hardware\Description\System\CentralProcessor\0", 
										 RegistryKeyPermissionCheck.ReadSubTree);
			if (centralProcessor != null && centralProcessor.GetValue("ProcessorNameString") != null)
				ProcName.Text = centralProcessor.GetValue("ProcessorNameString").ToString();			
		}

		void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			pbStatus.Value = e.ProgressPercentage;
		}

		private void Button_Click_2(object sender, RoutedEventArgs e)
		{
			btnRun.IsEnabled = false;
			pbStatus.Value = 0;
			pbStatus.IsIndeterminate = true;
			pbStatus.Orientation = Orientation.Horizontal;

			var worker = new BackgroundWorker()
			{
				WorkerReportsProgress = true,
			};

			worker.DoWork += worker_DoWork;
			worker.ProgressChanged += worker_ProgressChanged;

			worker.RunWorkerAsync();
		}

		void worker_DoWork(object sender, DoWorkEventArgs e)
		{
			var startTime = DateTime.Now;
			var lastPercent = -1;

			for (int j = 0; j < int.MaxValue; j++)
			{//we need user Convert.To instead (int) because it has more logic loading for CPU and at the end we catch 100 instead of 99 from (int) convertation
				var nextPercent = Convert.ToInt32((double)j * 100 / int.MaxValue);
				if (lastPercent != nextPercent)
				{
					lastPercent = nextPercent;
					this.Dispatcher.Invoke((Action)delegate()
					{
						btnRun.Content = lastPercent + "%";
					});
				}
			}

			var totalTime =(DateTime.Now - startTime).TotalSeconds;
			var mark = 100;//start values
			var curIncr = 10;
			var spliter = 10;

			do
			{
				var res = totalTime - spliter;
				if (res <= curIncr)
				{
					mark -= (int)(res / (curIncr / 10));
					break;
				}
				else
				{
					spliter += curIncr;
					curIncr += Incr;
					mark -= 10;
				}
			} while (totalTime - spliter >= 0);
			
			if (mark < 1)
				mark = 1;

			this.Dispatcher.Invoke((Action)delegate()
			{
				pbStatus.IsIndeterminate = false;
				pbStatus.Orientation = Orientation.Vertical;
			});

			for (int k = 0; k < mark; k++)
			{
				(sender as BackgroundWorker).ReportProgress(k);
				Thread.Sleep(10);
			}			
		}
	}
}