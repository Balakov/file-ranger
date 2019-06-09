using System;
using System.Collections;
using System.Windows.Forms;

namespace Etier.IconHelper
{
	/// <summary>
	/// Maintains a list of currently added file extensions
	/// </summary>
	public class IconListManager
	{
		private Hashtable _extensionList = new Hashtable();
		private int _directoryIcon = -1;
		private int _driveIcon = -1;
		private System.Collections.ArrayList _imageLists = new ArrayList();			//will hold ImageList objects
		private IconHelper.IconReader.IconSize _iconSize;
		bool ManageBothSizes = false; //flag, used to determine whether to create two ImageLists.

		/// <summary>
		/// Creates an instance of <c>IconListManager</c> that will add icons to a single <c>ImageList</c> using the
		/// specified <c>IconSize</c>.
		/// </summary>
		/// <param name="imageList"><c>ImageList</c> to add icons to.</param>
		/// <param name="iconSize">Size to use (either 32 or 16 pixels).</param>
		public IconListManager(System.Windows.Forms.ImageList imageList, IconReader.IconSize iconSize )
		{
			// Initialise the members of the class that will hold the image list we're
			// targeting, as well as the icon size (32 or 16)
            _imageLists.Add( imageList );
			_iconSize = iconSize;
		}
		
		/// <summary>
		/// Creates an instance of IconListManager that will add icons to two <c>ImageList</c> types. The two
		/// image lists are intended to be one for large icons, and the other for small icons.
		/// </summary>
		/// <param name="smallImageList">The <c>ImageList</c> that will hold small icons.</param>
		/// <param name="largeImageList">The <c>ImageList</c> that will hold large icons.</param>
		public IconListManager(System.Windows.Forms.ImageList smallImageList, System.Windows.Forms.ImageList largeImageList )
		{
			//add both our image lists
			_imageLists.Add( smallImageList );
			_imageLists.Add( largeImageList );

			//set flag
			ManageBothSizes = true;
		}

    	/// <summary>
		/// Called publicly to add a file's icon to the ImageList.
		/// </summary>
		/// <param name="filePath">Full path to the file.</param>
		/// <returns>Integer of the icon's position in the ImageList</returns>
		public int AddFileIcon( string filePath )
		{
			// Check if the file exists
			if (!System.IO.File.Exists( filePath ))
                return 0;
			
			// Split it down so we can get the extension
			string[] splitPath = filePath.Split(new Char[] {'.'});
			string extension = (string)splitPath.GetValue( splitPath.GetUpperBound(0) );
				
			//Check that we haven't already got the extension, if we have, then
			//return back its index
			if (_extensionList.ContainsKey( extension.ToUpper() ))
			{
				return (int)_extensionList[extension.ToUpper()];		//return existing index
			} 
			else 
			{
				// It's not already been added, so add it and record its position.

				int pos = ((ImageList)_imageLists[0]).Images.Count;		//store current count -- new item's index

				if (ManageBothSizes == true)
				{
					//managing two lists, so add it to small first, then large
					((ImageList)_imageLists[0]).Images.Add( IconReader.GetFileIcon( filePath, IconReader.IconSize.Small, false ) );
					((ImageList)_imageLists[1]).Images.Add( IconReader.GetFileIcon( filePath, IconReader.IconSize.Large, false ) );
				} 
				else
				{
					//only doing one size, so use IconSize as specified in _iconSize.
					((ImageList)_imageLists[0]).Images.Add( IconReader.GetFileIcon( filePath, _iconSize, false ) );	//add to image list
				}

                _extensionList.Add(extension.ToUpper(), pos);

				return pos;
			}
		}

        public int AddDriveIcon(string drivePath)
        {
            if (_driveIcon >= 0)
                return _driveIcon;

            return _driveIcon = AddPathIcon(drivePath);
        }

        public int AddFolderIcon(string dirPath)
        {
            if (_directoryIcon >= 0)
                return _directoryIcon;

            return _directoryIcon = AddPathIcon(dirPath);
        }

        private int AddPathIcon( string dirPath )
		{
            // Check if the directory exists
            if (!System.IO.Directory.Exists(dirPath))
                return 0;

            // It's not already been added, so add it and record its position.

            int pos = ((ImageList)_imageLists[0]).Images.Count;     //store current count -- new item's index

            if (ManageBothSizes == true)
            {
                //managing two lists, so add it to small first, then large
                ((ImageList)_imageLists[0]).Images.Add(IconReader.GetFolderIcon(dirPath, IconReader.IconSize.Small, IconReader.FolderType.Closed));
                ((ImageList)_imageLists[1]).Images.Add(IconReader.GetFolderIcon(dirPath, IconReader.IconSize.Large, IconReader.FolderType.Closed));
            }
            else
            {
                //only doing one size, so use IconSize as specified in _iconSize.
                ((ImageList)_imageLists[0]).Images.Add(IconReader.GetFolderIcon(dirPath, _iconSize, IconReader.FolderType.Closed)); //add to image list
            }

            return pos;
        }

		/// <summary>
		/// Clears any <c>ImageLists</c> that <c>IconListManager</c> is managing.
		/// </summary>
		public void ClearLists()
		{
			foreach( ImageList imageList in _imageLists )
			{
				imageList.Images.Clear();	//clear current imagelist.
			}
			
			_extensionList.Clear();			//empty hashtable of entries too.
		}
	}
}
