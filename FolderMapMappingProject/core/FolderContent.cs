using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;

namespace FolderMapMappingProject.core
{
	public class FolderContent
	{
		private StorageFolder currentfolder;
		private List<FolderContent> folders;
		private List<StorageFile> files;
		private ObservableCollection<IStorageItem> joinedList;

		public List<StorageFile> Files
		{
			get
			{
				return files;
			}
		}

		public List<FolderContent> Folders
		{
			get
			{
				return folders;
			}
		}

		public ObservableCollection<IStorageItem> GetJoinedFolderFileList
		{
			get
			{
				return joinedList;
			}
		}

		public string NodeName
		{
			get
			{
				return this.currentfolder.DisplayName;
			}
		}

		public FolderContent(StorageFolder folder)
		{
			this.currentfolder = folder;
			files = new List<StorageFile>();
			folders = new List<FolderContent>();
			//files = new ObservableCollection<StorageFile>();
			//folders = new ObservableCollection<FolderContent>();
			joinedList = new ObservableCollection<IStorageItem>();
			//files.CollectionChanged += filesOrFolders_CollectionChanged;
			//folders.CollectionChanged += filesOrFolders_CollectionChanged;
		}

		private void filesOrFolders_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
					int i = 0;
					if (sender is ObservableCollection<FolderContent>)
					{
						foreach (IStorageItem item in e.NewItems)
						{
							joinedList.Insert(e.NewStartingIndex + i++, item);
						}
					}
					else
					{
						int folderSize = folders.Count;
						foreach (IStorageItem item in e.NewItems)
						{
							joinedList.Insert(folderSize + e.NewStartingIndex + i++, item);
						}
					}
					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
					break;
				case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
					break;
				default:
					break;
			}
		}

		public async Task ExtractContent()
		{
			//int localIncr = 0;

			var tmpFolders = currentfolder.GetFoldersAsync();
			var tmpFiles = currentfolder.GetFilesAsync();

			//ParallelLoopResult parallelResult = Parallel.ForEach(tmpFolders, folder => {
			//	FolderContent tmp = new FolderContent(folder);
			//	this.folders.Add(tmp);
			//	tmp.ExtractContent();
			//});
			//foreach (StorageFolder folder in folders)
			//{
			//	FolderContent tmp = new FolderContent(folder);
			//	tmp.ExtractContent();
			//	this.folders.Add(tmp);
			//	//Debug.WriteLine(this.NodeName + (++localIncr).ToString() + tmp.NodeName);
			//}

			Task.WaitAll(extractFiles(tmpFiles), extractFolders(tmpFolders));
		}

		private async Task extractFiles(IAsyncOperation<IReadOnlyList<StorageFile>> roFileListAwaitable)
		{
			foreach (StorageFile file in await roFileListAwaitable)
			{
				files.Add(file);
			}
		}

		private async Task extractFolders(IAsyncOperation<IReadOnlyList<StorageFolder>> roFolderListAwaitable)
		{
			List<Task> awaitableList = new List<Task>();
			foreach (StorageFolder folder in await roFolderListAwaitable)
			{
				FolderContent tmp = new FolderContent(folder);
				awaitableList.Add(tmp.ExtractContent());
				this.folders.Add(tmp);
			}
			//ParallelLoopResult parallelResult = Parallel.ForEach(await roFolderListAwaitable, folder =>
			//{
			//	FolderContent tmp = new FolderContent(folder);
			//	awaitableList.Add(tmp.ExtractContent());
			//	this.folders.Add(tmp);
			//});

			Task.WaitAll(awaitableList.ToArray());
		}
	}
}