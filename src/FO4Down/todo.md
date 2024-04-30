Go to "..Steam\steamapps\common\Fallout 4" and open Fallout4_Default.ini. 
So edit the second line from sLanguage=en to sLanguage=it.  
To be honest I got good results with the interface, but audio voice is still English. 
I'm just trying to download the full language pack, then I'll test it again later. Meanwhile, only let me know.

- Go to Documents\My Games\Fallout -> Open Fallout4.ini and search this line 
- SResourceArchiveList=Fallout4 - Voices.ba2 in [ARCHIVE] and change it in this way 
- SResourceArchiveList=Fallout4 - Voices.ba2 => SResourceArchiveList=Fallout4 - Voicesces_it.ba2
- Go to Data folder and check if there is some Voices file duplicated or with wrong name. 
  In my case I found DLCRobot - Voices_en.ba2 and Fallout4 - Voices.ba2. 
  So I remove the first (it was duplicate) and edited the second to Fallout4 - Voices_it.ba2.




Potential Fix: 
	Add a checkbox "Update language in Fallout4.ini" on startup
	Update Fallout4.ini and update the language to target language that was used by the tool. 
	Remove any duplicate files in the Data folder.

Another issue: App crashes if read access is denied to a game folder (not necessarily fallout 4) 
			  in the steam game locator. It can also be a good idea to search for appmanifest_377160 in same folder as .exe 
			  and parent folders as a fallback when steam game is not found.