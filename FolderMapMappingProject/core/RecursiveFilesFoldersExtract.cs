using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace FolderMapMappingProject.core
{
	public class RecursiveFilesFoldersExtract
	{
		private FolderContent mainFolder;
		private ListView mainFolderDisplay;

		public RecursiveFilesFoldersExtract(StorageFolder folderName, ListView mainFolderDisplay)
		{
			this.mainFolderDisplay = mainFolderDisplay;
			this.mainFolder = new FolderContent(folderName);
			mainFolderDisplay.DataContext = this.mainFolder.GetJoinedFolderFileList;
		}

		public FolderContent Data
		{
			get
			{
				return this.mainFolder;
			}
		}

		/// <summary>
		/// update previous saved files and folders 's info
		/// </summary>
		public void UpdateExtractedInfo()
		{
			this.mainFolder.ExtractContent();
		}

		public void UpdateExtractedInfo(Action<FolderContent> callback, Button buttonToLock)
		{
			buttonToLock.IsEnabled = false;
			Task.Run(async () =>
			{
				//CoreCursor lastCursor = CoreApplication.MainView.CoreWindow.PointerCursor;
				//CoreApplication.MainView.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Wait, CoreApplication.MainView.CoreWindow.PointerCursor.Id);
				this.mainFolder.ExtractContent().Wait();
				//CoreApplication.MainView.CoreWindow.PointerCursor = lastCursor;
				await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
					{
						callback(mainFolder);
						buttonToLock.IsEnabled = true;
					});
			});
		}
	}
}
