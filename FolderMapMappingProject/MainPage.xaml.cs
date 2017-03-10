using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using FolderMapMappingProject.core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Xml;
using Windows.Storage.Streams;
using Windows.System;

// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FolderMapMappingProject
{
	/// <summary>
	/// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		RecursiveFilesFoldersExtract recFolder;
		public MainPage()
		{
			this.InitializeComponent();
		}

		private async void openButton_Click(object sender, RoutedEventArgs e)
		{
			FolderPicker folderPicker = new FolderPicker();
			folderPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
			folderPicker.FileTypeFilter.Add("*");
			folderPicker.SettingsIdentifier = "PickedFolderTokenFind";
			StorageFolder folder = await folderPicker.PickSingleFolderAsync();

			if (folder != null)
			{
				this.mainFolderNameDisplay.Text = folder.Path;
				recFolder = new RecursiveFilesFoldersExtract(folder, this.MainFolder);
				StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderTokenFind", folder);
				recFolder.UpdateExtractedInfo(exportTree, Open_folder_button);
			}
		}

		private void exportButton_Click(object sender, RoutedEventArgs e)
		{
			exportTree(recFolder.Data);
		}

		private async void exportTree(FolderContent dataToExport)
		{
			FileSavePicker fileSavePicker = new FileSavePicker();
			fileSavePicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
			fileSavePicker.SettingsIdentifier = "PickedFolderTokenExport";
			fileSavePicker.FileTypeChoices.Add("FreeMind", new List<string>() { ".mm" });
			fileSavePicker.SuggestedFileName = "WindowsTreeExport";

			StorageFile file = await fileSavePicker.PickSaveFileAsync();

			if (file != null)
			{
				StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderTokenExport", file);
				using (Stream outstream = await file.OpenStreamForWriteAsync())
				{
					using (XmlWriter outMap = XmlWriter.Create(outstream))
					{
						outMap.WriteStartElement("map");
						addAttribut(outMap, "VERSION", "0.8.0.1");
						writeFolder(outMap, dataToExport);
					}
				}
				await Launcher.LaunchFileAsync(file);
			}
		}

		private void writeFolder(XmlWriter xmlWriter, FolderContent folder)
		{
			startNode(xmlWriter, folder.NodeName);
			foreach (FolderContent item in folder.Folders)
			{
				writeFolder(xmlWriter, item);
			}
			foreach (StorageFile file in folder.Files)
			{
				voidNode(xmlWriter, file.Name, "file:///" + file.Path); //file.GetThumbnailAsync().GetResults()
			}
			endNode(xmlWriter);
		}

		private void startNode(XmlWriter xmlWriter, string attributTextValue)
		{
			xmlWriter.WriteStartElement("node");
			addAttribut(xmlWriter, "TEXT", attributTextValue);
		}

		private void startNode(XmlWriter xmlWriter, string attributTextValue, string attributLinkValue)
		{
			startNode(xmlWriter, attributTextValue);
			addAttribut(xmlWriter, "LINK", attributLinkValue);
		}

		private void addAttribut(XmlWriter xmlWriter, string attributName, string attributValue)
		{
			xmlWriter.WriteAttributeString(attributName, attributValue);
		}

		private void endNode(XmlWriter xmlWriter)
		{
			xmlWriter.WriteEndElement();
		}

		private void voidNode(XmlWriter xmlWriter, string attributTextValue)
		{
			startNode(xmlWriter, attributTextValue);
			endNode(xmlWriter);
		}

		private void voidNode(XmlWriter xmlWriter, string attributTextValue, string attributLinkValue)
		{
			startNode(xmlWriter, attributTextValue, attributLinkValue);
			endNode(xmlWriter);
		}
	}
}
